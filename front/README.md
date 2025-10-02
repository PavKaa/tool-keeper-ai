Tool Recognition Frontend
=========================

Фронтенд часть сервиса для распознавания инструментов.
Написан на React + Vite.

Как запустить
-------------

1. Установить зависимости:
   npm install

2. Запустить в режиме разработки:
   npm run dev

   По умолчанию проект поднимется на:
   http://localhost:5173

3. Сборка для продакшна:
   npm run build
   npm run preview

Подключение к API
-----------------

Фронтенд обращается к backend по адресу:

- Backend (ASP.NET / FastAPI): http://localhost:8000/api/v1
- ML сервис (PyTorch): http://localhost:3000

Эти значения можно вынести в .env и использовать в коде через import.meta.env.

Docker (опционально)
--------------------

Собрать контейнер:
   docker build -t tool-frontend .

Запустить:
   docker run -p 8080:80 tool-frontend

После этого фронт будет доступен по адресу:
   http://localhost:8080
