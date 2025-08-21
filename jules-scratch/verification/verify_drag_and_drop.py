from playwright.sync_api import Page, expect
import traceback

def test_drag_and_drop(page: Page):
    with open("jules-scratch/verification/output.log", "w") as f:
        try:
            f.write("Navigating to http://localhost:5132\n")
            page.goto("http://localhost:5132")
            f.write("Navigation complete\n")

            f.write("Waiting for 'Party' text to be visible\n")
            expect(page.get_by_text("Party")).to_be_visible()
            f.write("'Party' text is visible\n")

            f.write("Getting the first task\n")
            task = page.locator(".task").first
            f.write("Getting the first drop zone\n")
            drop_zone = page.locator(".character-display .drop-zone").first

            f.write("Dragging and dropping\n")
            task.drag_to(drop_zone)
            f.write("Drag and drop complete\n")

            f.write("Waiting for progress bar to be visible\n")
            expect(page.locator(".progress-bar")).to_be_visible()
            f.write("Progress bar is visible\n")

            f.write("Taking screenshot\n")
            page.screenshot(path="jules-scratch/verification/verification.png")
            f.write("Screenshot taken\n")
        except Exception as e:
            f.write(f"An error occurred: {e}\n")
            f.write(traceback.format_exc())
