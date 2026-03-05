import os
import random
import hashlib
from PIL import Image, ImageDraw, ImageFont

# DB8 Palette (Approximate)
PALETTE = [
    (20, 12, 28),    # Black/Deep Purple
    (68, 36, 52),    # Dark Red
    (48, 52, 109),   # Dark Blue
    (78, 74, 78),    # Grey
    (133, 76, 48),   # Brown
    (52, 101, 36),   # Green
    (208, 70, 72),   # Red
    (117, 113, 97),  # Stone
    (89, 125, 206),  # Sky Blue
    (210, 125, 44),  # Orange
    (133, 149, 161), # Light Grey
    (109, 170, 44),  # Lime
    (210, 170, 153), # Skin/Peach
    (109, 194, 202), # Cyan
    (238, 221, 175), # Yellow/Parchment
    (255, 255, 255)  # White
]

OUTPUT_DIR = "Mythril.Blazor/wwwroot/assets/sprites"

def ensure_dir():
    if not os.path.exists(OUTPUT_DIR):
        os.makedirs(OUTPUT_DIR)

def get_color_from_hash(name, salt=""):
    h = hashlib.md5((name + salt).encode()).hexdigest()
    idx = int(h, 16) % len(PALETTE)
    return PALETTE[idx]

def generate_symmetrical_sprite(name, size=32):
    """Generates a symmetrical pixel art sprite based on a hash of the name."""
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    seed = int(hashlib.md5(name.encode()).hexdigest(), 16)
    random.seed(seed)
    
    # 7x7 core
    core_size = 7
    primary_color = get_color_from_hash(name, "primary")
    secondary_color = get_color_from_hash(name, "secondary")
    
    # Draw core and mirror it
    margin = (size - (core_size * 2)) // 2
    for y in range(core_size):
        for x in range(core_size):
            if random.random() > 0.4:
                color = primary_color if random.random() > 0.3 else secondary_color
                # Mirror horizontally
                px = margin + x
                py = margin + y
                draw.rectangle([px*2, py*2, px*2+1, py*2+1], fill=color)
                
                mx = margin + (core_size * 2 - 1 - x)
                draw.rectangle([mx*2, py*2, mx*2+1, py*2+1], fill=color)

    # Outline
    # (Simplified outline logic for brevity)
    
    img.save(os.path.join(OUTPUT_DIR, f"{name.lower().replace(' ', '_')}.png"))

def bake_emoji(name, emoji, size=32):
    """Renders an emoji and applies a pixelation filter."""
    img = Image.new("RGBA", (size*4, size*4), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Try to find a font that supports emojis
    font_paths = [
        "C:\\Windows\\Fonts\\seguiemj.ttf", # Windows
        "/System/Library/Fonts/Apple Color Emoji.ttc", # macOS
        "/usr/share/fonts/truetype/noto/NotoColorEmoji.ttf" # Linux
    ]
    font = None
    for path in font_paths:
        if os.path.exists(path):
            try:
                font = ImageFont.truetype(path, size*3)
                break
            except:
                continue
    
    if font:
        draw.text((size//2, size//2), emoji, font=font, fill=(255, 255, 255, 255), embedded_color=True)
    else:
        # Fallback to initials if no emoji font
        draw.text((size, size), name[0], fill=(255, 255, 255, 255))

    # Pixelate: scale down then up
    img = img.resize((size, size), Image.NEAREST)
    img.save(os.path.join(OUTPUT_DIR, f"{name.lower().replace(' ', '_')}.png"))

def main():
    ensure_dir()
    
    # Items from quests.json and items.json
    items = [
        "Gold", "Iron Ore", "Crystal Shards", "Blue Coral", "Lost Parchment",
        "Potion", "Basic Gem", "Log", "Herb", "Leather", "Water", "Web", "Slime",
        "Moonberry", "Ancient Bark", "Mana Leaf", "Fire Shard", "Ice Shard",
        "Mythril Spark", "Solar Essence", "Sun-baked Scale"
    ]
    
    for item in items:
        generate_symmetrical_sprite(item)
        print(f"Generated sprite for {item}")

    # Spells (Emoji Baking)
    spells = {
        "Fire I": "🔥",
        "Ice I": "❄️",
        "Lightning I": "⚡",
        "Earth I": "⛰️",
        "Water I": "💧",
        "Haste I": "🏃",
        "Cure I": "💖"
    }
    
    for name, emoji in spells.items():
        bake_emoji(name, emoji)
        print(f"Baked emoji for {name}")

if __name__ == "__main__":
    main()
