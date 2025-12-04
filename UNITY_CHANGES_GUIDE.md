# راهنمای تغییرات Unity - مرحله به مرحله

## 📋 فهرست تغییرات

### بخش ۱: اضافه کردن Matchmaking (بازی آنلاین)
### بخش ۲: اضافه کردن Leaderboard (جدول امتیازات)

---

## 🎮 بخش ۱: اضافه کردن Matchmaking

### مرحله ۱: اضافه کردن دکمه در Lobby Panel

1. **باز کردن Scene** اصلی (`main_dooooz.unity`)
2. **پیدا کردن Lobby Panel** در Hierarchy
3. **اضافه کردن دکمه جدید**:
   - راست کلیک روی `lobbyPanel` → UI → Button - TextMeshPro
   - نام دکمه را `PlayOnlineButton` بگذارید
   - متن دکمه را "Play Online" یا "بازی آنلاین" قرار دهید
   - دکمه را کنار `createRoomButton` و `joinRoomModeButton` قرار دهید

### مرحله ۲: اضافه کردن Panel برای Matchmaking

1. **ایجاد Panel جدید**:
   - راست کلیک روی Canvas → UI → Panel
   - نام را `MatchmakingPanel` بگذارید
   - این Panel را غیرفعال کنید (Active = false)

2. **اضافه کردن المان‌های UI**:
   - **Text (TMP)**: برای نمایش وضعیت ("Searching for opponent...")
     - نام: `MatchmakingStatusLabel`
   - **Button**: برای لغو جستجو
     - نام: `CancelMatchmakingButton`
     - متن: "Cancel" یا "لغو"

### مرحله ۳: اضافه کردن State جدید در GameManager

1. **باز کردن `GameManager.cs`**
2. **افزودن State جدید** در enum `GameState`:
```csharp
private enum GameState
{
    AuthChoice,
    AuthForm,
    Lobby,
    JoinRoom,
    WaitingForOpponent,
    Matchmaking,  // ← اضافه کنید
    InGame,
    GameFinished
}
```

3. **اضافه کردن SerializeField ها** در بالای کلاس:
```csharp
[Header("Lobby Panel")]
[SerializeField] private TMP_Text welcomeLabel;
[SerializeField] private TMP_Text playerInfoLabel;
[SerializeField] private Button createRoomButton;
[SerializeField] private Button joinRoomModeButton;
[SerializeField] private Button playOnlineButton;  // ← اضافه کنید
[SerializeField] private Button logoutButton;

[Header("Matchmaking Panel")]
[SerializeField] private GameObject matchmakingPanel;  // ← اضافه کنید
[SerializeField] private TMP_Text matchmakingStatusLabel;  // ← اضافه کنید
[SerializeField] private Button cancelMatchmakingButton;  // ← اضافه کنید
```

### مرحله ۴: اضافه کردن متدهای Matchmaking

در `GameManager.cs`، این متدها را اضافه کنید:

```csharp
// در SetupButtonListeners() اضافه کنید:
playOnlineButton?.onClick.AddListener(OnPlayOnlineClicked);
cancelMatchmakingButton?.onClick.AddListener(OnCancelMatchmakingClicked);

// متدهای جدید:
public void OnPlayOnlineClicked()
{
    if (!EnsureLoggedIn()) return;
    if (requestInFlight) return;
    
    StartCoroutine(HandleQueueMatchmaking());
}

public void OnCancelMatchmakingClicked()
{
    StartCoroutine(HandleCancelMatchmaking());
}

private IEnumerator HandleQueueMatchmaking()
{
    requestInFlight = true;
    ClearError();
    ShowLoading(true);
    SetState(GameState.Matchmaking);
    
    yield return apiClient.QueueMatchmaking(
        response =>
        {
            if (response.mode == "matched")
            {
                // بازی فوراً شروع شد
                currentRoomId = response.roomId;
                DetermineLocalSymbolFromMatchmaking(response);
                StartCoroutine(HandleFetchRoomState());
            }
            else if (response.mode == "waiting")
            {
                // در انتظار حریف
                currentRoomId = response.roomId;
                if (matchmakingStatusLabel != null)
                {
                    matchmakingStatusLabel.text = GameStrings.MatchmakingWaiting;
                }
                // شروع polling
                StartCoroutine(PollRoomStateUntilStarted());
            }
        },
        error =>
        {
            ShowError(error);
            SetState(GameState.Lobby);
        });
    
    ShowLoading(false);
    requestInFlight = false;
}

private IEnumerator HandleCancelMatchmaking()
{
    requestInFlight = true;
    ClearError();
    
    yield return apiClient.CancelMatchmaking(
        response =>
        {
            Debug.Log(response.message);
            SetState(GameState.Lobby);
        },
        error =>
        {
            ShowError(error);
        });
    
    requestInFlight = false;
}

private void DetermineLocalSymbolFromMatchmaking(MatchmakingResponse response)
{
    var playerId = apiClient?.CurrentPlayerId ?? 0;
    if (response.player1 != null && response.player1.id == playerId)
    {
        localPlayerSymbol = response.player1.symbol;
    }
    else if (response.player2 != null && response.player2.id == playerId)
    {
        localPlayerSymbol = response.player2.symbol;
    }
}
```

