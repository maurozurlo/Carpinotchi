# Carpinotchi — Design Review

Scope: a design-only pass over the current build — core loop, pacing/balance, the two minigames, and tone. No code changes made or suggested as diffs; where I recommend a fix I'm describing the desired behavior, not the implementation. Cross-referenced against `Docs/STATUS.md` and `Docs/BACKLOG.md` so I'm not re-flagging things already tracked there (icon art, FireHazard cut, Cashier pricing being unhand-tuned, no settings screen) — I only add new opinion on top of those where I have one.

## Executive summary

Three things matter more than anything else below:

1. **The pet never visibly reacts to its own state.** `PetController.cs` already has a `Mood` enum (happy/sad/crazy/thirsty/hungry) and a `DisplayMood()` that fires an Animator trigger — and nothing in the project calls it. Zero references anywhere, including the scenes. For a Tamagotchi-genre game, the payoff of feeding/cleaning/comforting your pet *is* watching it visibly respond. Right now that payoff doesn't exist; you just watch four bars move. This is the single highest-leverage fix available, and the code is already sitting there half-built.
2. **Sickness is invisible.** `Pet.isSick` drives real mechanical weight (decay doubles, `TimeManager.cs:36`) but has no UI, icon, animation, or text anywhere in the project (confirmed by grep — the only references are the flag itself, the save schema, and the decay-rate check). A mechanic with a real cost and a real recovery condition that the player can't see or understand isn't a soft mechanic, it's a hidden one.
3. **One item is badly mis-costed.** Galletitas Rellenas is the most expensive consumable in the shop (price 100 — 5x the next-priciest item) and has a *net negative* stat effect (sanity -10, hygiene -2, energy +1, hunger +5 → net -6). The 2-cost Mate is a better purchase in every respect. This is the one number I'd fix before anyone plays this build.

Everything else below is either supporting detail for these three, or second-tier polish.

## 1. Core loop assessment

The loop as built: stats decay in real time → shop sells consumables that trade stats against each other → two minigames convert 2-4 minutes of skill-based play into money → money buys more consumables. That's a complete, legible resource loop and it's the right scope for a fast-shipped cozy game — I wouldn't add a third system to it right now.

What's actually compelling about it today:
- The core tension is legible at a glance (four bars, one currency) — no menu-diving required to understand your situation.
- Both minigames bank earnings incrementally and neither one wipes your progress on failure (`Cashier_Manager.GameOver()` and `Delivery_Manager.GameOver()` both call `WinMoney` unconditionally, even after a loss). That's the right instinct for a cozy game — losing should cost you *momentum*, not *possessions*.
- The item economy has real, readable trade-offs (see §2) rather than being pure stat-up-for-money — Cafe Cargado trading hunger/sanity for energy is a nice bit of implicit storytelling (coffee jitters) without a word of dialogue.

