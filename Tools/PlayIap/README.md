# Вбудовані покупки Google Play зі скрипта

`play_iap.py` створює разові продукти (in-app products) Hauntscope у Google Play з JSON-каталогу через Google Play Developer API (`monetization.onetimeproducts`). Скрипт не залежить від гри (перенесений з Potter Quiz): для іншої гри досить свого каталогу за зразком `hauntscope_iap.json`.

## Файли

- `play_iap.py`: скрипт (Python 3 + пакет `cryptography` для підпису ключа; більше нічого не потрібно).
- `hauntscope_iap.json`: каталог Hauntscope, 8 продуктів × 6 мов (`com.pavko.hauntscope`). **Джерело правди для текстів і цін у Play.** Тексти правити тут.
- `test_play_iap.py`: перевірка без мережі на фейковому API (створення, активація, повторний запуск, `game`-тексти не йдуть у Play, відсутній `expect_ids_in` дає лише попередження). `python -I Tools/PlayIap/test_play_iap.py` має закінчитися `ok`.
- `ppp_factors.py`, `regional_factors.json`: регіональні коефіцієнти цін (див. нижче).
- `prices_report.csv`: таблиця цін усіх країн з останнього `plan --report`.

## Продукти

| ID | Тип у грі | Ціна | Що дає |
|---|---|---|---|
| `full_version` | неспоживний | $4.99 | Перепустка бюро: без реклами між полюваннями, кожна ▶-нагорода одразу без відео, +1500 ектоплазми один раз, золотий лазер A0 AURUM |
| `starter_pack` | неспоживний (разова пропозиція) | $1.99 | Набір новобранця: 1000 ектоплазми, лазер T3 ПРИВ’ЯЗЬ, по 3 кожного бустера, 3 запасні батареї |
| `ecto_vial` | споживний | $0.99 | 500 ектоплазми |
| `ecto_jar` | споживний | $2.99 | 1800 ектоплазми (+20%) |
| `ecto_barrel` | споживний | $4.99 | 3500 ектоплазми (+40%) |
| `ecto_vault` | споживний | $9.99 | 8000 ектоплазми (+60%) |
| `field_kit` | споживний | $1.99 | по 3 кожного бустера + 3 запасні батареї |
| `laser_spectre` | неспоживний | $1.99 | ексклюзивний лазер S5 ФАНТОМ (EN: S5 SPECTRE) |

Поле `type` у каталозі лише для людей і коду гри: у Play разовий продукт не ділиться на споживний і неспоживний, це вирішує гра (споживні продукти гра `consume`-ить після видачі).

## Що потрібно до першого запуску

1. **AAB з білінгом у будь-якій доріжці** (internal testing підходить). Поки в Play немає збірки з дозволом `com.android.vending.BILLING`, Play не дасть створювати продукти ні вручну, ні через API: `apply` падає з `HTTP 400: Product "full_version": Can't create product. To fix, request billing permission.` Дозвіл додає Unity IAP (`com.unity.purchasing`) у збірку сам.
2. **Service account**: той самий JSON-ключ, що для fastlane (`PLAY_JSON_KEY`). У Play Console → Users and permissions йому потрібні для цього застосунку **View app information** і **Manage store presence**. Якщо на запис приходить 403, перевір ці права (зміни застосовуються кілька хвилин).
3. `pip install cryptography`, якщо пакета немає.

## Команди (з кореня проєкту)

```powershell
$env:PLAY_JSON_KEY = "D:\Keys\play-service-account.json"
python -I Tools/PlayIap/play_iap.py check --catalog Tools/PlayIap/hauntscope_iap.json      # офлайн: ліміти, мови, game-тексти, ID в IapAssetBuilder.cs
python -I Tools/PlayIap/play_iap.py list  --catalog Tools/PlayIap/hauntscope_iap.json      # що вже є в Play
python -I Tools/PlayIap/play_iap.py plan  --catalog Tools/PlayIap/hauntscope_iap.json --report Tools/PlayIap/prices_report.csv   # що буде зроблено, нічого не пише
python -I Tools/PlayIap/play_iap.py apply --catalog Tools/PlayIap/hauntscope_iap.json --yes             # створити й активувати
```

