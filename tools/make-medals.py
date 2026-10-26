# -*- coding: utf-8 -*-
"""Turns the animated medal SVGs into static versions for SAM.App/Assets/Medals.

    python tools/make-medals.py <folder with the medal SVGs>

The Windows SVG renderer (Direct2D, SvgImageSource) supports neither CSS nor filters. Therefore:
- the <style> with the animations is dropped, as are the shine stripe (class="shine") and the sparkles (class="sp"),
  which would be permanently visible without CSS - the app shows the first, calm state of the animation
  (tools/make-medal-frames.py renders the animation itself);
- dimming via feComponentTransfer is baked into the colors;
- masks (the gap between the purple medals) are rebuilt as a clip-path (needs opencv-python);
- "Medaille-unfertig.svg" (grey medal) is already static and is only trimmed.
The originals stay unchanged.
"""
import io
import math
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TARGET = os.path.join(ROOT, "SAM.App", "Assets", "Medals")

# Source -> target file in the app
FILES = {
    "Medaille-gross-blau.svg": "game-complete.svg",      # whole game completed
    "Medaille-blau.svg": "set-complete.svg",             # one base game set completed
    "Medaille-blau-doppelt.svg": "set-double-complete.svg",  # several base game sets completed
    "Medaille-lila-doppelt.svg": "dlc-set-complete.svg",  # DLC sets completed (always double on Steam)
    "Medaille-unfertig.svg": "unfinished.svg",           # still open (grey medal)
}


def scale_hex(color, slope):
    value = color.lstrip("#")
    if len(value) == 3:
        value = "".join(c * 2 for c in value)
    r, g, b = (int(value[i:i + 2], 16) for i in (0, 2, 4))
    return "#%02X%02X%02X" % tuple(min(255, int(round(c * slope))) for c in (r, g, b))


def element_end(text, start, tag):
    """Index after the closing tag of the element starting at start (children with the same name are counted)."""
    depth = 0
    pattern = re.compile(r"<(/?)%s\b[^>]*?(/?)>" % tag)
    for m in pattern.finditer(text, start):
        if m.group(1):
            depth -= 1
        elif not m.group(2):
            depth += 1
        if depth == 0:
            return m.end()
    raise ValueError("no end for <%s> from %d" % (tag, start))


def remove_elements(text, opening_regex, tag):
    while True:
        m = re.search(opening_regex, text)
        if not m:
            return text
        text = text[:m.start()] + text[element_end(text, m.start(), tag):]


# ---------------------------------------------------------------- Rebuild masks as clip paths
# The purple double medal uses a <mask> on the rear medal to cut out a strip around the front one
# (front medal filled + a 3 units wide stroke). Windows does not draw masks - so this area is
# rasterized, grown by the stroke, traced back into an outline and used as a clip-path (evenodd).

RASTER = 96  # pixels per SVG unit while rebuilding


