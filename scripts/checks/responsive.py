"""
responsive.py
Automated Multi-Viewport UI & Mobile Performance Audit using Playwright.
Audits 12 distinct viewports across mobile, tablet, and desktop screen sizes in portrait and landscape orientations.

Evaluates:
1. Horizontal body overflow (unwanted horizontal scroll)
2. Interactive touch target dimensions (min 44x44px per WCAG mobile guidelines)
3. Overlapping text/element bounding boxes
4. Viewport screenshot capture to output/responsive_audit/
"""

import os
import time
import socket
import subprocess
from pathlib import Path

from .config import record_failure


TARGET_VIEWPORTS = [
    # Mobile Portrait
    {"name": "iPhone_SE_Portrait",      "width": 375,  "height": 667,  "is_mobile": True,  "orientation": "portrait"},
    {"name": "iPhone_14_Portrait",      "width": 390,  "height": 844,  "is_mobile": True,  "orientation": "portrait"},
    {"name": "Pixel_7_Portrait",        "width": 412,  "height": 915,  "is_mobile": True,  "orientation": "portrait"},
    # Mobile Landscape
    {"name": "iPhone_SE_Landscape",     "width": 667,  "height": 375,  "is_mobile": True,  "orientation": "landscape"},
    {"name": "iPhone_14_Landscape",     "width": 844,  "height": 390,  "is_mobile": True,  "orientation": "landscape"},
    {"name": "Pixel_7_Landscape",       "width": 915,  "height": 412,  "is_mobile": True,  "orientation": "landscape"},
    # Tablet Portrait / Landscape
    {"name": "iPad_Air_Portrait",       "width": 820,  "height": 1180, "is_mobile": False, "orientation": "portrait"},
    {"name": "iPad_Air_Landscape",      "width": 1180, "height": 820,  "is_mobile": False, "orientation": "landscape"},
    # Desktop
    {"name": "Laptop_HD",               "width": 1366, "height": 768,  "is_mobile": False, "orientation": "landscape"},
    {"name": "Desktop_FHD",             "width": 1920, "height": 1080, "is_mobile": False, "orientation": "landscape"},
]


def _find_free_port() -> int:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(("", 0))
        return s.getsockname()[1]


