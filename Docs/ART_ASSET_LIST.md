# Carpinotchi — Art Asset List (v2 feature set)

Everything needed to build out house customization + tiers, stat-driven pet feedback, and the expanded consumable set from `FEATURE_PROPOSALS.md`. Grounded against what's actually in the project today (checked `Assets/Map/`, `Assets/Carpincho/`, `Assets/Items/`) — status column tells you what's real vs. net-new.

---

## 1. House shells

| Tier | Asset | Status |
|---|---|---|
| 1 — cheap (current) | `Assets/Map/house.FBX` | **Exists** |
| 2 — middle class | New house model | Needs creation |
| 3 — high class | New house model | Needs creation |

Only one house model exists in the project today — confirmed via `Assets/Map/house.FBX` (single file, no variants). Tiers 2/3 are full new models, not retextures — "bigger house, nicer neighborhood" per the design doc implies different room layout/scale, not just a palette swap.

**Window view resolved**: no separate neighborhood-backdrop system — the view out the window (currently the "bad neighborhood" texture) is just part of each house tier's model/texture, baked in per tier same as everything else about that model. No extra asset category needed beyond the 3 house shells themselves.

## 2. Floor & wall textures — 9 + 9

Current baseline: **one** floor material (`floor.mat`, textures `floor_n.png` normal + `floor_ao.png` AO, no separate diffuse file found — check if diffuse is packed or missing) and **one** wall material (`wall.mat`, diffuse `brickwall.png` + normal `wall_n.png`). That's the full existing set — everything below is net-new unless you want to keep current brick/floor as one of the 9.

| Tier | Floor textures | Wall textures |
|---|---|---|
| 1 — cheap | 3 variants (current brickwall/floor could count as 1 of these 3) | 3 variants |
| 2 — middle | 3 variants | 3 variants |
| 3 — high | 3 variants | 3 variants |

**Spec, matching current convention**: diffuse + normal map per texture at minimum; floor additionally has an AO map in the current set (`floor_ao.png`) — worth keeping for floors, optional for walls since the current wall set doesn't have one. Resolution not enforceable from existing files (need an in-Editor check of `floor_n.png`/`wall_n.png`'s actual import size to set a matching target for the new 16).

**Open question, resolved direction**: leaning triplanar (world-space projected, no UV1 unwrap needed) rather than per-mesh UV layouts — see the triplanar note at the end of this section. This also answers the "distinct UV layouts per tier" question from the previous draft: with triplanar, you don't need them, on any of the 3 house geometries.

**Triplanar mapping decision**: use triplanar shading for floor/wall materials across all 3 house tiers.
- Kills the need for hand-unwrapped UV1 on any of the house meshes — texture projects from world position/normal instead, works identically regardless of room shape. Directly reduces art time across 3 different house geometries.
- Does **not** remove the need for UV2 if baked lighting stays — UV2 is for lightmaps specifically, a separate system. `House_00.unity` currently bakes lightmaps (`m_EnableBakedLightmaps: 1`, real lightmap data present) — if the new house tiers keep baked lighting, they still need UV2 authored. Realtime-only lighting on the new houses would sidestep this, but that's a lighting-quality tradeoff to decide separately, not a triplanar side-effect.
- Textures still need to be normal tileable materials (clean repeat in a simple grid) — triplanar removes the need to match a texture to a specific mesh's unwrap, not the need for the texture to tile at all.
- GPU cost: negligible for this project's scope (small house interiors, handful of floor/wall materials, low overdraw) — 3x texture samples per map instead of 1x is a non-issue at this scale. The one real complexity is triplanar *normal* mapping needing correct per-axis reorientation/blending (a known, solvable technique, just not trivial like the albedo case).
- Implementation is a Shader Graph task — third one in this project alongside the overlay/fade shader (`FEATURE_PROPOSALS.md` §2.6) and the brush-erase mask shader (§3) — same rule: build in-Editor, don't hand-author the graph blind.

## 3. Furniture — stoves & beds

