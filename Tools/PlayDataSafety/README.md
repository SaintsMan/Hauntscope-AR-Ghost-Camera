# Data safety в Google Play зі скрипта

`data_safety.py` заповнює форму Data safety (App content → Data safety) з відповідей у самому скрипті й завантажує її через Play Developer API (`applications/{package}/dataSafety`). Завантаження **замінює всю форму**.

```bash
python -I Tools/PlayDataSafety/data_safety.py build            # лише зібрати hauntscope_data_safety.csv
python -I Tools/PlayDataSafety/data_safety.py apply --yes --key D:/Keys/play-service-account.json
```

- `template.csv` — порожній експорт форми Play Console (жовтень 2026): ID питань і варіантів, без відповідей. Якщо Play змінить форму, експортувати новий шаблон у Play Console (Export to CSV) і очистити колонку `Response value`.
- `hauntscope_data_safety.csv` — що піде в Play (генерується `build`; у Play потрапляє лише після `apply --yes`).
- Відповіді (`GENERAL`, `DATA_TYPES`) мають збігатися з політикою приватності та GDD 5.39: AdMob + UMP, Firebase Analytics (без рекламного ID), Firebase Crashlytics, Remote Config, Cloud Messaging, ARCore, Google Play Billing. Новий SDK або нові дані — спершу оновити тут і в політиці.

## Зміни для 1.0.2 (залито в Play 2026-10-11)

- **Crash logs** (App info and performance) — нове: збирається, не передається третім сторонам, обов'язкове; мета — App functionality, Analytics (Firebase Crashlytics).
- **Diagnostics** — до мет збору додано App functionality (Crashlytics); передача, як і раніше, — AdMob (Analytics, Fraud prevention, Advertising).
- Без змін: Approximate location, App interactions, Device or other IDs (тут і рекламний ID AdMob), Purchase history. Remote Config нових типів даних не додає (ID встановлення вже в Device or other IDs).
- Окремо в Play Console → App content: **Ads** = «Yes, my app contains ads»; **Advertising ID** = використовується (AdMob, дозвіл `AD_ID` у маніфесті), мета — Advertising/Analytics.
