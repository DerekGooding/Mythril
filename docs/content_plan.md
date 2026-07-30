# Mythril — Content Gap Implementation Plan
*Self-contained reference for any agent executing this work.*
*Generated: 2026-07-30 | Baseline completion time: 84.4 min | Target: 3–4 hours*

---

## Project Architecture Primer

Before touching any content, an agent **must** understand these facts:

### How Content Works
All game content lives in a single file:
**`Mythril.Blazor/wwwroot/data/content_graph.json`**

This is a flat JSON array of nodes. The engine (`ContentLoader.cs`) reads it on startup and builds all game data from it. There is **no other content data file** except:
- **`Mythril.Blazor/wwwroot/data/stat_augments.json`** — defines what stats each spell augments when junctioned (separate from the graph)

### Node ID Convention
IDs are `type_snake_case_name`. Examples:
- `item_fire_i`, `item_lost_parchment`, `item_blue_coral`
- `quest_hunt_goblins`, `quest_rekindling_the_spark`
- `location_hidden_oasis`, `location_village`
- `cadence_slayer`, `cadence_arcanist`
- `ability_recruit_j_str`, `ability_student_refine_fire`
- `recipe_refine_fire_basic_gem_fire_i`

**IDs must be globally unique across all nodes.**

### Node Types and Their Required Fields

#### Quest Node
```json
{
  "id": "quest_example_name",
  "type": "Quest",
  "name": "Example Name",
  "data": {
    "description": "Flavor text here.",
    "duration": 90,
    "quest_type": "Recurring",
    "primary_stat": "Strength",
    "required_stats": { "Strength": 20 },
    "stat_rewards": {}
  },
  "in_edges": {
    "requires_quest": ["quest_prerequisite_id"]
  },
  "out_edges": {
    "rewards": [{ "targetId": "item_example", "quantity": 2 }],
    "consumes": [{ "targetId": "item_example", "quantity": 1 }]
  }
}
```
- `quest_type`: `"Single"` (one-time), `"Recurring"` (loopable), `"Unlock"` (rarely used)
- `required_stats`: empty `{}` means no stat gate
- `stat_rewards`: empty `{}` means no permanent stat reward
- A quest that unlocks a cadence adds `"unlocks_cadence": [{ "targetId": "cadence_id", "quantity": 1 }]` to `out_edges`

#### Location Node
```json
{
  "id": "location_example",
  "type": "Location",
  "name": "Example Place",
  "data": { "region_type": "Water" },
  "in_edges": { "requires_quest": ["quest_that_unlocks_this"] },
  "out_edges": {
    "contains": [
      { "targetId": "quest_id_1", "quantity": 1 }
    ]
  }
}
```
- `region_type`: `"Plains"`, `"Forest"`, `"Mine"`, `"Water"`, `"Library"` (existing types)
- **All locations except `"Village"` must have a `requires_quest` in_edge** (enforced by `verify_graph.py`)

#### Item Node (Spell)
```json
{
  "id": "item_fire_ii",
  "type": "Item",
  "name": "Fire II",
  "data": {
    "description": "A powerful fire spell.",
    "item_type": "Spell"
  },
  "in_edges": {},
  "out_edges": {}
}
```
- `item_type`: `"Currency"`, `"Consumable"`, `"Material"`, `"Spell"`, `"KeyItem"`

#### Ability Node
```json
{
  "id": "ability_slayer_j_spirit",
  "type": "Ability",
  "name": "J-Spirit",
  "data": {
    "description": "Allows junctioning magic to the Spirit stat",
    "primary_stat": "Spirit",
    "metadata": {}
  },
  "in_edges": {},
  "out_edges": {
    "consumes": [{ "targetId": "item_example", "quantity": 10 }]
  },
  "effects": []
}
```
- `effects` for junction abilities: `[]` (empty)
- `effects` for AutoQuest: `[{ "Type": "AutoQuest", "Value": 1, "Target": "" }]`
- `effects` for Magic Capacity: `[{ "Type": "MagicCapacity", "Value": 60 }]`
- `effects` for Logistics: `[{ "Type": "Logistics", "Value": 1 }]`

