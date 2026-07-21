# Carpinotchi — Post-launch backlog

## Item icon art (real risk, do before any public release)
`Assets/Items/cocaine.png`, `alplax.png`, `enguaje.png`, `papita.png`, `pitusas.png` are real trademarked product photography (an actual Xanax box, Colgate Plax, Dia-brand chips, ParNor Pitusas cookies, and a bag depicted as cocaine). Item text/names/balance were reworked to drop the drug/brand references, but the icons themselves are untouched and still show the original branded photos. Needs real replacement art before this goes anywhere public. `mate.png` is fine as-is (generic, unbranded).

## FireHazard minigame
Cut from v1 (`Assets/FireHazard/`, scene `4_Fire.unity`). Not in Build Settings, no menu entry. If revived later:
- `Fire_FireParticlePool.InitializePool()` does `new Fire_Fire()` on a MonoBehaviour — illegal, errors on every load.
- `ResetGame()` never resets `buildingHealth`/`firesSpawned`/`fireSpots[]`.
- Game-over buttons have null `m_Target`.
- `Fire_FireController`, `Fire_FireControllerB`, `Fire_Fader` are orphaned, unattached scripts.
- Extinguish timing is framerate-dependent (`Mathf.CeilToInt` rounding bug in `Fire_Fire.DrainHealth`).

## Balance / tuning
- Cashier item pricing is still `Random.Range(1, 15)` per item (`Cashier_Item.cs`) — works, but not hand-tuned.
- Sickness trigger/recovery numbers and the 60-tick offline-decay cap (see STATUS.md) are first-pass guesses, need a playtest pass.
- Starting money (50) is a guess.

## Small cleanup
- `Delivery_Manager.GetCustomerSpawnPoint` silently drops a spawn (now logged) if the distance check fails — a retry loop would be nicer.
- `Pet.GetMaxValue()` / `Pet.Stat.ChangeLockState()` are dead API, never called.
- MainMenu is title + 3 buttons only, no settings/options screen.
