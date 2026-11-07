#!/usr/bin/env python3
"""Measure where each outfit overlay sits on every painted species frame.

Outfit art is painted only for the front, blink and cheer frames. Every `<species>_outfit_<n>_front.png` is exactly
`<species>_front.png` with `<species>_outfit_<n>_front_overlay.png` composited on top, so any other frame can wear the
outfit by drawing that overlay through a local transform. For each overlay and frame this script registers a
landmark image of the plain front frame (dark eye marks, beak, silhouette; fur texture is ignored) around the overlay
and the face against the same landmark image of the frame.

Most painted actions turn the head into a three-quarter view, where the far side of the face is foreshortened. The
transform therefore scales the two halves of the overlay (left and right of its pivot) independently:
  a canvas point p of the overlay is drawn at pivot + t + R(degrees) * (s(p) * (p.x - pivot.x), sy * (p.y - pivot.y))
  with s(p) = sx_left when p.x < pivot.x, else sx_right.
BirdView draws the overlay as a six-vertex textured polygon with exactly this mapping.

Output: PerchSort/birds/outfit_anchors.json
  {"<species>_outfit_<n>": {"pivot": [x, y], "frames": {"<frame>": [tx, ty, sx_left, sx_right, sy, degrees, cost]}}}

Usage: python3 tools/birds/measure_outfit_anchors.py [--sheet DIR] [--only berry_outfit_1 ...] [--sheets-only] [--repair]
--sheet writes one verification contact sheet per outfit (overlay composited on every measured frame).
Fits that tear the overlay apart or move it off the silhouette are refitted with tied half scales, else they copy
the nearest plausible frame of the same action; the "repaired" list names them. --repair re-runs only that check.
Needs numpy and Pillow. All 27 outfits take about 40 minutes on four cores.
"""
import argparse
import json
import os
import sys
from concurrent.futures import ProcessPoolExecutor

import numpy as np
from PIL import Image, ImageDraw

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
BIRDS = os.path.join(ROOT, "PerchSort", "birds")
OUT = os.path.join(BIRDS, "outfit_anchors.json")
MARGIN = 40
EYE_MARGIN = 10
FEET_Y_CUT = 262  # canvas rows below this are legs and feet
BEAK_WEIGHT = 0.25
ALPHA_WEIGHT = 0.05
## Overlays worn over the eyes; they are registered on the eye marks under the frame alone.
EYEWEAR = {"berry_outfit_1", "cocoa_outfit_3", "hoot_outfit_2", "ink_outfit_2", "sky_outfit_2"}
LO = [-120.0, -120.0, 0.25, 0.25, 0.8, -35.0]
HI = [120.0, 120.0, 1.3, 1.3, 1.2, 35.0]
POSES = ["fluff", "stretch", "yawn", "tilt_left", "tilt_right", "look_up", "look_down", "quirk1", "quirk2", "preen"]
SEQUENCES = {"turn_left": 4, "turn_right": 4, "preen": 5, "flap": 6, "hop": 6,
             "flight_takeoff": 3, "flight_flap": 6, "flight_land": 3,
             "flight_takeoff_l": 3, "flight_flap_l": 6, "flight_land_l": 3}
IDENTITY = [0.0, 0.0, 1.0, 1.0, 1.0, 0.0, 0.0]


def frame_names(species):
    names = ["front", "front_blink_01", "front_blink_02"]
    for seq, count in SEQUENCES.items():
        names += ["%s_%02d" % (seq, i) for i in range(1, count + 1)]
    names += POSES
    return [n for n in names if os.path.exists(os.path.join(BIRDS, "%s_%s.png" % (species, n)))]


def blur(a, sigma):
    """Separable Gaussian blur of a 2-D float array."""
    r = int(3 * sigma + 0.5)
    k = np.exp(-0.5 * (np.arange(-r, r + 1) / sigma) ** 2)
    k /= k.sum()
    a = np.apply_along_axis(lambda v: np.convolve(np.pad(v, r, mode="edge"), k, "valid"), 0, a)
    return np.apply_along_axis(lambda v: np.convolve(np.pad(v, r, mode="edge"), k, "valid"), 1, a)