#### Cadence Node (linking abilities)
The cadence node simply lists what abilities it provides. Ability cost is defined on the **ability node** via `consumes`, not on the cadence:
```json
"out_edges": {
  "provides_ability": [
    { "targetId": "ability_slayer_master_slayer", "quantity": 1 },
    { "targetId": "ability_slayer_j_spirit", "quantity": 1 }
  ]
}
```

#### Refinement Node
```json
{
  "id": "recipe_synthesis_fire_ii",
  "type": "Refinement",
  "name": "Synthesis - Fire II",
  "data": { "primary_stat": "Magic" },
  "in_edges": { "requires_ability": ["ability_mythril_weaver_synthesis_i"] },
  "out_edges": {
    "consumes": [
      { "targetId": "item_fire_i", "quantity": 5 },
      { "targetId": "item_crystal_shards", "quantity": 10 }
    ],
    "produces": [{ "targetId": "item_fire_ii", "quantity": 1 }]
  }
}
```
- Multiple `consumes` entries = compound recipe (multi-input)
- `requires_ability` in `in_edges` links this recipe to the unlocking ability

### Stat Augments (separate file)
Every new **Spell** item **must** have a corresponding entry in `Mythril.Blazor/wwwroot/data/stat_augments.json`:
```json
{ "Item": "Fire II", "Augments": [{ "Stat": "Strength", "ModifierAtFull": 200 }] }
```

### Validation Commands
After every edit, run:
```powershell
python scripts/verify_graph.py
python scripts/check_health.py --skip-tests
```

### Pacing Baseline
`docs/pacing_baseline.json` — update after content additions intentionally extend the game:
```json
{ "routed_completion_time_minutes": 84.4, "reachable_quests": 33 }
```

---

## Execution Order

Do issues in this order to avoid missing dependency items:

1. **Issue 3** — Alchemy II supply fix (trivial, independent)
2. **Issue 8** — Archive Sifting throughput (trivial, independent)
3. **Issue 1** — Hidden Oasis (adds `item_oasis_scale` needed by Issue 2)
4. **Issue 2** — Slayer expansion (needs `item_oasis_scale` from Issue 1)
5. **Issue 4** — Spirit content (needs `ability_slayer_j_spirit` from Issue 2)
6. **Issue 7** — J-Speed on Geologist (independent)
7. **Issue 5** — Tier 2 Spells (independent, largest addition)
8. **Issue 10** — Crystal Peaks depth (verify Mythril Spark pacing after)
9. **Issue 9** — Confirm stat_rewards reducer, validated by Issue 4's quest
10. **Issue 6** — Verify stat ceiling after Issues 2+5 are live in simulation

---

## Issue 1 — Hidden Oasis Is Empty
**Severity:** 🔴 Critical | **Files:** `content_graph.json`

Find `location_hidden_oasis`. Change `"out_edges": {}` to:
```json
"out_edges": {
  "contains": [
    { "targetId": "quest_gather_luminous_algae", "quantity": 1 },
    { "targetId": "quest_oasis_guardian_hunt", "quantity": 1 },
    { "targetId": "quest_channel_oasis_waters", "quantity": 1 }
  ]
}
```

