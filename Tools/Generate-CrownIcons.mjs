// Deterministically draws the three Stage crown icons in the neon-night
// language: CLEAN (cyan, three points), COMBO (magenta, five points, one band
// gem) and FULL COMBO (gold, five points, three band gems and a top sparkle).
// Each icon is a 256 x 256 transparent PNG; the result screen tints the same
// sprite for owned, newly earned and missing crowns.
//
// Usage: node Tools/Generate-CrownIcons.mjs
import { writeFileSync, mkdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { deflateSync } from 'node:zlib';

const Size = 256;
const Samples = 4;
const GlowWidth = 14;
// Shapes are drawn 14 px lower so the top sparkle's glow stays inside.
const OffsetY = 14;
const OutputFolder = join(
  dirname(fileURLToPath(import.meta.url)),
  '..', 'ColorGateRunner', 'Assets', 'Game', 'Art', 'UI', 'Theme01', 'Crowns');

const Crowns = [
  { name: 'CrownClean', color: 0x35e8ff, points: 3, gems: 0, sparkle: false },
  { name: 'CrownCombo', color: 0xff2bd6, points: 5, gems: 1, sparkle: false },
  { name: 'CrownFullCombo', color: 0xffc83d, points: 5, gems: 3, sparkle: true },
];

const White = [1, 1, 1];
const Ink = rgb(0x05060d);

function rgb(hex) {
  return [((hex >> 16) & 0xff) / 255, ((hex >> 8) & 0xff) / 255, (hex & 0xff) / 255];
}

function mix(a, b, t) {
  return a.map((value, index) => value + (b[index] - value) * t);
}

// Crown body outline (y down), tips listed for the orbs.
function crownShape(points) {
  if (points === 3) {
    const tips = [[50, 70], [128, 50], [206, 70]];
    return {
      tips,
      polygon: [[50, 172], tips[0], [90, 124], tips[1], [166, 124], tips[2], [206, 172]],
    };
  }
  const tips = [[48, 78], [88, 96], [128, 50], [168, 96], [208, 78]];
  return {
    tips,
    polygon: [
      [48, 172], tips[0], [70, 132], tips[1], [108, 128], tips[2],
      [148, 128], tips[3], [186, 132], tips[4], [208, 172],
    ],
  };
}

function segmentDistance(x, y, x0, y0, x1, y1) {
  const ex = x1 - x0;
  const ey = y1 - y0;
  const t = Math.max(0, Math.min(1, ((x - x0) * ex + (y - y0) * ey) / (ex * ex + ey * ey)));
  return Math.hypot(x - (x0 + ex * t), y - (y0 + ey * t));
}

// Signed distance (negative inside) for any simple polygon.
function polygonDistance(points, x, y) {
  let distance = Infinity;
  let inside = false;
  for (let i = 0, j = points.length - 1; i < points.length; j = i++) {
    const [xi, yi] = points[i];
    const [xj, yj] = points[j];
    distance = Math.min(distance, segmentDistance(x, y, xj, yj, xi, yi));
    if ((yi > y) !== (yj > y) && x < ((xj - xi) * (y - yi)) / (yj - yi) + xi) {
      inside = !inside;
    }
  }
  return inside ? -distance : distance;
}

function boxDistance(x, y, left, top, right, bottom) {
  const dx = Math.max(left - x, 0, x - right);
  const dy = Math.max(top - y, 0, y - bottom);
  const outside = Math.hypot(dx, dy);
  const inside = Math.min(0, Math.max(left - x, x - right, top - y, y - bottom));
  return outside > 0 ? outside : inside;
}

function diamondDistance(x, y, cx, cy, radius) {
  return (Math.abs(x - cx) + Math.abs(y - cy) - radius) / Math.SQRT2;
}

function over(dst, color, alpha) {
  if (alpha <= 0) return;
  const outA = alpha + dst[3] * (1 - alpha);
  for (let c = 0; c < 3; c++) {
    dst[c] = (color[c] * alpha + dst[c] * dst[3] * (1 - alpha)) / outA;
  }
  dst[3] = outA;
}

// A filled shape with a dark-to-bright fill and a bright neon stroke.
function paintShape(pixel, d, fill, stroke) {
  if (d > 0) return;
  over(pixel, fill, 1);
  if (d > -3.5) over(pixel, stroke, 1);
  else if (d > -6.5) over(pixel, stroke, 0.22);
}

function shade(crown, shape, x, y) {
  const pixel = [0, 0, 0, 0];
  const color = rgb(crown.color);
  const bright = mix(color, White, 0.55);
  const band = { left: 40, top: 166, right: 216, bottom: 206 };

  const body = polygonDistance(shape.polygon, x, y);
  const bandDistance = boxDistance(x, y, band.left, band.top, band.right, band.bottom);
  let orb = Infinity;
  shape.tips.forEach(([tx, ty], index) => {
    const radius = index === Math.floor(shape.tips.length / 2) ? 15 : 11;
    orb = Math.min(orb, Math.hypot(x - tx, y - ty) - radius);
  });
  let sparkle = Infinity;
  if (crown.sparkle) {
    // Four-point star (astroid) above the center tip.
    const radius = 19;
    const sx = Math.abs(x - 128) / radius;
    const sy = Math.abs(y - 21) / radius;
    // The astroid term underestimates distance along its arms, so the
    // circle bound keeps the glow round.
    sparkle = Math.max(
      (Math.pow(sx, 2 / 3) + Math.pow(sy, 2 / 3) - 1) * radius * 0.45,
      Math.hypot(x - 128, y - 21) - radius);
  }

  const union = Math.min(body, bandDistance, orb, sparkle);
  if (union > 0) {
    const glow = Math.max(0, 1 - union / GlowWidth);
    over(pixel, color, 0.5 * glow * glow);
    return pixel;
  }

  // Body: deep tint at the base rising to the crown color at the tips.
  const lift = Math.max(0, Math.min(1, (172 - y) / 120));
  paintShape(pixel, body, mix(mix(Ink, color, 0.45), color, lift * 0.75), bright);
  paintShape(pixel, bandDistance, mix(Ink, color, 0.32), bright);
  if (bandDistance <= 0 && Math.abs(y - (band.top + 9)) < 1.5 && bandDistance < -4) {
    over(pixel, bright, 0.55);
  }

  const gemCenters = crown.gems === 1 ? [128] : crown.gems === 3 ? [88, 128, 168] : [];
  for (const gx of gemCenters) {
    const gem = diamondDistance(x, y, gx, 186, gx === 128 ? 14 : 11);
    paintShape(pixel, gem, mix(color, White, 0.2), White);
  }

  if (orb <= 0) {
    over(pixel, bright, 1);
    if (orb > -3) over(pixel, White, 0.9);
    // Small specular highlight toward the upper left of each orb.
    for (const [tx, ty] of shape.tips) {
      if (Math.hypot(x - (tx - 4), y - (ty - 4)) < 4) over(pixel, White, 0.95);
    }
  }
  if (sparkle <= 0) over(pixel, mix(color, White, 0.7), 1);
  return pixel;
}

function render(crown) {
  const shape = crownShape(crown.points);
  const data = Buffer.alloc(Size * (Size * 4 + 1));
  for (let y = 0; y < Size; y++) {
    data[y * (Size * 4 + 1)] = 0;
    for (let x = 0; x < Size; x++) {
      const sum = [0, 0, 0, 0];
      for (let sy = 0; sy < Samples; sy++) {
        for (let sx = 0; sx < Samples; sx++) {
          const p = shade(
            crown, shape, x + (sx + 0.5) / Samples, y - OffsetY + (sy + 0.5) / Samples);
          for (let c = 0; c < 3; c++) sum[c] += p[c] * p[3];
          sum[3] += p[3];
        }
      }
      const offset = y * (Size * 4 + 1) + 1 + x * 4;
      for (let c = 0; c < 3; c++) {
        const straight = sum[3] > 0 ? sum[c] / sum[3] : 0;
        data[offset + c] = Math.round(Math.min(1, straight) * 255);
      }
      data[offset + 3] = Math.round((sum[3] / (Samples * Samples)) * 255);
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

mkdirSync(OutputFolder, { recursive: true });
for (const crown of Crowns) {
  writeFileSync(join(OutputFolder, `${crown.name}.png`), encodePng(render(crown)));
  console.log(`wrote ${crown.name}.png`);
}