### مرحله ۵: به‌روزرسانی UpdateUI()

در متد `UpdateUI()` این خطوط را اضافه کنید:

```csharp
private void UpdateUI()
{
    // Panel visibility
    authChoicePanel?.SetActive(currentState == GameState.AuthChoice);
    authFormPanel?.SetActive(currentState == GameState.AuthForm);
    lobbyPanel?.SetActive(currentState == GameState.Lobby);
    joinRoomPanel?.SetActive(currentState == GameState.JoinRoom);
    waitingPanel?.SetActive(currentState == GameState.WaitingForOpponent);
    matchmakingPanel?.SetActive(currentState == GameState.Matchmaking);  // ← اضافه کنید
    inGamePanel?.SetActive(currentState == GameState.InGame);
    finishedPanel?.SetActive(currentState == GameState.GameFinished);
    
    // ... بقیه کد
}
```

### مرحله ۶: اتصال در Inspector

1. **انتخاب GameManager** در Scene
2. **در Inspector**:
   - `Play Online Button` را به `playOnlineButton` بکشید
   - `MatchmakingPanel` را به `matchmakingPanel` بکشید
   - `MatchmakingStatusLabel` را به `matchmakingStatusLabel` بکشید
   - `CancelMatchmakingButton` را به `cancelMatchmakingButton` بکشید

---

## 🏆 بخش ۲: اضافه کردن Leaderboard

### مرحله ۱: اضافه کردن دکمه‌ها در Lobby

1. **در Lobby Panel** دو دکمه اضافه کنید:
   - **دکمه Leaderboard**: "Leaderboard" یا "جدول امتیازات"
     - نام: `LeaderboardButton`
   - **دکمه My Stats**: "My Stats" یا "آمار من"
     - نام: `MyStatsButton`

### مرحله ۲: ایجاد Leaderboard Panel

1. **ایجاد Panel جدید**:
   - نام: `LeaderboardPanel`
   - غیرفعال (Active = false)

2. **اضافه کردن المان‌ها**:
   - **Text (TMP)** برای عنوان: "Leaderboard"
   - **Text (TMP)** برای فصل: `SeasonLabel` (مثلاً "Season: 2025-12")
   - **ScrollView** برای لیست بازیکنان:
     - داخل ScrollView یک **Content** با Vertical Layout Group
     - برای هر بازیکن یک **Prefab** یا **Template** ایجاد کنید
   - **Button** برای بستن: `CloseLeaderboardButton`
   - **Button** برای Refresh: `RefreshLeaderboardButton` (اختیاری)

### مرحله ۳: ایجاد My Stats Panel

1. **ایجاد Panel جدید**:
   - نام: `MyStatsPanel`
   - غیرفعال (Active = false)

2. **اضافه کردن Text (TMP) ها**:
   - `MyStatsSeasonLabel`: فصل
   - `MyStatsRankLabel`: رتبه
   - `MyStatsRatingLabel`: امتیاز
   - `MyStatsWinsLabel`: بردها
   - `MyStatsLossesLabel`: باخت‌ها
   - `MyStatsDrawsLabel`: تساوی‌ها
   - `MyStatsGamesLabel`: تعداد بازی‌ها
   - **Button** برای بستن: `CloseMyStatsButton`

### مرحله ۴: اضافه کردن State ها و SerializeField ها

در `GameManager.cs`:

