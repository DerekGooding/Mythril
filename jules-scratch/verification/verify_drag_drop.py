from playwright.sync_api import sync_playwright, expect

def run(playwright):
    browser = playwright.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    # Listen for console messages and print them
    page.on("console", lambda msg: print(f"CONSOLE: {msg.text}"))

    try:
        page.goto("http://localhost:5078/")

        # Give the page time to load, Blazor WASM can be slow to start
        page.wait_for_selector(".card", timeout=10000)

        # Locate the source draggable element
        source_task = page.locator(".card", has_text="Implement the Core Game Loop").first
        expect(source_task).to_be_visible()

        # Locate the target drop zone
        drop_zone = page.get_by_text("Drop a task here to assign it.")
        expect(drop_zone).to_be_visible()

        # Perform the drag and drop
        source_task.drag_to(drop_zone)

        # After the drop, the task should be gone from the original list.
        expect(source_task).not_to_be_visible()

        page.screenshot(path="jules-scratch/verification/verification.png")

    except Exception as e:
        print(f"An error occurred: {e}")
        page.screenshot(path="jules-scratch/verification/error.png")

    finally:
        browser.close()

with sync_playwright() as playwright:
    run(playwright)
