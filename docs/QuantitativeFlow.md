# Extend Lattice Solver With Quantitative Resource Flow Analysis

The project already contains a constraint lattice based reachability solver.

Extend the system with a quantitative resource flow layer that models recurring production and consumption sustainability.

Do not replace the reachability solver.
Add a separate quantitative analysis layer that consumes its output.

This is an architectural extension.

---

## 1. Introduce QuantitativeFlowState

Create a new immutable record:

- ResourceRate: ImmutableDictionary<string, double>
- ResourceNet: ImmutableDictionary<string, double>
- SustainableActivities: ImmutableHashSet<string>
- UnsustainableActivities: ImmutableHashSet<string>

This is separate from GameState.

---

## 2. Scope of Quantitative Analysis

Only analyze:

- Recurring quests
- Repeatable refinements
- Passive production abilities
- Any system flagged as recurring in content definitions

Ignore one time quests.

---

## 3. Convert Recurring Content Into Flow Contributions

For each recurring quest or refinement:

Compute per second deltas:

- For each consumed resource:
  rate -= quantity / duration
- For each produced resource:
  rate += quantity / duration

Store these as ActivityFlow definitions.

Introduce a formal ActivityFlow type:

- Name
- Duration
- Input resources
- Output resources

---

## 4. Determine Feasible Activity Set

An activity is sustainable if:

For all consumed resources:
    current net production rate >= required rate

Implement iterative activation:

1. Start with activities that require no inputs.
2. Add their flow contributions.
3. Re-evaluate which activities become sustainable.
4. Repeat until fixpoint.

This is a monotonic activation lattice.

Termination guaranteed because activities only transition from inactive → active.

---

## 5. Detect Unsustainable Recurring Content

For every recurring quest or refinement:

If it is reachable but never becomes sustainable:
    classify as Unsustainable

Add to validation report.

---

## 6. Detect Positive Feedback Infinite Loops

After fixpoint:

For each resource:

If ResourceNet[resource] > 0
AND no explicit capacity cap exists
AND resource contributes to its own production chain
THEN flag as Unbounded Growth Loop

Implement simple cycle detection in activity dependency graph to confirm self amplification.

---

## 7. Derive Steady State Time Estimates

For each resource required by unreachable quests:

If ResourceNet[r] > 0:
    TimeToAmount = RequiredAmount / ResourceNet[r]

Use this to compute:

- Steady state unlock times
- Content pacing projections

Integrate with Time To New Content metrics.

---

## 8. Detect Economic Stalls

Define stall threshold T seconds.

For each next unlockable content:

If steady state time to required resources > T:
    mark as Economic Stall

Report:

- Longest projected stall
- Average projected stall
- Resources causing bottleneck

---

## 9. Validation Report Extensions

Extend validation JSON with:

- Sustainable activities
- Unsustainable activities
- Net resource rates
- Infinite growth loops
- Economic stalls
- Projected unlock times

---

## 10. Guarantees Required

Ensure:

- Activation process is monotonic
- No activity deactivates once activated
- Solver terminates deterministically
- No floating point instability causes oscillation

Add unit tests for:

- Simple production chain
- Self sustaining loop
- Draining loop
- Infinite amplification loop
- Bottleneck progression scenario

---

## 11. Do Not

- Do not merge flow logic into reachability solver.
- Do not mutate existing lattice structures.
- Do not introduce arbitrary iteration caps.
- Do not hard code resource names.

---

## Output Summary

After implementation provide:

1. Explanation of activation lattice.
2. Explanation of sustainability detection.
3. Description of infinite growth detection.
4. Known limitations of steady state approximation.