import React, { useState } from 'react';
import Button from './components/Button';
import styles from './App.module.css';

export default function App() {
  const [step, setStep] = useState('enterId');
  const [badge, setBadge] = useState('');
  const [inputVal, setInputVal] = useState('');
  const [tools, setTools] = useState([]);
  const [action, setAction] = useState(null);
  const [fileType, setFileType] = useState('image');
  const [selectedFile, setSelectedFile] = useState(null);
  const [loading, setLoading] = useState(false); // 🔥 индикатор загрузки

  function reset() {
    setStep('enterId');
    setBadge('');
    setInputVal('');
    setTools([]);
    setAction(null);
    setFileType('image');
    setSelectedFile(null);
    setLoading(false);
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
        alert("Выберите файл перед отправкой");
        return false;
      }

      setLoading(true); // 🚀 показываем "Загрузка..."

      const formData = new FormData();
      formData.append("file", selectedFile);

      let url =
        fileType === "image"
          ? "http://localhost:8001/api/v1/Tools/Test"
          : "http://localhost:8001/api/v1/Tools/TestZip";

      const response = await fetch(url, {
        method: "POST",
        body: formData,
      });

      if (!response.ok) {
        alert(`Ошибка: ${response.status} ${response.statusText}`);
        return false;
      }

      const data = await response.json();
      let normalized = [];

      // 🔧 нормализуем под оба типа
      if (Array.isArray(data)) {
        normalized = data;
      } else {
        for (const [filename, toolObj] of Object.entries(data)) {
          let tools = Object.entries(toolObj).map(([name, confidence]) => ({
            id: name,
            name: name,
            confidence: confidence,
          }));
          normalized.push({ filename, tools });
        }
      }

      setTools(normalized);
      return true;
    } catch (err) {
      console.error("Ошибка загрузки:", err);
      alert("Ошибка загрузки. Проверьте консоль.");
      return false;
    } finally {
      setLoading(false); // ✅ загрузка завершена
    }
  }

  function downloadJSON() {
    const blob = new Blob([JSON.stringify(tools, null, 2)], {
      type: "application/json",
    });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "recognized_tools.json";
    a.click();
    URL.revokeObjectURL(url);
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
            {loading ? (
              <Button disabled>Загрузка...</Button>
            ) : (
              <Button
                onClick={async () => {
                  const ok = await fetchTools();
                  if (ok) setStep('tools');
                }}
              >
                Отправить на проверку
              </Button>
            )}
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
                      {t.name}{" "}
                      <span
                        className={t.confidence > 0 ? styles.ok : styles.fail}
                      ></span>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>

          {/* 💾 Кнопка скачать JSON — только для ZIP */}
          {fileType === "zip" && (
            <div style={{ marginTop: 10 }}>
              <Button onClick={downloadJSON}>Скачать JSON</Button>
            </div>
          )}

          <div style={{ textAlign: 'right', marginTop: 20 }}>
            <Button onClick={() => setStep('action')}>Подтвердить список</Button>
          </div>
        </div>
      )}

      {/* Шаг 4: Выбор действия */}
      {step === 'action' && (
        <div className={styles.centered}>
          <h2>Выберите действие</h2>
          <Button onClick={() => { setAction('take'); setStep('report'); }}>
            Получить инструменты
          </Button>
          <Button onClick={() => { setAction('return'); setStep('report'); }}>
            Сдать инструменты
          </Button>
        </div>
      )}

      {/* Шаг 5: Рапорт */}
      {step === 'report' && (
        <div>
          <h2>Отчёт</h2>
          <p>
            Сотрудник с табельным номером <b>{badge}</b>{" "}
            {action === 'take' ? 'получил' : 'сдал'} набор инструментов:
          </p>

          <div className={styles.scrollBox}>
            {tools.map((file, idx) => (
              <div key={idx} style={{ marginBottom: 20 }}>
                <h4>{file.filename}</h4>
                <ul>
                  {file.tools.map((t, tidx) => (
                    <li key={tidx}>
                      {t.name}{" "}
                      <span
                        className={t.confidence > 0 ? styles.ok : styles.fail}
                      ></span>
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
      {step !== 'enterId' && (
        <div style={{ position: 'fixed', bottom: 20, left: 20 }}>
          <Button onClick={goBack}>Назад</Button>
        </div>
      )}
    </div>
  );
}
