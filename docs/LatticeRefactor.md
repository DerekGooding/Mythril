# Convert ReachabilitySimulator to Constraint Lattice Model

You are refactoring a headless reachability simulator for an incremental game.

The current implementation is an imperative mutation loop with arbitrary iteration caps and mutable collections. Replace it entirely with a formal constraint lattice solver.

This is an architectural rewrite, not a patch.

Do not preserve mutation based patterns unless required for performance and proven safe.

---

## 1. Define the Lattice State

Create an immutable GameState record representing the full simulation state:

- ResourceTime: ImmutableDictionary<string, double>
- QuestTime: ImmutableDictionary<string, double>
- StatMax: ImmutableDictionary<string, int>
- UnlockedAbilities: ImmutableHashSet<string>
- UnlockedCadences: ImmutableHashSet<string>

No mutable fields allowed inside the solver.

---

## 2. Define Partial Ordering

Implement a component wise partial ordering:

State A ≤ State B if:

- For all resources: A.ResourceTime[r] >= B.ResourceTime[r]
- For all quests: A.QuestTime[q] >= B.QuestTime[q]
- For all stats: A.StatMax[s] <= B.StatMax[s]
- A.UnlockedAbilities ⊆ B.UnlockedAbilities
- A.UnlockedCadences ⊆ B.UnlockedCadences

Document this clearly in code comments.

---

## 3. Implement Join Operator

Define a pure Join(GameState a, GameState b) method that returns:

- ResourceTime[r] = Min(a, b)
- QuestTime[q]    = Min(a, b)
- StatMax[s]      = Max(a, b)
- UnlockedAbilities = Union
- UnlockedCadences = Union

Join must be:

- Associative
- Commutative
- Idempotent

Add unit tests verifying these properties.

---

## 4. Define Bottom Element

Create a Bottom(SimulationSeed seed) method that initializes:

- All times = PositiveInfinity
- Stats = seed starting stats
- Unlocked sets = seed values

No implicit defaults allowed.

Introduce a formal SimulationSeed type containing:

- StartingResources
- StartingStats
- StartingUnlockedCadences
- StartingUnlockedAbilities

---

## 5. Replace Imperative Loop With Fixpoint Solver

Implement:
```c#
GameState Solve()
{
var state = Bottom(seed);

while (true)
{
    var candidate = ApplyTransfers(state);
    var next = Join(state, candidate);

    if (next.Equals(state))
        return state;

    state = next;
}

}
```

No iteration caps.
No mutation.
No changed flag.

Convergence must be guaranteed by monotonic state growth.

---

## 6. Convert All Logic Into Transfer Functions

Break current logic into pure functions:

- ApplyQuestTransfers(GameState)
- ApplyRefinementTransfers(GameState)
- ApplyAbilityUnlockTransfers(GameState)
- ApplyStatTransfers(GameState)

Each function:

- Takes GameState
- Returns a partial GameState representing improvements
- Does not mutate input

Combine results inside ApplyTransfers.

---

## 7. Enforce Monotonicity

Add runtime assertions ensuring:

- ResourceTime never increases
- QuestTime never increases
- StatMax never decreases
- Unlock sets never shrink

If violated, throw validation exception.

---

## 8. Remove String Based Ability Logic

Eliminate name based checks such as "Magic Pocket I".

Move scaling behavior into ability metadata so transfer logic is data driven.

---

## 9. Dead Content Detection

After Solve():

- Any QuestTime == Infinity → orphaned quest
- Any resource required but ResourceTime == Infinity → unreachable resource
- Any cadence or ability never unlocked → unreachable unlock
- Any stat requirement exceeding achievable StatMax → impossible stat gate

Produce structured validation report.

---

## 10. Time To New Content Metric

Using final QuestTime and ResourceTime:

Define new content events as first finite time of:

- Quest
- Resource
- Ability
- Cadence

Sort events by time.
Compute:

- Longest stall
- Average stall
- Stall intervals above configurable threshold

Include in report.

---

## 11. Add Unit Tests

Add tests covering:

- Circular quest dependencies
- Circular refinement chains
- Stat gate deadlocks
- Self dependent resource recipes
- Simple valid progression scenario

Tests must confirm solver convergence without iteration cap.

---

## 12. Output Summary

After implementation, output:

- Overview of new architecture
- Explanation of convergence guarantee
- List of guarantees provided
- Known limitations

Do not include commentary unrelated to implementation.