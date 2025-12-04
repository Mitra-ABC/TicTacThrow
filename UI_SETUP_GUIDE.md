# راهنمای مرحله به مرحله UI در Unity

## 📋 فهرست مراحل

1. [اضافه کردن دکمه Play Online](#مرحله-۱-اضافه-کردن-دکمه-play-online)
2. [ایجاد Matchmaking Panel](#مرحله-۲-ایجاد-matchmaking-panel)
3. [اضافه کردن دکمه‌های Leaderboard و My Stats](#مرحله-۳-اضافه-کردن-دکمه‌های-leaderboard-و-my-stats)
4. [ایجاد Leaderboard Panel](#مرحله-۴-ایجاد-leaderboard-panel)
5. [ایجاد My Stats Panel](#مرحله-۵-ایجاد-my-stats-panel)
6. [ایجاد LeaderboardItem Prefab](#مرحله-۶-ایجاد-leaderboarditem-prefab)
7. [اتصال همه چیز در Inspector](#مرحله-۷-اتصال-همه-چیز-در-inspector)

---

## مرحله ۱: اضافه کردن دکمه Play Online

### ۱.۱: پیدا کردن Lobby Panel

1. **باز کردن Scene** اصلی (`main_dooooz.unity`)
2. در **Hierarchy**، `LobbyPanel` را پیدا کنید
3. روی آن کلیک کنید تا انتخاب شود

### ۱.۲: اضافه کردن دکمه

1. **راست کلیک** روی `LobbyPanel` در Hierarchy
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. دکمه جدید ایجاد می‌شود

### ۱.۳: تنظیمات دکمه

1. **نام دکمه** را به `PlayOnlineButton` تغییر دهید
2. در **Inspector**:
   - **Rect Transform**: موقعیت و اندازه را تنظیم کنید (کنار `createRoomButton` و `joinRoomModeButton`)
   - **Button Component**: 
     - **Interactable**: ✅ فعال باشد
   - **Text (TMP)**: متن را به "Play Online" یا "بازی آنلاین" تغییر دهید

### ۱.۴: ذخیره

- تغییرات به صورت خودکار ذخیره می‌شوند

---

## مرحله ۲: ایجاد Matchmaking Panel

### ۲.۱: ایجاد Panel

1. در **Hierarchy**، **Canvas** را پیدا کنید
2. **راست کلیک** روی Canvas
3. **UI → Panel** را انتخاب کنید
4. نام را به `MatchmakingPanel` تغییر دهید

### ۲.۲: غیرفعال کردن Panel

1. در **Inspector**، تیک **Active** را بردارید (Panel غیرفعال شود)
   - این Panel فقط وقتی نمایش داده می‌شود که state به `Matchmaking` تغییر کند

### ۲.۳: اضافه کردن Text برای وضعیت

1. **راست کلیک** روی `MatchmakingPanel`
2. **UI → Text - TextMeshPro** را انتخاب کنید
3. نام را به `MatchmakingStatusLabel` تغییر دهید
4. در **Inspector**:
   - **Text**: "Searching for opponent..." یا "در حال جستجوی حریف..."
   - **Font Size**: مناسب تنظیم کنید (مثلاً 24)
   - **Alignment**: Center

### ۲.۴: اضافه کردن دکمه لغو

1. **راست کلیک** روی `MatchmakingPanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `CancelMatchmakingButton` تغییر دهید
4. در **Inspector**:
   - **Text (TMP)**: "Cancel" یا "لغو"
   - موقعیت را زیر `MatchmakingStatusLabel` قرار دهید

### ۲.۵: تنظیم Layout (اختیاری)

- می‌توانید یک **Vertical Layout Group** به `MatchmakingPanel` اضافه کنید تا المان‌ها به صورت خودکار چیده شوند

---

## مرحله ۳: اضافه کردن دکمه‌های Leaderboard و My Stats

### ۳.۱: اضافه کردن دکمه Leaderboard

1. **راست کلیک** روی `LobbyPanel` در Hierarchy
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `LeaderboardButton` تغییر دهید
4. **Text (TMP)**: "Leaderboard" یا "جدول امتیازات"
5. موقعیت را کنار دکمه‌های دیگر قرار دهید

### ۳.۲: اضافه کردن دکمه My Stats

1. **راست کلیک** روی `LobbyPanel` در Hierarchy
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `MyStatsButton` تغییر دهید
4. **Text (TMP)**: "My Stats" یا "آمار من"
5. موقعیت را کنار `LeaderboardButton` قرار دهید

---

## مرحله ۴: ایجاد Leaderboard Panel

### ۴.۱: ایجاد Panel

1. **راست کلیک** روی **Canvas**
2. **UI → Panel** را انتخاب کنید
3. نام را به `LeaderboardPanel` تغییر دهید
4. **Active** را غیرفعال کنید (تیک را بردارید)

### ۴.۲: اضافه کردن عنوان

1. **راست کلیک** روی `LeaderboardPanel`
2. **UI → Text - TextMeshPro** را انتخاب کنید
3. نام: `LeaderboardTitle`
4. **Text**: "Leaderboard" یا "جدول امتیازات"
5. **Font Size**: بزرگتر (مثلاً 32)
6. در بالای Panel قرار دهید

### ۴.۳: اضافه کردن Text برای فصل

1. **راست کلیک** روی `LeaderboardPanel`
2. **UI → Text - TextMeshPro** را انتخاب کنید
3. نام: `SeasonLabel`
4. **Text**: "Season: 2025-12" (به صورت پیش‌فرض)
5. زیر عنوان قرار دهید

### ۴.۴: ایجاد ScrollView

1. **راست کلیک** روی `LeaderboardPanel`
2. **UI → Scroll View** را انتخاب کنید
3. نام را به `LeaderboardScrollView` تغییر دهید
4. در **Inspector**:
   - **Scroll Rect**: تنظیمات پیش‌فرض مناسب است
   - **Viewport**: به صورت خودکار ایجاد می‌شود
   - **Content**: این مهم است! باید تنظیم شود

### ۴.۵: تنظیم Content برای ScrollView

1. در **Hierarchy**، `Content` را پیدا کنید (داخل `LeaderboardScrollView`)
2. روی آن کلیک کنید
3. در **Inspector**:
   - **Rect Transform**: 
     - **Anchor**: Top-Left
     - **Width**: مطابق با Viewport
   - **Add Component → Layout → Vertical Layout Group**:
     - **Spacing**: 5 یا 10 (فاصله بین آیتم‌ها)
     - **Padding**: Left, Right, Top, Bottom (مثلاً 10)
     - **Child Alignment**: Upper Center
     - **Child Force Expand**: 
       - ✅ Width
       - ❌ Height
   - **Add Component → Content Size Fitter**:
     - **Vertical Fit**: Preferred Size
     - این باعث می‌شود Content به اندازه محتوا بزرگ شود

### ۴.۶: اضافه کردن دکمه‌ها

1. **دکمه Close**:
   - راست کلیک روی `LeaderboardPanel`
   - **UI → Button - TextMeshPro**
   - نام: `CloseLeaderboardButton`
   - **Text**: "Close" یا "بستن"
   - در پایین Panel قرار دهید

2. **دکمه Refresh** (اختیاری):
   - راست کلیک روی `LeaderboardPanel`
   - **UI → Button - TextMeshPro`
   - نام: `RefreshLeaderboardButton`
   - **Text**: "Refresh" یا "بروزرسانی"
   - کنار دکمه Close قرار دهید

---

## مرحله ۵: ایجاد My Stats Panel

### ۵.۱: ایجاد Panel

1. **راست کلیک** روی **Canvas**
2. **UI → Panel** را انتخاب کنید
3. نام را به `MyStatsPanel` تغییر دهید
4. **Active** را غیرفعال کنید

### ۵.۲: اضافه کردن Text ها

برای هر یک از این موارد، یک **Text - TextMeshPro** اضافه کنید:

1. **عنوان**:
   - نام: `MyStatsTitle`
   - **Text**: "My Stats" یا "آمار من"
   - **Font Size**: بزرگتر

2. **فصل**:
   - نام: `MyStatsSeasonLabel`
   - **Text**: "Season: 2025-12"

3. **رتبه**:
   - نام: `MyStatsRankLabel`
   - **Text**: "Rank: -"

4. **امتیاز**:
   - نام: `MyStatsRatingLabel`
   - **Text**: "Rating: -"

5. **بردها**:
   - نام: `MyStatsWinsLabel`
   - **Text**: "Wins: 0"

6. **باخت‌ها**:
   - نام: `MyStatsLossesLabel`
   - **Text**: "Losses: 0"

7. **تساوی‌ها**:
   - نام: `MyStatsDrawsLabel`
   - **Text**: "Draws: 0"

8. **تعداد بازی‌ها**:
   - نام: `MyStatsGamesLabel`
   - **Text**: "Games Played: 0"

### ۵.۳: اضافه کردن دکمه Close

1. **راست کلیک** روی `MyStatsPanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام: `CloseMyStatsButton`
4. **Text**: "Close" یا "بستن"
5. در پایین Panel قرار دهید

### ۵.۴: تنظیم Layout (اختیاری)

- می‌توانید یک **Vertical Layout Group** به `MyStatsPanel` اضافه کنید

---

## مرحله ۶: ایجاد LeaderboardItem Prefab

### ۶.۱: ایجاد Panel برای آیتم

1. **راست کلیک** در **Hierarchy** (یا در پوشه `Assets/UI/`)
2. **Create → UI → Panel**
3. نام را به `LeaderboardItem` تغییر دهید

### ۶.۲: تنظیم اندازه Panel

1. در **Inspector**:
   - **Rect Transform**:
     - **Width**: 400-500 (یا مطابق با ScrollView)
     - **Height**: 60-80 (ارتفاع هر آیتم)

### ۶.۳: اضافه کردن Text ها

برای هر آیتم، یک **Text - TextMeshPro** اضافه کنید:

1. **Rank** (رتبه):
   - نام: `RankText`
   - **Text**: "1"
   - **Font Size**: 20-24
   - در سمت چپ قرار دهید

2. **Nickname** (نام):
   - نام: `NicknameText`
   - **Text**: "PlayerName"
   - **Font Size**: 18-22
   - در وسط قرار دهید

3. **Rating** (امتیاز):
   - نام: `RatingText`
   - **Text**: "1200"
   - **Font Size**: 18-22
   - در سمت راست قرار دهید

4. **(اختیاری) Wins, Losses, Draws, Games**:
   - می‌توانید این‌ها را هم اضافه کنید

### ۶.۴: اضافه کردن Script

1. در **Inspector** روی `LeaderboardItem` کلیک کنید
2. **Add Component** را بزنید
3. **Leaderboard Item** را جستجو و اضافه کنید
4. **Text ها را به Script متصل کنید**:
   - `RankText` → `rankText`
   - `NicknameText` → `nicknameText`
   - `RatingText` → `ratingText`
   - (و بقیه اگر اضافه کردید)

### ۶.۵: ذخیره به عنوان Prefab

1. **Project** window را باز کنید
2. پوشه `Assets/UI/` یا `Assets/Prefabs/` را پیدا کنید (یا ایجاد کنید)
3. **LeaderboardItem** را از Hierarchy به Project بکشید
4. یک Prefab ایجاد می‌شود
5. **LeaderboardItem** را از Hierarchy حذف کنید (Prefab کافی است)

---

## مرحله ۷: اتصال همه چیز در Inspector

### ۷.۱: پیدا کردن GameManager

1. در **Hierarchy**، GameObject که `GameManager` script دارد را پیدا کنید
2. روی آن کلیک کنید
3. در **Inspector**، `GameManager` component را ببینید

### ۷.۲: اتصال Lobby Panel Elements

در بخش **Lobby Panel**:

1. **Play Online Button**:
   - از Hierarchy، `PlayOnlineButton` را بکشید
   - به `Play Online Button` در Inspector بیندازید

2. **Leaderboard Button**:
   - `LeaderboardButton` را به `Leaderboard Button` متصل کنید

3. **My Stats Button**:
   - `MyStatsButton` را به `My Stats Button` متصل کنید

### ۷.۳: اتصال Matchmaking Panel

در بخش **Matchmaking Panel**:

1. **Matchmaking Panel**:
   - `MatchmakingPanel` را به `Matchmaking Panel` متصل کنید

2. **Matchmaking Status Label**:
   - `MatchmakingStatusLabel` را به `Matchmaking Status Label` متصل کنید

3. **Cancel Matchmaking Button**:
   - `CancelMatchmakingButton` را به `Cancel Matchmaking Button` متصل کنید

### ۷.۴: اتصال Leaderboard Panel

در بخش **Leaderboard Panel**:

1. **Leaderboard Panel**:
   - `LeaderboardPanel` را به `Leaderboard Panel` متصل کنید

2. **Season Label**:
   - `SeasonLabel` را به `Season Label` متصل کنید

3. **Leaderboard Content**:
   - `Content` (از داخل `LeaderboardScrollView`) را به `Leaderboard Content` متصل کنید
   - ⚠️ مهم: باید **Transform** باشد، نه GameObject

4. **Leaderboard Item Prefab**:
   - از **Project** window، `LeaderboardItem` prefab را به `Leaderboard Item Prefab` متصل کنید

5. **Close Leaderboard Button**:
   - `CloseLeaderboardButton` را متصل کنید

6. **Refresh Leaderboard Button**:
   - `RefreshLeaderboardButton` را متصل کنید

### ۷.۵: اتصال My Stats Panel

در بخش **My Stats Panel**:

1. **My Stats Panel**:
   - `MyStatsPanel` را متصل کنید

2. تمام Label ها:
   - `MyStatsSeasonLabel` → `My Stats Season Label`
   - `MyStatsRankLabel` → `My Stats Rank Label`
   - `MyStatsRatingLabel` → `My Stats Rating Label`
   - `MyStatsWinsLabel` → `My Stats Wins Label`
   - `MyStatsLossesLabel` → `My Stats Losses Label`
   - `MyStatsDrawsLabel` → `My Stats Draws Label`
   - `MyStatsGamesLabel` → `My Stats Games Label`

3. **Close My Stats Button**:
   - `CloseMyStatsButton` را متصل کنید

---

## ✅ چک‌لیست نهایی

بعد از اتصال همه چیز، این موارد را بررسی کنید:

### Lobby Panel:
- [ ] `playOnlineButton` متصل شده
- [ ] `leaderboardButton` متصل شده
- [ ] `myStatsButton` متصل شده

### Matchmaking Panel:
- [ ] `matchmakingPanel` متصل شده
- [ ] `matchmakingStatusLabel` متصل شده
- [ ] `cancelMatchmakingButton` متصل شده
- [ ] Panel غیرفعال است (Active = false)

### Leaderboard Panel:
- [ ] `leaderboardPanel` متصل شده
- [ ] `seasonLabel` متصل شده
- [ ] `leaderboardContent` متصل شده (⚠️ Transform!)
- [ ] `leaderboardItemPrefab` متصل شده
- [ ] `closeLeaderboardButton` متصل شده
- [ ] `refreshLeaderboardButton` متصل شده
- [ ] Panel غیرفعال است

### My Stats Panel:
- [ ] `myStatsPanel` متصل شده
- [ ] تمام Label ها متصل شده‌اند
- [ ] `closeMyStatsButton` متصل شده
- [ ] Panel غیرفعال است

### LeaderboardItem Prefab:
- [ ] Prefab در Project ذخیره شده
- [ ] Script `LeaderboardItem` اضافه شده
- [ ] تمام Text ها به Script متصل شده‌اند

---

## 🎯 نکات مهم

1. **تمام Panel های جدید باید غیرفعال باشند** (Active = false)
   - فقط وقتی state تغییر می‌کند، GameManager آن‌ها را فعال می‌کند

2. **Leaderboard Content**:
   - باید **Transform** باشد، نه GameObject
   - باید **Vertical Layout Group** داشته باشد
   - باید **Content Size Fitter** داشته باشد

3. **LeaderboardItem Prefab**:
   - باید در Project window ذخیره شود
   - باید Script `LeaderboardItem` داشته باشد
   - تمام Text ها باید به Script متصل باشند

4. **اگر چیزی کار نکرد**:
   - Console را بررسی کنید (Window → General → Console)
   - مطمئن شوید تمام المان‌ها متصل شده‌اند
   - مطمئن شوید API Server در حال اجرا است

---

## 🚀 تست

بعد از اتصال همه چیز:

1. **Play** را بزنید
2. **Login** کنید
3. **دکمه "Play Online"** را بزنید → باید Matchmaking Panel نمایش داده شود
4. **دکمه "Leaderboard"** را بزنید → باید Leaderboard Panel نمایش داده شود
5. **دکمه "My Stats"** را بزنید → باید My Stats Panel نمایش داده شود

---

## 📞 در صورت مشکل

اگر خطایی دریافت کردید:
- Console را بررسی کنید
- مطمئن شوید تمام المان‌ها متصل شده‌اند
- مطمئن شوید Prefab ها درست ایجاد شده‌اند
- مطمئن شوید API Server در حال اجرا است
