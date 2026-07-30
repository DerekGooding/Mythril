"""
Shared configuration and failure registry for all health check modules.
Loaded once from scripts/config.json; all check modules import from here.
"""
import json
import re
from pathlib import Path

CONFIG_PATH = Path(__file__).parent.parent / "config.json"

if CONFIG_PATH.exists():
    with open(CONFIG_PATH, "r") as f:
        _config = json.load(f)
else:
    _config = {}

MAX_LINES_PER_FILE        = _config.get("MAX_LINES_PER_FILE",         250)
MIN_OVERALL_COVERAGE      = _config.get("MIN_OVERALL_COVERAGE",        70.0)
MIN_FILE_COVERAGE         = _config.get("MIN_FILE_COVERAGE",           25.0)
MIN_BRANCH_COVERAGE       = _config.get("MIN_BRANCH_COVERAGE",         50.0)
MIN_MUTATION_SCORE        = _config.get("MIN_MUTATION_SCORE",           60.0)
DOCS_STALENESS_THRESHOLD  = _config.get("DOCS_STALENESS_THRESHOLD",    10)
MUTATION_STALENESS_THRESHOLD = _config.get("MUTATION_STALENESS_THRESHOLD", 5)

SOURCE_DIR         = Path(_config.get("SOURCE_DIR",       "."))
RESULTS_DIR        = Path(_config.get("RESULTS_DIR",      "TestResults"))
SOURCE_EXTENSIONS  = _config.get("SOURCE_EXTENSIONS",     [".cs", ".py", ".js", ".ts", ".razor"])
DOC_FILES          = _config.get("DOC_FILES",             ["docs/instructions.md"])
FEEDBACK_DIR       = Path(_config.get("FEEDBACK_DIR",     "docs/feedback"))
ERRORS_DIR         = Path(_config.get("ERRORS_DIR",       "docs/errors"))
TEST_COMMAND       = _config.get("TEST_COMMAND",          ["dotnet", "test"])
COVERAGE_PATTERN   = re.compile(_config.get("COVERAGE_REPORT_PATTERN", r"coverage\.cobertura\.xml"))

IGNORED_FILES = [
    "Models.cs", "Cadences.cs", "Program.cs",
    "ReachabilitySimulator.cs", "FlowSimulator.cs",
    "LatticeSimulator.cs", "RoutedSimulator.cs",
    "GameStateStore.cs", "GameStateStore_Reducer.cs",
    "GameState.cs", "GameActions.cs",
    "AdditionalUIComponentTests.cs", "ResourceManager_Inventory.cs",
]

# Module-level failure list - all checks append here.
FAILURES: list[dict] = []


def record_failure(category: str, message: str, metadata: dict | None = None) -> None:
    FAILURES.append({
        "category": category,
        "message":  message,
        "metadata": metadata or {},
    })
    print(f"[FAIL] {category}: {message}")
