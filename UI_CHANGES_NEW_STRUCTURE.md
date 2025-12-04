# راهنمای تغییرات UI - ساختار جدید

## 📋 تغییرات مورد نیاز

### تغییر ساختار Lobby:
- **دکمه "بازی مسابقه‌ای"** (Competitive Game) → مستقیماً Matchmaking را شروع می‌کند
- **دکمه "بازی دوستانه"** (Friendly Game) → به صفحه جدید می‌رود که شامل Create Room و Join Room است

---

## 🎯 مرحله ۱: تغییر دکمه‌های Lobby

### ۱.۱: تغییر نام دکمه Play Online

1. در **Hierarchy**، `PlayOnlineButton` را پیدا کنید
2. نام را به `CompetitiveGameButton` تغییر دهید (یا همان `PlayOnlineButton` بماند)
3. **Text (TMP)** را به "بازی مسابقه‌ای" یا "Competitive Game" تغییر دهید

### ۱.۲: حذف دکمه‌های Create Room و Join Room از Lobby

1. `createRoomButton` و `joinRoomModeButton` را از `LobbyPanel` **حذف کنید** (یا غیرفعال کنید)
2. این دکمه‌ها به صفحه جدید منتقل می‌شوند

### ۱.۳: اضافه کردن دکمه بازی دوستانه

1. **راست کلیک** روی `LobbyPanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `FriendlyGameButton` بگذارید
4. **Text (TMP)**: "بازی دوستانه" یا "Friendly Game"
5. کنار دکمه "بازی مسابقه‌ای" قرار دهید

---

## 🎯 مرحله ۲: ایجاد Friendly Game Panel

### ۲.۱: ایجاد Panel

1. **راست کلیک** روی **Canvas**
2. **UI → Panel** را انتخاب کنید
3. نام را به `FriendlyGamePanel` بگذارید
4. **Active** را غیرفعال کنید (تیک را بردارید)

### ۲.۲: اضافه کردن دکمه Create Room

1. **راست کلیک** روی `FriendlyGamePanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `CreateRoomButton` بگذارید
4. **Text (TMP)**: "Create Room" یا "ایجاد اتاق"
5. در بالای Panel قرار دهید

### ۲.۳: اضافه کردن دکمه Join Room

1. **راست کلیک** روی `FriendlyGamePanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `JoinRoomModeButton` بگذارید
4. **Text (TMP)**: "Join Room" یا "پیوستن به اتاق"
5. زیر دکمه Create Room قرار دهید

### ۲.۴: اضافه کردن دکمه Back

1. **راست کلیک** روی `FriendlyGamePanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `BackFromFriendlyGameButton` بگذارید
4. **Text (TMP)**: "Back" یا "بازگشت"
5. در پایین Panel قرار دهید

### ۲.۵: تنظیم Layout (اختیاری)

- می‌توانید یک **Vertical Layout Group** به `FriendlyGamePanel` اضافه کنید تا دکمه‌ها به صورت خودکار چیده شوند

---

## 🎯 مرحله ۳: اتصال در Inspector

### ۳.۱: اتصال Lobby Panel Elements

1. **GameManager** را در Hierarchy انتخاب کنید
2. در **Inspector**، بخش **Lobby Panel**:

   - **Competitive Game Button** (یا Play Online Button):
     - `CompetitiveGameButton` را از Hierarchy بکشید
     - به `Competitive Game Button` در Inspector بیندازید
   
   - **Friendly Game Button**:
     - `FriendlyGameButton` را به `Friendly Game Button` متصل کنید

### ۳.۲: اتصال Friendly Game Panel

1. در **Inspector**، بخش **Friendly Game Panel**:

   - **Friendly Game Panel**:
     - `FriendlyGamePanel` را به `Friendly Game Panel` متصل کنید
   
   - **Create Room Button**:
     - `CreateRoomButton` (از داخل `FriendlyGamePanel`) را به `Create Room Button` متصل کنید
   
   - **Join Room Mode Button**:
     - `JoinRoomModeButton` (از داخل `FriendlyGamePanel`) را به `Join Room Mode Button` متصل کنید
   
   - **Back From Friendly Game Button**:
     - `BackFromFriendlyGameButton` را به `Back From Friendly Game Button` متصل کنید

---

## ✅ چک‌لیست

### Lobby Panel:
- [ ] دکمه "بازی مسابقه‌ای" (`CompetitiveGameButton`) اضافه شده
- [ ] دکمه "بازی دوستانه" (`FriendlyGameButton`) اضافه شده
- [ ] دکمه‌های Create Room و Join Room از Lobby حذف شده‌اند

### Friendly Game Panel:
- [ ] Panel ایجاد شده و غیرفعال است
- [ ] دکمه Create Room اضافه شده
- [ ] دکمه Join Room اضافه شده
- [ ] دکمه Back اضافه شده
- [ ] تمام المان‌ها در Inspector متصل شده‌اند

---

## 🎮 جریان بازی

### بازی مسابقه‌ای (Competitive):
1. کاربر در Lobby روی "بازی مسابقه‌ای" کلیک می‌کند
2. مستقیماً Matchmaking شروع می‌شود
3. Matchmaking Panel نمایش داده می‌شود

### بازی دوستانه (Friendly):
1. کاربر در Lobby روی "بازی دوستانه" کلیک می‌کند
2. Friendly Game Panel نمایش داده می‌شود
3. کاربر می‌تواند:
   - "Create Room" را بزند → Waiting Panel نمایش داده می‌شود
   - "Join Room" را بزند → Join Room Panel نمایش داده می‌شود
   - "Back" را بزند → به Lobby برمی‌گردد

---

## 📝 خلاصه تغییرات کد

تمام تغییرات کد انجام شده است:

✅ State جدید `FriendlyGame` اضافه شده
✅ Panel جدید `friendlyGamePanel` اضافه شده
✅ دکمه‌های جدید در Lobby اضافه شده
✅ متدهای جدید:
   - `OnCompetitiveGameClicked()` - شروع Matchmaking
   - `OnFriendlyGameClicked()` - نمایش Friendly Game Panel
   - `OnBackFromFriendlyGame()` - بازگشت به Lobby
✅ `OnBackToLobby()` به‌روزرسانی شده تا از JoinRoom به FriendlyGame برگردد
✅ `UpdateUI()` به‌روزرسانی شده

---

## 🚀 تست

بعد از اتصال همه چیز:

1. **Play** را بزنید
2. **Login** کنید
3. در Lobby:
   - **"بازی مسابقه‌ای"** را بزنید → باید Matchmaking شروع شود
   - **"بازی دوستانه"** را بزنید → باید Friendly Game Panel نمایش داده شود
4. در Friendly Game Panel:
   - **"Create Room"** را بزنید → باید Waiting Panel نمایش داده شود
   - **"Join Room"** را بزنید → باید Join Room Panel نمایش داده شود
   - **"Back"** را بزنید → باید به Lobby برگردید

---

## 💡 نکات مهم

1. **دکمه‌های Create Room و Join Room** باید از Lobby حذف شوند (یا غیرفعال شوند)
2. **Friendly Game Panel** باید غیرفعال باشد (Active = false)
3. **Join Room Panel** و **Waiting Panel** همچنان کار می‌کنند، فقط از Friendly Game Panel دسترسی دارند
4. وقتی از JoinRoom یا WaitingForOpponent برمی‌گردید، به Friendly Game Panel برمی‌گردید، نه Lobby
