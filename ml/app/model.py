from PIL import Image
import torch
from torchvision.models.detection import fasterrcnn_resnet50_fpn
from torchvision.models.detection.faster_rcnn import FastRCNNPredictor
import albumentations as A
from albumentations.pytorch import ToTensorV2
from torchvision.ops import nms, box_iou

from app.utils import draw_detections
from typing import List, Dict, Union

def apply_nms(detections: List[Dict[str, Union[List[int], float]]], iou_threshold=0.5)->List[Dict[str, Union[List[int], float]]]:
    """
    predictions: выход модели FasterRCNN (list of dicts, один dict на изображение)
                 каждый dict содержит:
                 - 'boxes' : [N, 4] тензор с координатами [x1, y1, x2, y2]
                 - 'scores': [N] тензор с confidence
                 - 'labels': [N] тензор с классами
    iou_thresh: float, порог IoU для NMS
    score_thresh: float, отфильтровываем слабые детекции по confidence
    """

    if len(detections) == 0:
        return []

    # Преобразуем в тензоры
    boxes = torch.tensor([det["bbox"] for det in detections], dtype=torch.float32)
    scores = torch.tensor([det["score"] for det in detections], dtype=torch.float32)
    labels = [det["label"] for det in detections]

    # Применяем NMS
    keep_indices = nms(boxes, scores, iou_threshold)

    # Возвращаем только те боксы, которые прошли NMS
    nms_detections = [detections[i] for i in keep_indices]

    return nms_detections

def id2partname(results: Dict[str, float]):

    id2name = {
        "1": "466223255",
        "2": "48301002",
        "3": "4636",
        "4": "453",
        "5": "65751280",
        "6": "65015160",
        "7": "1048_250",
        "8": "4025_8",
        "9": "CANKEY01",
        "10": "open_box_13",
        "11": "66005160",
    }
    mapped_results = {value: results.get(key, 0) for key, value in id2name.items()}
    return mapped_results

def cross_class_nms(detections: List[Dict[str, Union[List[int], float]]], iou_threshold: float=0.1):
    """
    detections: список словарей {"bbox": [...], "label": int, "score": float}
    iou_threshold: float, порог IoU для подавления между всеми классами
    """
    if len(detections) == 0:
        return []

    boxes = torch.tensor([det["bbox"] for det in detections], dtype=torch.float32)
    scores = torch.tensor([det["score"] for det in detections], dtype=torch.float32)

    keep = torch.ones(len(detections), dtype=torch.bool)  # какие боксы оставить

    # Сортируем по score по убыванию
    sorted_indices = torch.argsort(scores, descending=True)

    for i in range(len(sorted_indices)):
        if not keep[sorted_indices[i]]:
            continue
        box_i = boxes[sorted_indices[i]].unsqueeze(0)  # [1,4]

        for j in range(i+1, len(sorted_indices)):
            if not keep[sorted_indices[j]]:
                continue
            box_j = boxes[sorted_indices[j]].unsqueeze(0)
            iou = box_iou(box_i, box_j)[0, 0].item()
            if iou > iou_threshold:
                keep[sorted_indices[j]] = False  # оставляем только более высокий score

    nms_detections = [detections[i] for i in range(len(detections)) if keep[i]]
    return nms_detections



class DetectionModel:
    """
    Класс для загрузки модели и выполнения предсказаний
    """
    def __init__(self, weights_path: str, device: str="cpu", batch_size: int = 4, score_threshold: float=0.5):
        """
        weights_path: путь к файлу с весами модели
        device: "cpu" или "cuda"
        score_threshold: порог confidence для фильтрации детекций
        """
        self.device = torch.device(device)
        self.score_threshold = score_threshold
        self.batch_size = batch_size

        # Загружаем предобученную FasterRCNN
        self.model = fasterrcnn_resnet50_fpn(weights=None)
        in_features = self.model.roi_heads.box_predictor.cls_score.in_features
        self.model.roi_heads.box_predictor = FastRCNNPredictor(in_features, num_classes=12)  # COCO по умолчанию

        # Загружаем свои веса
        self.model.load_state_dict(torch.load(weights_path, map_location=self.device))
        self.model.to(self.device)
        self.model.eval()

        self.transform = A.Compose([
            A.Normalize(
                mean=(0.485, 0.456, 0.406),
                std=(0.229, 0.224, 0.225),
                max_pixel_value=255.0,
                p=1.0
            ),
        ToTensorV2()
        ])

    def _postrocess(self, output: Dict[str, torch.Tensor]):
        """
        Преобразуем выход модели в удобный формат
        """
        results = []
        for box, label, score in zip(output["boxes"], output["labels"], output["scores"]):
            if score >= self.score_threshold:
                results.append({
                    "bbox": box.cpu().numpy().tolist(),
                    "label": int(label.cpu().numpy()),
                    "score": float(score.cpu().numpy())
                })
        return results

    def predict(self, image: Image.Image):
        """
        Функция для выполнения предсказания на одном изображении.

        image: PIL.Image
        return: список детекций вида
        [
          {"bbox": [x_min, y_min, x_max, y_max], "label": "cat", "score": 0.94},
          {"bbox": [x_min, y_min, x_max, y_max], "label": "dog", "score": 0.81}
        ]
        """
        out_transform = self.transform(image=image)
        img_tensor = out_transform["image"].unsqueeze(0).to(self.device)
        with torch.no_grad():
            from time import time
            s = time()
            outputs = self.model(img_tensor)
            print(f"⏱ Время инференса: {time()-s:.3f} сек")

        detections = self._postrocess(outputs[0])
        detections = apply_nms(detections)
        draw_detections(image, detections)
        result = {str(item['label']): float(item['score']) for item in detections}
        result = id2partname(result)
        return result

    def predict_batch(self, images: List[Image.Image]):
        """
        Функция для выполнения предсказания на батче изображений.

        images: список PIL.Image
        return: список списков детекций для каждого изображения
        """
        # Конвертируем в тензоры
        img_tensors = []
        for img in images:
            out_transform = self.transform(image=img)
            img_tensor = out_transform["image"].to(self.device)
            img_tensors.append(img_tensor)


        detections = []
        results = {}
        # Обрабатываем батчами
        for i in range(0, len(img_tensors), self.batch_size):
            batch = img_tensors[i:i+self.batch_size]
            with torch.no_grad():
                outputs = self.model(batch)  # FasterRCNN принимает list of tensors
            for output in outputs:
                detection = self._postrocess(output)
                detections.append(cross_class_nms(detection))
            # print(detections)
            for detection in detections:
                draw_detections(images[i], detection, output_path=f"output_{i}.jpg")
                result = {str(item['label']): float(item['score']) for item in detection}
                print(result)
                result = id2partname(result)
                print(result)

                results[len(results)] = result
        return results

            