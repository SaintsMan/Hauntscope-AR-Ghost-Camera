# Hauntscope 1.0.2 (versionCode 3) — чекліст Play Console перед Production

Складено 2026-10-11. ✅ — перевірено скриптами (лише читання, через тимчасовий edit Play API, який видалявся), ❓ — через API не видно, перевірити в Play Console, ⬜ — зробити.

## 0. Збірка
- ✅ AAB: `D:\Unity Projects\PAVKO\_Builds\Hauntscope_v3.aab` (1.0.2, versionCode 3, підписано upload-ключем проєкту, справжні ID AdMob, символи Public усередині для Play). Копія: `Build/Hauntscope-1.0.2-3.aab`.
- ✅ Маніфест (bundletool): launcher — `com.google.firebase.MessagingUnityPlayerActivity`; є `RECEIVE_BOOT_COMPLETED` (нагадування переживуть перезавантаження), `AD_ID`, `POST_NOTIFICATIONS`, `BILLING`, `CAMERA`; `com.google.android.gms.ads.APPLICATION_ID` = справжній `ca-app-pub-1798491222196618~9939462902`.
- ✅ Символи IL2CPP вивантажено в Crashlytics (`crashlytics:symbols:upload`, 2026-10-11). Архів: `_Builds/Hauntscope_v3-1.0.2-v3-IL2CPP.symbols.zip`.
- ✅ Release notes 6 мовами: `fastlane/metadata/android/<мова>/changelogs/3.txt`.

## 1. Доступ до Production (особистий акаунт розробника)
- ✅ Закритий тест: трек «First test», v2 (1.0.1) `completed`, тестувальники — Google-група `testimgoogleplay@googlegroups.com`, 177 країн + решта світу.
- ❓ **Скільки тестувальників і скільки днів.** Для акаунтів, створених після листопада 2023, Google вимагає ≥ 12 тестувальників, які **погодились (opted-in) і лишались у тесті 14 днів поспіль**. Число учасників групи через API не видно. Перша AAB гри (versionCode 1) — кінець вересня, GA4 бачить гравців з 4.10. Відкрий **Dashboard → «Apply for production»**: якщо кнопка активна — умову виконано; якщо ні, Play показує, скільки днів/тестувальників бракує.
- ⬜ Заявка на Production: Play питає про тестування (як знаходили тестувальників, що змінили за відгуками). Чесні відповіді: AR-полювання на 12 привидів, що змінили — Crashlytics, аналітика пушів, пауза перед рекламою, Remote Config, картка оновлення.

## 2. App content (Policy → App content)
- ❓ **Content rating** — анкета IARC пройдена? Гра: стилізовані привиди, лякалки (jump scares можна вимкнути в Settings), без крові й насильства, без чату, є покупки й реклама. Очікувано PEGI 7 / ESRB Everyone 10+ (рішення IARC).
- ❓ **Target audience**: **13+** (політика: «для гравців від 13 років»; оголошення від 13 до 17 і 18+). **Не** обирати групи до 13 — інакше вимоги Families і заборона більшості реклами.
- ⬜ **Ads**: «Yes, my app contains ads» (AdMob interstitial + rewarded).
- ⬜ **Advertising ID**: «Yes» — AdMob (`com.google.android.gms.permission.AD_ID` у маніфесті); мети — Advertising, Analytics, Fraud prevention.
- ✅ **Data safety**: залито з `Tools/PlayDataSafety` 2026-10-11 (Play API, форма замінена). Нове в 1.0.2: Crash logs, мета App functionality для Diagnostics.
- ✅ **Privacy policy**: `https://pavkoapp.web.app/hauntscope/privacy` задеплоєно 2026-10-11 (AdMob/UMP, Crashlytics, Remote Config, дата 11.10.2026). ❓ Посилання в Play App content — перевіряє `chrome_all_prompt.md`.
- ❓ News app — No; COVID — No; Government — No; Financial features — None; Health — No.
- ❓ **Camera permission**: Play може спитати про використання — «AR: показ привидів у кімнаті, зображення не залишають пристрій».

