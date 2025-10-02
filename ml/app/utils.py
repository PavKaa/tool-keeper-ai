import io
import numpy as np
from PIL import Image, ImageDraw, ImageFont
from typing import List, Dict, Union

def load_image(file) -> Image.Image:
    image = Image.open(io.BytesIO(file)).convert("RGB")
    image = np.array(image)
    return image


def draw_detections(image: Image.Image, detections: List[Dict[str, Union[List[int], float]]], output_path: str="output.jpg"):
    """
    detections: список вида
    [
      {"bbox": [x_min, y_min, x_max, y_max], "label": "cat", "score": 0.94},
      {"bbox": [x_min, y_min, x_max, y_max], "label": "dog", "score": 0.81}
    ]
    """
    CLASS_COLORS = {
        0: "red",
        1: "green",
        2: "blue",
        3: "orange",
        4: "purple",
        5: "cyan",
        6: "magenta",
        7: "yellow",
        8: "brown",
        9: "pink",
        10: "lime"
    }

    image = Image.fromarray(image)
    draw = ImageDraw.Draw(image)

    # Подключаем шрифт
    try:
        font = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial.ttf", 100)
    except:
        print("⚠️ Шрифт arial.ttf не найден, используем шрифт по умолчанию.")
        font = ImageFont.load_default()

    for detection in detections:
        bbox = detection["bbox"]
        label = detection.get("label", "obj")
        score = detection.get("score", 0.0)

        x1, y1, x2, y2 = map(int, bbox)
        color = CLASS_COLORS.get(label, "white")  # Цвет по классу, белый по умолчанию

        draw.rectangle([x1, y1, x2, y2], outline=color, width=5)
        text = f"{label}: {score:.2f}"
        draw.text((x1, y1 - 100), text, fill=color, font=font)
    image.save(output_path)
    print(f"✅ Изображение сохранено: {output_path}")