```csharp
private enum GameState
{
    AuthChoice,
    AuthForm,
    Lobby,
    JoinRoom,
    WaitingForOpponent,
    Matchmaking,
    InGame,
    GameFinished,
    Leaderboard,  // ← اضافه کنید
    MyStats       // ← اضافه کنید
}

// اضافه کردن SerializeField ها:
[Header("Lobby Panel")]
// ... موجود
[SerializeField] private Button leaderboardButton;  // ← اضافه کنید
[SerializeField] private Button myStatsButton;  // ← اضافه کنید

[Header("Leaderboard Panel")]
[SerializeField] private GameObject leaderboardPanel;  // ← اضافه کنید
[SerializeField] private TMP_Text seasonLabel;  // ← اضافه کنید
[SerializeField] private Transform leaderboardContent;  // ← برای لیست بازیکنان
[SerializeField] private GameObject leaderboardItemPrefab;  // ← Prefab برای هر بازیکن
[SerializeField] private Button closeLeaderboardButton;  // ← اضافه کنید
[SerializeField] private Button refreshLeaderboardButton;  // ← اضافه کنید

[Header("My Stats Panel")]
[SerializeField] private GameObject myStatsPanel;  // ← اضافه کنید
[SerializeField] private TMP_Text myStatsSeasonLabel;  // ← اضافه کنید
[SerializeField] private TMP_Text myStatsRankLabel;  // ← اضافه کنید
[SerializeField] private TMP_Text myStatsRatingLabel;  // ← اضافه کنید
[SerializeField] private TMP_Text myStatsWinsLabel;  // ← اضافه کنید
[SerializeField] private TMP_Text myStatsLossesLabel;  // ← اضافه کنید
[SerializeField] private TMP_Text myStatsDrawsLabel;  // ← اضافه کنید
[SerializeField] private TMP_Text myStatsGamesLabel;  // ← اضافه کنید
[SerializeField] private Button closeMyStatsButton;  // ← اضافه کنید
```

### مرحله ۵: اضافه کردن متدهای Leaderboard

```csharp
// در SetupButtonListeners():
leaderboardButton?.onClick.AddListener(OnLeaderboardClicked);
myStatsButton?.onClick.AddListener(OnMyStatsClicked);
closeLeaderboardButton?.onClick.AddListener(OnCloseLeaderboard);
closeMyStatsButton?.onClick.AddListener(OnCloseMyStats);
refreshLeaderboardButton?.onClick.AddListener(OnRefreshLeaderboard);

// متدهای جدید:
public void OnLeaderboardClicked()
{
    if (!EnsureLoggedIn()) return;
    SetState(GameState.Leaderboard);
    LoadLeaderboard();
}

public void OnMyStatsClicked()
{
    if (!EnsureLoggedIn()) return;
    SetState(GameState.MyStats);
    LoadMyStats();
}

public void OnCloseLeaderboard()
{
    SetState(GameState.Lobby);
}

public void OnCloseMyStats()
{
    SetState(GameState.Lobby);
}

public void OnRefreshLeaderboard()
{
    LoadLeaderboard();
}

private void LoadLeaderboard()
{
    // استفاده از فصل فعلی (می‌توانید بعداً dropdown اضافه کنید)
    string currentSeason = System.DateTime.Now.ToString("yyyy-MM");
    StartCoroutine(HandleLoadLeaderboard(currentSeason, 50));
}

private IEnumerator HandleLoadLeaderboard(string season, int limit)
{
    ShowLoading(true);
    ClearError();
    
    yield return apiClient.GetLeaderboard(season, limit,
        response =>
        {
            ShowLoading(false);
            DisplayLeaderboard(response);
        },
        error =>
        {
            ShowLoading(false);
            ShowError(error);
        });
}

private void DisplayLeaderboard(LeaderboardResponse response)
{
    if (seasonLabel != null)
    {
        seasonLabel.text = string.Format(GameStrings.SeasonFormat, response.season);
    }
    
    // پاک کردن لیست قبلی
    if (leaderboardContent != null)
    {
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }
    }
    
    // اضافه کردن بازیکنان
    if (response.players != null && leaderboardContent != null)
    {
        foreach (var player in response.players)
        {
            if (leaderboardItemPrefab != null)
            {
                var item = Instantiate(leaderboardItemPrefab, leaderboardContent);
                // تنظیم اطلاعات بازیکن در item
                // (باید یک script برای leaderboard item بنویسید)
                var itemScript = item.GetComponent<LeaderboardItem>();
                if (itemScript != null)
                {
                    itemScript.SetPlayer(player);
                }
            }
        }
    }
}

private void LoadMyStats()
{
    string currentSeason = System.DateTime.Now.ToString("yyyy-MM");
    StartCoroutine(HandleLoadMyStats(currentSeason));
}

private IEnumerator HandleLoadMyStats(string season)
{
    ShowLoading(true);
    ClearError();
    
    yield return apiClient.GetMyStats(season,
        response =>
        {
            ShowLoading(false);
            DisplayMyStats(response);
        },
        error =>
        {
            ShowLoading(false);
            ShowError(error);
        });
}

private void DisplayMyStats(MyStatsResponse response)
{
    if (myStatsSeasonLabel != null)
    {
        myStatsSeasonLabel.text = string.Format(GameStrings.SeasonFormat, response.season);
    }
    
    if (myStatsRankLabel != null)
    {
        myStatsRankLabel.text = response.rank >= 0 
            ? string.Format(GameStrings.RankFormat, response.rank)
            : GameStrings.NoRank;
    }
    
    if (myStatsRatingLabel != null)
    {
        myStatsRatingLabel.text = response.rating >= 0
            ? string.Format(GameStrings.RatingFormat, response.rating)
            : GameStrings.NoRating;
    }
    
    if (myStatsWinsLabel != null)
    {
        myStatsWinsLabel.text = string.Format(GameStrings.WinsFormat, response.wins);
    }
    
    if (myStatsLossesLabel != null)
    {
        myStatsLossesLabel.text = string.Format(GameStrings.LossesFormat, response.losses);
    }
    
    if (myStatsDrawsLabel != null)
    {
        myStatsDrawsLabel.text = string.Format(GameStrings.DrawsFormat, response.draws);
    }
    
    if (myStatsGamesLabel != null)
    {
        myStatsGamesLabel.text = string.Format(GameStrings.GamesPlayedFormat, response.gamesPlayed);
    }
}
```

