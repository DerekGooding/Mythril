# Recommended Visualization Strategy for Mythril Content Review

To facilitate educated decisions when moving content and balancing gameplay, the visualization system should evolve from a purely structural graph to a **Quantitative Progression Dashboard**.

## 1. Primary Visualization: The Chrono-Lattice
Instead of simple BFS tiers, the X-axis should represent **Simulated Unlock Time** (from the C# Lattice Solver).

*   **Pacing Awareness**: Designers can immediately see "content clusters" and "pacing deserts" (long horizontal gaps).
*   **Scale**: Use a logarithmic scale or "Logistics Tiers" to handle the exponential nature of incremental progression.
*   **Visual Anchors**: Major milestones (Cadence unlocks, Location changes) should be vertical "gates" that content must pass through.

## 2. Quantitative Flow Overlays
Leverage the **Quantitative Resource Flow Analysis** to visualize the economy's health.

*   **Flow Thickness**: Edge thickness should represent the magnitude of resource flow (Rate / Duration).
*   **Sustainability Heatmap**: 
    *   **Green**: Net-positive production (Sustainable).
    *   **Red**: Net-negative/Draining (Unsustainable).
    *   **Gray**: Stagnant/Unused.
*   **Loop Detection**: Highlight self-amplifying resource loops (Positive Feedback) in a distinct "Electric" color to warn of potential balance breaks.

## 3. The "Gating" Layer
Explicitly visualize the constraints that are *not* items.

*   **Stat Gating**: Show Stat requirements as "Walls" or "Fog" that prevent progression until the Lattice Solver determines the Stat Max is high enough.
*   **Bottleneck Highlighting**: Pulse the edges or nodes that are the **sole constraint** preventing the next 3 pieces of content from unlocking.

## 4. Interaction for Content Moving
To support the user's goal of "moving content around", the visualization should support:

*   **Ghost Dependencies**: The ability to select a node and "preview" its position in the Chrono-Lattice if a requirement is removed or changed.
*   **Drift Analysis**: Visually show how "moving" a mid-game quest to the early-game compresses the early-game tiers and creates a "power spike" (visualized as a vertical stack of newly reachable content).

## 5. Implementation Roadmap
1.  **Data Integration**: Update `data_processor.py` to ingest the `QuestTime` and `ResourceRate` outputs from the C# Simulation reports.
2.  **X-Axis Mapping**: Replace `n["x"] = t * TIER_WIDTH` with a mapping derived from `simulation_report.json -> QuestTime`.
3.  **Color Schema**: Align colors with `docs/QuantitativeFlow.md` directives (Sustainable vs Unsustainable).
4.  **Filter Controls**:
    *   *Progression Only*: Hide items/refinements, show only Quest -> Quest / Quest -> Cadence links.
    *   *Economic Only*: Show the flow of a specific resource (e.g., "Wood Flow").
    *   *Stall Finder*: Highlight nodes where `Time to Content > Threshold`.

## 6. Summary of Benefits
*   **Empirical Decisions**: Moving a quest is no longer a "guess"; the impact on the timeline is visible.
*   **Dead Content Identification**: Orphaned or impossible content (Time = Infinity) is immediately obvious.
*   **Visual Pacing**: The "Shape" of the graph tells the story of the game's difficulty curve.
