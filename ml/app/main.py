import yaml
from fastapi import FastAPI, UploadFile, File
from app.model import DetectionModel
from app.utils import load_image, draw_detections

# Загружаем конфиг
with open("app/config.yaml", "r") as f:
    config = yaml.safe_load(f)

model = DetectionModel(
    weights_path=config["model"]["weights"],
    device=config["model"]["device"],
    batch_size=config["model"].get("batch_size", 1),
)

app = FastAPI(title="FasterRCNN API")

@app.post("/predict")
async def predict(file: UploadFile = File(...)):
    """
    Предсказание на одном изображении
    
    file: загружаемое изображение
    """
    image = load_image(await file.read())
    detections = model.predict(image)
    return {"detections": detections}

@app.post("/predict_batch")
async def predict_batch(files: list[UploadFile] = File(...)):
    """
    Предсказание на батче изображений
    files: список загружаемых изображений
    """
    images = [load_image(await file.read()) for file in files]
    detections = model.predict_batch(images)
    return {"batch_detections": detections}

@app.get("/health")
def health():
    return {"status": "ok"}
