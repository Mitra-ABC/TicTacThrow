# duodooz — Project Documentation

| Document | Description |
|----------|-------------|
| [SETUP.md](SETUP.md) | Scene hierarchy and GameManager Inspector wiring |
| [UI_SETUP.md](UI_SETUP.md) | Store, Wallet, and Lobby UI setup in the Editor |
| [IAP.md](IAP.md) | Cafe Bazaar / Myket in-app purchase checklist |
| [WEBSOCKET.md](WEBSOCKET.md) | Socket.IO client setup and testing |
| [API_REFERENCE.md](API_REFERENCE.md) | Backend API and economy reference |

## Project layout

```
Assets/
├── Scenes/main_dooooz.unity   # Main game scene
├── Scripts/                   # Game code
├── Prefabs/UI/                # UI prefabs
├── Sprites/                   # Art assets
├── Fonts/                     # IRANSans
├── Configs/                   # ApiConfig ScriptableObject
├── Editor/                    # BuildScript (Bazaar/Myket APK)
├── Plugins/Android/           # Gradle templates
└── Bazaar/                    # Poolakey IAP SDK
```
