# -*- coding: utf-8 -*-
"""Renders the medal animation (shine and sparkles of the original SVGs) as frames for SAM.App.

    python tools/make-medal-frames.py <folder with the medal SVGs>

The Windows SVG renderer cannot play CSS animations. So Microsoft Edge (headless) renders every original SVG at
fixed points in time - the animations are paused via the Web Animations API and set to that time, which also keeps
the staggered sparkles right. Result per medal:

- SAM.App/Assets/Medals/Animated/<name>.png: all distinct frames side by side (image strip, with alpha);
- SAM.App/Helpers/MedalAnimations.g.cs: when which frame shows, frame size and position relative to the static version
  (the original has a margin for the sparkles, the static version from make-medals.py does not).

Needs Edge and Pillow. Run make-medals.py first. The originals stay unchanged.
"""
import io
import os
import re
import shutil
import subprocess
import sys
import tempfile

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TARGET = os.path.join(ROOT, "SAM.App", "Assets", "Medals", "Animated")
STATIC = os.path.join(ROOT, "SAM.App", "Assets", "Medals")
CODE = os.path.join(ROOT, "SAM.App", "Helpers", "MedalAnimations.g.cs")
EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

# Source -> (name in the app, MedalKind)
FILES = [
    ("Medaille-gross-blau.svg", "game-complete", "Game"),
    ("Medaille-blau.svg", "set-complete", "Set"),
    ("Medaille-blau-doppelt.svg", "set-double-complete", "SetDouble"),
    ("Medaille-lila-doppelt.svg", "dlc-set-complete", "DlcSet"),
]

DURATION_MS = 4000      # all animations of the originals run for 4 s
FPS = 25
FRAME_HEIGHT = 96       # pixels - enough for medals up to ~40 px high at 200 % scaling
COLUMNS = 20


def viewbox(svg):
    return [float(v) for v in re.search(r'viewBox="([^"]+)"', svg).group(1).split()]


def render_sheet(svg, times, frame_w, frame_h, work):
    """All points in time as a grid on one page, one screenshot - then cut apart."""
    cells = []
    for k, t in enumerate(times):
        copy = re.sub(r'id="([^"]+)"', lambda m: 'id="%s-f%d"' % (m.group(1), k), svg)
        copy = re.sub(r'url\(#([^)]+)\)', lambda m: 'url(#%s-f%d)' % (m.group(1), k), copy)
        copy = re.sub(r'href="#([^"]+)"', lambda m: 'href="#%s-f%d"' % (m.group(1), k), copy)
        copy = re.sub(r'(<svg xmlns="http://www.w3.org/2000/svg"[^>]*?) width="[\d.]+" height="[\d.]+"', lambda m: m.group(1), copy, count=1)
        copy = copy.replace("<svg xmlns", '<svg data-t="%d" width="%d" height="%d" xmlns' % (t, frame_w, frame_h), 1)
        cells.append('<div class="c">' + copy + "</div>")

    columns = min(COLUMNS, len(times))
    rows = (len(times) + columns - 1) // columns
    html = ("<!doctype html><html><head><meta charset=\"utf-8\"><style>"
            "html,body{margin:0;padding:0;background:transparent}"
            ".g{display:grid;grid-template-columns:repeat(%d,%dpx);grid-auto-rows:%dpx}"
            ".c{width:%dpx;height:%dpx;overflow:hidden}svg{display:block}"
            "</style></head><body><div class=\"g\">%s</div><script>"
            "for (const s of document.querySelectorAll('svg[data-t]')) {"
            "  for (const a of s.getAnimations({subtree: true})) { a.pause(); a.currentTime = parseInt(s.dataset.t); }"
            "}</script></body></html>") % (columns, frame_w, frame_h, frame_w, frame_h, "".join(cells))

    page = os.path.join(work, "frames.html")
    shot = os.path.join(work, "frames.png")
    io.open(page, "w", encoding="utf-8").write(html)
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--hide-scrollbars", "--force-device-scale-factor=1",
                    "--default-background-color=00000000", "--window-size=%d,%d" % (columns * frame_w, rows * frame_h + 100),
                    "--virtual-time-budget=5000", "--screenshot=" + shot, "file:///" + page.replace("\\", "/")],
                   check=True, capture_output=True, timeout=120)
    sheet = Image.open(shot).convert("RGBA")
    return [sheet.crop(((k % columns) * frame_w, (k // columns) * frame_h, (k % columns + 1) * frame_w, (k // columns + 1) * frame_h))
            for k in range(len(times))]


def main():
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    source = sys.argv[1]
    os.makedirs(TARGET, exist_ok=True)
    times = [round(i * 1000 / FPS) for i in range(DURATION_MS * FPS // 1000)]
    entries = []
    work = tempfile.mkdtemp(prefix="sam-medal-frames-")

    try:
        for name, target, kind in FILES:
            svg = io.open(os.path.join(source, name), encoding="utf-8").read()
            box = viewbox(svg)
            frame_w = round(FRAME_HEIGHT * box[2] / box[3])
            frames = render_sheet(svg, times, frame_w, FRAME_HEIGHT, work)

            # Store every frame only once (the calm state recurs); points in time only where the frame changes.
            unique, seen, keys = [], {}, []
            for t, frame in zip(times, frames):
                data = frame.tobytes()
                if data not in seen:
                    seen[data] = len(unique)
                    unique.append(frame)
                if not keys or keys[-1][1] != seen[data]:
                    keys.append((t, seen[data]))

            strip = Image.new("RGBA", (frame_w * len(unique), FRAME_HEIGHT), (0, 0, 0, 0))
            for i, frame in enumerate(unique):
                strip.paste(frame, (i * frame_w, 0))
            strip.save(os.path.join(TARGET, target + ".png"), optimize=True)

            static = viewbox(io.open(os.path.join(STATIC, target + ".svg"), encoding="utf-8").read())
            entries.append((kind, target, frame_w, FRAME_HEIGHT, len(unique), keys, box, static))
            print("%-26s -> Animated/%-24s %3d frames (%d points in time), %4d x %d px" % (
                name, target + ".png", len(unique), len(times), frame_w, FRAME_HEIGHT))
    finally:
        shutil.rmtree(work, ignore_errors=True)

    def numbers(values):
        return ", ".join(("%g" % v) for v in values)

    lines = [
        "// <auto-generated> by tools/make-medal-frames.py - do not edit by hand.",
        "namespace SAM.App.Helpers;",
        "",
        "internal static partial class MedalAnimations",
        "{",
        "    private static readonly Dictionary<MedalKind, MedalAnimation> Data = new()",
        "    {",
    ]
    for kind, target, frame_w, frame_h, count, keys, box, static in entries:
        lines += [
            "        [MedalKind.%s] = new(" % kind,
            '            "ms-appx:///Assets/Medals/Animated/%s.png", %d, %d, %d, %d,' % (target, frame_w, frame_h, count, DURATION_MS),
            "            [%s]," % numbers(k[0] for k in keys),
            "            [%s]," % numbers(k[1] for k in keys),
            "            [%s]," % numbers(box),
            "            [%s])," % numbers(static),
        ]
    lines += ["    };", "}", ""]
    io.open(CODE, "w", encoding="utf-8", newline="\n").write("\n".join(lines))
    print("data -> %s" % os.path.relpath(CODE, ROOT))


if __name__ == "__main__":
    main()