Add these new nodes to the array:
```json
{
  "id": "quest_gather_luminous_algae",
  "type": "Quest",
  "name": "Gather Luminous Algae",
  "data": { "description": "Harvest the glowing plants of the hidden springs.", "duration": 75, "quest_type": "Recurring", "primary_stat": "Speed", "required_stats": { "Speed": 25 }, "stat_rewards": {} },
  "in_edges": { "requires_quest": ["quest_locate_the_hidden_oasis"] },
  "out_edges": { "rewards": [{ "targetId": "item_luminous_algae", "quantity": 2 }] }
},
{
  "id": "quest_oasis_guardian_hunt",
  "type": "Quest",
  "name": "Hunt Oasis Guardians",
  "data": { "description": "Cull the fierce lizard sentinels of the springs.", "duration": 120, "quest_type": "Recurring", "primary_stat": "Strength", "required_stats": { "Strength": 35, "Speed": 25 }, "stat_rewards": {} },
  "in_edges": { "requires_quest": ["quest_gather_luminous_algae"] },
  "out_edges": { "rewards": [{ "targetId": "item_oasis_scale", "quantity": 1 }] }
},
{
  "id": "quest_channel_oasis_waters",
  "type": "Quest",
  "name": "Channel Oasis Waters",
  "data": { "description": "Redirect the sacred springs to power the forge network.", "duration": 600, "quest_type": "Single", "primary_stat": "Magic", "required_stats": { "Magic": 50 }, "stat_rewards": {} },
  "in_edges": { "requires_quest": ["quest_locate_the_hidden_oasis"] },
  "out_edges": { "rewards": [{ "targetId": "item_pure_water_crystal", "quantity": 3 }] }
},
{
  "id": "item_luminous_algae",
  "type": "Item",
  "name": "Luminous Algae",
  "data": { "description": "A glowing aquatic plant with magical properties.", "item_type": "Material" },
  "in_edges": {}, "out_edges": {}
},
{
  "id": "item_oasis_scale",
  "type": "Item",
  "name": "Oasis Scale",
  "data": { "description": "A shimmering scale from a desert lizard.", "item_type": "Material" },
  "in_edges": {}, "out_edges": {}
},
{
  "id": "item_pure_water_crystal",
  "type": "Item",
  "name": "Pure Water Crystal",
  "data": { "description": "Crystallized essence of the sacred springs.", "item_type": "Material" },
  "in_edges": {}, "out_edges": {}
}
```
**Verify:** `python scripts/verify_graph.py` passes. Simulation shows +3 reachable quests.

---

## Issue 2 — Slayer Has Only 1 Ability
**Severity:** 🔴 Critical | **Files:** `content_graph.json`
**Dependency:** Issue 1 must be done first (`item_oasis_scale` must exist).

Find `cadence_slayer` and update `provides_ability` to:
```json
"provides_ability": [
  { "targetId": "ability_slayer_master_slayer", "quantity": 1 },
  { "targetId": "ability_slayer_j_spirit", "quantity": 1 },
  { "targetId": "ability_slayer_refine_shadow", "quantity": 1 },
  { "targetId": "ability_slayer_lethal_cadence", "quantity": 1 }
]
```

Add new ability nodes:
```json
{
  "id": "ability_slayer_j_spirit",
  "type": "Ability",
  "name": "J-Spirit",
  "data": { "description": "Allows junctioning magic to the Spirit stat", "primary_stat": "Spirit", "metadata": {} },
  "in_edges": {},
  "out_edges": { "consumes": [{ "targetId": "item_sun_baked_scale", "quantity": 20 }, { "targetId": "item_gold", "quantity": 3000 }] },
  "effects": []
},
{
  "id": "ability_slayer_refine_shadow",
  "type": "Ability",
  "name": "Refine Shadow",
  "data": { "description": "Distil the essence of slain foes into Shadow magic", "primary_stat": "Spirit", "metadata": {} },
  "in_edges": {},
  "out_edges": { "consumes": [{ "targetId": "item_oasis_scale", "quantity": 10 }, { "targetId": "item_gold", "quantity": 2000 }] },
  "effects": []
},
{
  "id": "ability_slayer_lethal_cadence",
  "type": "Ability",
  "name": "Lethal Cadence",
  "data": { "description": "Enhances combat quest speed through perfected technique", "primary_stat": "Strength", "metadata": {} },
  "in_edges": {},
  "out_edges": { "consumes": [{ "targetId": "item_gold", "quantity": 20000 }, { "targetId": "item_mythril_spark", "quantity": 1 }] },
  "effects": []
}
```