| Item | Tier | Status | Gameplay tie-in |
|---|---|---|---|
| Stove | 1 — low (current model) | **Exists** (current single stove mesh; `stove_bits`/`stove_rust`/`stove_rust2`/`stove_wood` are sub-material slots on that *one* mesh, not tier variants — confirmed via file check) | Bad meal, replenishes little hunger, slow cook |
| Stove | 2 — medium | Needs creation | Medium meal, normal cook time |
| Stove | 3 — high | Needs creation | Best meal, presumably fastest/most hunger |
| Pot/pan prop | 1, shared across stove tiers unless you want tier-matched cookware | Needs creation | Sits on the stove while a meal is cooking — the visual "something is happening" cue for the real-time cook timer. Simple prop, cheap. |
| Bed | 1 — poor (current) | **Exists** (`bed.mat`, texture `CJ_BED_BASE.png`) | — |
| Bed | 2 — middle | Needs creation | — |
| Bed | 3 — high | Needs creation | — |

**Cooking mechanic note for engineering, not art, but worth flagging here since it constrains the stove asset**: "one meal per day, cook time passes even if in a minigame" means cook-time needs to run on real elapsed time via the existing `TimeManager`/`SaveManager` offline-decay pipeline (same mechanism that already handles stats decaying while away), not a simple in-scene coroutine timer — a coroutine timer would freeze the moment the player leaves `House_00` for a minigame, which contradicts "time passes even in minigame." Needs a `cookStartedUtc` timestamp in the save schema, same pattern as `lastSavedUtc`.

## 4. Decor — paintings & books

| Item | Count | Tiers |
|---|---|---|
| Paintings | 6 | 2 low, 2 mid, 2 high |
| Bookshelf | 1 model | Shared across all tiers |
| Books | 9 | 3 low, 3 mid, 3 high |

**Paintings**: flat texture on a simple frame quad/mesh — cheap, same production cost class as consumable icons (§6).

**Bookshelf**: one shared mesh across tiers, "stacks up to three shelves" — read as: tier isn't a different bookshelf model, it's how many of the 9 books the player owns/has placed, filling in progressively (1 book placed = sparse shelf, 9 = full). Needs the shelf mesh authored with 3 shelf levels' worth of placement slots/sockets for book props to sit in, but only **one** bookshelf model to build.

**Books**: 9 small props (not flat textures like paintings/consumables — these sit as 3D objects on the shelf) needing simple geometry + a spine texture each. Lower art cost than furniture, higher than a flat icon.

**Ties to sanity** (per `FEATURE_PROPOSALS.md` §2.4): decor needs to carry gameplay data (a passive sanity buff), not just be visual — doesn't change the art spec, but whoever owns the item data model needs a way to tag "this decor piece grants +X sanity" per painting/book, probably scaled by tier.

## 5. Hygiene props — droppings & environment grime

Called out separately from the pet-only dirt overlay (§7.2) — these are environment-level, not on the pet itself.

