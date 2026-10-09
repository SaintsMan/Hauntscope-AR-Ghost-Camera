# Сторінка Google Play зі скрипта

`upload_listing.py` завантажує тексти (назва, короткий і повний опис) і картинки (іконка, feature graphic, скріншоти) з `fastlane/metadata/android/<мова>/` у Google Play через Play Developer API (edits). Спільний клієнт API — `Tools/PlayIap/play_iap.py`.

```bash
# що зміниться (нічого не пише)
python -I Tools/PlayListing/upload_listing.py plan --key D:/Keys/play-service-account.json

# лише тексти, картинки в Play лишаються як є
python -I Tools/PlayListing/upload_listing.py apply --texts-only --yes --key D:/Keys/play-service-account.json
```

- Джерело правди — файли в `fastlane/metadata/android/`; `store_listing_all_languages.txt` — зведення для читання.
- Ліміти Play перевіряються до запиту: назва 30, короткий опис 80, повний опис 4000 символів, 2–8 скріншотів.
- Картинки порівнюються за SHA-256 і перезавантажуються, лише якщо змінились. Іконку й feature graphic перекладів можна не вантажити: Play бере їх зі сторінки мови за замовчуванням.
- Якщо Play не дає відправити зміни на перевірку автоматично, скрипт комітить їх без перевірки — тоді «Send changes for review» у Play Console.
