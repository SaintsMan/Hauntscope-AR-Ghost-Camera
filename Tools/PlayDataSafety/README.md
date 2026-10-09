# Data safety в Google Play зі скрипта

`data_safety.py` заповнює форму Data safety (App content → Data safety) з відповідей у самому скрипті й завантажує її через Play Developer API (`applications/{package}/dataSafety`). Завантаження **замінює всю форму**.

```bash
python -I Tools/PlayDataSafety/data_safety.py build            # лише зібрати hauntscope_data_safety.csv
python -I Tools/PlayDataSafety/data_safety.py apply --yes --key D:/Keys/play-service-account.json
```

- `template.csv` — порожній експорт форми Play Console (жовтень 2026): ID питань і варіантів, без відповідей. Якщо Play змінить форму, експортувати новий шаблон у Play Console (Export to CSV) і очистити колонку `Response value`.
- `hauntscope_data_safety.csv` — що саме пішло в Play (генерується).
- Відповіді (`GENERAL`, `DATA_TYPES`) мають збігатися з політикою приватності та GDD 5.39: AdMob + UMP, Firebase Analytics (без рекламного ID), Firebase Cloud Messaging, ARCore, Google Play Billing. Новий SDK або нові дані — спершу оновити тут і в політиці.
