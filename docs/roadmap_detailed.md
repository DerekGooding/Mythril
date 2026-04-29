# Detailed Roadmap: Visualization Noise Reduction

## 1. Data Processor Gating (`modules/visualization/data_processor.py`)
- **Hub Detection:** Any node with > 10 incoming or outgoing edges will be marked as a `ResourceHub`.
- **Edge Metadata:** Each edge in the JSON will be tagged with a `category`:
    - `progression`: `unlocks_cadence`, `requires_quest`, `contains`.
    - `economy`: `consumes`, `produces`, `rewards`.
- **Milestone Identification:**
    - Quests that unlock locations or cadences.
    - Items that are "Sustainable" per `simulation_report.md`.

## 2. Visual Filtering (`modules/visualization/lattice_rendering.js` & `lattice_data.js`)
- **Filtered Edge Loading:** `lattice_data.js` will maintain two sets of edges: `activeEdges` and `allEdges`.
- **Style Overrides:**
    - Milestones: Gold stroke/glow, larger scale.
    - Hubs: Dimmed by default, no labels unless hovered.
    - Progression Edges: Thicker lines with distinct colors.

## 3. UI/UX Enhancements (`modules/visualization/template_engine.py`)
- **Control Panel:**
    - Toggle: `Show Resource Hubs` (Default: Off)
    - Toggle: `Progression Only` (Default: On)
    - Toggle: `Simulation Overlay` (Default: Off)
- **Tooltip Upgrades:** Show net flow rates (from simulation) in item tooltips.

## 4. Simulation Integration
- **Parser:** A new utility to scrape `simulation_report.md` for:
    - Sustainable vs Unsustainable list.
    - Completion times.
    - Critical bottlenecks.
- **Visual Link:** Nodes in the graph will color-code based on their sustainability status.