Add new item `item_shadow_i` and refinement node:
```json
{
  "id": "item_shadow_i",
  "type": "Item",
  "name": "Shadow I",
  "data": { "description": "A spirit-attuned shadow spell.", "item_type": "Spell" },
  "in_edges": {}, "out_edges": {}
},
{
  "id": "recipe_refine_shadow_oasis_scale_shadow_i",
  "type": "Refinement",
  "name": "Refine Shadow - Shadow I",
  "data": { "primary_stat": "Spirit" },
  "in_edges": { "requires_ability": ["ability_slayer_refine_shadow"] },
  "out_edges": {
    "consumes": [{ "targetId": "item_oasis_scale", "quantity": 3 }],
    "produces": [{ "targetId": "item_shadow_i", "quantity": 5 }]
  }
}
```

**Add to `stat_augments.json`:**
```json
{ "Item": "Shadow I", "Augments": [{ "Stat": "Spirit", "ModifierAtFull": 200 }, { "Stat": "Speed", "ModifierAtFull": 50 }] }
```

---

## Issue 3 — Alchemy II Supply Chain Is Broken
**Severity:** 🔴 Critical | **Files:** `content_graph.json`

**Sub-fix A:** Find `quest_hunt_sand_sharks`. Change:
- `"required_stats": { "Strength": 20 }` → `{ "Strength": 15 }`
- `"rewards": [{ "targetId": "item_sun_baked_scale", "quantity": 1 }]` → `"quantity": 2`

**Sub-fix B:** Add a new quest to Greenwood Forest. Find `location_greenwood_forest` and add `{ "targetId": "quest_forage_for_reagents", "quantity": 1 }` to its `out_edges.contains`. Then add the quest node:
```json
{
  "id": "quest_forage_for_reagents",
  "type": "Quest",
  "name": "Forage for Reagents",
  "data": { "description": "Gather wild herbs and roots usable for basic potions.", "duration": 90, "quest_type": "Recurring", "primary_stat": "Speed", "required_stats": {}, "stat_rewards": {} },
  "in_edges": { "requires_quest": ["quest_chop_wood"] },
  "out_edges": { "rewards": [{ "targetId": "item_potion", "quantity": 1 }] }
}
```

---

## Issue 4 — Spirit Stat Is Vestigial
**Severity:** 🔴 Critical | **Files:** `content_graph.json`
**Dependency:** Issue 2 must be done first (`item_shadow_i` must exist, `ability_slayer_j_spirit` must exist).

**Step 1:** Add J-Spirit to Arcanist. Find `cadence_arcanist`, add to `provides_ability`:
`{ "targetId": "ability_arcanist_j_spirit", "quantity": 1 }`

Add new ability node:
```json
{
  "id": "ability_arcanist_j_spirit",
  "type": "Ability",
  "name": "J-Spirit",
  "data": { "description": "Allows junctioning magic to the Spirit stat", "primary_stat": "Spirit", "metadata": {} },
  "in_edges": {},
  "out_edges": { "consumes": [{ "targetId": "item_mana_leaf", "quantity": 30 }, { "targetId": "item_gold", "quantity": 5000 }] },
  "effects": []
}
```

**Step 2:** Add Spirit-gated quests. Find `location_mythril_sanctum`, add to `contains`:
`{ "targetId": "quest_spirit_walk", "quantity": 1 }` and `{ "targetId": "quest_master_the_void", "quantity": 1 }`

Add the quest nodes:
```json
{
  "id": "quest_spirit_walk",
  "type": "Quest",
  "name": "Spirit Walk",
  "data": { "description": "Commune with the ancient energies of the Sanctum.", "duration": 300, "quest_type": "Recurring", "primary_stat": "Spirit", "required_stats": { "Spirit": 50 }, "stat_rewards": {} },
  "in_edges": { "requires_quest": ["quest_rekindling_the_spark"] },
  "out_edges": { "rewards": [{ "targetId": "item_shadow_i", "quantity": 3 }] }
},
{
  "id": "quest_master_the_void",
  "type": "Quest",
  "name": "Master the Void",
  "data": { "description": "Channel void energy through the Mythril conduit.", "duration": 900, "quest_type": "Single", "primary_stat": "Spirit", "required_stats": { "Spirit": 100 }, "stat_rewards": { "Spirit": 10 } },
  "in_edges": { "requires_quest": ["quest_spirit_walk"] },
  "out_edges": { "rewards": [{ "targetId": "item_shadow_i", "quantity": 20 }] }
}
```

