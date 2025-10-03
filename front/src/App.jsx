import React, { useState } from 'react';
import Button from './components/Button';
import styles from './App.module.css';

export default function App() {
  const [step, setStep] = useState('enterId');
  const [badge, setBadge] = useState('');
  const [inputVal, setInputVal] = useState('');
  const [tools, setTools] = useState([]);
  const [action, setAction] = useState(null); // "take" или "return"
  const [fileType, setFileType] = useState('image'); // "image" или "zip"
  const [selectedFile, setSelectedFile] = useState(null);

  function reset() {
    setStep('enterId');
    setBadge('');
    setInputVal('');
    setTools([]);
    setAction(null);
    setFileType('image');
    setSelectedFile(null);
  }

  function goBack() {
    if (step === 'upload') setStep('enterId');
    else if (step === 'tools') setStep('upload');
    else if (step === 'action') setStep('tools');
    else if (step === 'report') setStep('action');
  }

  async function fetchTools() {
    try {
      if (!selectedFile) {
        console.error("❌ Файл не выбран");
        alert("Выберите файл перед отправкой");
        return false; // 🚨 неуспех
      }

      const formData = new FormData();
      formData.append("file", selectedFile);

      let url =
        fileType === "image"
          ? "http://localhost:8001/api/v1/Tools/Test"
          : "http://localhost:8001/api/v1/Tools/TestZip";

      console.log(`📡 Отправка запроса на: ${url}`, formData);

      const response = await fetch(url, {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        console.error(
          `❌ Сервер вернул ошибку: ${response.status} ${response.statusText}`
        );
        alert(`Ошибка: ${response.status} ${response.statusText}`);
        return false;
      }

      let data;
      try {
        data = await response.json();
      } catch (jsonErr) {
        console.error("❌ Ошибка парсинга JSON:", jsonErr);
        alert("Ошибка обработки ответа сервера (невалидный JSON)");
        return false;
      }

      let normalized = [];
      for (const [filename, toolObj] of Object.entries(data)) {
        let tools = Object.entries(toolObj).map(([name, confidence]) => ({
          id: name,
          name: name,
          confidence: confidence,
        }));
        normalized.push({ filename, tools });
      }

      if (normalized.length === 0) {
        console.warn("⚠️ Сервер вернул пустой результат", normalized);
      } else {
        console.log("✅ Полученные данные:", normalized);
      }

      setTools(normalized);
      return true; // успех
    } catch (err) {
      console.error("💥 Ошибка загрузки инструментов:", err);
      alert("Ошибка загрузки инструментов. Проверьте консоль для деталей.");
      return false;
    }
  }
  async function confirmReport() {
    try {
      let payload = [];

      if (fileType === "image") {
        payload = tools.map(t => ({
          toolId: t.id,
          action: action === "take" ? "take" : "return",
        }));
      } else {
        payload = tools.flatMap(fileBlock =>
          fileBlock.tools.map(t => ({
            toolId: t.id,
            action: action === "take" ? "take" : "return",
          }))
        );
      }

      const url =
        action === "take"
          ? `http://localhost:8001/api/v1/Tools/TakeTools/${badge}`
          : `http://localhost:8001/api/v1/Tools/ReturnTools/${badge}`;

      await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      reset();
    } catch (err) {
      console.error("Ошибка при подтверждении", err);
    }
  }


  return (
    <div className={styles.container}>
      <div className="background"></div>
      {/* Шаг 1: Ввод табельного номера */}
      {step === 'enterId' && (
        <div className={styles.centered}>
          <h2>Введите табельный номер</h2>
          <input
            type="text"
            className={styles.inputField}
            value={inputVal}
            onChange={e => setInputVal(e.target.value)}
            placeholder="Табельный номер"
          />
          <div style={{ marginTop: 20 }}>
            <Button onClick={() => {
              if (inputVal.trim()) {
                setBadge(inputVal.trim());
                setStep('upload');
              }
            }}>
              Продолжить
            </Button>
          </div>
        </div>
      )}

      {/* Шаг 2: Загрузка файла */}
      {step === 'upload' && (
        <div className={styles.centered}>
          <h2>Загрузите файл с инструментами</h2>

          <div style={{ marginBottom: 10 }}>
            <label>
              <input
                type="radio"
                checked={fileType === "image"}
                onChange={() => setFileType("image")}
              />
              Одно изображение
            </label>
            <label style={{ marginLeft: 20 }}>
              <input
                type="radio"
                checked={fileType === "zip"}
                onChange={() => setFileType("zip")}
              />
              ZIP архив
            </label>
          </div>

          <div className={styles.frame}>
            <label htmlFor="fileUpload">
              {selectedFile ? selectedFile.name : "Выберите файл"}
            </label>
            <input
              id="fileUpload"
              type="file"
              accept={fileType === "image" ? "image/*" : ".zip"}
              onChange={e => setSelectedFile(e.target.files[0])}
            />
          </div>

          <div style={{ marginTop: 20 }}>
            <Button
              onClick={async () => {
                const ok = await fetchTools();
                if (ok) {
                  setStep('tools');
                }
              }}
            >
              Отправить на проверку
            </Button>

          </div>
        </div>
      )}

      {/* Шаг 3: Список инструментов */}
      {step === 'tools' && (
        <div>
          <h2>Распознанные инструменты:</h2>
          <div className={styles.scrollBox}>
            {tools.map((file, idx) => (
              <div key={idx} style={{ marginBottom: 20 }}>
                <h4>{file.filename}</h4>
                <ul>
                  {file.tools.map((t, tidx) => (
                    <li key={tidx}>
                      {t.name} <span className={t.confidence === 1 ? styles.ok : styles.fail}></span>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
          <div style={{ textAlign: 'right', marginTop: 20 }}>
            <Button onClick={() => setStep('action')}>Подтвердить список</Button>
          </div>
        </div>
      )}




      {/* Шаг 4: Выбор действия */}
      {step === 'action' && (
        <div className={styles.centered}>
          <h2>Выберите действие</h2>
          <Button onClick={() => {
            setAction('take');
            setStep('report');
          }}>Получить инструменты</Button>
          <Button onClick={() => {
            setAction('return');
            setStep('report');
          }}>Сдать инструменты</Button>
        </div>
      )}

      {/* Шаг 5: Рапорт */}
      {step === 'report' && (
        <div>
          <h2>Отчёт</h2>
          <p>
            Сотрудник с табельным номером <b>{badge}</b>
            {action === 'take' ? ' получил' : ' сдал'} набор инструментов:
          </p>

          <div className={styles.scrollBox}>
            {tools.map((file, idx) => (
              <div key={idx} style={{ marginBottom: 20 }}>
                <h4>{file.filename}</h4>
                <ul>
                  {file.tools.map((t, tidx) => (
                    <li key={tidx}>
                      {t.name} <span className={t.confidence === 1 ? styles.ok : styles.fail}></span>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>

          <div style={{ textAlign: 'right', marginTop: 20 }}>
            <Button onClick={confirmReport}>Согласен</Button>
          </div>
        </div>
      )}



      {/* Кнопка Назад */}
      {
        step !== 'enterId' && (
          <div style={{ position: 'fixed', bottom: 20, left: 20 }}>
            <Button onClick={goBack}>Назад</Button>
          </div>
        )
      }
    </div >

  );
}