def load(path, dark_level=None):
    """Landmark feature image: dark eye marks (pupils, lashes, closed-eye lines), the orange beak and the
    silhouette, softly blurred. Fur texture and shading are ignored on purpose: they differ between separately
    painted frames, while eyes and beak are what an outfit must follow."""
    a = np.asarray(Image.open(path).convert("RGBA"), dtype=np.float32) / 255.0
    alpha = a[..., 3]
    rgb = a[..., :3]
    lum = rgb @ np.array([0.299, 0.587, 0.114], np.float32)
    solid = alpha > 0.8
    if dark_level is None:
        dark_level = 0.45 * float(np.median(lum[solid]))
    dark = (blur(lum, 1.2) < dark_level) & solid
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    beak = (r > 0.75) & (g > 0.42) & (g < 0.82) & (b < 0.35) & (r - g > 0.18) & solid
    beak[FEET_Y_CUT:, :] = False  # feet share the beak colour
    return np.stack([blur(dark.astype(np.float32), 2.5), BEAK_WEIGHT * blur(beak.astype(np.float32), 2.5),
                     ALPHA_WEIGHT * blur(alpha, 4.0)], axis=2).astype(np.float32), dark_level


def sample(img, x, y):
    """Bilinear sample of img (H, W, C) at float coordinates; outside the canvas is zero."""
    h, w = img.shape[:2]
    x0 = np.floor(x).astype(int)
    y0 = np.floor(y).astype(int)
    fx = (x - x0)[:, None]
    fy = (y - y0)[:, None]
    out = np.zeros((x.size, img.shape[2]), np.float32)
    for dx, dy, wt in ((0, 0, (1 - fx) * (1 - fy)), (1, 0, fx * (1 - fy)), (0, 1, (1 - fx) * fy), (1, 1, fx * fy)):
        xi = x0 + dx
        yi = y0 + dy
        ok = (xi >= 0) & (xi < w) & (yi >= 0) & (yi < h)
        v = np.zeros((x.size, img.shape[2]), np.float32)
        v[ok] = img[yi[ok], xi[ok]]
        out += wt * v
    return out


