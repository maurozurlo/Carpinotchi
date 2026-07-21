# Carpinotchi — Ship Status

## Flow

MainMenu -> (New Game / Continue) -> House_00 -> Cashier / Delivery -> back to House_00.
FireHazard exists in the project (`Assets/FireHazard/`) but is not in Build Settings and has no menu entry. Cut for v1, see BACKLOG.md.

## Persistence

Only data holders persist — no UI. `Pet` (stats + `isSick`), `TimeManager` (decay coroutine, no UI refs), `SaveManager` and `MoneyManager` (self-healing lookup of `MoneyDisplay` via `GameObject.Find`, no serialized UI ref) live on `GameController`/their own objects with `DontDestroyOnLoad` + a `control` singleton guard.

`UIManager` was pulled off `GameController` onto its own scene-local `StatsUI` object precisely so it does **not** persist — it holds direct Inspector references to the House_00 Canvas's meter Images, and those would go stale the moment Canvas got destroyed/recreated by a scene load. `ItemManager`, `ShopManager`, `MinigameManager`, and the `Canvas` prefab instance are all scene-local too, recreated fresh every time House_00 loads, so their mutual Inspector wiring is always internally consistent — no persistent-but-hidden UI tree needed.

`Pet.UpdateUI()` sends `"UpdateStatsUI"` with `SendMessageOptions.DontRequireReceiver`, since `TimeManager`'s decay coroutine keeps running (and can call it) even while `UIManager` doesn't exist, i.e. mid-minigame.

Save file: `Application.persistentDataPath/save.json`, written by `SaveManager` (`Assets/Scripts/SaveManager.cs`). Saves on: quit, app pause, right when a minigame's `GameOver()` awards money (so `GoBackHome()` — which bypasses `MinigameManager` — can't return to a stale save), before `MinigameManager.LoadScene()` transitions, and once per `TimeManager` decay tick (free periodic autosave). Loads on every `House_00` scene load (via `SceneManager.sceneLoaded`, not just once at boot), which is what lets `ItemManager`/`UIManager` rebuild themselves from persisted data each time they're freshly recreated. `SaveGame()` preserves the last-known inventory instead of wiping it if it fires while `ItemManager` doesn't exist (mid-minigame).

Save schema (`SaveData`): `energy`, `hunger`, `sanity`, `hygiene`, `money`, `isSick`, `lastSavedUtc` (ISO 8601 UTC), parallel `itemIds`/`itemQtys` lists (JsonUtility can't serialize dictionaries).

## Offline decay / neglect

`TimeManager.hourLength = 3600` (1 real hour per -1 stat tick, -2 while sick). On load, `SaveManager` computes elapsed real time since `lastSavedUtc`, converts to ticks, caps at 60, and applies them synchronously before the live coroutine resumes.

Sickness (`Pet.isSick`): triggers after 3 consecutive decay ticks with any of sanity/hunger/hygiene at 0. While sick, decay doubles. Clears after 2 consecutive ticks with all 4 stats at >=50%. No permadeath, no gating of shop/minigames — pure stat-pressure mechanic. These numbers (3 ticks to trigger, 50%/2 ticks to recover, 60-tick offline cap) are starting points, not final — retune after playtesting.

## Economy

Starting money on a fresh save: 50. Cashier and Delivery both call `MoneyManager.WinMoney()` from their `GameOver()`. `MoneyManager.UpdateGraphics()` no-ops if there's no `MoneyDisplay` in the scene (only exists in House_00), so it's safe to call from inside a minigame scene.

## Known-safe build path

Unity 2022.3.62f3, Standalone Windows64, Mono scripting backend (default, left unchanged). `Assets/Editor/BuildScript.cs` exposes `Build > Build Windows` in the Editor menu (and a batch-mode-callable `BuildScript.BuildWindows`) — outputs to `Builds/Windows/Carpinotchi.exe` using whatever scenes are enabled in Build Settings.
