using System;

public static class GameStrings
{
    public const string UsernameRequired = "نام کاربری را وارد کنید.";
    public const string PasswordRequired = "رمز عبور را وارد کنید.";
    public const string PasswordTooShort = "رمز عبور باید حداقل ۴ حرف باشد.";
    public const string LoginFailed = "ورود ناموفق بود. اطلاعات را بررسی کنید.";
    public const string RegistrationFailed = "ثبت‌نام انجام نشد. دوباره تلاش کنید.";
    public const string UsernameExists = "این نام کاربری قبلاً ثبت شده.";
    public const string NotLoggedIn = "لطفاً اول وارد شوید.";
    public const string SessionExpired = "نشست شما تمام شد. دوباره وارد شوید.";
    public const string LoggingIn = "در حال ورود...";
    public const string Registering = "در حال ثبت‌نام...";

    public const string WaitingForOpponent = "در انتظار حریف...";
    public const string ShareRoomFormat = "این شناسه را برای دوستت بفرست: {0}";
    public const string PlayerInfoFormat = "{0} (شناسه: {1})";
    public const string PlayerInfoPlaceholder = "وارد نشده‌اید";
    public const string WelcomeFormat = "خوش آمدی، {0}!";
    public const string WelcomeGuest = "خوش آمدی";
    public const string WelcomeTitle = "به دوووز\nخوش آمدید";
    public const string PlayerFallbackFormat = "بازیکن {0}";
    public const string RoomInfoFormat = "اتاق: {0}";
    public const string RoomInfoPlaceholder = "وارد اتاقی نشده‌اید";
    public const string CreatingRoom = "در حال ساخت اتاق...";
    public const string JoiningRoom = "در حال ورود به اتاق...";

    public const string PlayerNamesFormat = "بازیکن ۱: {0}\nبازیکن ۲: {1}";
    public const string UnknownPlayer = "در انتظار...";
    public const string UnknownNickname = "ناشناس";
    public const string StatusFormat = "وضعیت: {0}";
    public const string StatusUnknown = "نامشخص";
    public const string StatusWaitingDisplay = "در انتظار";
    public const string StatusPlayingDisplay = "در حال بازی";
    public const string StatusFinishedDisplay = "تمام شده";
    public const string YourTurn = "نوبت شما";
    public const string OpponentTurn = "نوبت حریف";
    public const string ErrorPrefix = "خطا: ";
    public const string Draw = "مساوی!";
    public const string YouWin = "بردی!";
    public const string YouLose = "باختی!";
    public const string ResultUnknown = "بازی تمام شد";

    public const string NicknameRequired = "نام نمایشی را وارد کنید.";
    public const string JoinRoomIdRequired = "کد اتاق را وارد کنید.";
    public const string JoinRoomIdInvalid = "کد اتاق باید ۶ رقم باشد.";
    public const int RoomCodeLength = 6;
    public const string PlayerNotCreated = "لطفاً اول وارد شوید.";
    public const string WsNotReady = "در حال اتصال... کمی صبر کنید.";
    public const string WsDisconnected = "اتصال بازی قطع است. کمی صبر کنید.";
    public const string InvalidMatchRoom = "اتاق مسابقه ساخته نشد. دوباره تلاش کنید.";
    public const string MatchFoundWaiting = "حریف پیدا شد. در حال شروع بازی...";

    public const string StatusWaiting = "waiting";
    public const string StatusInProgress = "in_progress";
    public const string StatusFinished = "finished";

    public const string SymbolX = "X";
    public const string SymbolO = "O";
    public const string ResultDraw = "draw";

    public const string QueueMatchmaking = "ورود به صف";
    public const string MatchmakingTitle = "جستجوی حریف";
    public const string CancelMatchmaking = "لغو جستجو";
    public const string SearchingForOpponent = "در حال پیدا کردن حریف...";
    public const string MatchmakingWaiting = "در انتظار حریف...";
    public const string MatchmakingMatched = "حریف پیدا شد!";
    public const string MatchmakingCancelled = "جستجو لغو شد";
    public const string MatchmakingFailed = "جستجوی حریف ناموفق بود";

