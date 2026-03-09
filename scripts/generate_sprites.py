import os
import random
import hashlib
import json
from PIL import Image, ImageDraw, ImageFont

# DB8 Palette (Approximate)
PALETTE = [
    (20, 12, 28, 255),    # 0: Black/Deep Purple (Outline)
    (68, 36, 52, 255),    # 1: Dark Red
    (48, 52, 109, 255),   # 2: Dark Blue
    (78, 74, 78, 255),    # 3: Grey
    (133, 76, 48, 255),   # 4: Brown
    (52, 101, 36, 255),   # 5: Green
    (208, 70, 72, 255),   # 6: Red
    (117, 113, 97, 255),  # 7: Stone
    (89, 125, 206, 255),  # 8: Sky Blue
    (210, 125, 44, 255),  # 9: Orange
    (133, 149, 161, 255), # 10: Light Grey
    (109, 170, 44, 255),  # 11: Lime
    (210, 170, 153, 255), # 12: Skin/Peach
    (109, 194, 202, 255), # 13: Cyan
    (238, 221, 175, 255), # 14: Yellow/Parchment
    (255, 255, 255, 255)  # 15: White
]

OUTPUT_DIR = "Mythril.Blazor/wwwroot/assets/sprites"
ITEMS_JSON = "Mythril.Blazor/wwwroot/data/items.json"

def ensure_dir():
    if not os.path.exists(OUTPUT_DIR):
        os.makedirs(OUTPUT_DIR)

def get_color_from_hash(name, salt=""):
    h = hashlib.md5((name + salt).encode()).hexdigest()
    # Avoid index 0 (black/outline) and 15 (pure white) for primary colors
    idx = 1 + (int(h, 16) % (len(PALETTE) - 2)) 
    return PALETTE[idx]

def generate_symmetrical_sprite(name, size=32):
    """Generates a symmetrical pixel art sprite based on a hash of the name."""
    # Work at 16x16 for pixel-perfect look when scaled to 32x32
    render_size = 16
    img = Image.new("RGBA", (render_size, render_size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    seed = int(hashlib.md5(name.encode()).hexdigest(), 16)
    random.seed(seed)
    
    primary_color = get_color_from_hash(name, "primary")
    secondary_color = get_color_from_hash(name, "secondary")
    outline_color = PALETTE[0]
    
    # Random object shape: width 4-6 (mirrored to 8-12), height 6-12
    w = random.randint(3, 5) 
    h = random.randint(6, 12)
    
    start_x = (render_size // 2) - w
    start_y = (render_size - h) // 2
    
    pixels = set()
    
    for y in range(h):
        # Probability density: higher in center and middle height
        for x in range(w):
            # Center of the half-width is (w-1)
            dist_from_center = (w - 1 - x) / w
            prob = 0.8 - dist_from_center * 0.5
            
            if random.random() < prob:
                color = primary_color if random.random() > 0.4 else secondary_color
                
                px = start_x + x
                mx = start_x + (w * 2 - 1 - x)
                py = start_y + y
                
                draw.point((px, py), fill=color)
                draw.point((mx, py), fill=color)
                pixels.add((px, py))
                pixels.add((mx, py))

    # Add outline
    outline_pixels = set()
    for px, py in pixels:
        for dx, dy in [(-1,0), (1,0), (0,-1), (0,1), (-1,-1), (1,-1), (-1,1), (1,1)]:
            nx, ny = px+dx, py+dy
            if (nx, ny) not in pixels:
                outline_pixels.add((nx, ny))
    
    for ox, oy in outline_pixels:
        if 0 <= ox < render_size and 0 <= oy < render_size:
            draw.point((ox, oy), fill=outline_color)

    # Scale to final size (32x32)
    img = img.resize((size, size), Image.NEAREST)
    img.save(os.path.join(OUTPUT_DIR, f"{name.lower().replace(' ', '_')}.png"))

def bake_emoji(name, emoji, size=32):
    """Renders an emoji and applies a pixelation filter."""
    # Render at 64x64 then downscale to 32x32 for better alignment
    render_size = 64
    img = Image.new("RGBA", (render_size, render_size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    font_paths = [
        "C:\\Windows\\Fonts\\seguiemj.ttf", # Windows
        "/System/Library/Fonts/Apple Color Emoji.ttc", # macOS
        "/usr/share/fonts/truetype/noto/NotoColorEmoji.ttf" # Linux
    ]
    font = None
    for path in font_paths:
        if os.path.exists(path):
            try:
                font = ImageFont.truetype(path, int(render_size * 0.8))
                break
            except:
                continue
    
    if font:
        # Use textbbox to center the emoji
        bbox = draw.textbbox((0, 0), emoji, font=font, embedded_color=True)
        w = bbox[2] - bbox[0]
        h = bbox[3] - bbox[1]
        draw.text(((render_size - w) // 2, (render_size - h) // 2 - bbox[1]), emoji, font=font, fill=(255, 255, 255, 255), embedded_color=True)
    else:
        # Fallback to initials if no emoji font
        draw.text((render_size//4, render_size//4), name[0], fill=(255, 255, 255, 255))

    # Resize to 32x32
    # We use BILINEAR first to smooth it slightly before it becomes "pixels", or just NEAREST if we want raw pixels
    img = img.resize((size, size), Image.NEAREST)
    img.save(os.path.join(OUTPUT_DIR, f"{name.lower().replace(' ', '_')}.png"))

def main():
    ensure_dir()
    
    if not os.path.exists(ITEMS_JSON):
        print(f"Error: {ITEMS_JSON} not found.")
        return

    with open(ITEMS_JSON, 'r') as f:
        items = json.load(f)
    
    spells_emojis = {
        "Fire I": "🔥",
        "Ice I": "❄️",
        "Lightning I": "⚡",
        "Earth I": "⛰️",
        "Water I": "💧",
        "Haste I": "🏃",
        "Cure I": "💖"
    }

    for item in items:
        name = item["Name"]
        item_type = item["ItemType"]
        
        if item_type == "Spell" and name in spells_emojis:
            bake_emoji(name, spells_emojis[name])
            print(f"Baked emoji for {name}")
        else:
            generate_symmetrical_sprite(name)
            print(f"Generated sprite for {name}")

if __name__ == "__main__":
    main()