`apply` без `--yes` показує те саме, що й `plan`, і нічого не пише. `plan --out plan.json` зберігає повні тіла запитів.

## Як воно працює

- Ціна в каталозі задається одна, у доларах США без податку (`price_usd`). Скрипт викликає `pricing:convertRegionPrices`, і Google сам рахує ціни для всіх регіонів у місцевих валютах, як кнопка «конвертувати» в Play Console. Звідти ж береться `regionsVersion`. Ціну в окремій країні можна задати вручну полем `region_prices`, наприклад `"region_prices": {"UA": "39.99"}` (у валюті цієї країни).
- Кожен продукт отримує одну опцію покупки `buy` з `legacyCompatible: true`. Без цього Unity IAP зі старою Play Billing Library продукт не побачить.
- Створення йде через `oneTimeProducts:batchUpdate` з `allowMissing`. Потім опції покупки активуються (`purchaseOptions:batchUpdateStates`). Без `--latency tolerant` зміни доходять до пристроїв за хвилини.
- **Що вже є в Play, скрипт не перезаписує.** Існуючий продукт отримує нові тексти лише з `--update-listings`. Ціни існуючих продуктів скрипт не міняє: якщо ціна в доларах розходиться з каталогом, він лише попередить. Змінюй ціну в Play Console. Продукти, яких немає в каталозі, скрипт не чіпає і нічого не видаляє.
- **ID продукту назавжди.** Після видалення в Play той самий ID більше не можна використати. Продукти, створені старим API `inappproducts`, новим API не керуються, і навпаки.

## Тексти в грі (`game`)

Кожен продукт має ключ `game` з короткою назвою й описом для магазину в грі: `"game": {"<локаль Unity>": {"name": "...", "description": "..."}}`. Локалі Unity: `en`, `uk`, `es-419`, `pt-BR`, `de`, `id` (поле `game_languages`). У Play ці тексти не надсилаються. `check` перевіряє, що всі мови є, назва до `game_name_max` (24) символів, опис до `game_description_max` (90). Назви — капсом, як інші товари складу агенції; терміни (лазери, бустери, ектоплазма) узяті з таблиць `Store` і `UI`.

## Перевірка ID у коді (`expect_ids_in`)

`expect_ids_in` вказує на `Assets/_Hauntscope/Scripts/Editor/IapAssetBuilder.cs` — генератор, що створює ассети продуктів гри (`Data/Iap/Iap_<id>.asset`). Кожен ID каталогу має бути там у лапках, тож продукт у Play без продукту в грі (чи з одруківкою в ID) — помилка `check`.

Тексти `game` потрапляють у таблицю `Store` гри як `iap.<id>.name` / `iap.<id>.description` (імпорт робиться з цього каталогу, щоб тексти в Play і в грі не розходилися).

## Регіональні ціни (паритет купівельної спроможності)

Стандартна конвертація Google лише переводить долар за курсом і додає місцевий податок. Тому, наприклад, в Україні виходило б дорожче, ніж у США. Щоб так не було, кожна країна отримує коефіцієнт:

`коефіцієнт = PLI + (1 − PLI) × min(1, ВВП на душу / 60 000 $)`, обмежений 0.3–1.0

