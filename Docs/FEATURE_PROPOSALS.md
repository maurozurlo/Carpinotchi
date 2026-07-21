# Carpinotchi — Feature Proposals (v2 draft)

Post-v1 feature brainstorm. Not scoped for the current ship — captured here for design review before any engineering estimate.

---

## 1. House customization & progression

**Concept**: the house currently reads as poor (exposed brick, bad flooring, bad-neighborhood view through the window). Let the player spend money on:
- Decor: posters, wall art, small objects
- Structural: paint walls, change flooring
- Progression: a bigger house in a nicer neighborhood (a real "you made it" milestone, not just a shop SKU)

**Why it matters**: right now money only buys consumables that get eaten and vanish. Customization is the first *permanent, visible* money sink — it's what turns "grinding minigames" into "building something."

**Ties to stat system**: user wants decor to feed back into sanity (see §2.4) — posters/books as sanity buffs, not just cosmetic.

**Decided in review**:
- **Equip flow**: buy from the store (reuses the existing `ShopManager`/`ItemManager` buy flow as-is), then apply/equip from inventory — no drag-and-drop world placement. New UI needed for the "equip" step (select an owned decor item, assign it to a slot — wall/floor/bed/stove/bookshelf) since nothing like this exists today; `ConsumableItem`'s model is one-shot-and-gone, decor is owned-and-swappable, so this also needs a new `Item` subtype (see engineering note below), not a reuse of `ConsumableItem`.
- **Progression gate**: money-gated (no separate day/level requirement). Exact thresholds are an economy-design call, not ours — see the note at the end of this doc.
- Paint/flooring: swappable Materials on the existing room meshes via the triplanar system (`ART_ASSET_LIST.md` §2) — no new geometry needed.
- Neighborhood view: baked into the house tier model itself (the window's texture/view changes per house tier), not a separate backdrop system — see `ART_ASSET_LIST.md` §1.

**Engineering note — new `Item` subtypes needed**: `Item.cs` currently only has one subtype, `ConsumableItem` (one-shot, consumed on use). This feature set needs at least two more:
- An **equipable/decor item** type — owned persistently, assignable to a slot, swappable, and (per §2.4) carries passive stat-effect data (e.g. sanity buff) rather than a one-shot delta.
- A **durable item** type — the mop (§2.2), N uses then gone, distinct from both of the above.

---

## 2. Stat-driven visual & gameplay feedback ("full Sims" pass)

Right now all 4 stats (`Pet.cs`) only move 4 UI fill bars (`UIManager.cs`). Zero world-space feedback. `PetController.cs` already defines a `Mood` enum (happy/sad/crazy/thirsty/hungry) and a working `DisplayMood()` → Animator trigger call — it is never invoked anywhere in the project (confirmed in the last design review pass). That's the obvious hook point for most of this.

### 2.1 Hunger → visibly thinner pet

**Concept**: as hunger drops, the pet's model visibly thins out.

**Technical options** (answering the direct question — morph targets vs. cheaper alternative):
- **Morph targets (blend shapes)**: needs the source mesh authored with a "thin" blend shape target, then driven at runtime via `SkinnedMeshRenderer.SetBlendShapeWeight()`. Highest visual quality, but `Assets/Carpincho/idle_carpincho.FBX` / `walk_carpincho.FBX` need to be checked in-Editor for whether any blend shapes already exist on the mesh — from the asset list alone (single FBX per anim, no "_thin"/"_blend" variants) it looks like none are authored yet. If not, this is an **art task** (re-export the FBX with a sculpted thin-shape target), not just code.
- **Cheaper alternative (bone scale)**: scale a torso/belly bone on the existing rig based on hunger%, driven every stat update from a script (no new art needed *if* the current rig already separates a belly/torso bone from the rest of the spine — needs an in-Editor check of the rig hierarchy). This is the pragmatic v1 approach: cheap, reversible, no new mesh authoring, and "good enough" for a stylized capybara. Recommend starting here and only moving to real blend shapes if bone-scaling looks too crude in practice.

### 2.2 Hygiene → droppings + dirt overlay

**Concept**: below some hygiene threshold, the pet leaves droppings at random spots in the house that the player must clean up; player buys a mop (with a lifespan/durability) to clean; a fading dirt texture overlay on the pet (and optionally floor/walls) too.

**Technical notes**:
- Droppings: a simple prefab pool (same pattern already used for fire particles in the cut FireHazard minigame — `Fire_FireParticlePool.cs` — reusable pattern, though note that pool has its own bug per the engineering backlog) spawned at random NavMesh-valid points on a hygiene-decay tick.
- Mop with lifespan: a new inventory item type, but unlike `ConsumableItem` (one-shot, consumed on use) this needs *durability* — a new field/subclass, since nothing in `Item.cs`/`ConsumableItem.cs` currently models "N uses then gone."
- Dirt overlay on the pet: this is one of the layers in the shared overlay/fade system, see §2.6 — not a separate one-off shader.

#### 2.2.1 Bathing interaction (Pou-style)

**Concept**: instead of (or in addition to) a passive hygiene consumable, an active bathing interaction — player drags soap/a sponge across the pet's body to scrub the dirt off, Pou-style.

**Key insight**: this is mechanically the *same interaction* as the Graffiti minigame's "drag to erase" (§3/§4) — both are "brush-erase a mask by dragging over a surface." Build the GPU brush/mask erase system once (RenderTexture mask + brush-quad blit, per §3's recommended direction) and reuse it for both the dirt overlay (§2.6) and the graffiti walls, rather than treating them as two separate systems. Progress tracking should follow the same rule as §3 too: procedural running estimate, not a texture readback.

