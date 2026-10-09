# Пуші гравцям зі скрипта

`send_push.py` надсилає пуш через Firebase Cloud Messaging (HTTP v1): одне повідомлення на кожну мову гри, на тему `lang_<мова>`, у канал «Новини Бюро» (`hauntscope_news`). Гра тримає на цих темах лише пристрої, де гравець увімкнув сповіщення (GDD 5.36).

```bash
# тексти й ліміти, без мережі
python -I Tools/Push/send_push.py check Tools/Push/messages/test_signal.json

# Firebase перевіряє повідомлення, але нікому не доставляє
python -I Tools/Push/send_push.py send Tools/Push/messages/test_signal.json --key D:/Keys/hauntscope-firebase-admin.json

# тест на свій телефон (гра українською)
python -I Tools/Push/send_push.py send Tools/Push/messages/test_signal.json --langs uk --yes --key D:/Keys/hauntscope-firebase-admin.json

# усім гравцям, 6 мов
python -I Tools/Push/send_push.py send Tools/Push/messages/<файл>.json --yes --key D:/Keys/hauntscope-firebase-admin.json
```

- Файл повідомлення — `messages/<назва>.json`: `{"en": {"title": ..., "body": ...}, "uk": {...}, "es-419", "pt-BR", "de", "id"}`. Без `--langs` потрібні всі 6 мов.
- Заголовок до 50, текст до 150 символів: довші шторка Android обрізає (скрипт попереджає).
- Ключ — сервісний акаунт Firebase Admin (`D:\Keys\hauntscope-firebase-admin.json` або змінна `FIREBASE_JSON_KEY`). Ніколи не класти в репозиторій.
- Тихих годин у пушів немає (на відміну від нагадувань гри): надсилати вдень за часом основної аудиторії мови.
- У тексті — лише те, що в грі справді є.
