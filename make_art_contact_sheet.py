from pathlib import Path
from PIL import Image, ImageDraw

paths = [
    "Assets/Art/Background/MenuBg.png",
    "Assets/Art/Background/bg1.png",
    "Assets/Art/Background/bg2.png",
    "Assets/Art/Background/bg3.png",
    "Assets/Sprites/Squat.png",
    "Assets/Sprites/Squat2.png",
    "Assets/Sprites/push.png",
    "Assets/Sprites/boxing_sprite 1.png",
    "Assets/Art/supply/高蛋白.png",
    "Assets/Art/supply/肌酸.png",
    "Assets/Art/supply/維他命.png",
    "Assets/Art/supply/碳水.png",
    "Assets/Art/StressBall/messageImage_1778000153003.png",
    "Assets/Art/StressBall/messageImage_1778000141129.png",
    "Assets/Art/BrokeStressBall/messageImage_1778001474526.png",
    "Assets/Art/UI/panel.png",
    "Assets/Art/UI/HealthBg.png",
    "Assets/Art/UI/TimerBg.png",
    "Assets/UI/LOGO.jpg",
]

root = Path("F:/汗水加工廠")
tiles = []
for rel in paths:
    f = root / rel
    if not f.exists():
        continue
    im = Image.open(f).convert("RGBA")
    bg = Image.new("RGBA", im.size, (245, 245, 245, 255))
    bg.alpha_composite(im)
    im = bg.convert("RGB")
    im.thumbnail((260, 190), Image.Resampling.LANCZOS)
    tile = Image.new("RGB", (300, 245), "white")
    tile.paste(im, ((300 - im.width) // 2, 18))
    label = rel.replace("Assets/", "")
    ImageDraw.Draw(tile).text((10, 214), label[:42], fill=(0, 0, 0))
    tiles.append(tile)

cols = 3
rows = (len(tiles) + cols - 1) // cols
out = Image.new("RGB", (cols * 310 + 10, rows * 255 + 10), "white")
for idx, tile in enumerate(tiles):
    x = 10 + (idx % cols) * 310
    y = 10 + (idx // cols) * 255
    out.paste(tile, (x, y))

out_path = root / "game_art_contact_sheet.jpg"
out.save(out_path, quality=92)
print(out_path)
