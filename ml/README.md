# FasterRCNN API Documentation

## 1. Общая информация
**Название сервиса:** FasterRCNN API  
**Описание:** REST API для детекции объектов на изображениях с использованием PyTorch FasterRCNN.  
**Формат ответа:** JSON с `bbox`, `label`, `score`.  
**Поддержка batch:** да, с настраиваемым `batch_size`.  
**Встроенная очистка детекций:** cross-class NMS (опционально).  

---

## 2. Запуск через Docker

### Docker build
```bash
docker build -t fasterrcnn-api .
```

### Docker run
```bash
docker run -p 8000:8000 fasterrcnn-api
```

- `-p 8000:8000` — проброс порта на локальную машину.  
- Сервис будет доступен по адресу: `http://localhost:8000`.  

**Опционально (GPU):**
```bash
docker run --gpus all -p 8000:8000 fasterrcnn-api
```

---

## 3. API Эндпоинты

### 3.1 Healthcheck
**GET /health**  
Проверка состояния сервиса.

**Пример запроса:**
```bash
curl http://localhost:8000/health
```

**Пример ответа:**
```json
{"status": "ok"}
```

---

### 3.2 Predict (одно изображение)
**POST /predict**  
Детекция объектов на одном изображении.

**Параметры:**
- `file` — изображение, тип `multipart/form-data`.

**Пример запроса:**
```bash
curl -X POST "http://localhost:8000/predict" \
-F "file=@test.jpg"
```

**Пример ответа:**
```json
{
  "detections": [
    {"bbox": [2138, 1027, 2676, 2493], "label": 11, "score": 0.997},
    {"bbox": [805, 568, 1361, 2792], "label": 7, "score": 0.994}
  ]
}
```

---

### 3.3 Predict batch (несколько изображений)
**POST /predict_batch**  
Детекция объектов на нескольких изображениях с настройкой размера батча.

**Параметры:**
- `files` — список изображений, `multipart/form-data`.  
- `batch_size` (опционально) — размер батча, default=4.

**Пример запроса:**
```bash
curl -X POST "http://localhost:8000/predict_batch?batch_size=4" \
-F "files=@img1.jpg" \
-F "files=@img2.jpg" \
-F "files=@img3.jpg"
```

**Пример ответа:**
```json
{
  "batch_detections": [
    [
      {"bbox": [50, 50, 200, 200], "label": 1, "score": 0.9}
    ],
    [
      {"bbox": [60, 60, 210, 210], "label": 2, "score": 0.85}
    ],
    [
      {"bbox": [300, 300, 400, 400], "label": 2, "score": 0.95}
    ]
  ]
}
```

---

## 4. Конфигурация

Файл `config.yaml`:
```yaml
model:
  weights: "app/models/fasterrcnn.pth"
  device: "cpu"            # cpu или cuda
  score_threshold: 0.5     # минимальный score для детекций

api:
  host: "0.0.0.0"
  port: 8000
```

Можно менять `device` и `score_threshold` без изменения кода.

---

## 5. Дополнительно

- **Шрифты для отрисовки:** рекомендуется положить `.ttf` файл в проект (`app/fonts/`), если нужен вывод изображений с боксами.  
- **Cross-class NMS:** можно включить при необходимости для фильтрации пересекающихся объектов разных классов.  
- **Swagger UI:** документация автоматически доступна по адресу `http://localhost:8000/docs`.  

---

## 6. Рекомендации по эксплуатации

- Для больших батчей и ускорения inference — использовать GPU.  
- На сервере — рекомендовано поднимать через Docker Compose с volume для модели и логов.  
- Логи ошибок и запросов можно подключить через `logging` в `main.py`.

