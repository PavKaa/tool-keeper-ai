FROM python:3.12-slim

WORKDIR /app

COPY app/ app/
COPY requirements.txt .

RUN apt-get update && apt-get install -y wget curl jq ca-certificates && rm -rf /var/lib/apt/lists/* \
 && mkdir -p app/models \
 && API_URL='https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key=https%3A%2F%2Fdisk.yandex.ru%2Fd%2FR4sdbN0ae_0PPA' \
 && json=$(curl -sS "$API_URL" || true) \
 && echo "$json" > /tmp/yd.json \
 && href=$(echo "$json" | jq -r '.href') \
 && if [ -z "$href" ] || [ "$href" = "null" ]; then echo "ERROR: failed to get href from Yandex API. Response:"; cat /tmp/yd.json; exit 1; fi \
 && wget --tries=3 --timeout=30 -O app/models/fasterrcnn.pth "$href"


RUN pip install --no-cache-dir -r requirements.txt

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
