Ти керуєш моїм браузером (Chrome, де я вже залогінений у Google). Гра — **Hauntscope: AR Ghost Camera**, Android-пакет `com.pavko.hauntscope`. Версія 1.0.2 (versionCode 3) щойно залита в закритий тест «First test». Треба пройти чотири консолі: Google Play Console, Google Analytics 4, AdMob і Firebase Crashlytics. Роби тільки те, що описано нижче, і нічого не видаляй.

Зупинись і спитай мене, якщо сторінка просить:
- оплату чи план Blaze;
- юридичну згоду чи створення нового акаунта або проєкту;
- видалення будь-чого;
- будь-що, чого немає в цій інструкції.

Перед кожною кнопкою, яка **відправляє** щось на перевірку Google, теж спитай мене. Це кнопки «Submit», «Send for review», «Apply for production», «Publish». Звичайне «Save» у формах дозволено.

## 1. Google Play Console (play.google.com/console → Hauntscope: AR Ghost Camera)

### 1.1 Закритий тест
1. Відкрий Test and release → Testing → Closed testing → «First test». Перевір, що реліз `1.0.2 (3)` там є, і запиши його статус (In review / Available to testers / помилка).
2. На вкладці Testers запиши, скільки тестувальників і як вони підключені. Очікується Google-група `testimgoogleplay@googlegroups.com`. Скопіюй opt-in посилання.
3. Нічого не змінюй.

### 1.2 Доступ до Production
1. Відкрий Dashboard і знайди «Apply for production» / «Production access». Запиши стан:
   - кнопка активна;
   - або скільки тестувальників і днів ще бракує (Google вимагає ≥ 12 тестувальників, які лишаються в тесті 14 днів поспіль).
2. Якщо кнопка активна, відкрий анкету і заповни чернетку. Відповіді про те, як я знаходив тестувальників і що вони казали, спитай у мене, не вигадуй.
3. Для питання «що змінили» є факти. Версія 1.0.2 додала:
   - Firebase Crashlytics (звіти про збої);
   - аналітику пушів і покупок;
   - коротку паузу із затемненням екрана перед interstitial, щоб не було випадкових кліків по рекламі;
   - Remote Config;
   - картку «є нова версія» в меню.
4. **Не відправляй анкету без мого «так».**

### 1.3 Policy → App content
Для кожного пункту запиши стан: зроблено / потребує дій. Заповнюй лише те, що не зроблено.
- **Privacy policy:** `https://pavkoapp.web.app/hauntscope/privacy`. Відкрий посилання в новій вкладці і переконайся, що сторінка відкривається. Якщо в Play стоїть інше посилання (наприклад, GitHub Pages), заміни на це.
- **Ads:** «Yes, my app contains ads» (AdMob: interstitial і rewarded).
- **Advertising ID:** «Yes, my app uses advertising ID». Цілі: Advertising or marketing, Analytics, Fraud prevention, security, and compliance.
- **Target audience and content:** вікові групи 13–15, 16–17 і 18+. **Не обирай групи до 13 років.** На питання, чи може гра непреднамірено приваблювати дітей, відповідай «No». Якщо Play наполягає, спитай мене.
- **Content rating (IARC):** якщо рейтинг уже є, лише запиши його. Якщо ні, заповни анкету так:
  - категорія — Game;
  - насильство — лише фантастичне, без крові: стилізовані привиди, їх «ловлять» променем камери, ніхто не гине;
  - страшний контент — так, лякалки (jump scares), їх можна вимкнути в налаштуваннях;
  - сексуального контенту, лайки, наркотиків і азартних ігор немає;
  - взаємодії між користувачами немає: без чату, без UGC, без обміну геолокацією;
  - є цифрові покупки (in-app products) і реклама;
  - гра ділиться фото лише через системне меню «Поділитися».
  
  Перед «Submit» анкети спитай мене.