> ⚠️ **Code audit required before `stat_rewards` is live:** Search `Mythril.Data/GameStateStore_Reducer.cs` for `StatRewards`. If no handler exists, either add one or remove `stat_rewards` from `quest_master_the_void` until it is implemented.

---

## Issue 5 — No Tier 2 Spells Exist
**Severity:** 🔴 Critical | **Files:** `content_graph.json`, `stat_augments.json`

**5 New Spell items** (add to content_graph.json):
```json
{ "id": "item_fire_ii",      "type": "Item", "name": "Fire II",      "data": { "description": "A concentrated fire spell.",  "item_type": "Spell" }, "in_edges": {}, "out_edges": {} },
{ "id": "item_ice_ii",       "type": "Item", "name": "Ice II",       "data": { "description": "A piercing ice spell.",        "item_type": "Spell" }, "in_edges": {}, "out_edges": {} },
{ "id": "item_lightning_ii", "type": "Item", "name": "Lightning II", "data": { "description": "A volatile lightning spell.",  "item_type": "Spell" }, "in_edges": {}, "out_edges": {} },
{ "id": "item_earth_ii",     "type": "Item", "name": "Earth II",     "data": { "description": "A tectonic earth spell.",      "item_type": "Spell" }, "in_edges": {}, "out_edges": {} },
{ "id": "item_water_ii",     "type": "Item", "name": "Water II",     "data": { "description": "A surging water spell.",       "item_type": "Spell" }, "in_edges": {}, "out_edges": {} }
```

**Add to `stat_augments.json`:**
```json
{ "Item": "Fire II",      "Augments": [{ "Stat": "Strength", "ModifierAtFull": 200 }, { "Stat": "Magic",  "ModifierAtFull": 100 }] },
{ "Item": "Ice II",       "Augments": [{ "Stat": "Magic",    "ModifierAtFull": 220 }, { "Stat": "Speed",  "ModifierAtFull": 180 }] },
{ "Item": "Lightning II", "Augments": [{ "Stat": "Magic",    "ModifierAtFull": 250 }, { "Stat": "Speed",  "ModifierAtFull": 150 }] },
{ "Item": "Earth II",     "Augments": [{ "Stat": "Strength", "ModifierAtFull": 250 }, { "Stat": "Vitality","ModifierAtFull": 200 }] },
{ "Item": "Water II",     "Augments": [{ "Stat": "Vitality", "ModifierAtFull": 255 }, { "Stat": "Speed",  "ModifierAtFull": 180 }] }
```

**New Synthesis I ability** (add to Mythril Weaver cadence):
Find `cadence_mythril_weaver`, add to `provides_ability`: `{ "targetId": "ability_mythril_weaver_synthesis_i", "quantity": 1 }`

```json
{
  "id": "ability_mythril_weaver_synthesis_i",
  "type": "Ability",
  "name": "Synthesis I",
  "data": { "description": "Master the art of compounding elemental spells into Tier 2 magic", "primary_stat": "Magic", "metadata": {} },
  "in_edges": {},
  "out_edges": { "consumes": [{ "targetId": "item_mythril_spark", "quantity": 1 }, { "targetId": "item_gold", "quantity": 10000 }] },
  "effects": []
}
```