What's missing for someone to want to open this again tomorrow, in rough priority order:
- **No feedback loop from stats to pet behavior/appearance** (see summary #1). This is the loop's whole emotional engine and it's currently unplugged.
- **No visible sickness state** (summary #2) — the one mechanic designed to create urgency is silent.
- **Nothing accumulates except a number.** Money buys consumables you immediately burn; there are no cosmetic unlocks, no new furniture, no pet growth stages, no minigame content that changes over a play history. After the first few sessions there's no new *content* to reach for, only the same numbers going up and down. Fine for a v1 scope, but it's the reason I'd expect engagement to fall off around day 3-4 rather than day 10.
- **No daily-return hook.** There's no day counter, streak, or login reward, and — because offline decay caps at 60 ticks (~2.5 days, see §2) — there's *no penalty* for skipping up to two and a half days versus checking in every few hours. A cozy game doesn't need punishing FOMO mechanics, but right now there's also no *positive* reason (a streak bonus, a daily freebie, a "your pet missed you") to prefer checking in daily over checking in every third day. The system is neutral on cadence when it could be gently rewarding it.
- **Two minigames, zero run-to-run variation.** Same batch tables, same 8-block course, same layout every time (see §3). This will likely wear out faster than the house-care loop does, precisely because minigames are usually the *replay* engine in this genre.

## 2. Pacing / balance opinion

### Decay rate (`hourLength = 3600`)

At -1/stat/hour, a stat at 100 takes ~4.2 days of total neglect to bottom out; a stat at 50 takes ~2.1 days. That's a reasonable "check in about once a day" cadence for a desktop cozy game — it's not punishing, and it's slow enough that missing a day doesn't feel like a crisis. I don't think this number needs to move.

What I would change: **new-game starting stats are `energy 60, hygiene 50, sanity 90, hunger 80`** (`Pet.cs:54-60`), not a clean 100/100/100/100. Hygiene alone reaches 0 in ~50 hours of *total inaction from the moment a brand-new save is created* — meaning a first-time player who doesn't immediately understand the systems (there is no tutorial anywhere in the project — I checked) can find their pet already sick before they've learned what any of the four bars mean. I'd start new saves at full stats so day one is a clean slate, and let the countdown begin only once the player has actually done something.

### Sickness thresholds (3-tick trigger / 50%-for-2-ticks recovery)

The trigger and the recovery condition are asymmetric in a way that matters: the trigger only requires **one** of sanity/hunger/hygiene (not energy — `Pet.cs:76` explicitly excludes energy from the neglect check) to sit at 0 for 3 hours. Recovery requires **all four** stats, energy included, above 50% simultaneously for 2 straight hours. Easy to fall in, comparatively hard to climb out — that's a defensible "cozy but has teeth" curve on its own.

Where it breaks down is the interaction with the item economy: two of the six shop items (Papitas, Galletitas Rellenas) *reduce* sanity while restoring hunger. A sick pet whose owner buys the hunger-fixing item is actively working against the "all four ≥50%" recovery condition. There is currently no item that touches all four stats positively, i.e. no "emergency escape valve" for a sick pet. I'd either soften negative side-effects specifically while `isSick` is true, or add one mild all-positive item (a proper home-cooked meal, thematically) as a costlier but always-safe recovery purchase.

I'd also just point out the excluded-energy asymmetry explicitly, since it's the kind of thing that's easy to have been unintentional: you can run energy to 0 forever and never trigger sickness from it, only from the other three. If that's deliberate (tiredness ≠ neglect, in the way dirty/hungry/sad are), fine — just confirm it's a choice and not an oversight, because it meaningfully changes which stat a player can safely ignore.

### Offline decay cap (60 ticks)

60 hours (~2.5 days) as a ceiling on how much a long absence can hurt you is the right instinct — it stops a month away from being catastrophic. My objection isn't the number, it's the *presentation*: `SaveManager.ApplyOfflineDecay()` runs its catch-up loop silently on scene load, with no summary screen. A player who comes back after a weekend can open the game to a pet that's already been sick for however many of those 60 ticks it took to trigger sickness, with zero explanation of what happened while they were away. I'd add a lightweight "while you were away" recap modal — this is squarely a design ask, not a numbers change, but it's the cheapest way to make the offline-decay system read as considerate rather than as an unexplained gut-punch on load.

### Starting money (50)

A new save can afford two Cafe Cargados (40), five Jabon Perfumados (50), or twenty-five Mates — but can't touch Galletitas Rellenas (100) at all. Read generously, this is "your first session is about learning you need to go earn money," which is a fine intent. But it only works if the game actually points you at the minigames, and nothing does — no tutorial, no nudge, no in-fiction hint. I'd keep 50 as the number but pair it with even one line of guidance on a fresh save ("cash is tight — a delivery run or a cashier shift would help").

### Item pricing & effects

| Item | Price | Sanity | Hygiene | Energy | Hunger | Net Δ | Net/Price |
|---|---|---|---|---|---|---|---|
| Mate | 2 | -5 | 0 | +2 | +5 | +2 | 1.0 |
| Te de Tilo | 5 | +10 | 0 | -5 | 0 | +5 | 1.0 |
| Jabon Perfumado | 10 | 0 | +10 | 0 | 0 | +10 | 1.0 |
| Papitas | 15 | -15 | -5 | +5 | +10 | **-5** | -0.33 |
| Cafe Cargado | 20 | -10 | 0 | +20 | -10 | 0 | 0.0 |
| Galletitas Rellenas | 100 | -10 | -2 | +1 | +5 | **-6** | -0.06 |

Four of six items (Mate, Te de Tilo, Jabon Perfumado, Cafe Cargado) follow a clean, legible pattern: net delta is either a consistent 1:1 with price, or exactly 0 for the one item that's explicitly a stat *transfer* rather than a stat *gain* (Cafe Cargado — trade sanity/hunger for energy, thematically spot-on for coffee, and I like that it's balanced to zero rather than being secretly a free lunch). That's good, deliberate-feeling design and I wouldn't touch those four.

Papitas being net-negative (-5 at a modest 15 price) actually reads fine to me — chips as a guilty-pleasure junk food that costs you sanity/hygiene to feel full is a nice bit of thematic tension, and the price is low enough that the trade-off feels like a real choice rather than a trap.

Galletitas Rellenas is the outlier that doesn't work: it's simultaneously the priciest item in the shop *and* the worst value, net-negative by more than Papitas while costing nearly 7x as much. There's no read of "expensive treat" that justifies a treat being strictly worse than the 2-cost option. I'd either cut the price to somewhere in the 10-15 range to match its modest/mixed profile, or keep the premium price and make the hunger/energy restore actually substantial (e.g. hunger +25) so paying more visibly buys more.

One more gap worth naming: **no item restores energy without a steep trade-off.** Cafe Cargado is the only meaningfully energy-positive item, and it costs sanity and hunger to get there. There's a `bed` GameObject and `bed.mat` sitting in the House_00 scene right now, purely decorative — an obvious, cheap future fix is to make it interactive (click the bed, restore some energy for free or near-free), which also gives that prop a reason to exist beyond set dressing.

### Minigame payouts

Rough-mathing both: a clean 3-minute Cashier run (avg item price ~7.5, realistic catch rate against the batch spawn tables) lands somewhere in the 150-400 money range. A clean, no-crash Delivery run (points formula below, ~23 deliveries across the course) lands somewhere in the 400-650 range over a longer, self-extending ~2-4 minute session. Those are in the same ballpark — I don't think one minigame is going to make the other feel like a waste of time, which is the thing I'd have flagged if the gap were wide.

One specific number worth a second look: `Delivery_Manager.GetPointsFromIndex(index) = Mathf.CeilToInt(Mathf.Log(10) * index)`. `Mathf.Log(10)` is a **constant** (natural log of 10, ≈2.303) — so despite the name, this formula is linear in `index`, not logarithmic. Each successive customer is worth a fixed ~2.3 points more than the last, forever, uncapped. Meanwhile `GetStaySecondsFromIndex` *is* capped (at 8 seconds). The net effect is that late-course deliveries have a strictly *improving* reward-to-time ratio — the game gets more efficient to play the longer you survive. That's a legitimate, even good escalating-stakes curve (it gives you a real reason to push for one more delivery), but I'd want to know whether it was designed on purpose or whether "Log" in the name meant someone intended diminishing returns and got the opposite. Either way, I'd lean into it rather than fight it — maybe even surface a visible "per-delivery value climbing" readout, since the system already produces the feeling good idle/runner games chase deliberately.

## 3. Minigame design

### Cashier

Falling/rolling grocery items get dragged into a type-matching bag before they hit the dead zone; 3 lives, a 3-minute hard cap (`maxGameTime = 180`), and a batch-based spawn table that escalates from all-easy for the first 3 batches into a mix that's ~60% hard by batch 20 (`difficultyChance = batchCount/20`, clamped). That escalation curve mostly completes within the session length, which is good pacing math — the ramp and the clock line up.

One tuning note for whoever touches this next: **the batch tables live on the `Cashier_Manager` component inside `Assets/Scenes/Cashier.unity`**, which is the scene actually wired into Build Settings. There's a second scene, `Assets/Scenes/Cashier2.unity`, with a much thinner easy/medium/hard set (one medium batch, one hard batch, vs. three each in the real scene) that is **not** in Build Settings. It looks like an earlier draft or a duplicate left behind. Not a bug, just flagging it so a future balance pass doesn't get made against the scene that isn't actually shipping.

The lives count is labeled 3, but playing through the loss condition shows the run actually survives a 4th drop before ending (`lives == -1` check after three decrements) — worth knowing only because it means the game is a little more forgiving than its own UI number suggests, which if anything undersells itself; I wouldn't change it.

Fun assessment: this is a legible, well-paced arcade sorting game — think Diner Dash/Overcooked in miniature — and the escalating batch pressure gives it real tension inside a tight 3-minute box, which is the right shape for a "one more round" cozy minigame. The ceiling on long-term replay is that nothing changes structurally between runs: same six batch templates recombined randomly, same items with a cosmetically random price (1-14) fully decoupled from item *type*. There's no way to learn "prioritize meat, it's worth more" because a veggie and a cut of meat are drawn from the same price range — all the skill is in catching everything, none of it is in triage. Giving categories different average price bands (meat/cheese pricier than veggies, say) would add a light strategic layer worth doing post-v1, not urgent now.

### Delivery

The bike moves forward continuously; the *only* player input is braking (hold mouse or press Vertical — there's no steering or lane-change anywhere in `Delivery_Bike.cs`). Danger comes entirely from cross-traffic at intersections gated by a streetlight state machine, and safety is about *timing*, not *positioning* — functionally a Frogger-style patience/timing game wearing a delivery-courier skin. That's a distinctive, well-differentiated pairing against Cashier's twitch-drag-sorting (patience vs. dexterity), and I think it's the more interesting of the two mechanically.

The thing I'd actually flag: **traffic difficulty doesn't escalate.** `Delivery_Streetlight.MoveLanes()` physically translates the *same* streetlight and the *same* `Delivery_CarSpawner` to each successive crossing rather than instancing a new one per block — confirmed by the scene only containing a single `minSpawnTime` (1) and a single `waitingTime` ({3,5}) value total. So every one of the ~7 intersections in a run has identical spawn density and identical light timing. The *only* thing that escalates over the course of a run is the money/time reward per delivery (the linear index formula above) — the actual challenge of surviving traffic is flat from block 1 to block 7. For a game whose whole tension is "it's getting more dangerous the further you go," I'd want the traffic to actually reflect that — tightening `minSpawnTime` or shortening the green-light window slightly as `currentStreet` increases would make the rising stakes feel earned instead of just numerically bigger.

The fail state is a single crash → instant run end, no lives cushion, which reads harsher in the moment than Cashier's multi-strike tolerance. But — same as Cashier — dying still banks whatever `moneyEarned` was accumulated before the crash (`GameOver()` calls `WinMoney(moneyEarned)` unconditionally), so the actual stakes are "the run stops early," not "you lose everything you made." I'd surface that explicitly on the game-over screen (something like "Banked $X — better luck next trip!") so the fail state *reads* as gentle as it actually mechanically is, instead of feeling like a hard fail purely because of the one-hit framing.

Small aside: `maxPackages = 10` is set in the scene but never referenced anywhere in `Delivery_Manager.cs` — the run is currently bounded only by the timer and physically reaching the finish-line trigger. Not flagging it as broken, just noting it in case it was meant to be an early-win condition that never got wired up; worth a product decision either way, not urgent.

Fun assessment: the brake-only Frogger hybrid combined with the "every delivery adds time" rubber-band (up to +10s per delivery, capped) gives it a genuine "one more delivery" pull, and I like it. The flat traffic difficulty is the one thing standing between "good bones" and "actually escalating."

## 4. Cozy-game tone check

**The renames landed well.** Cafe Cargado / Te de Tilo / Jabon Perfumado / Galletitas Rellenas / Papitas / Mate all read completely clean, locally-flavored (Argentine Spanish — mate, tilo, papitas), and tonally consistent with a cozy pet game. Good fix.

**But the scrub isn't finished.** The underlying asset files on disk are still literally `Farlopa.asset`, `Xanax.asset`, `Plax.asset`, `Pitusas.asset` (`Assets/Resources/Consumables/`), and the `m_Name` field baked into each `.asset`'s YAML still carries the old name too — only the in-game `name`/`description` *fields* were changed, not the ScriptableObject's actual identity. This doesn't show up in normal play, but it means the references the rename was meant to remove are still present in the shipped project — visible to anyone poking at the build's files, and liable to leak into a debug log the moment anything does a plain `Debug.Log(item)` (Unity's default `ToString()` prints the asset name, "Farlopa (ConsumableItem)", not the `name` field). `BACKLOG.md` already tracks the icon art half of this cleanup (still-branded photos); I'd bundle the asset-file rename into the same pass, since finishing one half without the other leaves the job half-done.

**Sickness, tonally, cuts both ways.** Mechanically it's appropriately soft (no permadeath, nothing gated) — good cozy-game instincts. But total invisibility isn't automatically "cozy," it's just silent. A cozy game probably *shouldn't* show anything graphic (no vomiting, no distress imagery), so I don't think the answer is a harsh sick overlay — but total silence means the player experiences unexplained faster stat drain with no visible cause, which can read as a hidden penalty rather than a considerate one. A small, gentle tell (a sniffle animation, a thermometer or tissue icon over the pet, a slightly desaturated palette while sick) would close the gap without breaking tone.

**The minigames themselves are tonally fine** — grocery bagging and bike deliveries are wholesome, low-stakes activities with no content clash. The one beat that's a half-step harder than the rest of the game's register is Delivery's crash: `Delivery_Bike.Explode()` triggers a literal explosion sound/VFX and a camera shake for what's narratively a fender-bender. I'd soften the presentation (a cartoony "boing"/spill rather than an explosion) to match everything else's register — the underlying instant-fail mechanic is fine per §3, this is purely a presentation note.

**FireHazard being cut is the right call for tone**, independent of the scope reasons it's already cut for — a fire-fighting minigame (property damage, personal danger) is a bigger tonal reach than grocery-bagging or bike delivery, and if it's ever revisited, I'd want a tone pass on it specifically, not just a bug pass.

## 5. Prioritized recommendations

**Fix before anyone plays this build (cheap, high-impact):**
1. Re-balance Galletitas Rellenas — currently the most expensive item in the shop and net stat-negative. Drop the price to ~10-15, or raise the hunger/energy restore enough to justify the premium.
2. Finish the drug/brand scrub — rename the underlying `.asset` files/identities to match the new display names, not just the visible strings (bundle with the icon-art replacement already tracked in `BACKLOG.md`).
3. Give sickness *some* visible signal — even a minimal icon or color tint — so its real cost (2x decay) is legible to the player experiencing it.
4. Note/flag that `Assets/Scenes/Cashier.unity` (not `Cashier2.unity`) holds the tuning that actually ships, so a future balance pass doesn't land in the wrong scene.

**Worth doing if there's time before v1:**
5. Start new saves at 100/100/100/100 rather than the current 60/50/90/80, or add a one-beat first-session tutorial, so a new player's countdown starts after they've learned the systems, not before.
6. Add a "while you were away" recap modal when offline decay fires on load, instead of silently presenting an already-degraded (possibly already-sick) pet with no explanation.
7. Wire up `PetController`'s mood/animator system to the four stats. It's already built and already unused — this is the highest-leverage, lowest-cost thing on this entire list for making the core loop actually feel like a Tamagotchi rather than a spreadsheet with a 3D model on top.

**Post-v1 backlog:**
8. Escalate traffic difficulty across Delivery's course (currently flat — only the reward curve escalates).
9. Category-based price variance in Cashier (meat/cheese worth more than veggies) to add a light triage decision to the sorting.
10. A cheap/free energy restorer — the decorative `bed` in House_00 is sitting right there unused.
11. Soften Delivery's crash presentation (explosion → something cozier) to match the rest of the game's register.
12. A day counter, streak, or other gentle return-cadence hook, plus some form of unlock/progression beyond raw currency, to give the loop legs past the first few days.
13. Revisit FireHazard for reintroduction only after the above are solid, and only with a dedicated tone pass given its harder subject matter.
