# -*- coding: utf-8 -*-
"""Draws the app icon (trophy on a gradient) and writes SAM.ico and PNGs to SAM.App/Assets.

    python tools/make-icons.py
"""
import os
from PIL import Image, ImageDraw, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSETS = os.path.join(ROOT, "SAM.App", "Assets")
S = 1024  # drawing canvas, scaled down for anti-aliasing


def gradient(size, top, bottom):
    img = Image.new("RGBA", (size, size))
    px = img.load()
    for y in range(size):
        for x in range(size):
            t = (x * 0.35 + y * 0.65) / size
            px[x, y] = tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3)) + (255,)
    return img


def rounded_mask(size, radius):
    mask = Image.new("L", (size, size), 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, size - 1, size - 1), radius=radius, fill=255)
    return mask


def trophy(size):
    layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    white = (255, 255, 255, 255)
    cx = size // 2
    # handles
    w = int(size * 0.045)
    d.arc((int(size * 0.20), int(size * 0.26), int(size * 0.42), int(size * 0.52)), 90, 270, fill=white, width=w)
    d.arc((int(size * 0.58), int(size * 0.26), int(size * 0.80), int(size * 0.52)), 270, 90, fill=white, width=w)
    # cup: straight at the top, round at the bottom
    top, left, right = int(size * 0.22), int(size * 0.30), int(size * 0.70)
    d.rectangle((left, top, right, int(size * 0.43)), fill=white)
    d.chord((left, int(size * 0.18), right, int(size * 0.64)), 0, 180, fill=white)
    # stem and base
    d.polygon([(cx - int(size * 0.04), int(size * 0.60)), (cx + int(size * 0.04), int(size * 0.60)),
               (cx + int(size * 0.06), int(size * 0.72)), (cx - int(size * 0.06), int(size * 0.72))], fill=white)
    d.rounded_rectangle((int(size * 0.33), int(size * 0.71), int(size * 0.67), int(size * 0.80)), radius=int(size * 0.025), fill=white)
    # star cut out of the cup
    import math
    r_out, r_in, sy = size * 0.085, size * 0.036, size * 0.37
    pts = []
    for i in range(10):
        r = r_out if i % 2 == 0 else r_in
        a = math.pi / 2 + i * math.pi / 5
        pts.append((cx + r * math.cos(a), sy - r * math.sin(a)))
    d.polygon(pts, fill=(0, 0, 0, 0))
    return layer


def build():
    base = gradient(S, (38, 94, 214), (122, 54, 196))
    shade = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    glyph = trophy(S)
    shadow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    shadow.paste((10, 10, 40, 110), (0, int(S * 0.02)), glyph.split()[3])
    shadow = shadow.filter(ImageFilter.GaussianBlur(S * 0.02))
    base = Image.alpha_composite(base, shadow)
    base = Image.alpha_composite(base, glyph)
    out = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    out.paste(base, (0, 0), rounded_mask(S, int(S * 0.22)))
    return out


def main():
    os.makedirs(ASSETS, exist_ok=True)
    icon = build()
    for px in (256, 64, 32, 16):
        icon.resize((px, px), Image.LANCZOS).save(os.path.join(ASSETS, "SAM-%d.png" % px))
    icon.resize((256, 256), Image.LANCZOS).save(
        os.path.join(ASSETS, "SAM.ico"), sizes=[(16, 16), (20, 20), (24, 24), (32, 32), (40, 40), (48, 48), (64, 64), (128, 128), (256, 256)])
    print("Icons written to", ASSETS)


if __name__ == "__main__":
    main()
