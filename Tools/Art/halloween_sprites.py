"""Cuts the Halloween event's art (GDD 5.42) out of the Codex-generated sources in Tools/Art/halloween/src.

The sources are painted on flat chroma-key green (the cobweb on black); the prompts that made them are in
Tools/Art/halloween/prompts. This script keys the background out, crops and sizes every sprite, derives the knife outline
from the pumpkin's silhouette and writes the PNGs into Assets/_Hauntscope/Art/Sprites/UI/Halloween (and the Jack-o'-Lantern's
Bestiary icon). Needs numpy, scipy and Pillow. Run: python -I Tools/Art/halloween_sprites.py
"""
import os

import numpy as np
from PIL import Image, ImageChops, ImageFilter
from scipy import ndimage

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SRC = os.path.join(ROOT, "Tools", "Art", "halloween", "src")
OUT = os.path.join(ROOT, "Assets", "_Hauntscope", "Art", "Sprites", "UI", "Halloween")
GHOST_OUT = os.path.join(ROOT, "Assets", "_Hauntscope", "Art", "Sprites", "Ghosts")


def load(name):
    return Image.open(os.path.join(SRC, name + ".png")).convert("RGB")


def key_green(image):
    """Chroma-keys the flat green out and pulls the green spill off the edge pixels."""
    rgb = np.asarray(image).astype(np.float32)
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    dominance = g - np.maximum(r, b)
    alpha = 1.0 - np.clip((dominance - 15.0) / 140.0, 0.0, 1.0)
    rgb[..., 1] = np.minimum(g, np.maximum(r, b) + 6.0)
    out = np.dstack([rgb, alpha * 255.0]).clip(0, 255).astype(np.uint8)
    return Image.fromarray(out, "RGBA")


def key_black(image):
    """Luminance becomes opacity, so glowing light on black lands as light on transparent."""
    rgb = np.asarray(image).astype(np.float32) / 255.0
    alpha = np.clip(rgb.max(axis=2) * 1.35 - 0.04, 0.0, 1.0)
    safe = np.maximum(alpha, 1e-3)[..., None]
    color = np.clip(rgb / safe, 0.0, 1.0)
    out = np.dstack([color, alpha]) * 255.0
    return Image.fromarray(out.astype(np.uint8), "RGBA")


def bbox_of(image, threshold=24):
    alpha = image.getchannel("A").point(lambda v: 255 if v > threshold else 0)
    return alpha.getbbox()


def main_part(image, gap=10):
    """Drops the slivers of neighbouring sprites that a sheet crop picks up: keeps the biggest blob of opaque pixels."""
    alpha = np.asarray(image.getchannel("A")) > 24
    labels, count = ndimage.label(ndimage.binary_dilation(alpha, iterations=gap))
    if count < 2:
        return image
    sizes = ndimage.sum(alpha, labels, range(1, count + 1))
    keep = labels == (int(np.argmax(sizes)) + 1)
    cleaned = np.asarray(image).copy()
    cleaned[..., 3] = np.where(keep, cleaned[..., 3], 0)
    return Image.fromarray(cleaned, "RGBA")


def square_crop(image, box, pad):
    """The box padded by pad (a fraction) and widened to a square; the canvas is extended where the box runs out."""
    left, top, right, bottom = box
    side = int(max(right - left, bottom - top) * (1.0 + pad * 2.0))
    cx, cy = (left + right) // 2, (top + bottom) // 2
    canvas = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    canvas.paste(image, (side // 2 - cx, side // 2 - cy))
    return canvas


def fit(image, size, pad=0.04):
    return square_crop(image, bbox_of(image), pad).resize((size, size), Image.LANCZOS)


def save(image, name, folder=OUT):
    os.makedirs(folder, exist_ok=True)
    path = os.path.join(folder, name + ".png")
    image.save(path)
    print("wrote", os.path.relpath(path, ROOT))


def carve_layers(size=1024):
    """The carved pumpkin and its empty twin share one crop, so the face lights up exactly where the skin was smooth."""
    carved = key_green(load("jack_b"))
    empty = key_green(load("decoy"))
    box = bbox_of(empty)
    carved_out = square_crop(carved, box, 0.03).resize((size, size), Image.LANCZOS)
    empty_out = square_crop(empty, box, 0.03).resize((size, size), Image.LANCZOS)
    return carved_out, empty_out


def knife_outline(empty, width=9, glow=14):
    """A bright cut line along the pumpkin's edge, white so the view tints it."""
    alpha = empty.getchannel("A").point(lambda v: 255 if v > 128 else 0)
    outer = alpha.filter(ImageFilter.MaxFilter(width * 2 + 1))
    inner = alpha.filter(ImageFilter.MinFilter(3))
    line = ImageChops.subtract(outer, inner).filter(ImageFilter.GaussianBlur(1.2))
    halo = line.filter(ImageFilter.GaussianBlur(glow)).point(lambda v: min(255, int(v * 1.6)))
    mask = ImageChops.lighter(line, halo)
    image = Image.new("RGBA", alpha.size, (255, 244, 214, 0))
    image.putalpha(mask)
    return image


def candies(size=256):
    sheet = key_green(load("candy"))
    boxes = {
        "CandyIcon": (30, 100, 650, 620),
        "CandyLolly": (750, 50, 1200, 680),
        "CandyCorn": (70, 660, 500, 1160),
        "CandyToffee": (620, 670, 1225, 1170),
    }
    for name, box in boxes.items():
        save(fit(main_part(sheet.crop(box)), size, 0.06), name)


def skins(size=256):
    sheet = key_green(load("skins"))
    crops = {
        "SkinBeam": (50, 60, 625, 635),
        "SkinLens": (635, 60, 1215, 635),
        "SkinRadar": (335, 605, 915, 1185),
    }
    for name, box in crops.items():
        save(fit(main_part(sheet.crop(box)), size, 0.03), name)


def cobweb(size=768):
    save(key_black(load("cobweb")).resize((size, size), Image.LANCZOS), "CobwebCorner")


def banner():
    """The strip behind the menu banner. The title sits left of centre and the candy count on the right, so the painted
    pumpkins are dimmed under them and only glow at the far edges."""
    image = Image.open(os.path.join(SRC, "banner.png")).convert("RGB")
    strip = image.crop((0, 394, image.width, 394 + 239)).resize((1440, 224), Image.LANCZOS)
    x = np.linspace(0.0, 1.0, strip.width)
    gain = np.interp(x, [0.0, 0.1, 0.2, 0.75, 0.84, 1.0], [0.9, 0.9, 0.3, 0.3, 0.12, 0.12])
    pixels = np.asarray(strip).astype(np.float32) * gain[None, :, None]
    save(Image.fromarray(pixels.clip(0, 255).astype(np.uint8)).convert("RGBA"), "HalloweenBanner")


def main():
    carved, empty = carve_layers()
    save(empty, "CarvePumpkin")
    save(carved, "CarveFace")
    save(knife_outline(empty), "CarveOutline")
    save(fit(key_green(load("jack_b")), 256, 0.04), "HalloweenEmblem")
    candies()
    skins()
    cobweb()
    banner()
    save(fit(key_green(load("ghost")), 384, 0.03), "JackOLanternIcon", GHOST_OUT)


if __name__ == "__main__":
    main()