    public const string LeaderboardTitle = "جدول امتیازات";
    public const string MyStatsTitle = "آمار من";
    public const string SeasonFormat = "فصل: {0}";
    public const string RankFormat = "رتبه: {0}";
    public const string RatingFormat = "امتیاز: {0}";
    public const string WinsFormat = "برد: {0}";
    public const string LossesFormat = "باخت: {0}";
    public const string DrawsFormat = "مساوی: {0}";
    public const string GamesPlayedFormat = "تعداد بازی: {0}";
    public const string NoRating = "هنوز امتیازی ندارید";
    public const string NoRank = "بدون رتبه";
    public const string LoadingLeaderboard = "در حال بارگذاری جدول...";
    public const string LoadingStats = "در حال بارگذاری آمار...";
    public const string LeaderboardError = "جدول امتیازات بارگذاری نشد";
    public const string StatsError = "آمار بارگذاری نشد";

    public const string CoinsFormat = "سکه: {0}";
    public const string HeartsFormat = "قلب: {0}/{1}";
    public const string PlayWithFriendsButton = "بازی با دوستان";
    public const string LobbyRankButton = "رتبه‌بندی";
    public const string LobbyBoostersButton = "تقویت‌کننده‌ها";
    public const string NextHeartFormat = "قلب بعدی: {0}";
    public const string HeartsFull = "قلب‌ها پر است";
    public const string NotEnoughCoins = "سکه کافی ندارید";
    public const string NotEnoughHearts = "قلب کافی ندارید";
    public const string HeartsAtMax = "قلب‌های شما پر است.";
    public const string LoadingWallet = "در حال بارگذاری کیف پول...";
    public const string WalletError = "کیف پول بارگذاری نشد";

    public const string StoreTitle = "فروشگاه";
    public const string BuyHeartButton = "خرید قلب";
    public const string BuyBoosterButton = "خرید بوستر";
    public const string HeartPriceFormat = "قیمت: {0} سکه";
    public const string BoosterPriceFormat = "قیمت: {0} سکه";
    public const string BoosterDurationFormat = "مدت: {0} دقیقه";
    public const string BuySuccess = "خرید با موفقیت انجام شد.";
    public const string BuyFailed = "خرید انجام نشد";
    public const string LoadingStore = "در حال بارگذاری فروشگاه...";
    public const string StoreError = "فروشگاه بارگذاری نشد";
    public const string BoostersError = "بوسترها بارگذاری نشد";
    public const string CoinPackMissing = "بسته سکه پیدا نشد.";
    public const string BillingNotReady = "فروشگاه هنوز آماده نیست. کمی صبر کنید.";
    public const string PurchaseFailed = "خرید انجام نشد.";
    public const string IapDisabled = "خرید فقط از نسخهٔ استور امکان‌پذیر است.";
    public const string VerifyFailed = "تأیید خرید انجام نشد.";
    public const string InvalidProduct = "محصول نامعتبر است.";
    public const string CoinPackFallback = "بسته سکه";
    public const string CoinBonusFormat = "+{0} سکه جایزه";
    public const string BuyButton = "خرید";

    public const string BoostersTitle = "بوسترها";
    public const string ActiveBoosters = "بوسترهای فعال";
    public const string NoActiveBoosters = "بوستر فعالی ندارید";
    public const string BoosterExpiresFormat = "انقضا: {0}";
    public const string BoosterExpired = "منقضی شد";
    public const string BoosterTimeRemainingFormat = "مانده: {0}";

    public const string NoHeartsTitle = "قلبی باقی نمانده";
    public const string NoHeartsMessage = "برای بازی به قلب نیاز دارید. با سکه بخرید؟";
    public const string NoHeartsMessageWithPrice = "برای بازی به قلب نیاز دارید. یکی را با {0} سکه بخرید؟";
    public const string NoHeartsBuyButton = "خرید قلب";
    public const string NoHeartsCancelButton = "انصراف";