**Open question**: is bathing a full-screen dedicated mode (like a minigame, blocks other House interaction while active) or a lightweight in-place drag gesture on the pet in the normal House view? Affects UI/state-machine scope a lot — a dedicated mode is closer to "third minigame," an in-place gesture is closer to "extra interaction on the existing DrageableObject-style flow."

### 2.3 Energy → sleep state + comic fast-move

**Concept**: at 0 energy, pet stops moving and plays a sleep animation; wire in the existing bed model so the player can explicitly put the pet to sleep (no minigames while asleep); at very high/overflowing energy, pet moves *comically fast* around the house.

**Technical notes**:
- Bed model already exists in the house (`Assets/Map/bed.mat` etc.) as set dressing only — needs an interaction trigger (click bed → play sleep anim, freeze `Wander.cs` movement, gate `MinigameManager.LoadScene` while asleep) and a wake condition (energy back above some threshold, or player-initiated wake).
- "No minigames while asleep" is a real design choice worth flagging in review: does it block the House→Cashier/Delivery buttons outright, or just refuse silently? UX needs an explicit "can't leave, pet's asleep" message either way.
- Comic-fast movement at energy overflow is the easy part — just a `Wander.cs` speed multiplier keyed off energy%.

### 2.4 Sanity → cascading chaos (most fun potential, per your note)

