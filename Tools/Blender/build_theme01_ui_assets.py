import bpy
import math
import os
import sys


def output_root():
    args = sys.argv
    marker = args.index("--") if "--" in args else len(args)
    if marker + 1 >= len(args):
        raise RuntimeError("Pass the Unity Assets directory after --")
    return os.path.join(
        os.path.abspath(args[marker + 1]),
        "Game", "Art", "UI", "Theme01")


def clamp01(value):
    return max(0.0, min(1.0, value))


def mix(a, b, amount):
    amount = clamp01(amount)
    return tuple(a[i] + ((b[i] - a[i]) * amount) for i in range(4))


def over(base, top):
    alpha = top[3]
    return (
        base[0] * (1.0 - alpha) + top[0] * alpha,
        base[1] * (1.0 - alpha) + top[1] * alpha,
        base[2] * (1.0 - alpha) + top[2] * alpha,
        clamp01(base[3] + alpha * (1.0 - base[3])),
    )


def rounded_box(x, y, half_x, half_y, radius):
    qx = abs(x) - half_x + radius
    qy = abs(y) - half_y + radius
    return math.hypot(max(qx, 0.0), max(qy, 0.0)) + min(max(qx, qy), 0.0) - radius


def polygon_distance(x, y, points):
    inside = False
    minimum = 10.0
    previous = points[-1]
    for current in points:
        ax, ay = previous
        bx, by = current
        vx, vy = bx - ax, by - ay
        wx, wy = x - ax, y - ay
        denom = vx * vx + vy * vy
        t = clamp01((wx * vx + wy * vy) / denom) if denom else 0.0
        minimum = min(minimum, math.hypot(wx - vx * t, wy - vy * t))
        if ((ay > y) != (by > y)) and (x < (bx - ax) * (y - ay) / (by - ay) + ax):
            inside = not inside
        previous = current
    return -minimum if inside else minimum


def stroke(base, distance, width, color, glow=0.0):
    if glow > 0.0:
        glow_alpha = clamp01(1.0 - abs(distance) / glow) * color[3] * 0.32
        base = over(base, (color[0], color[1], color[2], glow_alpha))
    alpha = clamp01((width - abs(distance)) * 160.0) * color[3]
    return over(base, (color[0], color[1], color[2], alpha))


NAVY = (0.018, 0.035, 0.085, 0.96)
NAVY_2 = (0.045, 0.085, 0.16, 0.98)
CYAN = (0.05, 0.82, 1.0, 1.0)
BLUE = (0.08, 0.34, 1.0, 1.0)
GREEN = (0.08, 0.95, 0.48, 1.0)
CORAL = (1.0, 0.22, 0.34, 1.0)
GOLD = (1.0, 0.72, 0.08, 1.0)
WHITE = (0.86, 0.96, 1.0, 1.0)


def panel_pixel(x, y, accent, kind):
    shape = rounded_box(x, y, 0.45, 0.45, 0.105)
    if shape > 0.055:
        return (0.0, 0.0, 0.0, 0.0)
    result = (0.0, 0.0, 0.0, 0.0)
    if shape > 0.0:
        return over(result, (accent[0], accent[1], accent[2], clamp01(1.0 - shape / 0.055) * 0.24))
    vertical = clamp01(y + 0.5)
    fill = mix(NAVY, NAVY_2, vertical * 0.68)
    if kind == "primary":
        fill = mix((0.015, 0.16, 0.25, 0.98), (0.025, 0.48, 0.54, 0.98), vertical)
    elif kind == "danger":
        fill = mix((0.18, 0.025, 0.075, 0.98), (0.54, 0.045, 0.13, 0.98), vertical)
    elif kind == "chip":
        fill = mix((0.025, 0.05, 0.11, 0.98), (0.075, 0.13, 0.22, 0.98), vertical)
    result = over(result, fill)
    result = stroke(result, shape + 0.012, 0.010, accent, 0.045)
    inner = rounded_box(x, y, 0.414, 0.414, 0.082)
    result = stroke(result, inner, 0.004, (0.55, 0.88, 1.0, 0.42), 0.0)
    # Repeating diagonal circuitry and clipped corner nodes remain outside the 9-slice center.
    if shape < -0.012:
        circuit = abs(((x - y + 1.0) % 0.22) - 0.11)
        if circuit < 0.006:
            result = over(result, (accent[0], accent[1], accent[2], 0.07))
        for sx in (-1.0, 1.0):
            for sy in (-1.0, 1.0):
                node = math.hypot(x - sx * 0.35, y - sy * 0.35)
                if node < 0.018:
                    result = over(result, (accent[0], accent[1], accent[2], 0.7))
    return result