- PLI: рівень споживчих цін країни відносно США. Рахується як PPP приватного споживання (`PA.NUS.PRVT.PP`), поділене на ринковий курс (`PA.NUS.FCRF`) за той самий рік, дані Світового банку.
- Заможніші країни ближчі до базової ціни, бідніші отримують більшу знижку. Дорожче за базову ціну не буває ніде.
- `ppp_factors.py` завантажує дані й пише `regional_factors.json`: коефіцієнт, PLI, ВВП, рік і джерело для кожної країни. Країни без даних вписані в коді (`MANUAL`, `--poor-without-data`). Оновлювати раз на рік: `python -I Tools/PlayIap/ppp_factors.py --out Tools/PlayIap/regional_factors.json`.
- `play_iap.py` множить базову ціну на коефіцієнт і бере найближчу «гарну» ціну з сітки Google (`convertRegionPrices` для дешевших доларових цін). Так місцеве округлення й податок лишаються правильними. Країна без коефіцієнта отримує стандартну ціну Google.
- `plan --report ...` зберігає таблицю всіх країн: валюта, коефіцієнт, ціна кожного продукту й приблизний еквівалент у доларах. Відкривається в Excel.
- Ціни в ЄС, Україні та більшості країн включають ПДВ, у США ціна без податку. Тому €1.09 у Німеччині при $0.99 у США нормально.

Приклад з `plan` (2026-10-08, regions version 2026/01) для $4.99: US 4.99 USD, DE 5.49 EUR, UA 94.99 UAH, BR 13.99 BRL, MX 75 MXN, ID 32000 IDR, IN 170 INR.

## Значки продуктів

У Play Console продукт має поле «Значок» (PNG 1:1, 512–1080 px, до 8 МБ, без тексту й брендів). Через API значок **не завантажити** (у ресурсі продукту немає такого поля), тому його додають вручну: **Monetize → Products → In-app products → продукт → Icon**.

Готові значки 1024×1024 лежать у `icons/<id>.png` (по одному на продукт, назва файла = ID). Їх малює код, у тому ж стилі камкордера, що й гра: темний екран, світіння кольору продукту, рядки розгортки, кутики видошукача, неоновий гліф. Перегенерувати: Unity → **Hauntscope/Build IAP Icons** (`Scripts/Editor/IapIconGenerator.cs`; разом з ними оновлюються й гліфи для карток у грі).

## Формат каталогу

```json
{
  "package": "com.pavko.hauntscope",
  "default_language": "en-US",
  "languages": ["en-US", "uk", "es-419", "pt-BR", "de-DE", "id"],
  "game_languages": ["en", "uk", "es-419", "pt-BR", "de", "id"],
  "game_name_max": 24,
  "game_description_max": 90,
  "purchase_option_id": "buy",
  "banned": "regex заборонених слів у текстах (необов'язково)",
  "expect_ids_in": "шлях до файла з ID (необов'язково, відносно каталогу; якщо файла немає — попередження)",
  "regional_factors": "regional_factors.json (необов'язково, без нього стандартні ціни Google)",
  "show_regions": ["US", "UA", "DE"],
  "products": [
    {"id": "ecto_vial", "type": "consumable", "price_usd": "0.99", "region_prices": {},
     "listings": {"en-US": {"title": "...", "description": "..."}},
     "game": {"en": {"name": "...", "description": "..."}}}
  ]
}
```

Ліміти Google: назва до 55 символів, опис до 200, ID продукту з малих латинських літер, цифр, `_` і `.`. Мови мають збігатися з мовами сторінки застосунку в Play (`fastlane/metadata/android/`).

## Стан і що не перевірено

- 2026-10-08: `list` і `plan` проти справжнього API працюють (0 продуктів у Play, 174 регіони). `apply` зупинився на `Can't create product. To fix, request billing permission.`: у Play ще немає AAB з білінгом. Після завантаження такої збірки повторити `apply --yes`.
- Після `apply` звір 2–3 країни в Play Console. Що поле ціни продукту чекає саме ціну з податком з `convertRegionPrices`, — припущення (так роблять сторонні інструменти).
- Чи Unity IAP 5.1.2 бачить продукти, створені новим API: перевірити на пристрої тестовою покупкою (License testing).
- Тексти 5 мов, крім української, носії не вичитували.
