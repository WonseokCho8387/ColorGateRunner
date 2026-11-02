// Deterministically regenerates the seven Theme01 9-slice UI surfaces in the
// Iteration 61 neon-night language. Output keeps the existing file names,
// 256 x 256 size and 28 px slice border so Builder routing is unchanged.
//
// Usage: node Tools/Generate-NeonUiSurfaces.mjs
import { writeFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { deflateSync } from 'node:zlib';

const Size = 256;
const SliceBorder = 28;
const Margin = 8;
const Samples = 4;
const OutputFolder = join(
  dirname(fileURLToPath(import.meta.url)),
  '..', 'ColorGateRunner', 'Assets', 'Game', 'Art', 'UI', 'Theme01');

// Mirrors ColorGateRunner.Presentation.NeonUiPalette.
const Palette = {
  ink: 0x05060d,
  night: 0x0a0f1f,
  signal: 0xc8f4ff,
  magenta: 0xff2bd6,
  danger: 0xff5a4e,
};

const Surfaces = [
  { name: 'Panel', fill: Palette.night, fillAlpha: 0.86, stroke: Palette.signal, strokeAlpha: 0.78, accent: Palette.signal, cut: 16, glow: 0.28 },
  { name: 'Modal', fill: Palette.ink, fillAlpha: 0.94, stroke: Palette.signal, strokeAlpha: 0.9, accent: Palette.magenta, cut: 18, glow: 0.4 },
  { name: 'ItemCard', fill: Palette.night, fillAlpha: 0.9, stroke: Palette.signal, strokeAlpha: 0.85, accent: Palette.signal, cut: 14, glow: 0.3 },
  { name: 'ButtonPrimary', fill: Palette.magenta, fillAlpha: 0.3, stroke: Palette.magenta, strokeAlpha: 1, accent: Palette.signal, cut: 14, glow: 0.55, base: Palette.night },
  { name: 'ButtonSecondary', fill: Palette.night, fillAlpha: 0.88, stroke: Palette.signal, strokeAlpha: 0.85, accent: Palette.signal, cut: 14, glow: 0.3 },
  { name: 'ButtonDanger', fill: Palette.danger, fillAlpha: 0.22, stroke: Palette.danger, strokeAlpha: 1, accent: Palette.danger, cut: 14, glow: 0.45, base: Palette.night },
  { name: 'ResourceChip', fill: Palette.night, fillAlpha: 0.8, stroke: Palette.signal, strokeAlpha: 0.6, accent: Palette.signal, cut: 10, glow: 0.18 },
];

function rgb(hex) {
  return [((hex >> 16) & 0xff) / 255, ((hex >> 8) & 0xff) / 255, (hex & 0xff) / 255];
}

// Asymmetric octagon: large top-left / bottom-right cuts, small opposite cuts.
function polygon(cut) {
  const a = Margin;
  const b = Size - Margin;
  const small = 4;
  return [
    [a + cut, a], [b - small, a], [b, a + small], [b, b - cut],
    [b - cut, b], [a + small, b], [a, b - small], [a, a + cut],
  ];
}

// Signed distance (negative inside) for a convex polygon; exact inside.
function signedDistance(points, x, y) {
  let inside = -Infinity;
  let outside = Infinity;
  let isOutside = false;
  for (let i = 0; i < points.length; i++) {
    const [x0, y0] = points[i];
    const [x1, y1] = points[(i + 1) % points.length];
    const ex = x1 - x0;
    const ey = y1 - y0;
    const length = Math.hypot(ex, ey);
    // Clockwise in screen space: outward normal is (ey, -ex).
    const plane = ((x - x0) * ey - (y - y0) * ex) / length;
    inside = Math.max(inside, plane);
    if (plane > 0) isOutside = true;
    const t = Math.max(0, Math.min(1, ((x - x0) * ex + (y - y0) * ey) / (length * length)));
    outside = Math.min(outside, Math.hypot(x - (x0 + ex * t), y - (y0 + ey * t)));
  }
  return isOutside ? outside : inside;
}

function over(dst, color, alpha) {
  const outA = alpha + dst[3] * (1 - alpha);
  if (outA <= 0) return [0, 0, 0, 0];
  for (let c = 0; c < 3; c++) {
    dst[c] = (color[c] * alpha + dst[c] * dst[3] * (1 - alpha)) / outA;
  }
  dst[3] = outA;
  return dst;
}

function shade(surface, points, x, y) {
  const d = signedDistance(points, x, y);
  const pixel = [0, 0, 0, 0];
  const stroke = rgb(surface.stroke);
  if (d > 0) {
    const glow = Math.max(0, 1 - d / Margin);
    return over(pixel, stroke, surface.glow * glow * glow);
  }

  if (surface.base !== undefined) over(pixel, rgb(surface.base), 0.92);
  // Gentle vertical lift toward the top edge; stretches cleanly when sliced.
  const lift = 1 + 0.18 * (1 - y / Size);
  const fill = rgb(surface.fill).map(c => Math.min(1, c * lift));
  over(pixel, fill, surface.fillAlpha);

  const depth = -d;
  if (depth <= 2) over(pixel, stroke, surface.strokeAlpha);
  else if (depth > 5 && depth <= 6) over(pixel, stroke, surface.strokeAlpha * 0.22);

  // Bright accent along the two large chamfers.
  const onTopLeft = x + y < Margin * 2 + surface.cut + 6;
  const onBottomRight = x + y > Size * 2 - Margin * 2 - surface.cut - 6;
  if ((onTopLeft || onBottomRight) && depth <= 4) {
    over(pixel, rgb(surface.accent), 1);
  }
  return pixel;
}

function render(surface) {
  const points = polygon(surface.cut);
  if (Margin + surface.cut > SliceBorder) {
    throw new Error(`${surface.name} corner exceeds the 9-slice border.`);
  }
  const data = Buffer.alloc(Size * (Size * 4 + 1));
  for (let y = 0; y < Size; y++) {
    data[y * (Size * 4 + 1)] = 0;
    for (let x = 0; x < Size; x++) {
      const sum = [0, 0, 0, 0];
      for (let sy = 0; sy < Samples; sy++) {
        for (let sx = 0; sx < Samples; sx++) {
          const p = shade(
            surface,
            points,
            x + (sx + 0.5) / Samples,
            y + (sy + 0.5) / Samples);
          for (let c = 0; c < 3; c++) sum[c] += p[c] * p[3];
          sum[3] += p[3];
        }
      }
      const count = Samples * Samples;
      const alpha = sum[3] / count;
      const offset = y * (Size * 4 + 1) + 1 + x * 4;
      for (let c = 0; c < 3; c++) {
        const straight = sum[3] > 0 ? sum[c] / sum[3] : 0;
        data[offset + c] = Math.round(Math.min(1, straight) * 255);
      }
      data[offset + 3] = Math.round(alpha * 255);
    }
  }
  return data;
}

const CrcTable = Array.from({ length: 256 }, (_, n) => {
  let c = n;
  for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
  return c >>> 0;
});

function crc32(buffer) {
  let c = 0xffffffff;
  for (const byte of buffer) c = CrcTable[(c ^ byte) & 0xff] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
}

function chunk(type, payload) {
  const length = Buffer.alloc(4);
  length.writeUInt32BE(payload.length);
  const body = Buffer.concat([Buffer.from(type, 'ascii'), payload]);
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(body));
  return Buffer.concat([length, body, crc]);
}

function encodePng(raw) {
  const header = Buffer.alloc(13);
  header.writeUInt32BE(Size, 0);
  header.writeUInt32BE(Size, 4);
  header[8] = 8;
  header[9] = 6;
  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    chunk('IHDR', header),
    chunk('IDAT', deflateSync(raw, { level: 9 })),
    chunk('IEND', Buffer.alloc(0)),
  ]);
}

for (const surface of Surfaces) {
  const path = join(OutputFolder, `${surface.name}.png`);
  writeFileSync(path, encodePng(render(surface)));
  console.log(`wrote ${surface.name}.png`);
}