    public const string AuthTagline = "بازی کن، برنده شو، بالا بیا!";
    public const string AuthVersion = "نسخه ۱.۰";
    public const string AuthEnterGame = "ورود به بازی";
    public const string AuthSwitchToRegister = "حساب کاربری ندارید؟ <color=#FFD24A>ثبت‌نام</color>";
    public const string AuthSwitchToLogin = "حساب دارید؟ <color=#FFD24A>ورود</color>";
    public const string LoginButton = "ورود";
    public const string RegisterButton = "ثبت‌نام";
    public const string LogoutButton = "خروج";
    public const string CreateRoomButton = "ساخت اتاق";
    public const string CreateRoomHint = "یک اتاق خصوصی بساز";
    public const string JoinRoomButton = "ورود به اتاق";
    public const string JoinRoomHint = "با کد دوستت وارد شو";
    public const string JoinRoomInstruction = "کد اتاقی را که دوستت فرستاده وارد کن";
    public const string JoinRoomNumbersOnly = "کد اتاق ۶ رقمی است";
    public const string RoomCreatedTitle = "اتاق ساخته شد";
    public const string WaitingForFriend = "در انتظار پیوستن دوست شما...";
    public const string YourRoomCode = "کد اتاق شما";
    public const string CopyButton = "کپی";
    public const string ShareButton = "اشتراک‌گذاری";
    public const string CancelRoomButton = "لغو اتاق";
    public const string RoomCodeCopied = "کد اتاق کپی شد.";
    public const string JoinButton = "ورود";
    public const string BackButton = "بازگشت";
    public const string BackToLobbyButton = "بازگشت به لابی";
    public const string CloseButton = "بستن";
    public const string CancelButton = "انصراف";
    public const string PlayAgainButton = "بازی دوباره";
    public const string PlayOnlineButton = "بازی آنلاین";
    public const string FriendlyMatchButton = "بازی دوستانه";
    public const string WithFriends = "با دوستان";
    public const string LeaderboardButton = "جدول امتیازات";
    public const string MyStatsButton = "آمار من";
    public const string RefreshButton = "تازه‌سازی";
    public const string StoreButton = "فروشگاه";
    public const string BoostersButton = "بوسترها";
    public const string LobbyTitle = "لابی";
    public const string Loading = "در حال بارگذاری...";
    public const string UsernamePlaceholder = "نام کاربری";
    public const string PasswordPlaceholder = "رمز عبور";
    public const string NicknamePlaceholder = "نام نمایشی";
    public const string RoomIdPlaceholder = "شناسه اتاق";

    public static string FormatRemaining(TimeSpan remaining)
    {
        if (remaining.TotalHours >= 1)
            return string.Format(BoosterTimeRemainingFormat, $"{(int)remaining.TotalHours} ساعت و {remaining.Minutes} دقیقه");
        if (remaining.TotalMinutes >= 1)
            return string.Format(BoosterTimeRemainingFormat, $"{remaining.Minutes} دقیقه و {remaining.Seconds} ثانیه");
        return string.Format(BoosterTimeRemainingFormat, $"{remaining.Seconds} ثانیه");
    }

    public static string FormatLobbyCoins(int coins)
    {
        return ToPersianDigits(WithThousands(coins));
    }

    public static string FormatLobbyHearts(int hearts, int maxHearts)
    {
        return ToPersianDigits(hearts + "/" + maxHearts);
    }

    public static string FormatLobbyScore(int score)
    {
        return ToPersianDigits(WithThousands(score));
    }

    public static string NormalizeRoomCode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var chars = new char[value.Length];
        var count = 0;
        foreach (var c in value)
        {
            var digit = ToWesternDigit(c);
            if (digit != '\0')
                chars[count++] = digit;
        }

        return new string(chars, 0, count);
    }

    public static string PadRoomCode(int roomId)
    {
        return roomId > 0 ? roomId.ToString().PadLeft(RoomCodeLength, '0') : string.Empty;
    }

    public static string PadRoomCode(string code)
    {
        var digits = NormalizeRoomCode(code);
        if (string.IsNullOrEmpty(digits))
            return string.Empty;
        return digits.Length >= RoomCodeLength
            ? digits.Substring(0, RoomCodeLength)
            : digits.PadLeft(RoomCodeLength, '0');
    }

    public static string FormatRoomCode(string code)
    {
        return ToPersianDigits(PadRoomCode(code));
    }

    public static char ToWesternDigit(char c)
    {
        if (c >= '0' && c <= '9')
            return c;
        if (c >= '۰' && c <= '۹')
            return (char)('0' + (c - '۰'));
        if (c >= '٠' && c <= '٩')
            return (char)('0' + (c - '٠'));
        return '\0';
    }

    public static string ToPersianDigits(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (chars[i] >= '0' && chars[i] <= '9')
                chars[i] = (char)('۰' + (chars[i] - '0'));
            else if (chars[i] == ',')
                chars[i] = '٬';
        }

        return new string(chars);
    }

    private static string WithThousands(int value)
    {
        var raw = Math.Abs(value).ToString();
        var sb = new System.Text.StringBuilder(raw.Length + raw.Length / 3 + 1);
        if (value < 0)
            sb.Append('-');
        for (var i = 0; i < raw.Length; i++)
        {
            if (i > 0 && (raw.Length - i) % 3 == 0)
                sb.Append(',');
            sb.Append(raw[i]);
        }

        return sb.ToString();
    }
}