**Concept, refined in review**: at low sanity, two separate effects, both buildable now with zero new art:
1. **"Jumping around" = a movement behavior, not randomized stat numbers.** `Wander.cs` already picks a new random NavMesh destination on a fixed cycle (`InvokeRepeating("changeDestination", 0, 5)`, §see engineering notes). At low sanity, shrink that interval (e.g. 0.5–1s) and raise `m_NavAgent.speed` — the pet darts around erratically using the exact same movement system, no new code paths. This reads as "erratic," not "buggy," which is the risk with literally randomizing the other 3 stat values — recommend not doing that.
2. **"Too crazy to [care action]" gate.** Generalized rule, not just eating: too crazy to eat, too crazy to sleep, too crazy to bathe — any player-initiated care action gets blocked at low sanity with a short in-world message (e.g. "too crazy to eat right now"). This should be one shared check (e.g. a `Pet.CanPerformCareAction()`-style gate) reused at each interaction's entry point, not three copy-pasted threshold checks:
   - Eating: `DrageableObject.OnMouseUp()` (Assets/Scripts/DrageableObject.cs:31-57) is the consume point — block before `ReduceAmountOfItem`/`Consume()`, snap the item back same as the existing "not over target" case.
   - Sleeping: gate on the bed-interaction trigger from §2.3.
   - Bathing: gate on the bathing interaction from §2.2.1.
   - House_00 has no toast/message UI today (Cashier and Delivery both have one, House_00 doesn't) — smallest net-new UI piece this whole doc needs, and it's shared by all three gates.

Decor (posters, books) helping sanity stay up ties back to §1 — decor needs a passive stat-effect model (e.g. +sanity ceiling or slower sanity decay while owned/placed), different from `ConsumableItem`'s one-shot delta.

**Relationship to `Pet.isSick` still needs deciding**: sickness (triggers at 3 consecutive zero-stat ticks on *any* of sanity/hunger/hygiene, doubles decay, see `Pet.EvaluateSickness()`) and "sanity-zero chaos" are two different systems that can both be active at once. Recommend treating them as layered, not merged: sickness is the mechanical penalty (decay rate), sanity-chaos is the sanity-specific behavioral/visual expression. See §2.5 for sickness's own missing visual feedback.

### 2.5 Sickness (`Pet.isSick`) has zero visual feedback

**Confirmed gap** (flagged in the last design review, still true): `isSick` doubles decay but nothing shows it. Two pieces, both code-only:
- Slower movement: same `Wander.cs` speed knob as §2.4's chaos effect, opposite direction — reduce `m_NavAgent.speed` while sick.
- Pale/sick tint: a layer in the shared overlay/fade system, §2.6 below — **not** a separate discrete material asset (see §2.6 for why that approach was rejected).

### 2.6 Shared overlay/fade rendering system

**Decided in review**: no discrete per-state material assets (my first pass proposed a separate `sickMaterial` — rejected). Instead, one system: multiple texture overlays on the pet (dirt, sick tint, and whatever comes later), each with its own independent fade amount, all layered on the pet's *existing* material rather than swapped out.

**Why this matters more than it sounds like**: `VisualManager.cs` currently implements the drag-hover highlight by swapping the *entire* `Material` (`Glow()`/`Dull()` reassign `skinnedMesh.material` wholesale). If overlay fades are set as properties on the normal material, hovering an item over the pet would swap to `glowMaterial` — a completely different Material asset — and any dirt/sick fade values set on the normal material would vanish for as long as the hover lasts. **These two systems conflict as currently built.** Fix: fold `Glow`/`Dull` into the same property-driven model (a `_GlowFade` property on the one shared material) instead of a material-object swap, so hover-highlight and overlay fades compose instead of fighting each other.

**Technical reality check — this needs real shader work, and it has a risk tradeoff worth being upfront about**:
- `Carpincho.mat` uses standard **URP Lit** (confirmed by reading the material — `_BaseMap`/`_BumpMap`/`_Metallic`/`_WorkflowMode` property set, shader guid matches Unity's built-in URP Lit). A correct implementation adds overlay-blend nodes *on top of* URP Lit's existing PBR lighting (via Shader Graph's Lit target, which the project already uses elsewhere — there's a Shader Graph asset in `Assets/Graffiti/`) rather than hand-writing a full PBR shader from scratch in HLSL, which would be a large, regression-prone undertaking to author blind.
- Hand-authoring a `.shadergraph` file's internal node-graph JSON directly (the way I've been safely hand-editing `.unity`/`.prefab` YAML this whole session) is a **materially different risk profile** — Shader Graph files are node/edge/port-linked data that Unity's graph editor manages, not simple flat YAML, and there's no way to verify a hand-typed graph compiles or renders correctly without opening it in the Shader Graph window. Recommend this piece specifically be built in-Editor (by you, or by me later if/when there's a live Unity MCP connection to drive the Shader Graph editor directly) rather than attempted as blind file surgery.
- Once the shader exposes `_DirtFade`, `_SickFade`, `_GlowFade` (etc.) as float properties, driving them from script is trivial and totally safe for me to build blind — `VisualManager` just needs `material.SetFloat(...)` calls wired to hygiene/isSick/hover state. That half of this feature has no risk.

---

## 3. Graffiti minigame revival

**Concept** (from your notes): pet is hired by the city to clean graffiti off walls. Original attempt is still in the project (`Assets/Graffiti/`) but was shelved over performance.

**Shares its core tech with §2.2.1 (Bathing)**: both are "drag a brush over a surface to erase a mask." Build the brush/mask erase system once, use it for both walls and pet-dirt. Don't scope or estimate these as two separate systems.

**What's actually there**: `Graffiti_Decal.cs` + `GraffitiShader.shader`/`Graffiti2.shader`/`Graffiti3.shader` + a Shader Graph + a stencil texture + several test PNGs — evidence of multiple serious attempts, not just a stub.

**Root cause of the perf problem** (confirmed by reading the code, not guessing): `Graffiti_Decal.cs` does CPU-side `Texture2D.GetPixels()` → modify → `SetPixels()` → `texture.Apply()` **every single `Update()` frame** while the mouse is held down. That's a full CPU texture readback + GPU re-upload every frame — that's the actual framerate killer, not the shader itself (`GraffitiShader.shader` is just a simple stencil-alpha `clip()` mask, cheap on its own).

**Recommended technical direction for a real revival**: move the "erase" step to the GPU entirely —
- Maintain a small **RenderTexture mask** per graffiti wall (or a shared mask atlas for mobile memory budget).
- On click/drag, render a soft-edged brush quad into that mask at the hit UV (a single cheap draw call via `Graphics.Blit`/a custom Blit-like pass), instead of touching a `Texture2D` on the CPU at all.
- The wall shader samples that mask to `clip()`/alpha-out the graffiti, same idea as the existing stencil approach but the mask is now GPU-authored and cheap to update every frame.
- This is mobile-friendly by construction (small RenderTexture, one draw call per brush stroke, no CPU stalls) — directly matches your stated mobile-friendly goal.

**Core loop (decided)**: fixed round timer, N pre-placed walls in the scene, each wall counts as clean at 100% graffiti coverage removed, per-wall percent UI, tally of walls cleaned when time runs out. Deliberately low-pressure/not "exciting" — this is a community-service, teach-good-values minigame, cozy tone over tension. Structurally this is Cashier's "clear the queue before the timer" session shape (`GameState.start/playing/ended`, round timer, end-of-round tally feeding `MoneyManager`) minus the drag-and-match mechanic — simpler to build than Cashier, not harder, since it reuses that same session skeleton.

**Percent tracking — do not read the mask texture back.** Reading the RenderTexture (or `AsyncGPUReadback`) to compute real coverage % reintroduces the same class of CPU/GPU stall that killed the original attempt, just moved. Instead, track progress **procedurally**: each brush dab adds an estimated coverage amount to a running `float` per wall (capped at 100%), independent of the mask render. Approximate, but framerate-safe and plenty accurate for a minigame that isn't trying to be pixel-precise. UI is a simple fill/percent readout per wall, same `Image.fillAmount` pattern already used for the pet's stat bars.

---

## 4. Save data & persistence additions

Everything above needs new fields in `SaveManager`'s `SaveData` (`Assets/Scripts/SaveManager.cs`) — writing it down now so it doesn't get rediscovered piecemeal during implementation. Current schema only covers the 4 stats, money, `isSick`, and a flat inventory (`itemIds`/`itemQtys`). Needs to grow to also cover:

- **House tier owned** — which of the 3 tiers the player currently has.
- **Decor owned + equipped** — two different lists: everything the player has *bought* (persists even if unequipped) vs. what's currently *equipped* per slot (wall paintings, floor, wall paint, bed, stove, which books are on the shelf). Matches the buy-then-equip flow decided in §1.
- **Cook state** — `cookStartedUtc` timestamp (same pattern as the existing `lastSavedUtc`) + which stove tier + what's cooking, so "time passes even in a minigame" works via the same offline-elapsed-time math `ApplyOfflineDecay` already does, not a scene-local coroutine that would freeze the moment the player leaves `House_00`.
- **Mop durability remaining** — the one durable (not consumable, not equipable) item type from §1's engineering note.
- **Sanity-chaos / sickness visual state** — probably derivable from existing `isSick`/stat values at load time rather than needing its own saved field, but worth double-checking once §2.4/§2.5 are actually implemented rather than assuming.

## 5. Explicitly out of scope for this document

Flagging so these don't get silently re-decided later:
- **All pricing/economy balance** — house tier costs, decor prices, mop cost, cook-time-per-tier, consumable prices for the expanded 3-per-category set. Owned by a dedicated economy designer, not decided here.
- **Exact progression-gate thresholds** (how much money for tier 2/3 house) — same owner, same reason; the *shape* of the gate (money-only, no day/level requirement) is decided, the numbers aren't.

---

## Summary of what's genuinely new engineering surface here (not reuse of existing systems)

Grouped by shared system, since several of the above collapse into the same underlying work rather than being independent features:

- **Overlay/fade shader** (§2.6) — one URP Lit + Shader Graph overlay-blend material serves dirt, sick tint, and hover-glow (folding `VisualManager.Glow`/`Dull` in to resolve the material-swap conflict). The one piece in this whole doc that needs real Editor/Shader-Graph work rather than blind file edits — everything else here is safe for me to build without a live Editor session.
- **Brush/mask erase system** (§2.2.1 + §3) — one GPU RenderTexture-mask-and-blit system serves both Bathing and Graffiti. Procedural coverage estimate, never a texture readback, in both uses.
- **"Too crazy to [care action]" gate** (§2.4) — one shared check reused at the eat/sleep/bathe interaction points, plus one new toast/message UI in House_00 (doesn't exist yet, smallest net-new UI piece here).
- **`Wander.cs` speed/interval modulation** — already covers sanity-chaos (fast+frequent), sickness (slow), and energy overflow (fast) with the same two knobs (`m_NavAgent.speed`, the `changeDestination` interval). No new movement code, just state-driven parameters.
- **Durability/lifespan items** (mop) — `Item`/`ConsumableItem` don't model "N uses then gone" today, needs a new field/subclass.
- **Passive/ongoing stat effects from owned decor** — different from `ConsumableItem`'s one-shot delta model, needed for posters/books buffing sanity (§2.4) and tied to house customization (§1).
- **Mood/Animator wiring** — `PetController.cs` lives on a prefab that's never instantiated, and `Carpincho.controller` has no mood-trigger parameters or animation clips beyond idle/walk yet. Treated as a separate, lower-priority, art-dependent task — none of the movement/overlay/gate systems above depend on it.
- **House customization/placement system** + a "neighborhood tier" progression concept — entirely new, no existing system to extend.