class Fit:
    def __init__(self, front, alpha, eyewear=False):
        ys, xs = np.nonzero(alpha > 0.04)
        self.pivot = np.array([np.average(xs, weights=alpha[ys, xs]), np.average(ys, weights=alpha[ys, xs])])
        h, w = alpha.shape
        if eyewear:
            x0, x1 = max(0, xs.min() - EYE_MARGIN), min(w, xs.max() + EYE_MARGIN + 1)
            y0, y1 = max(0, ys.min() - EYE_MARGIN), min(h, ys.max() + EYE_MARGIN + 1)
            front = front[..., :1]
        else:
            # The patch covers the overlay and the whole face (eye marks and beak), so the outfit follows the face
            # even when it sits on the crown or the neck.
            by, _ = np.nonzero(front[..., 1] > 0.5 * BEAK_WEIGHT)
            face = (front[..., 0] > 0.5) | (front[..., 1] > 0.5 * BEAK_WEIGHT)
            face[int(by.max()) + 12:, :] = False  # eye marks end at the beak; lower dark marks are wing shading
            fy, fx = np.nonzero(face)
            x0, x1 = max(0, min(xs.min(), fx.min()) - MARGIN), min(w, max(xs.max(), fx.max()) + MARGIN + 1)
            y0, y1 = max(0, min(ys.min(), fy.min()) - MARGIN), min(h, max(ys.max(), fy.max()) + MARGIN + 1)
        gy, gx = np.mgrid[y0:y1:3, x0:x1:3]
        self.px = gx.ravel().astype(np.float32) - self.pivot[0]
        self.py = gy.ravel().astype(np.float32) - self.pivot[1]
        self.values = front[gy.ravel(), gx.ravel()]
        self.front = front
        self.rect = (x0, x1, y0, y1)

    def cost(self, frame, p):
        tx, ty, sxl, sxr, sy, deg = p
        r = np.deg2rad(deg)
        c, s = np.cos(r), np.sin(r)
        u = np.where(self.px < 0, sxl, sxr) * self.px
        v = sy * self.py
        x = self.pivot[0] + tx + c * u - s * v
        y = self.pivot[1] + ty + s * u + c * v
        d = sample(frame, x, y) - self.values
        return float(np.mean(d * d))

    def warped(self, sxl, sxr, sy, deg, size):
        """Template and mask on the canvas after the half scales and rotation about the pivot (no translation)."""
        h, w = size
        r = np.deg2rad(deg)
        c, s = np.cos(r), np.sin(r)
        gy, gx = np.mgrid[0:h, 0:w].astype(np.float32)
        qx = gx.ravel() - self.pivot[0]
        qy = gy.ravel() - self.pivot[1]
        rx = c * qx + s * qy      # R^-1 (q - pivot); the half scales keep the sign of x
        ry = -s * qx + c * qy
        px = np.where(rx < 0, rx / sxl, rx / sxr) + self.pivot[0]
        py = ry / sy + self.pivot[1]
        x0, x1, y0, y1 = self.rect
        inside = (px >= x0) & (px <= x1 - 1) & (py >= y0) & (py <= y1 - 1)
        tmpl = np.zeros((h * w, self.front.shape[2]), np.float32)
        tmpl[inside] = sample(self.front, px[inside], py[inside])
        return tmpl.reshape(h, w, -1), inside.reshape(h, w).astype(np.float32)

    def translations(self, frame, sxl, sxr, sy, deg, reach=90):
        """Masked SSD for every integer translation via FFT; returns (cost, tx, ty) of the best one."""
        h, w = frame.shape[:2]
        tmpl, mask = self.warped(sxl, sxr, sy, deg, (h, w))
        shape = (2 * h, 2 * w)
        fr = np.fft.rfft2
        acc = 0
        for ch in range(frame.shape[2]):
            acc = acc + fr(frame[..., ch], shape) * np.conj(fr(mask * tmpl[..., ch], shape))
        cross = np.fft.irfft2(acc, shape)
        energy = np.fft.irfft2(fr((frame * frame).sum(axis=2), shape) * np.conj(fr(mask, shape)), shape)
        ssd = energy - 2.0 * cross + float((mask[..., None] * tmpl * tmpl).sum())
        ty = np.arange(shape[0])
        ty = np.where(ty >= h, ty - shape[0], ty)
        tx = np.arange(shape[1])
        tx = np.where(tx >= w, tx - shape[1], tx)
        ok = (np.abs(ty)[:, None] <= reach) & (np.abs(tx)[None, :] <= reach)
        ssd = np.where(ok, ssd, np.inf) / max(1.0, float(mask.sum()))
        i = np.unravel_index(int(np.argmin(ssd)), ssd.shape)
        return float(ssd[i]), float(ty[i[0]]), float(tx[i[1]])

    def solve(self, frame, symmetric=False):
        """Best anchor for `frame`. symmetric=True ties both half scales (the repair fit for implausible results)."""
        frame = frame[..., :self.front.shape[2]]
        best = None
        # FFT translation search over half scales (turned heads) and tilts, then a pattern search on all six.
        scales = (1.0, 0.85, 0.7) if symmetric else (1.0, 0.75, 0.5)
        for sxl in scales:
            for sxr in scales:
                if symmetric and sxr != sxl:
                    continue
                for deg in (-12.0, 0.0, 12.0):
                    cst, ty, tx = self.translations(frame, sxl, sxr, 1.0, deg)
                    if best is None or cst < best[0]:
                        best = (cst, [tx, ty, sxl, sxr, 1.0, deg])
        p = best[1]
        cst = self.cost(frame, p)
        steps = [2.0, 2.0, 0.05, 0.05, 0.04, 3.0]
        for _ in range(7):
            improved = True
            while improved:
                improved = False
                for i in range(6):
                    if symmetric and i == 3:
                        continue
                    for sign in (1, -1):
                        q = list(p)
                        q[i] = min(HI[i], max(LO[i], q[i] + sign * steps[i]))
                        if symmetric:
                            q[2] = q[3] = max(0.6, q[2])
                        c2 = self.cost(frame, q)
                        if c2 < cst - 1e-7:
                            cst, p, improved = c2, q, True
            steps = [s * 0.55 for s in steps]
        return [round(p[0], 2), round(p[1], 2), round(p[2], 4), round(p[3], 4), round(p[4], 4), round(p[5], 2),
                round(cst, 5)]