| Asset | Notes |
|---|---|
| Droppings prop | Small prop, spawned at random floor spots below the hygiene threshold (`FEATURE_PROPOSALS.md` §2.2) — needs cleaning up by the player. Simple/cheap geometry, one or two variants is plenty. |
| Floor/wall grime overlay | A fading dirt texture on the floor/wall materials themselves (distinct from the pet's own dirt overlay in §7.2) — same fade-driven approach as the rest of the overlay system, applied to the triplanar floor/wall shader from §2 rather than the pet shader from §2.6. Worth building as another blend layer on the same triplanar material rather than a fourth separate shader. |

## 6. Consumables — 3 variants per stat category

Current: 6 single-purpose items (Cafe Cargado, Te de Tilo, Jabon Perfumado, Papitas, Galletitas Rellenas, Mate — see `Assets/Resources/Consumables/`). Request is **3 items per stat-focus category** instead of one, so more variety in the shop.

**Spec, matching current convention** (confirmed from actual files): flat icon texture, 256×256 or 512×512 PNG with alpha, no 3D model — this is already the cheapest asset class in the project and the plan is to keep it that way. At 3-per-category across the existing 4 stat-focus categories (energy/hunger, sanity, hygiene, plus whatever the food-only "Papitas/Galletitas" flavor-food category counts as) that's roughly a dozen-plus new icons — exact count depends on how you want to split "food" as its own category vs. folding into hunger. Cheapest bucket of this whole list either way.

---

## 7. Pet — animations, overlays, blend shapes

Added per your callout — this was missing from the first pass. Current baseline: `Assets/Carpincho/idle_carpincho.FBX` (idle) + `walk_carpincho.FBX`/`walk.anim` (walk). `Carpincho.controller` (Animator) only has idle/Blend Tree/speed states — **no mood or action animations exist today.**

### 7.1 Animation clips needed

| Clip | Used by |
|---|---|
| Sleep (loop) | Energy → 0, bed interaction (§2.3) |
| Wake-up (transition) | Leaving sleep state |
| Happy / Sad / Crazy / Thirsty / Hungry | `PetController.Mood` enum already defines these 5 — currently zero clips exist for any of them |
| Eat reaction | Consuming an item |
| Bathe reaction | Bathing interaction (§2.2.1) |
| Fast-run (or a sped-up walk cycle) | Energy overflow "comically fast" movement — cheapest option is reusing `walk.anim` at a higher playback speed rather than authoring a new clip; flag as a design call, not a hard requirement |

That's up to 9 new clips depending on how the fast-run question above is resolved. This is the single biggest animation gap in the project — right now the pet is capable of exactly two states (standing still, walking).

### 7.2 Overlay textures needed

Per `FEATURE_PROPOSALS.md` §2.6 (shared overlay/fade shader on the pet — pet-only, see §5 for the separate environment grime overlay):
- Dirt overlay (hygiene, on the pet)
- Sick tint/overlay (sickness)
- Soft round brush-alpha texture — used by the shared brush/mask-erase system for **both** bathing (§2.2.1) and graffiti wall-cleaning (§3); one texture, two reuses, cheap.

### 7.3 Blend shapes — conditional, don't commit yet

Only needed if the hunger-thinning effect (§2.1) turns out to need real morph targets instead of the cheaper bone-scale approach. **Do not author these yet** — first needs an in-Editor check of whether `idle_carpincho.FBX`'s rig already separates a belly/torso bone that a scale-based approach could use without any new art at all. Only fall back to blend-shape authoring (re-exporting the FBX with a sculpted "thin" target) if bone-scaling looks too crude once tried.

---

## 8. Graffiti minigame (if revived)

Existing art in `Assets/Graffiti/` is all test/placeholder quality — `test1.png`, `test2.png`, `sdfsdf.png`, `ttt.png`, a stock "never give up" graffiti PNG, `stencil.png`, `gar.tga`. None of it reads as ship-quality wall art. If the minigame moves forward per the design doc, needs real graffiti decal textures per wall (count depends on how many walls per round get built) — separate task from the brush-alpha texture in §7.2, which is reused infrastructure, not wall art itself.

---

## 9. Audio (draft — placeholders expected for now, but drafting the list anyway)

Nothing in the existing game covers any of this new surface — a rough first pass so it's not forgotten later, not a spec to hold anyone to yet:

| Sound | Trigger |
|---|---|
| Cooking start / cooking loop / meal ready | Stove interaction (§3) |
| Soap/water scrub (loop while dragging) | Bathing interaction (§2.2.1) |
| Mood stings (happy/sad/crazy/thirsty/hungry) | Same 5 states as `PetController.Mood` — pairs with the animation clips in §7.1 |
| Sanity-crazy cue | Onset of the sanity-chaos movement state (§2.4) |
| "Too crazy to [care action]" gate rejection | Same UI toast as the gate itself (§2.4) |
| Sleep / wake | Bed interaction (§2.3) |
| Purchase / equip confirmation | New buy-then-equip decor UI (§1) |
| Per-house-tier ambience (optional) | Passive, plays while in `House_00` — lowest priority of this list, easy to cut |

Not counted in the summary table below since these are explicitly placeholder-for-now, but worth having the list once someone does pick it up.

---

## Summary counts

| Category | Count |
|---|---|
| House shells | 2 new (1 exists) |
| Floor textures | 9 (baseline: 1 exists) |
| Wall textures | 9 (baseline: 1 exists) |
| Stoves | 2 new (1 exists) |
| Pot/pan prop | 1 new |
| Beds | 2 new (1 exists) |
| Paintings | 6 new |
| Bookshelf | 1 new (shared mesh) |
| Books | 9 new |
| Droppings prop | 1 new (maybe 2 variants) |
| Floor/wall grime overlay | 1 new (extra blend layer on the triplanar shader) |
| Consumable icons | ~9-12+ new (3 per category, category count TBD) |
| Pet animation clips | up to 9 new |
| Pet overlay textures | 3 new (dirt, sick, brush-alpha) |
| Pet blend shapes | 0 for now — conditional on rig check |
| Graffiti wall art | TBD, only if that minigame is revived |
| Shader Graph tasks | 3 total: pet overlay/fade (§2.6 of the design doc), brush-erase mask (§2.2.1/§3), triplanar floor/wall (§2) — all need in-Editor authoring, none safe to hand-write blind |
