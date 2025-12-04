# ✅ تغییرات انجام شده در کد

## 📝 خلاصه

تمام تغییرات کد انجام شده است. فقط باید UI المان‌ها را در Unity Editor اضافه و متصل کنید.

---

## ✅ کارهای انجام شده

### 1. GameManager.cs
- ✅ State های جدید اضافه شده: `Matchmaking`, `Leaderboard`, `MyStats`
- ✅ تمام SerializeField ها اضافه شده
- ✅ متدهای Matchmaking پیاده‌سازی شده:
  - `OnPlayOnlineClicked()`
  - `OnCancelMatchmakingClicked()`
  - `HandleQueueMatchmaking()`
  - `HandleCancelMatchmaking()`
  - `DetermineLocalSymbolFromMatchmaking()`
- ✅ متدهای Leaderboard پیاده‌سازی شده:
  - `OnLeaderboardClicked()`
  - `OnMyStatsClicked()`
  - `OnCloseLeaderboard()`
  - `OnCloseMyStats()`
  - `OnRefreshLeaderboard()`
  - `LoadLeaderboard()`
  - `HandleLoadLeaderboard()`
  - `DisplayLeaderboard()`
  - `LoadMyStats()`
  - `HandleLoadMyStats()`
  - `DisplayMyStats()`
- ✅ `SetupButtonListeners()` به‌روزرسانی شده
- ✅ `UpdateUI()` به‌روزرسانی شده
- ✅ `PollRoomStateUntilStarted()` برای Matchmaking به‌روزرسانی شده
- ✅ `SetState()` برای handle کردن state های جدید به‌روزرسانی شده

### 2. LeaderboardItem.cs
- ✅ Script جدید برای نمایش هر آیتم در لیست Leaderboard ایجاد شده
- ✅ متد `SetPlayer()` برای تنظیم اطلاعات بازیکن

---

## 🎨 کارهای باقی‌مانده (در Unity Editor)

### بخش ۱: Matchmaking

1. **اضافه کردن دکمه در Lobby Panel**:
   - دکمه "Play Online" اضافه کنید
   - در Inspector به `playOnlineButton` متصل کنید

2. **ایجاد Matchmaking Panel**:
   - Panel جدید با نام `MatchmakingPanel` ایجاد کنید
   - Text (TMP) برای `matchmakingStatusLabel` اضافه کنید
   - Button برای `cancelMatchmakingButton` اضافه کنید
   - Panel را غیرفعال کنید (Active = false)
   - در Inspector به GameManager متصل کنید

### بخش ۲: Leaderboard

1. **اضافه کردن دکمه‌ها در Lobby**:
   - دکمه "Leaderboard" اضافه کنید → به `leaderboardButton` متصل کنید
   - دکمه "My Stats" اضافه کنید → به `myStatsButton` متصل کنید

2. **ایجاد Leaderboard Panel**:
   - Panel جدید با نام `LeaderboardPanel` ایجاد کنید
   - Text (TMP) برای `seasonLabel` اضافه کنید
   - ScrollView اضافه کنید:
     - داخل ScrollView یک GameObject با نام `Content` ایجاد کنید
     - به `Content` یک `Vertical Layout Group` اضافه کنید
     - `Content` را به `leaderboardContent` در Inspector متصل کنید
   - Button "Close" → به `closeLeaderboardButton` متصل کنید
   - Button "Refresh" → به `refreshLeaderboardButton` متصل کنید
   - Panel را غیرفعال کنید

3. **ایجاد My Stats Panel**:
   - Panel جدید با نام `MyStatsPanel` ایجاد کنید
   - Text (TMP) ها اضافه کنید:
     - `myStatsSeasonLabel`
     - `myStatsRankLabel`
     - `myStatsRatingLabel`
     - `myStatsWinsLabel`
     - `myStatsLossesLabel`
     - `myStatsDrawsLabel`
     - `myStatsGamesLabel`
   - Button "Close" → به `closeMyStatsButton` متصل کنید
   - Panel را غیرفعال کنید

4. **ایجاد Leaderboard Item Prefab**:
   - راست کلیک → Create → UI → Panel
   - نام: `LeaderboardItem`
   - Text (TMP) ها اضافه کنید:
     - Rank
     - Nickname
     - Rating
     - (اختیاری: Wins, Losses, Draws, Games Played)
   - Script `LeaderboardItem` را به Panel اضافه کنید
   - Text ها را در Inspector به script متصل کنید
   - به عنوان Prefab ذخیره کنید
   - Prefab را به `leaderboardItemPrefab` در GameManager متصل کنید

---

## 📋 چک‌لیست Inspector

بعد از ایجاد UI المان‌ها، این موارد را در Inspector GameManager بررسی کنید:

### Lobby Panel:
- [ ] `playOnlineButton`
- [ ] `leaderboardButton`
- [ ] `myStatsButton`

### Matchmaking Panel:
- [ ] `matchmakingPanel`
- [ ] `matchmakingStatusLabel`
- [ ] `cancelMatchmakingButton`

### Leaderboard Panel:
- [ ] `leaderboardPanel`
- [ ] `seasonLabel`
- [ ] `leaderboardContent` (Transform)
- [ ] `leaderboardItemPrefab` (GameObject)
- [ ] `closeLeaderboardButton`
- [ ] `refreshLeaderboardButton`

### My Stats Panel:
- [ ] `myStatsPanel`
- [ ] `myStatsSeasonLabel`
- [ ] `myStatsRankLabel`
- [ ] `myStatsRatingLabel`
- [ ] `myStatsWinsLabel`
- [ ] `myStatsLossesLabel`
- [ ] `myStatsDrawsLabel`
- [ ] `myStatsGamesLabel`
- [ ] `closeMyStatsButton`

---

## 🎯 نکات مهم

1. **تمام Panel های جدید باید غیرفعال باشند** (Active = false) تا فقط وقتی state تغییر می‌کند نمایش داده شوند

2. **Leaderboard Content** باید یک `Vertical Layout Group` داشته باشد تا آیتم‌ها به درستی چیده شوند

3. **LeaderboardItem Prefab** باید در پوشه `Assets/UI/` یا `Assets/Prefabs/` ذخیره شود

4. **تمام Button ها** باید در `SetupButtonListeners()` متصل شده باشند (✅ انجام شده)

5. **State Management** به صورت خودکار Panel ها را نمایش/مخفی می‌کند (✅ انجام شده)

---

## 🚀 آماده برای تست

بعد از اتصال تمام المان‌ها در Inspector، می‌توانید تست کنید:

1. **Matchmaking**: دکمه "Play Online" را بزنید → باید Matchmaking Panel نمایش داده شود
2. **Leaderboard**: دکمه "Leaderboard" را بزنید → باید لیست بازیکنان نمایش داده شود
3. **My Stats**: دکمه "My Stats" را بزنید → باید آمار شما نمایش داده شود

---

## 📞 در صورت مشکل

اگر خطایی دریافت کردید:
1. مطمئن شوید تمام المان‌ها در Inspector متصل شده‌اند
2. Console را برای خطاها بررسی کنید
3. مطمئن شوید API Server در حال اجرا است