def coverage(silhouette, points, weights, pivot, p):
    """Share of the overlay (alpha-weighted) that lands on the bird's silhouette when drawn through anchor p."""
    tx, ty, sxl, sxr, sy, deg = p[:6]
    r = np.deg2rad(deg)
    c, s = np.cos(r), np.sin(r)
    dx = points[:, 0] - pivot[0]
    dy = points[:, 1] - pivot[1]
    u = np.where(dx < 0, sxl, sxr) * dx
    v = sy * dy
    x = pivot[0] + tx + c * u - s * v
    y = pivot[1] + ty + s * u + c * v
    on = sample(silhouette[..., None], x, y)[:, 0]
    return float((on * weights).sum() / weights.sum())


def plausible(p, cov, base_cov, eyewear):
    """Reject fits that tear the overlay apart or move it off the bird (the front coverage is the reference)."""
    ratio = max(p[2], p[3]) / max(1e-3, min(p[2], p[3]))
    return (cov >= base_cov - 0.25 and abs(p[5]) <= 30.0 and min(p[2], p[3]) >= (0.25 if eyewear else 0.4)
            and ratio <= (3.5 if eyewear else 2.2))


def measure(key, repair_only=None):
    species = key.split("_outfit_")[0]
    front, dark_level = load(os.path.join(BIRDS, species + "_front.png"))
    alpha = np.asarray(Image.open(os.path.join(BIRDS, key + "_front_overlay.png")).convert("RGBA"), np.float32)[..., 3]
    alpha = alpha / 255.0
    eyewear = key in EYEWEAR
    fit = Fit(front, alpha, eyewear)
    ys, xs = np.nonzero(alpha > 0.04)
    points = np.stack([xs, ys], axis=1).astype(np.float32)
    weights = alpha[ys, xs]

    def silhouette(name):
        a = np.asarray(Image.open(os.path.join(BIRDS, "%s_%s.png" % (species, name))).convert("RGBA"), np.float32)
        return (a[..., 3] / 255.0 > 0.5).astype(np.float32)

    base_cov = coverage(silhouette("front"), points, weights, fit.pivot, IDENTITY)
    frames = dict(repair_only["frames"]) if repair_only else {}
    flagged = []
    for name in frame_names(species):
        if name in ("front", "front_blink_01", "front_blink_02"):
            frames[name] = list(IDENTITY)
            continue
        feat = None
        if name not in frames:
            feat = load(os.path.join(BIRDS, "%s_%s.png" % (species, name)), dark_level)[0]
            frames[name] = fit.solve(feat)
        sil = silhouette(name)
        if plausible(frames[name], coverage(sil, points, weights, fit.pivot, frames[name]), base_cov, eyewear):
            continue
        if feat is None:
            feat = load(os.path.join(BIRDS, "%s_%s.png" % (species, name)), dark_level)[0]
        again = fit.solve(feat, symmetric=True)
        if plausible(again, coverage(sil, points, weights, fit.pivot, again), base_cov, eyewear):
            frames[name] = again
            flagged.append(name + ":refit")
        else:
            frames[name] = None
            flagged.append(name + ":neighbour")
    # Frames with no plausible fit take the nearest plausible frame of the same sequence (or the front).
    names = list(frames.keys())
    for i, name in enumerate(names):
        if frames[name] is not None:
            continue
        stem = name.rsplit("_", 1)[0] if name[-1].isdigit() else name
        near = [n for n in names if frames[n] is not None and n.startswith(stem)]
        pick = min(near, key=lambda n: abs(names.index(n) - i)) if near else "front"
        frames[name] = list(frames[pick])
    if flagged:
        print(key, "repaired", " ".join(flagged), file=sys.stderr)
    return key, {"pivot": [round(float(fit.pivot[0]), 2), round(float(fit.pivot[1]), 2)], "frames": frames,
                 "repaired": flagged}