def icon_shape(name, x, y):
    result = (0.0, 0.0, 0.0, 0.0)
    distance = 10.0
    accent = CYAN
    if name == "Coin":
        distance = abs(math.hypot(x, y) - 0.30) - 0.045
        accent = GOLD
        diamond = polygon_distance(x, y, [(0, .17), (.17, 0), (0, -.17), (-.17, 0)])
        result = stroke(result, diamond, 0.035, WHITE, 0.08)
    elif name == "Heart":
        xx, yy = x * 1.72, (y + 0.01) * 1.72
        value = (xx * xx + yy * yy - 0.30) ** 3 - xx * xx * yy ** 3
        distance = value * 2.2
        accent = CORAL
    elif name == "Shield":
        distance = polygon_distance(x, y, [(0, .34), (.29, .22), (.24, -.15), (0, -.36), (-.24, -.15), (-.29, .22)])
        accent = CYAN
        inner = polygon_distance(x, y, [(0, .22), (.17, .14), (.14, -.09), (0, -.23), (-.14, -.09), (-.17, .14)])
        result = stroke(result, inner, 0.022, WHITE, 0.06)
    elif name in ("Booster", "Continue"):
        accent = GOLD if name == "Booster" else GREEN
        for offset in (-0.13, 0.13) if name == "Continue" else (-0.10, 0.12):
            chevron = polygon_distance(x, y - offset, [(-.26, -.06), (0, .18), (.26, -.06), (.20, -.13), (0, .05), (-.20, -.13)])
            result = over(result, (accent[0], accent[1], accent[2], clamp01(-chevron * 70.0)))
        distance = 10.0
    elif name == "Play":
        distance = polygon_distance(x, y, [(-.18, .30), (.30, 0), (-.18, -.30)])
        accent = GREEN
    elif name == "Pause":
        d1 = rounded_box(x - .13, y, .07, .28, .025)
        d2 = rounded_box(x + .13, y, .07, .28, .025)
        distance = min(d1, d2)
        accent = WHITE
    elif name == "Back":
        distance = polygon_distance(x, y, [(.24, .28), (-.08, 0), (.24, -.28), (.12, -.36), (-.30, 0), (.12, .36)])
        accent = CYAN
    elif name == "Settings":
        radius = math.hypot(x, y)
        angle = math.atan2(y, x)
        teeth = 0.245 + 0.055 * (0.5 + 0.5 * math.cos(angle * 8.0))
        outer = radius - teeth
        inner = 0.11 - radius
        distance = max(outer, inner)
        accent = CYAN
    elif name == "Retry":
        radius = math.hypot(x, y)
        angle = math.atan2(y, x)
        arc = abs(radius - .25) - .045
        gap = 1.0 if -0.45 < angle < 0.55 else 0.0
        distance = arc + gap
        arrow = polygon_distance(x, y, [(.06, .28), (.33, .31), (.27, .05)])
        result = over(result, (CYAN[0], CYAN[1], CYAN[2], clamp01(-arrow * 80.0)))
        accent = CYAN
    if distance < 9.0:
        glow_alpha = clamp01(1.0 - max(distance, 0.0) / 0.09) * 0.30
        result = over(result, (accent[0], accent[1], accent[2], glow_alpha))
        result = over(result, (accent[0], accent[1], accent[2], clamp01(-distance * 90.0)))
        result = stroke(result, distance, 0.018, WHITE, 0.03)
    return result


def write_png(path, size, sampler, supersample=2):
    high = size * supersample
    accum = [(0.0, 0.0, 0.0, 0.0)] * (size * size)
    for py in range(high):
        y = ((py + 0.5) / high) - 0.5
        target_y = py // supersample
        for px in range(high):
            x = ((px + 0.5) / high) - 0.5
            target_x = px // supersample
            index = target_y * size + target_x
            color = sampler(x, y)
            old = accum[index]
            accum[index] = tuple(old[i] + color[i] / (supersample * supersample) for i in range(4))
    image = bpy.data.images.new(os.path.basename(path), width=size, height=size, alpha=True)
    image.filepath_raw = path
    image.file_format = "PNG"
    image.colorspace_settings.name = "sRGB"
    image.pixels.foreach_set([channel for pixel in accum for channel in pixel])
    image.save()
    bpy.data.images.remove(image)


root = output_root()
os.makedirs(root, exist_ok=True)
panels = {
    "Panel": (CYAN, "panel"),
    "Modal": (BLUE, "panel"),
    "ItemCard": (GREEN, "panel"),
    "ButtonPrimary": (GREEN, "primary"),
    "ButtonSecondary": (CYAN, "chip"),
    "ButtonDanger": (CORAL, "danger"),
    "ResourceChip": (GOLD, "chip"),
}
for name, data in panels.items():
    accent, kind = data
    write_png(os.path.join(root, name + ".png"), 128, lambda x, y, a=accent, k=kind: panel_pixel(x, y, a, k))
for name in ("Coin", "Heart", "Shield", "Booster", "Settings", "Play", "Back", "Pause", "Retry", "Continue"):
    write_png(os.path.join(root, "Icon" + name + ".png"), 96, lambda x, y, n=name: icon_shape(n, x, y))
print("Theme01 UI assets generated:", root)
