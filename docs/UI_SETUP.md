# راهنمای UI در Unity - فقط مراحل Unity Editor

## ✅ کدها انجام شده است!

تمام کدهای لازم در `GameManager.cs` اضافه شده است. فقط باید UI المان‌ها را در Unity بسازید و متصل کنید.

---

## 📋 فهرست مراحل

1. [نمایش Wallet در Lobby](#مرحله-۱-نمایش-wallet-در-lobby)
2. [ایجاد Store Panel](#مرحله-۲-ایجاد-store-panel)
3. [ایجاد Wallet Panel](#مرحله-۳-ایجاد-wallet-panel)
4. [ایجاد Coin Pack Item Prefab](#مرحله-۵-ایجاد-coin-pack-item-prefab)
5. [ایجاد Booster Item Prefab](#مرحله-۶-ایجاد-booster-item-prefab)
6. [اتصال در Inspector](#مرحله-۴-اتصال-در-inspector)

---

## مرحله ۱: نمایش Wallet در Lobby

### ۱.۱: اضافه کردن Text برای Coins

1. **باز کردن Scene** (`Assets/Scenes/main_dooooz.unity`)
2. در **Hierarchy**، `LobbyPanel` را پیدا کنید
3. **راست کلیک** روی `LobbyPanel`
4. **UI → Text - TextMeshPro** را انتخاب کنید
5. نام را به `CoinsLabel` تغییر دهید
6. در **Inspector**:
   - **Text (TMP)**: "Coins: 0"
   - **Font Size**: مناسب تنظیم کنید (مثلاً 20-24)
7. در بالای Lobby Panel قرار دهید (مثلاً کنار `welcomeLabel`)

### ۱.۲: اضافه کردن Text برای Hearts

1. **راست کلیک** روی `LobbyPanel`
2. **UI → Text - TextMeshPro** را انتخاب کنید
3. نام را به `HeartsLabel` تغییر دهید
4. در **Inspector**:
   - **Text (TMP)**: "Hearts: 0/5"
   - **Font Size**: همان اندازه `CoinsLabel`
5. کنار `CoinsLabel` قرار دهید

### ۱.۳: اضافه کردن دکمه Store

1. **راست کلیک** روی `LobbyPanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `StoreButton` تغییر دهید
4. در **Inspector**:
   - **Text (TMP)**: "Store" یا "فروشگاه"
5. کنار دکمه‌های دیگر قرار دهید (مثلاً کنار `LeaderboardButton`)

### ۱.۴: اضافه کردن دکمه Wallet (اختیاری)

1. **راست کلیک** روی `LobbyPanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام را به `WalletButton` تغییر دهید
4. در **Inspector**:
   - **Text (TMP)**: "Wallet" یا "کیف پول"
5. کنار `StoreButton` قرار دهید

---

## مرحله ۲: ایجاد Store Panel

### ۲.۱: ایجاد Panel اصلی

1. **راست کلیک** روی **Canvas** در Hierarchy
2. **UI → Panel** را انتخاب کنید
3. نام را به `StorePanel` بگذارید
4. در **Inspector**:
   - **Active**: تیک را بردارید (Panel غیرفعال شود)

### ۲.۲: اضافه کردن عنوان

1. **راست کلیک** روی `StorePanel`
2. **UI → Text - TextMeshPro** را انتخاب کنید
3. نام: `StoreTitle`
4. در **Inspector**:
   - **Text**: "Store" یا "فروشگاه"
   - **Font Size**: بزرگتر (مثلاً 32)
   - **Alignment**: Center
5. در بالای Panel قرار دهید

### ۲.۳: اضافه کردن Text برای قیمت Heart

1. **راست کلیک** روی `StorePanel`
2. **UI → Text - TextMeshPro** را انتخاب کنید
3. نام: `HeartPriceLabel`
4. در **Inspector**:
   - **Text**: "Price: 15 coins" (به صورت پیش‌فرض)
5. زیر عنوان قرار دهید

### ۲.۴: اضافه کردن دکمه Buy Heart

1. **راست کلیک** روی `StorePanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام: `BuyHeartButton`
4. در **Inspector**:
   - **Text (TMP)**: "Buy Heart" یا "خرید قلب"
5. زیر `HeartPriceLabel` قرار دهید

### ۲.۵: اضافه کردن ScrollView برای Coin Packs

1. **راست کلیک** روی `StorePanel`
2. **UI → Scroll View** را انتخاب کنید
3. نام: `CoinPacksScrollView`
4. در **Inspector**:
   - **Scroll Rect**: تنظیمات پیش‌فرض
5. **Content** را پیدا کنید (داخل `CoinPacksScrollView`)
6. روی `Content` کلیک کنید
7. در **Inspector**:
   - **Add Component → Layout → Vertical Layout Group**:
     - **Spacing**: 5 یا 10
     - **Padding**: Left, Right, Top, Bottom (مثلاً 10)
   - **Add Component → Content Size Fitter**:
     - **Vertical Fit**: Preferred Size

### ۲.۶: اضافه کردن ScrollView برای Boosters

1. **راست کلیک** روی `StorePanel`
2. **UI → Scroll View** را انتخاب کنید
3. نام: `BoostersScrollView`
4. در **Inspector**:
   - **Scroll Rect**: تنظیمات پیش‌فرض
5. **Content** را پیدا کنید (داخل `BoostersScrollView`)
6. روی `Content` کلیک کنید
7. در **Inspector**:
   - **Add Component → Layout → Vertical Layout Group**:
     - **Spacing**: 5 یا 10
     - **Padding**: Left, Right, Top, Bottom (مثلاً 10)
   - **Add Component → Content Size Fitter**:
     - **Vertical Fit**: Preferred Size

### ۲.۷: اضافه کردن دکمه Close

1. **راست کلیک** روی `StorePanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام: `CloseStoreButton`
4. در **Inspector**:
   - **Text (TMP)**: "Close" یا "بستن"
5. در پایین Panel قرار دهید

---

## مرحله ۳: ایجاد Wallet Panel

### ۳.۱: ایجاد Panel

1. **راست کلیک** روی **Canvas**
2. **UI → Panel** را انتخاب کنید
3. نام را به `WalletPanel` بگذارید
4. در **Inspector**:
   - **Active**: تیک را بردارید (Panel غیرفعال شود)

### ۳.۲: اضافه کردن Text ها

برای هر یک از این موارد، یک **Text - TextMeshPro** اضافه کنید:

1. **عنوان**:
   - راست کلیک روی `WalletPanel` → **UI → Text - TextMeshPro**
   - نام: `WalletTitle`
   - **Text**: "Wallet" یا "کیف پول"
   - **Font Size**: بزرگتر (مثلاً 32)
   - در بالای Panel قرار دهید

2. **Coins**:
   - راست کلیک روی `WalletPanel` → **UI → Text - TextMeshPro**
   - نام: `WalletCoinsLabel`
   - **Text**: "Coins: 0"
   - زیر عنوان قرار دهید

3. **Hearts**:
   - راست کلیک روی `WalletPanel` → **UI → Text - TextMeshPro**
   - نام: `WalletHeartsLabel`
   - **Text**: "Hearts: 0/5"
   - زیر `WalletCoinsLabel` قرار دهید

4. **Next Heart** (اختیاری):
   - راست کلیک روی `WalletPanel` → **UI → Text - TextMeshPro**
   - نام: `NextHeartLabel`
   - **Text**: "Next heart: --"
   - زیر `WalletHeartsLabel` قرار دهید

### ۳.۳: اضافه کردن دکمه Close

1. **راست کلیک** روی `WalletPanel`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام: `CloseWalletButton`
4. در **Inspector**:
   - **Text (TMP)**: "Close" یا "بستن"
5. در پایین Panel قرار دهید

### ۳.۴: تنظیم Layout (اختیاری)

- می‌توانید یک **Vertical Layout Group** به `WalletPanel` اضافه کنید

---

## مرحله ۴: اتصال در Inspector

### ۴.۱: پیدا کردن GameManager

1. در **Hierarchy**، GameObject که `GameManager` script دارد را پیدا کنید
2. روی آن کلیک کنید
3. در **Inspector**، `GameManager` component را ببینید

### ۴.۲: اتصال Lobby Panel - Wallet

در بخش **Lobby Panel - Wallet**:

1. **Coins Label**:
   - از Hierarchy، `CoinsLabel` را بکشید
   - به `Coins Label` در Inspector بیندازید

2. **Hearts Label**:
   - `HeartsLabel` را به `Hearts Label` متصل کنید

3. **Store Button**:
   - `StoreButton` را به `Store Button` متصل کنید

4. **Wallet Button** (اگر اضافه کردید):
   - `WalletButton` را به `Wallet Button` متصل کنید

### ۴.۳: اتصال Store Panel

در بخش **Store Panel**:

1. **Store Panel**:
   - `StorePanel` را به `Store Panel` متصل کنید

2. **Store Title** (اگر اضافه کردید):
   - `StoreTitle` را به `Store Title` متصل کنید

3. **Heart Price Label**:
   - `HeartPriceLabel` را به `Heart Price Label` متصل کنید

4. **Buy Heart Button**:
   - `BuyHeartButton` (از داخل `StorePanel`) را به `Buy Heart Button` متصل کنید

5. **Coin Packs Content**:
   - `Content` (از داخل `CoinPacksScrollView`) را به `Coin Packs Content` متصل کنید
   - ⚠️ مهم: باید **Transform** باشد، نه GameObject

6. **Coin Pack Item Prefab**:
   - از **Project** window، `CoinPackItem` prefab را به `Coin Pack Item Prefab` متصل کنید
   - (اگر prefab را هنوز نساخته‌اید، مرحله ۵ را ببینید)

7. **Boosters Content**:
   - `Content` (از داخل `BoostersScrollView`) را به `Boosters Content` متصل کنید
   - ⚠️ مهم: باید **Transform** باشد

8. **Booster Item Prefab**:
   - از **Project** window، `BoosterItem` prefab را به `Booster Item Prefab` متصل کنید
   - (اگر prefab را هنوز نساخته‌اید، مرحله ۶ را ببینید)

9. **Close Store Button**:
   - `CloseStoreButton` را به `Close Store Button` متصل کنید

### ۴.۴: اتصال Wallet Panel

در بخش **Wallet Panel**:

1. **Wallet Panel**:
   - `WalletPanel` را به `Wallet Panel` متصل کنید

2. **Wallet Coins Label**:
   - `WalletCoinsLabel` را به `Wallet Coins Label` متصل کنید

3. **Wallet Hearts Label**:
   - `WalletHeartsLabel` را به `Wallet Hearts Label` متصل کنید

4. **Next Heart Label** (اگر اضافه کردید):
   - `NextHeartLabel` را به `Next Heart Label` متصل کنید

5. **Close Wallet Button**:
   - `CloseWalletButton` را به `Close Wallet Button` متصل کنید

---

## ✅ چک‌لیست نهایی

### Lobby Panel:
- [ ] `CoinsLabel` اضافه شده و متصل شده
- [ ] `HeartsLabel` اضافه شده و متصل شده
- [ ] `StoreButton` اضافه شده و متصل شده
- [ ] `WalletButton` اضافه شده و متصل شده (اختیاری)

### Store Panel:
- [ ] `StorePanel` ایجاد شده و غیرفعال است
- [ ] `BuyHeartButton` اضافه شده و متصل شده
- [ ] `CloseStoreButton` اضافه شده و متصل شده
- [ ] `StorePanel` در Inspector متصل شده

### Store Panel:
- [ ] `StorePanel` ایجاد شده و غیرفعال است
- [ ] `StoreTitle` اضافه شده (اختیاری)
- [ ] `HeartPriceLabel` اضافه شده و متصل شده
- [ ] `BuyHeartButton` اضافه شده و متصل شده
- [ ] `CoinPacksScrollView` با Content ایجاد شده
- [ ] `CoinPacksContent` متصل شده (Transform)
- [ ] `CoinPackItemPrefab` ایجاد شده و متصل شده
- [ ] `BoostersScrollView` با Content ایجاد شده
- [ ] `BoostersContent` متصل شده (Transform)
- [ ] `BoosterItemPrefab` ایجاد شده و متصل شده
- [ ] `CloseStoreButton` اضافه شده و متصل شده
- [ ] `StorePanel` در Inspector متصل شده

### Wallet Panel:
- [ ] `WalletPanel` ایجاد شده و غیرفعال است
- [ ] `WalletCoinsLabel` اضافه شده و متصل شده
- [ ] `WalletHeartsLabel` اضافه شده و متصل شده
- [ ] `NextHeartLabel` اضافه شده و متصل شده (اختیاری)
- [ ] `CloseWalletButton` اضافه شده و متصل شده
- [ ] `WalletPanel` در Inspector متصل شده

---

## 🎮 تست

بعد از اتصال همه چیز:

1. **Play** را بزنید
2. **Login** کنید
3. در Lobby:
   - باید Coins و Hearts نمایش داده شوند
   - **"Store"** را بزنید → باید Store Panel نمایش داده شود
   - **"Wallet"** را بزنید → باید Wallet Panel نمایش داده شود
4. در Store:
   - **"Buy Heart"** را بزنید → باید قلب خریداری شود
   - Coins و Hearts باید به‌روزرسانی شوند
5. در Wallet:
   - باید اطلاعات کامل wallet نمایش داده شود

---

## 💡 نکات مهم

1. **تمام Panel های جدید باید غیرفعال باشند** (Active = false)
   - فقط وقتی state تغییر می‌کند، GameManager آن‌ها را فعال می‌کند

2. **نام‌گذاری مهم است!**
   - نام المان‌ها باید دقیقاً همان باشد که در راهنما نوشته شده
   - این باعث می‌شود راحت‌تر در Inspector پیدا کنید

3. **اگر چیزی کار نکرد:**
   - Console را بررسی کنید (Window → General → Console)
   - مطمئن شوید تمام المان‌ها در Inspector متصل شده‌اند
   - مطمئن شوید API Server در حال اجرا است

---

## مرحله ۵: ایجاد Coin Pack Item Prefab

### ۵.۱: ایجاد Panel برای آیتم

1. **راست کلیک** در **Hierarchy** (یا در پوشه `Assets/Prefabs/UI/`)
2. **Create → UI → Panel**
3. نام را به `CoinPackItem` تغییر دهید

### ۵.۲: تنظیم اندازه Panel

1. در **Inspector**:
   - **Rect Transform**:
     - **Width**: 400-500 (یا مطابق با ScrollView)
     - **Height**: 80-100 (ارتفاع هر آیتم)

### ۵.۳: اضافه کردن Text ها

برای هر آیتم، یک **Text - TextMeshPro** اضافه کنید:

1. **Pack Name** (نام بسته):
   - نام: `PackNameText`
   - **Text**: "Small Pack"
   - **Font Size**: 20-24
   - در سمت چپ قرار دهید

2. **Description** (توضیحات):
   - نام: `DescriptionText`
   - **Text**: "100 coins"
   - **Font Size**: 16-18
   - زیر نام قرار دهید

3. **Coins Amount** (مقدار سکه):
   - نام: `CoinsAmountText`
   - **Text**: "100"
   - **Font Size**: 18-22
   - در سمت راست قرار دهید

4. **Bonus** (اختیاری):
   - نام: `BonusText`
   - **Text**: "+25 bonus"
   - **Font Size**: 14-16
   - کنار `CoinsAmountText` قرار دهید
   - به صورت پیش‌فرض غیرفعال کنید

### ۵.۴: اضافه کردن دکمه Buy

1. **راست کلیک** روی `CoinPackItem`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام: `BuyButton`
4. **Text (TMP)**: "Buy" یا "خرید"
5. در پایین Panel قرار دهید

### ۵.۵: اضافه کردن Script

1. در **Inspector** روی `CoinPackItem` کلیک کنید
2. **Add Component** را بزنید
3. **Coin Pack Item** را جستجو و اضافه کنید
4. **Text ها و Button را به Script متصل کنید**:
   - `PackNameText` → `packNameText`
   - `DescriptionText` → `descriptionText`
   - `CoinsAmountText` → `coinsAmountText`
   - `BonusText` → `bonusText`
   - `BuyButton` → `buyButton`

### ۵.۶: ذخیره به عنوان Prefab

1. **Project** window را باز کنید
2. پوشه `Assets/Prefabs/UI/` یا `Assets/Prefabs/` را پیدا کنید (یا ایجاد کنید)
3. **CoinPackItem** را از Hierarchy به Project بکشید
4. یک Prefab ایجاد می‌شود
5. **CoinPackItem** را از Hierarchy حذف کنید (Prefab کافی است)

---

## مرحله ۶: ایجاد Booster Item Prefab

### ۶.۱: ایجاد Panel برای آیتم

1. **راست کلیک** در **Hierarchy** (یا در پوشه `Assets/Prefabs/UI/`)
2. **Create → UI → Panel**
3. نام را به `BoosterItem` تغییر دهید

### ۶.۲: تنظیم اندازه Panel

1. در **Inspector**:
   - **Rect Transform**:
     - **Width**: 400-500
     - **Height**: 80-100

### ۶.۳: اضافه کردن Text ها

برای هر آیتم، یک **Text - TextMeshPro** اضافه کنید:

1. **Booster Name** (نام بوستر):
   - نام: `BoosterNameText`
   - **Text**: "Double Reward"
   - **Font Size**: 20-24
   - در سمت چپ قرار دهید

2. **Description** (توضیحات):
   - نام: `DescriptionText`
   - **Text**: "Double coins on match win"
   - **Font Size**: 14-16
   - زیر نام قرار دهید

3. **Price** (قیمت):
   - نام: `PriceText`
   - **Text**: "Price: 50 coins"
   - **Font Size**: 16-18
   - در سمت راست قرار دهید

4. **Duration** (مدت زمان):
   - نام: `DurationText`
   - **Text**: "Duration: 60 minutes"
   - **Font Size**: 14-16
   - زیر قیمت قرار دهید

### ۶.۴: اضافه کردن دکمه Buy

1. **راست کلیک** روی `BoosterItem`
2. **UI → Button - TextMeshPro** را انتخاب کنید
3. نام: `BuyButton`
4. **Text (TMP)**: "Buy" یا "خرید"
5. در پایین Panel قرار دهید

### ۶.۵: اضافه کردن Script

1. در **Inspector** روی `BoosterItem` کلیک کنید
2. **Add Component** را بزنید
3. **Booster Item** را جستجو و اضافه کنید
4. **Text ها و Button را به Script متصل کنید**:
   - `BoosterNameText` → `boosterNameText`
   - `DescriptionText` → `descriptionText`
   - `PriceText` → `priceText`
   - `DurationText` → `durationText`
   - `BuyButton` → `buyButton`

### ۶.۶: ذخیره به عنوان Prefab

1. **Project** window را باز کنید
2. **BoosterItem** را از Hierarchy به Project بکشید
3. یک Prefab ایجاد می‌شود
4. **BoosterItem** را از Hierarchy حذف کنید

---

## 🎯 خلاصه

**کدها:** ✅ انجام شده  
**UI در Unity:** باید انجام دهید (فقط drag & drop در Inspector)

بعد از اتصال تمام المان‌ها در Inspector، همه چیز آماده است! 🚀