def flatten(d, steps=24):
    """Splits an SVG path (M L H V C S Q A Z, absolute and relative) into subpaths of points."""
    tokens = re.findall(r"[MmLlHhVvCcSsQqTtAaZz]|-?(?:\d+\.?\d*|\.\d+)(?:[eE][-+]?\d+)?", d)
    i, cmd = 0, None
    x = y = sx = sy = 0.0
    last_ctrl = None
    paths, current = [], []

    def number():
        nonlocal i
        value = float(tokens[i])
        i += 1
        return value

    def bezier(points):
        p0, p1, p2, p3 = points
        for k in range(1, steps + 1):
            t = k / steps
            mt = 1 - t
            current.append((mt ** 3 * p0[0] + 3 * mt * mt * t * p1[0] + 3 * mt * t * t * p2[0] + t ** 3 * p3[0],
                            mt ** 3 * p0[1] + 3 * mt * mt * t * p1[1] + 3 * mt * t * t * p2[1] + t ** 3 * p3[1]))

    def arc(x1, y1, rx, ry, phi, large, sweep, x2, y2):
        # Convert the SVG arc (endpoint form) to center form and sample it
        if rx == 0 or ry == 0:
            current.append((x2, y2))
            return
        rx, ry = abs(rx), abs(ry)
        cos_p, sin_p = math.cos(math.radians(phi)), math.sin(math.radians(phi))
        dx, dy = (x1 - x2) / 2, (y1 - y2) / 2
        xp, yp = cos_p * dx + sin_p * dy, -sin_p * dx + cos_p * dy
        lam = xp * xp / (rx * rx) + yp * yp / (ry * ry)
        if lam > 1:
            rx, ry = rx * math.sqrt(lam), ry * math.sqrt(lam)
        num = rx * rx * ry * ry - rx * rx * yp * yp - ry * ry * xp * xp
        den = rx * rx * yp * yp + ry * ry * xp * xp
        coef = math.sqrt(max(0.0, num / den)) if den else 0.0
        if large == sweep:
            coef = -coef
        cxp, cyp = coef * rx * yp / ry, -coef * ry * xp / rx
        cx = cos_p * cxp - sin_p * cyp + (x1 + x2) / 2
        cy = sin_p * cxp + cos_p * cyp + (y1 + y2) / 2

        def angle(ux, uy, vx, vy):
            a = math.atan2(ux * vy - uy * vx, ux * vx + uy * vy)
            return a

        t1 = angle(1, 0, (xp - cxp) / rx, (yp - cyp) / ry)
        dt = angle((xp - cxp) / rx, (yp - cyp) / ry, (-xp - cxp) / rx, (-yp - cyp) / ry)
        if not sweep and dt > 0:
            dt -= 2 * math.pi
        elif sweep and dt < 0:
            dt += 2 * math.pi
        n = max(4, int(abs(dt) / (math.pi / 32)))
        for k in range(1, n + 1):
            t = t1 + dt * k / n
            current.append((cos_p * rx * math.cos(t) - sin_p * ry * math.sin(t) + cx,
                            sin_p * rx * math.cos(t) + cos_p * ry * math.sin(t) + cy))

    while i < len(tokens):
        if re.match(r"[A-Za-z]", tokens[i]):
            cmd = tokens[i]
            i += 1
            if cmd in "Zz":
                if current:
                    paths.append(current)
                current = []
                x, y = sx, sy
                last_ctrl = None
                continue
        rel = cmd.islower()
        c = cmd.upper()
        ox, oy = (x, y) if rel else (0.0, 0.0)
        if c == "M":
            if current:
                paths.append(current)
            x, y = ox + number(), oy + number()
            sx, sy = x, y
            current = [(x, y)]
            cmd = "l" if rel else "L"
            last_ctrl = None
        elif c == "L":
            x, y = ox + number(), oy + number()
            current.append((x, y))
            last_ctrl = None
        elif c == "H":
            x = ox + number()
            current.append((x, y))
            last_ctrl = None
        elif c == "V":
            y = (y if rel else 0.0) + number()
            current.append((x, y))
            last_ctrl = None
        elif c in "CS":
            if c == "C":
                c1 = (ox + number(), oy + number())
            else:
                c1 = (2 * x - last_ctrl[0], 2 * y - last_ctrl[1]) if last_ctrl else (x, y)
            c2 = (ox + number(), oy + number())
            end = (ox + number(), oy + number())
            bezier([(x, y), c1, c2, end])
            last_ctrl = c2
            x, y = end
        elif c == "Q":
            q = (ox + number(), oy + number())
            end = (ox + number(), oy + number())
            bezier([(x, y), (x + 2 / 3 * (q[0] - x), y + 2 / 3 * (q[1] - y)), (end[0] + 2 / 3 * (q[0] - end[0]), end[1] + 2 / 3 * (q[1] - end[1])), end])
            last_ctrl = None
            x, y = end
        elif c == "A":
            rx, ry, phi, large, sweep = number(), number(), number(), number(), number()
            end = (ox + number(), oy + number())
            arc(x, y, rx, ry, phi, int(large), int(sweep), end[0], end[1])
            last_ctrl = None
            x, y = end
        else:
            raise ValueError("path command %s is not supported" % cmd)
    if current:
        paths.append(current)
    return paths