**5 New Refinement nodes** (compound recipes):
```json
{
  "id": "recipe_synthesis_fire_ii",
  "type": "Refinement", "name": "Synthesis - Fire II", "data": { "primary_stat": "Magic" },
  "in_edges": { "requires_ability": ["ability_mythril_weaver_synthesis_i"] },
  "out_edges": { "consumes": [{ "targetId": "item_fire_i", "quantity": 5 }, { "targetId": "item_crystal_shards", "quantity": 10 }], "produces": [{ "targetId": "item_fire_ii", "quantity": 1 }] }
},
{
  "id": "recipe_synthesis_ice_ii",
  "type": "Refinement", "name": "Synthesis - Ice II", "data": { "primary_stat": "Magic" },
  "in_edges": { "requires_ability": ["ability_mythril_weaver_synthesis_i"] },
  "out_edges": { "consumes": [{ "targetId": "item_ice_i", "quantity": 5 }, { "targetId": "item_moonberry", "quantity": 20 }], "produces": [{ "targetId": "item_ice_ii", "quantity": 1 }] }
},
{
  "id": "recipe_synthesis_lightning_ii",
  "type": "Refinement", "name": "Synthesis - Lightning II", "data": { "primary_stat": "Magic" },
  "in_edges": { "requires_ability": ["ability_mythril_weaver_synthesis_i"] },
  "out_edges": { "consumes": [{ "targetId": "item_lightning_i", "quantity": 5 }, { "targetId": "item_iron_ore", "quantity": 15 }], "produces": [{ "targetId": "item_lightning_ii", "quantity": 1 }] }
},
{
  "id": "recipe_synthesis_earth_ii",
  "type": "Refinement", "name": "Synthesis - Earth II", "data": { "primary_stat": "Magic" },
  "in_edges": { "requires_ability": ["ability_mythril_weaver_synthesis_i"] },
  "out_edges": { "consumes": [{ "targetId": "item_earth_i", "quantity": 5 }, { "targetId": "item_ancient_bark", "quantity": 10 }], "produces": [{ "targetId": "item_earth_ii", "quantity": 1 }] }
},
{
  "id": "recipe_synthesis_water_ii",
  "type": "Refinement", "name": "Synthesis - Water II", "data": { "primary_stat": "Magic" },
  "in_edges": { "requires_ability": ["ability_mythril_weaver_synthesis_i"] },
  "out_edges": { "consumes": [{ "targetId": "item_water_i", "quantity": 5 }, { "targetId": "item_blue_coral", "quantity": 15 }], "produces": [{ "targetId": "item_water_ii", "quantity": 1 }] }
}
```

---

## Issue 6 — Stat Ceiling Underutilized
**Severity:** 🟡 Moderate | **No direct JSON changes needed**

This is resolved by Issues 2 (Shadow I at Spirit 200) and 5 (Tier 2 spells at 200–255). After those are implemented, run the simulation and confirm max stats are approaching 255. If Str or Magic remain below 200, consider adding a second compound recipe path for Fire II/Earth II using alternative materials.

---

## Issue 7 — J-Speed Only on Student Cadence
**Severity:** 🟡 Moderate | **Files:** `content_graph.json`

Find `cadence_geologist`, add to `provides_ability`: `{ "targetId": "ability_geologist_j_speed", "quantity": 1 }`

Add the ability node:
```json
{
  "id": "ability_geologist_j_speed",
  "type": "Ability",
  "name": "J-Speed",
  "data": { "description": "Allows junctioning magic to the Speed stat", "primary_stat": "Speed", "metadata": {} },
  "in_edges": {},
  "out_edges": { "consumes": [{ "targetId": "item_crystal_shards", "quantity": 30 }, { "targetId": "item_gold", "quantity": 3000 }] },
  "effects": []
}
```

---

## Issue 8 — Archive Sifting Throughput Is Too Low
**Severity:** 🟡 Moderate | **Files:** `content_graph.json`

Find `quest_archive_sifting`. Apply two changes:
1. Change `"duration": 180` → `"duration": 120`
2. Change `"rewards": [{ "targetId": "item_lost_parchment", "quantity": 1 }]` → `"quantity": 2`

---

## Issue 9 — Stat Rewards Not Implemented in Reducer
**Severity:** 🟡 Moderate | **Files:** `Mythril.Data/GameStateStore_Reducer.cs`

Search `GameStateStore_Reducer.cs` for `StatRewards`. If no handler applies them on quest completion, add one that reads `QuestDetail.StatRewards` and applies each stat bonus. The stat boost should be stored in `SaveData.CharacterStatBoosts` or a global permanent boost dictionary.

