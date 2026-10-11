Ти керуєш моїм браузером (Chrome, де я вже залогінений у Google). Треба додати custom dimensions і одну custom metric у Google Analytics 4 для моєї гри. Більше нічого в консолях не змінюй і нічого не видаляй. Якщо сторінка просить щось, чого немає в цій інструкції (новий акаунт, оплата, юридична згода, видалення даних), зупинись і спитай мене.

## Де
- analytics.google.com → property **558234091** (Firebase-проєкт `hauntscope-37e08`, гра Hauntscope: AR Ghost Camera, Android-пакет `com.pavko.hauntscope`). Там уже є 12 custom definitions: event-параметри `enabled`, `environment`, `ghost`, `method`, `mode`, `outcome`, `product`, `result` і user properties `game_lang`, `notifications`, `play_environment`, `premium` — так ти впізнаєш правильний property.
- Admin (шестерня зліва внизу) → Data display → Custom definitions.

## Що створити

### Custom dimensions (вкладка Custom dimensions → Create custom dimension, Scope = Event)

| Dimension name | Event parameter | Звідки (для довідки) |
|---|---|---|
| Push topic | `topic` | `push_open`, `push_received` |
| Push language | `lang` | `push_subscribed` |
| Reminder kind | `kind` | `reminder_open` |
| Product id | `product_id` | `purchase_cancelled`, `purchase_failed` |
| Purchase fail reason | `reason` | `purchase_failed` |
| Shared ghost | `item_id` | `share` |

### Custom metric (вкладка Custom metrics → Create custom metric)

| Metric name | Scope | Event parameter | Unit of measurement |
|---|---|---|---|
| Hunt duration | Event | `duration_s` | Seconds (якщо такого варіанту немає — Standard) |

## Правила
- Event parameter вписуй рівно так, малими літерами. Якщо його немає у випадному списку, введи вручну (подій із новою версією гри ще не було — це нормально).
- Description можна лишити порожнім.
- Уже зареєстровані параметри (`enabled`, `environment`, `ghost`, `method`, `mode`, `outcome`, `product`, `result`) не створюй повторно і не змінюй.
- Якщо dimension із таким самим Event parameter уже існує, нову не створюй — просто повідом.
- Якщо GA4 каже, що така назва вже зайнята, додай до назви префікс `Hauntscope ` (наприклад, `Hauntscope Push topic`), а Event parameter не змінюй.
- Якщо GA4 повідомить про ліміт custom dimensions чи metrics, зупинись і напиши, скільки вже створено.

## Фінальний звіт
Список: які dimensions і metrics створено (назва → параметр → scope), що пропущено і чому, і скільки всього custom dimensions і custom metrics тепер у property.