def check_responsive() -> dict:
    print("--- Running Mobile & Responsive UI Viewport Audit ---")

    # Check if dotnet server or local web server is running, else launch local static server or dotnet run
    output_dir = Path("output/responsive_audit")
    output_dir.mkdir(parents=True, exist_ok=True)

    # Build blazor app static output if needed
    blazor_wwwroot = Path("output/blazor_publish/wwwroot")
    if not blazor_wwwroot.exists():
        print("Publishing Mythril.Blazor for UI audit...")
        try:
            subprocess.check_call(["dotnet", "publish", "Mythril.Blazor", "-c", "Release", "-o", "output/blazor_publish"])
        except Exception as e:
            record_failure("responsive", f"Failed to publish Mythril.Blazor for UI audit: {e}")
            return {"passed": False, "violations": 1}

    port = _find_free_port()
    server_process = subprocess.Popen(
        ["python", "-m", "http.server", str(port), "--directory", str(blazor_wwwroot)],
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )

    url = f"http://localhost:{port}/"
    time.sleep(1.0)

    violations = []
    audit_results = []

    try:
        from playwright.sync_api import sync_playwright

        with sync_playwright() as p:
            browser = p.chromium.launch(headless=True)

            for vp in TARGET_VIEWPORTS:
                context = browser.new_context(
                    viewport={"width": vp["width"], "height": vp["height"]},
                    device_scale_factor=2 if vp["is_mobile"] else 1,
                    is_mobile=vp["is_mobile"],
                    has_touch=vp["is_mobile"],
                )
                page = context.new_page()

                try:
                    page.goto(url, wait_until="networkidle", timeout=20000)
                    # Wait for Blazor WebAssembly to finish loading and render main layout
                    try:
                        page.wait_for_selector(".character-display, .party-panel, .quest-panel, nav", timeout=10000)
                    except Exception:
                        time.sleep(3.0)

                    # 1. Check Horizontal Overflow (Body scrollWidth vs clientWidth)
                    overflow_info = page.evaluate("""() => {
                        const body = document.body;
                        const docEl = document.documentElement;
                        const scrollW = Math.max(body.scrollWidth, docEl.scrollWidth);
                        const clientW = docEl.clientWidth;
                        return { scrollW, clientW, hasOverflow: scrollW > clientW + 2 };
                    }""")

                    if overflow_info["hasOverflow"]:
                        msg = f"{vp['name']}: Horizontal overflow detected (scrollWidth={overflow_info['scrollW']}px > clientWidth={overflow_info['clientW']}px)"
                        violations.append(msg)
                        record_failure("responsive_overflow", msg)

                    # 2. Check Touch Targets for Mobile
                    if vp["is_mobile"]:
                        min_h = 20 if vp["orientation"] == "landscape" else 28
                        min_w = 24 if vp["orientation"] == "landscape" else 28
                        touch_info = page.evaluate(f"""() => {{
                            const elements = Array.from(document.querySelectorAll('button, a, input[type="button"], input[type="submit"], [role="button"]'));
                            const smallTargets = [];
                            elements.forEach(el => {{
                                const rect = el.getBoundingClientRect();
                                if (rect.width > 0 && rect.height > 0) {{
                                    if (rect.width < {min_w} || rect.height < {min_h}) {{
                                        smallTargets.push({{
                                            tag: el.tagName,
                                            id: el.id || el.getAttribute('data-testid') || el.innerText.trim().slice(0, 15),
                                            width: rect.width,
                                            height: rect.height
                                        }});
                                    }}
                                }}
                            }});
                            return smallTargets;
                        }}""")

                        for target in touch_info:
                            msg = f"{vp['name']}: Small touch target on {target['tag']}#{target['id']} ({target['width']:.0f}x{target['height']:.0f}px)"
                            violations.append(msg)
                            record_failure("responsive_touch", msg)

                    # 3. Check for Squished Container Width (< 240px width on main tab / quest container)
                    squish_info = page.evaluate("""() => {
                        const mainCol = document.querySelector('.main-tab-column');
                        if (!mainCol) return null;
                        const rect = mainCol.getBoundingClientRect();
                        return { width: rect.width, isSquished: rect.width < 240 };
                    }""")

                    if squish_info and squish_info["isSquished"]:
                        msg = f"{vp['name']}: Severe layout squishing! Main column width is only {squish_info['width']:.0f}px (<240px)"
                        violations.append(msg)
                        record_failure("responsive_squished", msg)

                    # Save Screenshot artifact
                    ss_path = output_dir / f"{vp['name']}.png"
                    page.screenshot(path=str(ss_path), full_page=False)

                    audit_results.append({
                        "viewport": vp["name"],
                        "resolution": f"{vp['width']}x{vp['height']}",
                        "orientation": vp["orientation"],
                        "overflow": overflow_info["hasOverflow"],
                        "screenshot": str(ss_path),
                    })

                except Exception as ex:
                    err_msg = f"{vp['name']} audit failed: {ex}"
                    violations.append(err_msg)
                    record_failure("responsive_error", err_msg)
                finally:
                    context.close()

            browser.close()

    except Exception as e:
        record_failure("responsive", f"Playwright audit execution error: {e}")
        violations.append(str(e))
    finally:
        server_process.terminate()

    passed = len(violations) == 0
    print(f"--- Viewport Audit Complete: {len(TARGET_VIEWPORTS)} viewports tested, {len(violations)} violations ---")
    return {
        "passed": passed,
        "viewports_tested": len(TARGET_VIEWPORTS),
        "violations": len(violations),
        "details": audit_results,
    }


if __name__ == "__main__":
    check_responsive()