If the `stat_rewards` feature is deferred, remove `"stat_rewards": { "Spirit": 10 }` from `quest_master_the_void` (Issue 4) until it is implemented and replace with an empty `{}`.

---

## Issue 10 — Crystal Peaks Is Thin Behind a Hard Gate
**Severity:** 🟡 Moderate | **Files:** `content_graph.json`

Find `location_crystal_peaks`. Add to `contains`:
`{ "targetId": "quest_harvest_geodes", "quantity": 1 }` and `{ "targetId": "quest_defeat_crystal_warden", "quantity": 1 }`

Add quest nodes:
```json
{
  "id": "quest_harvest_geodes",
  "type": "Quest",
  "name": "Harvest Geodes",
  "data": { "description": "Crack open crystal formations for their raw magical cores.", "duration": 150, "quest_type": "Recurring", "primary_stat": "Strength", "required_stats": { "Strength": 40 }, "stat_rewards": {} },
  "in_edges": { "requires_quest": ["quest_power_the_forge"] },
  "out_edges": { "rewards": [{ "targetId": "item_crystal_shards", "quantity": 3 }] }
},
{
  "id": "quest_defeat_crystal_warden",
  "type": "Quest",
  "name": "Defeat the Crystal Warden",
  "data": { "description": "An ancient golem formed from the peak's own minerals stands guard.", "duration": 1200, "quest_type": "Single", "primary_stat": "Strength", "required_stats": { "Strength": 60, "Magic": 40 }, "stat_rewards": {} },
  "in_edges": { "requires_quest": ["quest_shatter_the_crystals"] },
  "out_edges": { "rewards": [{ "targetId": "item_mythril_spark", "quantity": 1 }, { "targetId": "item_crystal_shards", "quantity": 20 }] }
}
```

> ⚠️ `quest_defeat_crystal_warden` rewards a `Mythril Spark`. After adding this, run the full simulation to ensure this doesn't unintentionally trivialize the Mythril Weaver cadence unlock pacing. If it does, change the reward to `item_pure_water_crystal` instead.

---

## Final Verification Checklist

```powershell
# 1. Validate all graph contracts
python scripts/verify_graph.py

# 2. Full health check (fast mode)
python scripts/check_health.py --skip-tests

# 3. Review simulation_report.md and confirm:
#    - All quests reachable (✅ no orphans)
#    - Routed Completion Time significantly > 84.4m
#    - Unsustainable activities list reduced
#    - Max stats approaching 255 on at least Str/Magic/Vit

# 4. Run dotnet tests
dotnet test

# 5. Update pacing baseline
#    Edit docs/pacing_baseline.json:
#    { "routed_completion_time_minutes": <new_value>, "reachable_quests": <new_count> }

# 6. Commit and push
git add .; git commit -m "feat(content): fix all 10 content gaps - Tier 2 spells, Spirit stat, Hidden Oasis, Slayer expansion"; git push origin main
```

---

## Content File Summary

| What | File |
|---|---|
| All quests, items, cadences, abilities, refinements, locations | `Mythril.Blazor/wwwroot/data/content_graph.json` |
| Spell junction stat values | `Mythril.Blazor/wwwroot/data/stat_augments.json` |
| Pacing baseline | `docs/pacing_baseline.json` |
| Graph validation | `python scripts/verify_graph.py` |
| Full health suite | `python scripts/check_health.py` |
| Simulation output | `simulation_report.md` (regenerated each run) |
| Design decisions | `docs/guidence_knowledge_base.md` |
| Game state reducer | `Mythril.Data/GameStateStore_Reducer.cs` |

---

## Expected Outcome After All Issues Fixed

| Metric | Before | After (Estimated) |
|---|---|---|
| Quests | 33 | ~45–48 |
| Spells | 7 | ~13 |
| Cadence Abilities | 34 | ~42 |
| Cadences with J-Spirit | 0 | 2 (Slayer, Arcanist) |
| Cadences with J-Speed | 1 | 2 (Student, Geologist) |
| Max Spirit | 25 | ~200+ |
| Routed Completion Time | 84.4 min | ~2.5–4 hours |
