# Persistent Problems

This document lists problems that the Gemini CLI agent has been unable to solve despite multiple attempts. These issues are primarily related to the Myra UI library and its integration within the current project setup.

## 1. Myra UI Input Event System (Critical Blocker)

**Problem:** The most critical and persistent issue is correctly implementing Myra UI's input event system for custom widgets (`CardWidget`, `DropZoneWidget`). Despite extensive investigation into Myra's source code and various attempts to use `MouseEventArgs` (which appears to not exist or be inaccessible) or `EventArgs` with event subscriptions, compilation consistently fails.

*   **Specific Errors:**
    *   `CS0234: The type or namespace name 'Input' does not exist in the namespace 'Myra.Graphics2D'` (This suggests `Myra.Graphics2D.Input` is not recognized, even though it's where `MouseEventArgs` is expected to be).
    *   `CS0115: 'Widget.OnMouse...(EventArgs)': no suitable method found to override` (This indicates that `OnMouseDown`, `OnMouseUp`, `OnMouseMove`, `OnMouseEnter`, `OnMouseLeave` are not intended to be overridden with `EventArgs` as a parameter, despite initial assumptions from Myra's `Widget.cs` source).
    *   `CS1061: 'Widget' does not contain a definition for 'MouseDown' (or MouseUp, MouseMoved, MouseEntered, MouseExited) and no accessible extension method...` (This suggests that direct event subscriptions like `this.MouseDown += ...` are also not working as expected, or the events themselves are not publicly exposed on the `Widget` class as initially thought).
    *   `CS0246: The type or namespace name 'MouseEventArgs' could not be found` (This is the original, persistent error, indicating the core type for mouse event data is not being resolved).

**Impact:** This issue is a critical blocker, preventing any interactive UI elements (drag-and-drop, hover effects, button clicks) from functioning correctly. Without a clear understanding of Myra's input event API, further UI development is impossible.

## 2. Myra UI Styling Properties

**Problem:** The agent has been unable to correctly implement or utilize certain Myra UI styling properties programmatically. While some progress was made by using skin XML files, direct programmatic control over properties like `Background` and `Padding` (using `SolidBrush` or `Thickness`) still leads to compilation errors or unexpected behavior.

*   **Specific Errors:**
    *   `CS0144: Cannot create an instance of the abstract type or interface 'ProgressBar'` (Encountered when trying to instantiate `ProgressBar` directly, indicating it might be an abstract class or interface that requires a concrete implementation or factory method).
    *   `CS0117: 'Button' does not contain a definition for 'Text'` (Indicates that `Button` content is not set via a `Text` property, but likely through its `Content` property, typically a `Label`).

**Impact:** This limits the ability to dynamically style UI elements and requires all styling to be managed through external skin files, which can be less flexible for dynamic UI changes.

## 3. Project Integration Issues

**Problem:** Several integration issues have arisen, indicating a lack of complete understanding of the project's architecture and dependencies.

*   **Specific Errors:**
    *   `CS7036: There is no argument given that corresponds to the required parameter 'resourceManager' of 'GameManager.GameManager(ResourceManager)'` (Indicates a missing parameter in a constructor call).
    *   `CS0234: The type or namespace name 'AssetManager' does not exist in the namespace 'Myra.Utility'` (Suggests `Myra.Utility` or `AssetManager` is not correctly referenced or is in a different namespace).
    *   `CS0234: The type or namespace name 'Stylesheet' does not exist in the namespace 'Myra.Graphics2D.UI'` (Suggests `Myra.Graphics2D.UI` or `Stylesheet` is not correctly referenced or is in a different namespace).

**Impact:** These errors prevent the project from compiling and running, indicating fundamental issues with how different parts of the game (UI, game logic, resource management) are connected.

## Conclusion

The cumulative effect of these issues, particularly the critical input event system problem, renders the project unbuildable and prevents further progress. The discrepancies between expected Myra UI API usage (based on common patterns and initial `troubleshooting.md` information) and actual API behavior (as revealed by compilation errors and direct source code inspection) are significant.

Further investigation by a human developer with direct access to the Myra UI documentation, source code, and a working example project is strongly recommended to resolve these fundamental problems.