def mask_to_clip(svg):
    """Replaces every <mask> (white area, black paths with a stroke) with an equivalent clip-path."""
    import cv2
    import numpy as np

    for m in list(re.finditer(r'<mask id="([^"]+)"[^>]*>(.*?)</mask>', svg, re.S)):
        mask_id, body = m.group(1), m.group(2)
        rect = re.search(r'<rect x="([\d.-]+)" y="([\d.-]+)" width="([\d.]+)" height="([\d.]+)" fill="#fff"/>', body)
        group = re.search(r'<g transform="translate\(([-\d.]+) ([-\d.]+)\) scale\(([\d.]+)\)"[^>]*stroke-width="([\d.]+)"[^>]*>(.*?)</g>', body, re.S)
        rx0, ry0, rw, rh = (float(v) for v in rect.groups())
        tx, ty, scale, stroke = (float(v) for v in group.groups()[:4])

        width, height = int(round(rw * RASTER)), int(round(rh * RASTER))
        hidden = np.zeros((height, width), np.uint8)
        for d in re.findall(r'd="([^"]+)"', group.group(5)):
            for points in flatten(d):
                poly = np.array([[((px * scale + tx) - rx0) * RASTER, ((py * scale + ty) - ry0) * RASTER] for px, py in points], np.float64)
                cv2.fillPoly(hidden, [np.round(poly * 16).astype(np.int32)], 255, lineType=cv2.LINE_AA, shift=4)
        _, hidden = cv2.threshold(hidden, 127, 255, cv2.THRESH_BINARY)

        # The stroke (stroke-linejoin round) acts like growing the shape by half the stroke width.
        radius = stroke * scale / 2 * RASTER
        size = int(round(radius * 2)) | 1
        hidden = cv2.dilate(hidden, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (size, size)))

        contours, _ = cv2.findContours(hidden, cv2.RETR_CCOMP, cv2.CHAIN_APPROX_NONE)
        parts = ["M%g %gH%gV%gH%gZ" % (rx0, ry0, rx0 + rw, ry0 + rh, rx0)]
        for contour in contours:
            contour = cv2.approxPolyDP(contour, 0.6, True)
            if len(contour) < 3:
                continue
            coords = ["%.3f %.3f" % (rx0 + p[0][0] / RASTER, ry0 + p[0][1] / RASTER) for p in contour]
            parts.append("M" + "L".join(coords) + "Z")

        clip_id = mask_id + "-clip"
        clip = '<clipPath id="%s"><path clip-rule="evenodd" d="%s"/></clipPath>' % (clip_id, "".join(parts))
        svg = svg.replace(m.group(0), clip)
        svg = svg.replace('mask="url(#%s)"' % mask_id, 'clip-path="url(#%s)"' % clip_id)
    return svg


def make_static(svg):
    svg = re.sub(r"<style>.*?</style>", "", svg, flags=re.S)
    # Shine stripe: <g clip-path="url(#..-clip)" pointer-events="none"><rect class="shine" .../></g>
    svg = remove_elements(svg, r'<g clip-path="url\(#[^)]*\)" pointer-events="none">', "g")
    # Sparkles: <g transform="translate(..)"><g class="sp" ...>...</g></g>
    svg = remove_elements(svg, r'<g transform="translate\([^)]*\)">\s*<g class="sp"', "g")
    # Glow filter of the sparkles
    svg = remove_elements(svg, r'<defs>\s*<filter id="[^"]*-glow"', "defs")

    # Dimming: <g filter="url(#x-dim)"> ... </g> -> convert the colors inside, drop the filter
    for m in list(re.finditer(r'<filter id="([^"]*-dim)"[^>]*>.*?</filter>', svg, re.S)):
        slope = float(re.search(r'slope="([\d.]+)"', m.group(0)).group(1))
        name = m.group(1)
        start = svg.index('<g filter="url(#%s)">' % name)
        end = element_end(svg, start, "g")
        inner = svg[start:end]
        inner = re.sub(r"#[0-9A-Fa-f]{6}\b|#[0-9A-Fa-f]{3}\b",
                       lambda c: c.group(0) if c.group(0).lower() in ("#fff", "#ffffff", "#000", "#000000") else scale_hex(c.group(0), slope),
                       inner)
        inner = inner.replace('<g filter="url(#%s)">' % name, "<g>", 1)
        svg = svg[:start] + inner + svg[end:]
    svg = re.sub(r'<filter id="[^"]*-dim"[^>]*>.*?</filter>', "", svg, flags=re.S)
    svg = re.sub(r"<defs>\s*</defs>", "", svg)
    svg = mask_to_clip(svg)

    # The 8 unit margin all around was only there for the sparkles - without them show just the medal.
    m = re.search(r'viewBox="-8 -8 ([\d.]+) ([\d.]+)"', svg)
    if m:
        width, height = float(m.group(1)) - 16, float(m.group(2)) - 16
        svg = svg.replace(m.group(0), 'viewBox="0 0 %g %g"' % (width, height), 1)
    # Drop the fixed size so the app can scale it freely.
    svg = re.sub(r'(<svg xmlns="http://www.w3.org/2000/svg"[^>]*?) width="[\d.]+" height="[\d.]+"', lambda m: m.group(1), svg, count=1)
    return svg


def main():
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    source = sys.argv[1]
    os.makedirs(TARGET, exist_ok=True)

    for name, target in FILES.items():
        svg = io.open(os.path.join(source, name), encoding="utf-8").read()
        svg = make_static(svg)
        for leftover in ("<style", 'class="shine', 'class="sp"', "filter="):
            if leftover in svg:
                sys.exit("%s: still contains %s" % (name, leftover))
        with io.open(os.path.join(TARGET, target), "w", encoding="utf-8", newline="\n") as f:
            f.write(svg.strip() + "\n")
        print("%-28s -> %-22s %6d characters" % (name, target, len(svg)))


if __name__ == "__main__":
    main()
