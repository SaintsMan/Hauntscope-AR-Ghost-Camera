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
| `premium_offer_at_captures` | NUMBER | `3` | 0–1000 | На якому улові одразу після полювання показати офер повної версії (0 — лише після реклами). 1.0.7+ |
| `kit_offer_every_days` | NUMBER | `2` | 1–365 | Днів між двома оферами польового набору |
| `witching_hour_start` | NUMBER | `23` | 0–23 | Година початку відьомської години (місцевий час) |
| `witching_hour_end` | NUMBER | `4` | 0–23 | Година кінця відьомської години |
| `min_version_code` | NUMBER | `0` | 0–100000 | Збірки з меншим versionCode бачать у меню картку «Оновити» (Play In-App Update, інакше сторінка в Play). `0` — нікого |
| `season_event` | STRING | `""` (у шаблоні `halloween`) | — | Сезонна подія (GDD 5.42). `halloween`: привид Гарбузник, цукерки в кімнаті, банер і полиця скінів у меню, поки ключ = `halloween` і не минула дата `halloween_end`. Порожній / інший — подія вимкнена без білда (гарбузи й цукерки зникають, куплені скіни лишаються) |
| `halloween_jack_percent` | NUMBER | `35` | 0–100 | Шанс, що полювання (з `halloween_jack_from_hunt`, у 1.0.5 — з 3-го) — проти Гарбузника, поки триває подія |
| `halloween_candy_per_hunt` | STRING (число) | порожньо = 3–5 випадково | 0–10 | Скільки цукерок лежить у кімнаті за полювання |
| `halloween_end` | STRING | порожньо = `2026-11-02` | `yyyy-MM-dd` | Останній день події; цукерки згорають наступного дня |
| `halloween_price_beam_pumpkin` | NUMBER | `12` | 0–1000 | Ціна скіна «Гарбузовий промінь» у цукерках |
| `halloween_price_lens_cobweb` | NUMBER | `20` | 0–1000 | Ціна скіна «Рамка з павутинням» |
| `halloween_price_radar_jack` | NUMBER | `30` | 0–1000 | Ціна скіна «Радар Гарбузника» |
| `progressive_ui` | BOOLEAN | `true` | — | Меню відкривається поступово (GDD 5.41). `false` — усе меню з першого запуску (контрольна група A/B). Старі збереження (≥1 полювання чи переглянутий брифінг) бачать усе завжди |
| `unlock_ectoplasm_hunts` | NUMBER | `1` | 0–1000 | Полювань до показу балансу ектоплазми |
| `unlock_ration_hunts` | NUMBER | `1` | 0–1000 | Полювань до іконки й картки щоденного пайка |
| `unlock_contracts_hunts` | NUMBER | `1` | 0–1000 | Полювань до КОНТРАКТІВ |
| `unlock_shop_hunts` | NUMBER | `2` | 0–1000 | Полювань до МАГАЗИНУ й ряду спорядження |
| `unlock_bestiary_captures` | NUMBER | `1` | 0–1000 | Уловів до БЕСТІАРІЮ |
| `unlock_archive_captures` | NUMBER | `1` | 0–1000 | Уловів до АРХІВУ касет |
| `notify_prompt_after_hunts` | NUMBER | `3` (до 1.0.7 — `1`) | 0–1000 | Полювань до картки сповіщень і перемикача в Settings, якщо гравець ще нікого не впіймав |
| `notify_prompt_after_captures` | NUMBER | `1` | 0–1000 | Уловів, після яких відкривається картка сповіщень (0 — лише за полюваннями). 1.0.7+ |
| `briefing_starts_hunt` | BOOLEAN | `true` | — | Остання сторінка першого брифінгу — кнопка «ПОЧАТИ ПЕРШУ СПРАВУ», що одразу запускає перше полювання |
| `first_hunt_virtual` | BOOLEAN | `true` з 1.0.7 (у 1.0.6 — `false`, у шаблоні `true`) | — | Перше полювання одразу у Віртуальній кімнаті, без діалогу камери (наступні — за режимом меню) |
| `rookie_fade_captures` | NUMBER | `12` | 1–1000 | За скільки уловів допомога новачку (`RookieConfig`) згасає до нуля. 1.0.6+ |
| `rookie_capture_rate` | NUMBER | `1.5` | 0.1–5 | Множник заряду променя для новачка без уловів (далі згасає з уловами) |
| `rookie_ghost_speed` | NUMBER | `0.8` | 0.1–5 | Множник швидкості привида для новачка |
| `rookie_beamed_speed` | NUMBER | `0.35` | 0.05–5 | Додатковий множник швидкості привида **під променем** для новачка (0.35 — ледве рухається, поки тримаєш) |
| `rookie_leash_m` | NUMBER | `2.5` | 0–10 | Поки промінь на привиді, він не відходить від камери далі цієї відстані, м (0 — вимкнено). Лінза бачить Wisp до 3 м |
| `rookie_leash_captures` | NUMBER | `6` | 0–1000 | Скільки перших уловів діє «повідець» |
| `rookie_leash_growth_m` | NUMBER | `0.1` | 0–10 | На скільки метрів «повідець» довшає з кожним уловом (2.5 → 3.0 м за 5 уловів), щоб допомога згасала плавно |
| `beamed_flee_scale` | NUMBER | `1` | 0.05–1 | Множник швидкості **будь-якого** привида під променем, до лазерів і допомоги новачку (1 — як у 1.0.5). Ручка на випадок, якщо й ветерани не ловитимуть |
| `rookie_free_charge_hunts` | NUMBER | `3` | 0–1000 | Перші N полювань: при сілій батареї картка дає безкоштовну зарядку (раз за полювання) |
| `rookie_free_charge` | NUMBER | `0.6` | 0–1 | Яку частку батареї дає безкоштовна зарядка |
| `rookie_easy_captures` | NUMBER | `4` | 0–1000 | Перші улови з додатковою допомогою (`FirstCatchesConfig`): повна на 1-му полюванні, менша з кожним уловом, після N уловів — нема (0 — вимкнено). 1.0.7+ |
| `rookie_easy_capture_rate` | NUMBER | `1.6` | 0.1–5 | Множник заряду променя в допомозі перших уловів |
| `rookie_easy_reticle` | NUMBER | `1.5` | 0.5–5 | Множник розміру кільця променя |
| `rookie_easy_ghost_speed` | NUMBER | `0.6` | 0.1–5 | Множник швидкості привида |
| `rookie_easy_beamed_speed` | NUMBER | `0.3` | 0.05–5 | Додатковий множник швидкості привида під променем |
| `rookie_easy_drain` | NUMBER | `0` | 0–1 | Множник розряду батареї (пасивний, лінза, промінь); 0 — на 1-му полюванні батарея не сідає |
| `rookie_easy_emf_range` | NUMBER | `1.5` | 0.5–5 | Множник дальності EMF-радара |
| `rookie_easy_lens_angle` | NUMBER | `1.6` | 1–5 | Множник кута лінзи (30° × 1.6 = 48°) |
| `rookie_easy_lens_range` | NUMBER | `1.5` | 1–5 | Множник дальності лінзи (Wisp: 3 м × 1.5 = 4.5 м) |
| `rookie_easy_leash_m` | NUMBER | `1.5` | 0–10 | Поки промінь на першому привиді, той не відходить далі цієї відстані, м; з уловами плавно переходить у `rookie_leash_m` |
| `rookie_easy_arrow` | BOOLEAN | `true` | — | Стрілка на HUD до джерела EMF, поки допомога перших уловів ≥ половини (перші 3 улови) |
| `review_min_captures` | NUMBER | `1` | 1–1000 | Уловів до запиту оцінки в Play (було 3 у коді до 1.0.6) |
| `review_min_photo_stars` | NUMBER | `3` | 0–3 | Збережене фото з такою кількістю ★ теж просить оцінку, ще до улову (0 — вимкнено) |
| `halloween_welcome_candy` | NUMBER | `10` | 0–100 | Вітальні цукерки, раз кожному гравцю, поки триває Хелловін. 1.0.6+ |
| `halloween_jack_from_hunt` | NUMBER | `2` | 2–1000 | З якого полювання (рахуючи з 1) може прийти Гарбузник. 1.0.6+ |
| `ab_1`, `ab_2`, `ab_3` | STRING | `""` | ≤36 символів | Слоти A/B-тестів консолі LiveOps: `<id тесту>:<варіант>`; гра дзеркалить непорожні в user properties GA4 з тими ж назвами (`LiveOpsAB`). Самі значення задає CRM, у коді їх нема |

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
