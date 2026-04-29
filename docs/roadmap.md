# Visualization Noise Reduction Roadmap

## Goal
Transform the Mythril Lattice visualization from a cluttered "hairball" into a clear progression and economic map by gating high-frequency resource nodes and highlighting simulation milestones.

## Phase 1: Data Architecture & Gating
- [ ] Implement hub detection in `data_processor.py` (Frequency thresholding).
- [ ] Categorize edges into `Core` (Progression) vs `Auxiliary` (Economy).
- [ ] Integrate simulation results for `Sustainability` and `Milestone` flagging.

## Phase 2: Visual Logic & Interactivity
- [ ] Add filtering logic to `lattice_data.js`.
- [ ] Implement milestone-specific rendering styles in `lattice_rendering.js`.
- [ ] Add hover-driven edge visibility for auxiliary links.

## Phase 3: UI/UX Controls
- [ ] Add Dashboard controls for "Progression Only" and "Hide Resource Hubs".
- [ ] Implement sustainability overlays based on health check metrics.
- [ ] Final validation with `scripts/check_health.py`.