### مرحله ۶: به‌روزرسانی UpdateUI()

```csharp
private void UpdateUI()
{
    // Panel visibility
    // ... موجود
    leaderboardPanel?.SetActive(currentState == GameState.Leaderboard);  // ← اضافه کنید
    myStatsPanel?.SetActive(currentState == GameState.MyStats);  // ← اضافه کنید
    // ... بقیه
}
```

### مرحله ۷: ایجاد Leaderboard Item Prefab (اختیاری)

1. **ایجاد Prefab** برای هر آیتم لیست:
   - راست کلیک → Create → UI → Panel
   - نام: `LeaderboardItem`
   - اضافه کردن Text (TMP) ها:
     - Rank (رتبه)
     - Nickname (نام)
     - Rating (امتیاز)
     - Wins/Losses/Draws (اختیاری)

2. **ایجاد Script** برای LeaderboardItem:
```csharp
using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text ratingText;
    
    public void SetPlayer(LeaderboardPlayer player)
    {
        if (rankText != null) rankText.text = player.rank.ToString();
        if (nicknameText != null) nicknameText.text = player.nickname;
        if (ratingText != null) ratingText.text = player.rating.ToString();
    }
}
```

3. **ذخیره به عنوان Prefab** و اتصال به `leaderboardItemPrefab` در GameManager

### مرحله ۸: اتصال در Inspector

تمام المان‌های جدید را در Inspector به GameManager متصل کنید:
- دکمه‌های Lobby
- Panel ها
- Label ها
- Button ها
- Content برای لیست

---

## ✅ چک‌لیست نهایی

- [ ] دکمه Play Online در Lobby اضافه شده
- [ ] Matchmaking Panel ایجاد شده
- [ ] State Matchmaking اضافه شده
- [ ] متدهای Matchmaking پیاده‌سازی شده
- [ ] دکمه‌های Leaderboard و My Stats اضافه شده
- [ ] Leaderboard Panel ایجاد شده
- [ ] My Stats Panel ایجاد شده
- [ ] State های Leaderboard و My Stats اضافه شده
- [ ] متدهای Leaderboard پیاده‌سازی شده
- [ ] تمام المان‌ها در Inspector متصل شده‌اند
- [ ] تست شده و کار می‌کند

---

## 💡 نکات مهم

1. **Polling**: Matchmaking به صورت خودکار room را poll می‌کند (مثل WaitingForOpponent)
2. **Nullable Values**: برای `rating` و `rank` که ممکن است null باشند، از `-1` استفاده شده
3. **Season Format**: فرمت فصل باید `YYYY-MM` باشد (مثلاً "2025-12")
4. **Error Handling**: تمام متدها error handling دارند