def place(overlay_img, pivot, p):
    """Overlay drawn through anchor p on a transparent 320 canvas (same mapping as BirdView)."""
    tx, ty, sxl, sxr, sy, deg = p[:6]
    r = np.deg2rad(deg)
    c, s = np.cos(r), np.sin(r)
    out = Image.new("RGBA", overlay_img.size)
    cut = int(round(pivot[0]))
    for sx, box in ((sxl, (0, 0, cut, overlay_img.height)), (sxr, (cut, 0, overlay_img.width, overlay_img.height))):
        half = Image.new("RGBA", overlay_img.size)
        half.paste(overlay_img.crop(box), box[:2])
        # Inverse affine for PIL: canvas q -> overlay p = pivot + S^-1 R^-1 (q - pivot - t)
        a, b = c / sx, s / sx
        d, e = -s / sy, c / sy
        ox, oy = pivot[0] + tx, pivot[1] + ty
        coeffs = (a, b, pivot[0] - a * ox - b * oy, d, e, pivot[1] - d * ox - e * oy)
        out.alpha_composite(half.transform(half.size, Image.AFFINE, coeffs, resample=Image.BICUBIC))
    return out


def sheet(key, data, out_dir):
    species = key.split("_outfit_")[0]
    overlay = Image.open(os.path.join(BIRDS, key + "_front_overlay.png")).convert("RGBA")
    names = list(data["frames"].keys())
    cell, cols = 256, 8
    rows = (len(names) + cols - 1) // cols
    img = Image.new("RGB", (cols * cell, rows * (cell + 14)), (150, 170, 140))
    draw = ImageDraw.Draw(img)
    for i, name in enumerate(names):
        frame = Image.open(os.path.join(BIRDS, "%s_%s.png" % (species, name))).convert("RGBA")
        frame.alpha_composite(place(overlay, data["pivot"], data["frames"][name]))
        frame = frame.resize((cell, cell), Image.LANCZOS)
        x, y = (i % cols) * cell, (i // cols) * (cell + 14)
        img.paste(frame, (x, y), frame)
        draw.text((x + 2, y + cell), "%s %.4f" % (name[:22], data["frames"][name][6]), fill=(0, 0, 0))
    img.save(os.path.join(out_dir, key + ".jpg"), quality=88)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--sheet", help="directory for verification contact sheets")
    ap.add_argument("--only", nargs="*", help="outfit keys to measure, e.g. berry_outfit_1")
    ap.add_argument("--sheets-only", action="store_true", help="draw sheets from the existing JSON")
    ap.add_argument("--repair", action="store_true", help="only re-check and repair the frames of the existing JSON")
    args = ap.parse_args()
    keys = sorted(f[: -len("_front_overlay.png")] for f in os.listdir(BIRDS) if f.endswith("_front_overlay.png"))
    if args.only:
        keys = [k for k in keys if k in args.only]
    table = {}
    if os.path.exists(OUT):
        with open(OUT) as fh:
            table = json.load(fh)
    if not args.sheets_only:
        jobs = [table.get(k) if args.repair else None for k in keys]
        with ProcessPoolExecutor() as pool:
            for key, data in pool.map(measure, keys, jobs):
                table[key] = data
                print("measured", key, file=sys.stderr)
                with open(OUT, "w") as fh:
                    json.dump(dict(sorted(table.items())), fh, separators=(",", ":"))
                    fh.write("\n")
    if args.sheet:
        os.makedirs(args.sheet, exist_ok=True)
        for key in keys:
            sheet(key, table[key], args.sheet)


if __name__ == "__main__":
    main()
