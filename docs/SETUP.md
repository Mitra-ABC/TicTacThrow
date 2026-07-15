# راهنمای کامل تنظیمات Unity برای TicTacThrow

این راهنما به صورت گام به گام، نحوه تنظیم فایل‌های Unity را بعد از تغییرات کد توضیح می‌دهد.

---

## 📋 فهرست مطالب

1. [بررسی Scene اصلی](#1-بررسی-scene-اصلی)
2. [تنظیم GameManager](#2-تنظیم-gamemanager)
3. [ایجاد WebSocketManager GameObject](#3-ایجاد-websocketmanager-gameobject)
4. [بررسی اتصالات](#4-بررسی-اتصالات)
5. [تست در Unity](#5-تست-در-unity)

---

## 1. بررسی Scene اصلی

### مرحله 1.1: باز کردن Scene

1. در Unity، به **Assets → Scenes** بروید
2. فایل `Assets/Scenes/main_dooooz.unity` را باز کنید

### مرحله 1.2: بررسی Hierarchy

در **Hierarchy**، باید این GameObject ها وجود داشته باشند:
- `GameManager` (با script GameManager)
- `ApiClient` (با script ApiClient)
- `AuthManager` (با script AuthManager)
- `WebSocketManager` (اختیاری - به صورت خودکار ایجاد می‌شود)

---

## 2. تنظیم GameManager

### مرحله 2.1: پیدا کردن GameManager

1. در **Hierarchy**، GameObject `GameManager` را پیدا کنید
2. روی آن کلیک کنید
3. در **Inspector**، بخش **Game Manager (Script)** را پیدا کنید

### مرحله 2.2: بررسی Core Components

در بخش **Core Components**، باید این فیلدها وجود داشته باشند:

- **Api Client**: باید به GameObject `ApiClient` متصل باشد
- **Auth Manager**: باید به GameObject `AuthManager` متصل باشد
- **Board View**: باید به GameObject با BoardView script متصل باشد
- **Web Socket Manager**: می‌تواند خالی باشد (به صورت خودکار ایجاد می‌شود)

**اگر فیلدها خالی هستند:**

1. در **Hierarchy**، GameObject مورد نظر را پیدا کنید
2. آن را **بکشید** و در فیلد مربوطه در Inspector **رها کنید**

**⚠️ نکته:** `WebSocketManager` به صورت خودکار در `GameManager.Awake()` ایجاد می‌شود، اما می‌توانید آن را دستی هم اضافه کنید.

---

## 3. ایجاد WebSocketManager GameObject (اختیاری)

اگر می‌خواهید WebSocketManager را دستی اضافه کنید:

### مرحله 3.1: ایجاد GameObject

1. در **Hierarchy**، راست کلیک کنید
2. **Create Empty** را انتخاب کنید
3. نام آن را به `WebSocketManager` تغییر دهید

### مرحله 3.2: اضافه کردن Script

1. روی GameObject `WebSocketManager` کلیک کنید
2. در **Inspector**، روی **Add Component** کلیک کنید
3. `WebSocketManager` را جستجو و اضافه کنید

### مرحله 3.3: تنظیمات

در **Inspector**، بخش **WebSocket Manager (Script)**:

- **Server Url**: می‌توانید خالی بگذارید (به صورت خودکار از ApiClient گرفته می‌شود)
- **Verbose Logging**: تیک بزنید برای Debug

### مرحله 3.4: اتصال به GameManager

1. GameObject `WebSocketManager` را از Hierarchy **بکشید**
2. در Inspector `GameManager`، بخش **Core Components**
3. در فیلد **Web Socket Manager** **رها کنید**

---

## 4. بررسی اتصالات

### مرحله 4.1: بررسی ApiClient

1. GameObject `ApiClient` را پیدا کنید
2. در **Inspector**، بخش **Api Client (Script)** را بررسی کنید:
   - **Api Config**: باید به ApiConfig asset متصل باشد (اختیاری)
   - **Base Url Override**: می‌تواند خالی باشد
   - **Request Timeout Seconds**: پیش‌فرض 10
   - **Verbose Logging**: می‌توانید تیک بزنید

### مرحله 4.2: بررسی AuthManager

1. GameObject `AuthManager` را پیدا کنید
2. در **Inspector**، بخش **Auth Manager (Script)** را بررسی کنید:
   - **Api Client**: باید به GameObject `ApiClient` متصل باشد

### مرحله 4.3: بررسی GameManager - Panels

در **Inspector** `GameManager`، بخش **Panels**:

تمام Panel ها باید متصل باشند:
- **Auth Choice Panel**
- **Auth Form Panel**
- **Lobby Panel**
- **Join Room Panel**
- **Waiting Panel**
- **Matchmaking Panel**
- **In Game Panel**
- **Finished Panel**
- **Leaderboard Panel**
- **My Stats Panel**
- **Loading Overlay**

**اگر Panel ها خالی هستند:**

1. در **Hierarchy**، Panel مورد نظر را پیدا کنید
2. آن را **بکشید** و در فیلد مربوطه **رها کنید**

### مرحله 4.4: بررسی GameManager - Buttons

تمام Button ها باید متصل باشند:
- **Choose Login Button**
- **Choose Register Button**
- **Submit Auth Button**
- **Competitive Game Button**
- **Friendly Game Button**
- و سایر Button ها...

**اگر Button ها خالی هستند:**

1. در **Hierarchy**، Button مورد نظر را پیدا کنید
2. آن را **بکشید** و در فیلد مربوطه **رها کنید**

### مرحله 4.5: بررسی GameManager - Input Fields

تمام Input Field ها باید متصل باشند:
- **Username Input**
- **Password Input**
- **Nickname Input**
- **Join Room Input**

### مرحله 4.6: بررسی GameManager - Labels

تمام Label ها باید متصل باشند:
- **Welcome Label**
- **Player Info Label**
- **Room ID Label**
- **Turn Label**
- **Status Label**
- و سایر Label ها...

### مرحله 4.7: بررسی GameManager - Prefabs

Prefab ها باید متصل باشند:
- **Leaderboard Item Prefab**
- **Coin Pack Item Prefab**
- **Booster Item Prefab**

### مرحله 4.8: بررسی GameManager - Content Transforms

Transform ها باید متصل باشند:
- **Leaderboard Content**: Transform از داخل ScrollView
- **Coin Packs Content**: Transform از داخل ScrollView
- **Boosters Content**: Transform از داخل ScrollView

---

## 5. تست در Unity

### مرحله 5.1: بررسی خطاهای کامپایل

1. **Console** را باز کنید (Window → General → Console)
2. بررسی کنید که **هیچ خطای کامپایلی** وجود ندارد
3. اگر خطا دارید، آن‌ها را برطرف کنید

### مرحله 5.2: Play Mode Test

1. **Play** را بزنید
2. بررسی کنید که:
   - Scene بدون خطا Load می‌شود
   - GameManager به درستی Initialize می‌شود
   - WebSocketManager به صورت خودکار ایجاد می‌شود (اگر در Scene نیست)

### مرحله 5.3: تست Login

1. روی **Login** کلیک کنید
2. Username و Password وارد کنید
3. بررسی کنید که:
   - Login موفق می‌شود
   - WebSocket متصل می‌شود (در Console باید پیام "WebSocket Connected!" ببینید)

### مرحله 5.4: تست Create Room

1. بعد از Login، به **Lobby** بروید
2. روی **Play with Friends** کلیک کنید
3. روی **Create Room** کلیک کنید
4. بررسی کنید که:
   - Room ایجاد می‌شود
   - Room ID نمایش داده می‌شود
   - در Console پیام "Room created" را می‌بینید

---

## ✅ چک‌لیست نهایی

قبل از شروع کار، این موارد را بررسی کنید:

### Scene Setup:
- [ ] Scene اصلی باز است
- [ ] GameManager GameObject وجود دارد
- [ ] ApiClient GameObject وجود دارد
- [ ] AuthManager GameObject وجود دارد

### GameManager Inspector:
- [ ] **Core Components**:
  - [ ] Api Client متصل است
  - [ ] Auth Manager متصل است
  - [ ] Board View متصل است
  - [ ] Web Socket Manager (اختیاری - می‌تواند خالی باشد)

- [ ] **Panels**: همه Panel ها متصل هستند
- [ ] **Buttons**: همه Button ها متصل هستند
- [ ] **Input Fields**: همه Input Field ها متصل هستند
- [ ] **Labels**: همه Label ها متصل هستند
- [ ] **Prefabs**: همه Prefab ها متصل هستند
- [ ] **Content Transforms**: همه Transform ها متصل هستند

### ApiClient Inspector:
- [ ] Api Config متصل است (اختیاری)
- [ ] Base Url تنظیم شده است

### AuthManager Inspector:
- [ ] Api Client متصل است

### Console:
- [ ] هیچ خطای کامپایلی وجود ندارد
- [ ] Warning های مربوط به FindObjectOfType برطرف شده‌اند

---

## 🐛 عیب‌یابی

### مشکل: فیلدها در Inspector خالی هستند

**راه حل:**
1. GameObject ها را از Hierarchy به فیلدهای مربوطه بکشید
2. یا از دکمه **Circle** کنار فیلد استفاده کنید و GameObject را انتخاب کنید

### مشکل: WebSocketManager پیدا نمی‌شود

**راه حل:**
1. مطمئن شوید که script `WebSocketManager.cs` در پوشه `Assets/Scripts` وجود دارد
2. Unity را Refresh کنید (Assets → Refresh)
3. یا WebSocketManager را دستی در Scene اضافه کنید

### مشکل: خطای کامپایل در Console

**راه حل:**
1. خطا را در Console بخوانید
2. فایل مربوطه را بررسی کنید
3. مطمئن شوید که همه using statements درست هستند
4. مطمئن شوید که کتابخانه SocketIOUnity نصب شده است

### مشکل: Panel ها یا Button ها کار نمی‌کنند

**راه حل:**
1. بررسی کنید که در Inspector `GameManager` متصل هستند
2. بررسی کنید که Event Listener ها در `SetupButtonListeners()` تنظیم شده‌اند
3. Console را برای خطاها بررسی کنید

---

## 📝 نکات مهم

1. **WebSocketManager** به صورت خودکار ایجاد می‌شود اگر در Scene نباشد
2. **فیلدهای خالی** در Inspector مشکلی ایجاد نمی‌کنند اگر کد به صورت خودکار آن‌ها را پیدا کند
3. **همیشه Console را بررسی کنید** برای خطاها و warning ها
4. **Save Scene** را فراموش نکنید بعد از تغییرات

---

## 🎮 بعد از تنظیمات

بعد از انجام همه تنظیمات:

1. **Scene را Save کنید** (Ctrl+S یا File → Save)
2. **Play** را بزنید و تست کنید
3. اگر همه چیز کار می‌کند، **Build** کنید

---

**موفق باشید! 🚀**
