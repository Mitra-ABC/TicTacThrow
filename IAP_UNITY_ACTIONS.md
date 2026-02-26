# اقدامات سمت یونیتی برای IAP چندفروشگاهی (Bazaar / Myket)

این سند خلاصه کارهایی است که در یونیتی انجام شده و کارهایی که شما باید خودتان انجام دهید.

## کارهای انجام‌شده در کد

1. **ApiModels.cs** — مدل `VerifyIAPResponse` (status, message) اضافه شد.
2. **ApiResponseParser.cs** — متد `ParseVerifyIAPResponse` اضافه شد.
3. **ApiClient.cs** — متد `VerifyIAP(sku, token, store, onSuccess, onError)` برای فراخوانی `POST /api/iap/verify` با JWT و بدنه JSON.
4. **IAPManager.cs** — سینگلتون جدید با:
   - پشتیبانی شرطی `#if BAZAAR_IAP` و `#if MYKET_IAP` (با reflection تا بدون پلاگین هم کامپایل شود)
   - `RequestSkuPrices(skus)` و رویداد `SkuPricesReady` برای نمایش قیمت از SDK
   - `Purchase(platformProductId)` و رویدادهای `OnPurchaseVerifySuccess` / `OnPurchaseVerifyFailed`
   - فیلدهای کلید عمومی Bazaar و Myket در Inspector
5. **CoinPackItem.cs** — کالبک به `Action<CoinPack>`؛ فیلد قیمت (`priceText`)؛ امضای `SetCoinPack(pack, priceString, onBuy)`.
6. **GameManager.cs** — فلوی استور: هنگام باز شدن شاپ درخواست قیمت از IAPManager؛ نمایش لیست با قیمت؛ `OnCoinPackClicked(CoinPack)` و در صورت IAP فراخوانی `IAPManager.Purchase` و پس از verify به‌روزرسانی wallet.
7. **Assets/Editor/BuildScript.cs** — منوهای `Build / Build Bazaar APK` و `Build / Build Myket APK` با تعریف سمبل و مسیر خروجی.
8. **.github/workflows/build-android.yml** — workflow نمونه برای بیلد Bazaar و Myket و آپلود آرتیفکت.

---

## کارهایی که باید خودتان انجام دهید

### ۱. اضافه کردن پلاگین‌های رسمی یونیتی

- **کافه‌بازار:** از [CafebazaarUnity](https://github.com/cafebazaar/CafebazaarUnity) پلاگین را بگیرید؛ `BazaarIAB.jar` را از پوشه JavaPlugin بسازید (`gradlew createJar`) و اسکریپت‌های C# و تنظیمات AndroidManifest را طبق README اضافه کنید.
- **مایکت:** از [myket.ir](https://myket.ir/kb/pages/unity-with-gradle-fa/) پلاگین پرداخت درون‌برنامه‌ای مایکت را برای نسخه یونیتی خود (۶ یا ۲۰۲۱/۲۰۲۲) دانلود و وارد پروژه کنید. در صورت استفاده از Unity 2022.2 یا قدیمی‌تر، دو خط `namespace` و `ndkPath` را از `launcherTemplate.gradle` حذف کنید.

### ۲. قرار دادن IAPManager در صحنه

- یک GameObject در صحنه اصلی (مثلاً در کنار ApiClient) بسازید و اسکریپت `IAPManager` را به آن اضافه کنید.
- در Inspector فیلد **Api Client** را به همان ApiClient صحنه وصل کنید.
- **Bazaar Public Key** و **Myket Public Key** را از پنل توسعه‌دهنده هر استور پر کنید (برای بیلد Bazaar فقط کلید Bazaar و برای بیلد Myket فقط کلید Myket لازم است).

### ۳. نمایش قیمت در پریفت استور

- در پریفت **CoinPackItem** یک فیلد متنی (مثلاً TMP_Text) برای قیمت اضافه کنید و در اسکریپت `CoinPackItem` آن را به **Price Text** در Inspector وصل کنید. اگر از قبل فیلد قیمت دارید، فقط به همان رفرنس دهید.

### ۴. تعریف سمبل و بیلد

- برای بیلد **کافه‌بازار:** در Player Settings > Other Settings > Script Compilation > Script Define Symbols مقدار `BAZAAR_IAP` را اضافه کنید (و در صورت وجود `MYKET_IAP` را حذف کنید)، سپس از منو **Build / Build Bazaar APK** استفاده کنید.
- برای بیلد **مایکت:** به همین شکل فقط `MYKET_IAP` را قرار دهید و از **Build / Build Myket APK** استفاده کنید.
- خروجی‌ها در `Builds/Bazaar/Game_Bazaar.apk` و `Builds/Myket/Game_Myket.apk` ساخته می‌شوند.

### ۵. CI (در صورت استفاده از GitHub Actions)

- فایل `.github/workflows/build-android.yml` اضافه شده است. اگر Unity روی رانر نصب نیست، یا از [game-ci/unity-builder](https://github.com/game-ci/unity-builder) استفاده کنید یا مسیر یونیتی را در workflow با محیط خود تطبیق دهید.

### ۶. تست

- روی دستگاه اندروید با اپ استور (کافه‌بازار یا مایکت) نصب‌شده، بیلد مربوطه را اجرا کنید؛ استور را باز کنید و مطمئن شوید قیمت از SDK نمایش داده می‌شود و پس از خرید، verify سمت سرور و به‌روزرسانی کیف پول درست انجام می‌شود.

---

## نکات امنیتی و بهترین روش‌ها

- پاداش فقط بعد از پاسخ موفق `POST /api/iap/verify` داده می‌شود (در بک‌اند).
- در درخواست verify فقط JWT در هدر ارسال می‌شود؛ `userId` در بدنه نفرستید.
- از `platformProductId` برگشتی از اقتصاد (مثلاً `duodooz.currency.coin.starter`) به‌عنوان SKU برای خرید و verify استفاده می‌شود.
- در رلیز مایکت `enableLogging(false)` را در نظر بگیرید (در پلاگین مایکت).
