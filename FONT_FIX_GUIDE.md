# راهنمای Fix کردن فونت‌های TMP_Text

## مشکل
بعضی از متن‌های TMP_Text در Unity فونت ندارند و به صورت مربع یا خطوط نمایش داده می‌شوند.

## راه حل

### روش 1: استفاده از Editor Script (توصیه می‌شود)

1. در Unity Editor، به منوی `Tools` بروید
2. روی `Fix TMP Fonts` کلیک کنید
3. یک پنجره باز می‌شود که می‌توانید:
   - فونت default را ببینید (از TMP Settings یا IRANSans SDF)
   - روی `Fix Fonts in Current Scene` کلیک کنید تا فونت‌های missing در scene فعلی fix شوند
   - روی `Fix Fonts in All Scenes` کلیک کنید تا همه scene ها fix شوند
   - روی `Fix Fonts in Prefabs` کلیک کنید تا prefab ها fix شوند

### روش 2: استفاده از Runtime Script

اگر می‌خواهید فونت‌ها در runtime fix شوند:

1. یک GameObject خالی در scene ایجاد کنید
2. Component `FontHelper` را به آن اضافه کنید
3. (اختیاری) فونت default را در Inspector تنظیم کنید
4. این script در `Awake()` همه TMP_Text components را در children پیدا می‌کند و فونت default را تنظیم می‌کند

یا می‌توانید از `GameManager` استفاده کنید:

```csharp
// در GameManager.Awake() یا Start()
FontHelper.FixAllFontsInScene();
```

### روش 3: تنظیم دستی در Inspector

1. هر TMP_Text component را انتخاب کنید
2. در Inspector، فیلد `Font Asset` را پیدا کنید
3. فونت `IRANSans SDF` یا `LiberationSans SDF` را drag & drop کنید

## فونت‌های موجود

- **IRANSans SDF**: فونت فارسی که در `Assets/Fonts/IRANSans SDF.asset` قرار دارد
- **LiberationSans SDF**: فونت default از TextMesh Pro در `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset`

## نکات مهم

1. **Editor Script** فقط در Unity Editor کار می‌کند و برای fix کردن scene ها و prefab ها استفاده می‌شود
2. **Runtime Script** در بازی اجرا می‌شود و می‌تواند فونت‌های missing را fix کند
3. بهتر است از **Editor Script** استفاده کنید تا فونت‌ها در scene و prefab ها ذخیره شوند
4. اگر فونت default در TMP Settings تنظیم شده باشد، script ها از آن استفاده می‌کنند

## تنظیم TMP Settings

برای تنظیم فونت default در TMP Settings:

1. به `Window > TextMeshPro > Settings` بروید
2. در `Default Font Asset`، فونت مورد نظر را تنظیم کنید
3. این فونت به صورت خودکار برای همه TMP_Text components جدید استفاده می‌شود
