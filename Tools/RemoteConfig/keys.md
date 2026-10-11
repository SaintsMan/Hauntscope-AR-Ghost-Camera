# Remote Config — Hauntscope: AR Ghost Camera

Проєкт Firebase: `hauntscope-37e08`. Ключі в коді: `Assets/_Hauntscope/Scripts/Gameplay/Config/RemoteKeys.cs`; значення за замовчуванням — у `GameConfig` (`AdsConfig`, `IapConfig`, `NightConfig`, `LiveOpsConfig`); читає `RemoteTuning` (GDD 5.40). Шаблон для консолі — `remoteconfig.template.json` (ті самі ключі й ті самі значення, крім `season_event`).

- Без інтернету й без публікації гра поводиться так само, як із шаблоном: дефолти живуть у коді, у SDK нічого не завантажується.
- Невалідне значення (не число, поза діапазоном, не `true`/`false`) ігнорується → дефолт із коду. Друкарська помилка в консолі гру не зламає.
- Fetch раз на 12 год (`LiveOpsConfig`), у debug-збірці — щозапуску. Значення, отримані на одному запуску, діють з **наступного** (нічого не міняється під гравцем посеред сесії). Найперший запуск після встановлення бере значення одразу.
- Значення читаються в момент рішення (наступна реклама, наступний офер, наступне полювання).

| Ключ | Тип | Дефолт | Діапазон | Що робить |
|---|---|---|---|---|
| `interstitial_enabled` | BOOLEAN | `true` | — | Interstitial між полюваннями взагалі. `false` — лише добровільні ▶-нагороди |
| `interstitial_min_sessions` | NUMBER | `3` | 0–1000 | Перший interstitial лише після стількох полювань за весь час |
| `interstitial_every_hunts` | NUMBER | `3` | 1–1000 | Полювань між двома interstitial |
| `interstitial_min_interval_s` | NUMBER | `150` | 0–86400 | Секунд від будь-якої реклами (і rewarded) до interstitial |
| `offers_enabled` | BOOLEAN | `true` | — | Попапи платних оферів після полювання. `false` — магазин продає, але сам нічого не пропонує |
| `starter_offer_after_hunts` | NUMBER | `2` | 0–1000 | Набір новобранця (`starter_pack`) — від стількох полювань |
| `premium_offer_after_hunts` | NUMBER | `6` | 0–1000 | Повна версія після реклами — від стількох полювань |
| `premium_offer_every_days` | NUMBER | `3` | 1–365 | Днів між двома оферами повної версії |
| `kit_offer_every_days` | NUMBER | `2` | 1–365 | Днів між двома оферами польового набору |
| `witching_hour_start` | NUMBER | `23` | 0–23 | Година початку відьомської години (місцевий час) |
| `witching_hour_end` | NUMBER | `4` | 0–23 | Година кінця відьомської години |
| `min_version_code` | NUMBER | `0` | 0–100000 | Збірки з меншим versionCode бачать у меню картку «Оновити» (Play In-App Update, інакше сторінка в Play). `0` — нікого |
| `season_event` | STRING | `""` (у шаблоні `halloween`) | — | Сезонна подія. Гра читає ключ, але сезонного контенту поки немає — прапорець на майбутнє |

**Обережно:**
- `witching_hour_start`: тексти гри й нагадування кажуть «з 23:00». Міняти початок — лише разом із текстами (новий білд). Кінець (`witching_hour_end`) можна подовжити без текстів, наприклад на Хелловін до 6.
- `min_version_code`: ставити лише на versionCode, який **уже** на 100% у Production, інакше гравці побачать «Оновити», а оновлення ще немає (картку можна закрити «Не зараз», але вона повертатиметься в кожному меню).
- Ціни оферів у Remote Config немає й не буде: ціни приходять з Google Play у валюті гравця.

## Публікація (лише після «так» від Pavko)

Шаблон **повністю замінює** те, що в консолі. На 2026-10-11 у консолі порожньо (`remoteconfig:get` → без параметрів).

```powershell
$env:PATH = "$env:LOCALAPPDATA\nvm\v24.13.0;$env:PATH"
npx --yes firebase-tools@latest remoteconfig:get --project hauntscope-37e08          # що зараз у консолі
npx --yes firebase-tools@latest deploy --only remoteconfig --project hauntscope-37e08 --config "D:\Unity Projects\PAVKO\Hauntscope AR Ghost Camera\Tools\RemoteConfig\firebase.json"
npx --yes firebase-tools@latest remoteconfig:versions:list --project hauntscope-37e08 # відкат: remoteconfig:rollback -v <версія>
```

Не публікувати через обгортку `Hawkins…\Tools\Firestore\firebase.cmd`: вона робить `cd` у Hawkins і бере **його** `firebase.json`.