## 3. Магазин
- ✅ Сторінка 6 мовами: `en-US` (основна), `uk`, `es-419`, `pt-BR`, `de-DE`, `id`; іконка, feature graphic, 8 скріншотів. Контакти: `pavkoapp@gmail.com`, сайт `https://pavkoapp.blogspot.com`.
- ⬜ Нова іконка — після A/B тесту сторінки (`Docs/Art/Icons/2026-10/`, вкладка ASO в LiveOps).
- ✅ IAP: у Play 8 разових товарів (як у `Tools/PlayIap/hauntscope_iap.json`). ❓ Усі `Active`, регіональні ціни — звірити в Monetize → In-app products.
- ❓ **Країни Production**: обрати (рекомендація — ті ж 177 + решта світу, як у закритому тесті). Якщо гра офіційно не перекладена для країни — англійська сторінка.
- ❓ Category: Games → Adventure (або Simulation); Tags; Store settings → Contact details.

## 4. Консолі Firebase / AdMob (через браузерні промпти, `Tools/ConsoleSetup/`)
- ⬜ GA4 custom definitions для нових подій: `ga4_dimensions_prompt.md` (`topic`, `lang`, `kind`, `product_id`, `reason`, `item_id`; метрика `duration_s`).
- ⬜ AdMob ↔ Firebase (і Play, коли гра публічна) + перевірка GDPR-повідомлення: `admob_firebase_link_prompt.md`. У GA4 є `ad_reward`, але немає `ad_impression` з доходом — схоже, не прив'язано.
- ⬜ Crashlytics: після першого запуску 1.0.2 на телефоні перевірити дашборду; якщо порожньо — `crashlytics_enable_prompt.md`.
- ⬜ Remote Config: шаблон `Tools/RemoteConfig/remoteconfig.template.json` (опубліковувати не обов'язково — дефолти в коді ті самі; публікація лише після «так», `keys.md`).

## 5. Перевірка на телефоні (OnePlus 10 Pro, перед завантаженням у Play)
Встанови 1.0.2 з закритого тесту (або universal APK з AAB через bundletool) і пройди:
1. Старт: сплеш → меню, без вильоту; у ЄЕЗ — форма згоди UMP до будь-якої реклами; Settings → «Приватність» є, якщо форма її вимагає.
2. AR-полювання: скан кімнати, привид, захоплення; повернутись у меню.
3. Фото привида → зберегти в галерею й поділитись.
4. Тестова покупка (license tester): пройти й **скасувати** одну — у DebugView має бути `purchase_cancelled`.
5. Реклама: після 3+ полювань вийти з картки результату — екран на мить гасне, потім interstitial; тапи в цей момент нічого не відкривають.
6. Нагадування: згорнути гру з увімкненими сповіщеннями, змінити час телефона / дочекатися → тап по нагадуванню відкриває гру (DebugView: `reminder_open`).
7. Пуш: розсилка LiveOps на `lang_uk` (лише через розклад) → тап відкриває гру без краша (`push_open`).
8. DebugView: `adb shell setprop debug.firebase.analytics.app com.pavko.hauntscope` → Firebase → DebugView: `hunt_start`, `hunt_end`, `screen_view`, `push_subscribed`.
9. Crashlytics: через кілька хвилин після першого запуску в консолі видно версію `1.0.2 (3)`.

## 6. Реліз (лише після «так» від Pavko)
1. ✅ v3 у **закритому тесті** («First test») з 2026-10-11: реліз `1.0.2 (3)`, `completed`, release notes 6 мовами (Play API). Далі — перевірка Google і 1–2 дні тестувальникам.
2. Заявка на Production (якщо доступ ще не відкрито) — перевірка Google може тривати до 7 днів, тож подавати **до ~20.10**, щоб встигнути до 31.10.
3. Production поетапно: 20% → 50% → 100% з кроком 1–2 дні, якщо Crashlytics/Vitals чисті (crash-free ≥ 99%, ANR < 0,47%).
4. Після 100% — можна ставити `min_version_code = 3` у Remote Config (якщо потрібно відсікти 1.0.1).