- **Data safety:** лише подивись і запиши, чи є в розділі App info and performance → «Crash logs». Нічого не змінюй: цю форму я заллю окремо з CSV.
- Інші декларації (News app, COVID-19, Government apps, Financial features, Health, Actor/Account deletion) — лише запиши стан. Правильні відповіді: News = No, COVID = No, Government = No, Financial features = «My app doesn't provide any financial features», Health = No. Якщо щось не заповнено, заповни так і збережи.

### 1.4 Monetize → Products → In-app products
Має бути 8 товарів. Запиши кожен `productId` і статус Active / Inactive. Нічого не змінюй.

### 1.5 Grow → Store presence → Store settings
Запиши категорію (Games → …), теги і контакти. Очікувані контакти: `pavkoapp@gmail.com`, сайт `https://pavkoapp.blogspot.com`. Нічого не змінюй.

## 2. Google Analytics 4: custom definitions

Відкрий analytics.google.com → property **558234091** (Firebase-проєкт `hauntscope-37e08`). Далі Admin → Data display → Custom definitions.

Там уже є 12 визначень, так ти впізнаєш правильний property:
- event-параметри: `enabled`, `environment`, `ghost`, `method`, `mode`, `outcome`, `product`, `result`;
- user properties: `game_lang`, `notifications`, `play_environment`, `premium`.

Створи custom dimensions зі scope = Event:

| Dimension name | Event parameter |
|---|---|
| Push topic | `topic` |
| Push language | `lang` |
| Reminder kind | `kind` |
| Product id | `product_id` |
| Purchase fail reason | `reason` |
| Shared ghost | `item_id` |

Створи custom metric: Hunt duration, scope = Event, параметр `duration_s`, одиниця — Seconds (якщо її немає — Standard).

Правила:
- Параметр вписуй рівно так, малими літерами. Якщо його немає у списку, введи вручну.
- Уже наявні параметри не дублюй і не змінюй.
- Якщо назва зайнята, додай префікс `Hauntscope ` до назви. Параметр при цьому не змінюй.
- Якщо GA4 каже про ліміт, зупинись і напиши, скільки вже створено.

## 3. AdMob (apps.admob.com)

Застосунок має App ID `ca-app-pub-1798491222196618~9939462902`. У списку він може називатися «Hauntscope» або «Ghost Detector». Рекламні блоки, медіацію, платежі й налаштування акаунта не чіпай.
1. Відкрий App settings → Firebase / Linked services.
   - Якщо застосунок уже прив'язаний, запиши проєкт.
   - Якщо ні, натисни «Link to Firebase» і обери наявний проєкт `hauntscope-37e08` із застосунком `com.pavko.hauntscope`. Новий проєкт не створюй.
2. App store details: якщо магазин не прив'язаний, спробуй Add → Google Play → `com.pavko.hauntscope`. Гра поки лише в закритому тесті, тож Play може її не знайти. Це нормально, просто запиши.
3. Privacy & messaging → European regulations (GDPR): перевір, чи є **опубліковане** повідомлення, яке діє для цього застосунку. Нічого не створюй і не публікуй, лише запиши назву і статус.

## 4. Firebase Crashlytics

Відкрий console.firebase.google.com → проєкт `hauntscope-37e08` → Android `com.pavko.hauntscope` → Crashlytics.
- Якщо бачиш «Enable Crashlytics» / «Get started», увімкни. SDK уже в грі, тож кроки з налаштування SDK просто пройди до кінця.
- Якщо бачиш «Waiting for your first report», нічого не тисни. Це очікувано, поки ніхто не відкрив 1.0.2.
- Якщо дані вже є, запиши:
  - кількість Issues;
  - crash-free users;
  - версії гри;
  - non-fatal-и з назвами `ar_session`, `iap`, `ads`, `photo`, `save`.
- Нічого не закривай і не змінюй сповіщення.

## Фінальний звіт
Одним списком по розділах 1.1–4:
- що було і що ти змінив;
- що чекає мого «так» (анкета на Production, IARC);
- що не вдалося і чому.

Окремо вкажи:
- статус релізу 1.0.2 у закритому тесті;
- скільки тестувальників і днів бракує до Production.
