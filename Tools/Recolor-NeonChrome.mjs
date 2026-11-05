// Deterministically recolors the remaining gold chrome on the Frontend frames
// and Theme01 icons into the Iteration 61 neon language (Gold is reserved for
// currency). Pixels in the gold hue band blend into a cool steel-cyan chrome
// that keeps the original shading; everything else is untouched. The Play
// icon's green fill becomes Magenta, the primary-action color. File names,
// sizes, alpha and ancillary PNG chunks are unchanged, so Builder routing and
// layout stay the same.
//
// The source is always the pre-recolor art at SourceRevision, read through
// git, so repeated runs write identical files.
//
// Usage:
//   node Tools/Recolor-NeonChrome.mjs          recolor from SourceRevision
//   node Tools/Recolor-NeonChrome.mjs --audit  report the working-tree gold share
import { execFileSync } from 'node:child_process';
import { readFileSync, writeFileSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { deflateSync, inflateSync } from 'node:zlib';

const RepoRoot = join(dirname(fileURLToPath(import.meta.url)), '..');
const ThemePath = 'ColorGateRunner/Assets/Game/Art/UI/Theme01';
const SourceRevision = '6e7f615';

// IconCoin stays gold: it is the currency. IconContinue has no gold.
export const Targets = [
  'Frontend/TopResourceBar', 'Frontend/BottomNavigationDock',
  'Frontend/NavigationSelection', 'Frontend/LobbyStageCard',
  'Frontend/PageHeader',
  'Navigation/IconNavShop', 'Navigation/IconNavRank', 'Navigation/IconNavHome',
  'Navigation/IconNavJourney', 'Navigation/IconNavCollection',
  'IconHeart', 'IconProfile', 'IconBooster', 'IconShield',
  'IconBack', 'IconPause', 'IconPlay', 'IconRetry', 'IconSettings',
];

// Hue in degrees; a pixel counts as gold inside this band (soft edges).
// Dark bronze shadows, pale tan and yellow-green highlights belong to the
// same trim.
const GoldHue = [26, 80];
const GoldEdge = 6;
const GoldSaturation = [0.1, 0.22];
const GoldValue = [0.05, 0.14];
const ChromeHue = 192;
const ChromeSaturation = 0.42;
const GreenHue = [95, 165];
const MagentaHue = 313;

function smooth(edge0, edge1, x) {
  const t = Math.min(1, Math.max(0, (x - edge0) / (edge1 - edge0)));
  return t * t * (3 - 2 * t);
}

function band(hue, [low, high], edge) {
  return smooth(low - edge, low, hue) * (1 - smooth(high, high + edge, hue));
}

function toHsv(r, g, b) {
  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);
  const delta = max - min;
  let h = 0;
  if (delta > 0) {
    if (max === r) h = 60 * (((g - b) / delta) % 6);
    else if (max === g) h = 60 * ((b - r) / delta + 2);
    else h = 60 * ((r - g) / delta + 4);
  }
  if (h < 0) h += 360;
  return [h, max === 0 ? 0 : delta / max, max];
}

function fromHsv(h, s, v) {
  const c = v * s;
  const x = c * (1 - Math.abs(((h / 60) % 2) - 1));
  const m = v - c;
  const [r, g, b] = h < 60 ? [c, x, 0] : h < 120 ? [x, c, 0]
    : h < 180 ? [0, c, x] : h < 240 ? [0, x, c] : h < 300 ? [x, 0, c] : [c, 0, x];
  return [r + m, g + m, b + m];
}

// Weight of the gold band for one pixel (0..1).
export function goldWeight(r, g, b) {
  const [h, s, v] = toHsv(r / 255, g / 255, b / 255);
  return band(h, GoldHue, GoldEdge) * smooth(...GoldSaturation, s) *
    smooth(...GoldValue, v);
}

function decodePng(buffer) {
  let offset = 8;
  let width = 0;
  let height = 0;
  let colorType = 0;
  const idat = [];
  // Ancillary chunks (color space, density, provenance) pass through as-is.
  const before = [];
  const after = [];
  while (offset < buffer.length) {
    const length = buffer.readUInt32BE(offset);
    const type = buffer.toString('ascii', offset + 4, offset + 8);
    const body = buffer.subarray(offset + 8, offset + 8 + length);
    if (type === 'IHDR') {
      width = body.readUInt32BE(0);
      height = body.readUInt32BE(4);
      colorType = body[9];
      if (body[8] !== 8 || (colorType !== 2 && colorType !== 6) || body[12] !== 0) {
        throw new Error('Unsupported PNG layout');
      }
    } else if (type === 'IDAT') {
      idat.push(body);
    } else if (type !== 'IEND') {
      (idat.length === 0 ? before : after).push(
        buffer.subarray(offset, offset + 12 + length));
    }
    offset += 12 + length;
  }
  const bpp = colorType === 6 ? 4 : 3;
  const raw = inflateSync(Buffer.concat(idat));
  const stride = width * bpp;
  const pixels = new Uint8Array(width * height * 4);
  const previous = new Uint8Array(stride);
  const line = new Uint8Array(stride);
  for (let y = 0; y < height; y++) {
    const start = y * (stride + 1);
    const filter = raw[start];
    for (let i = 0; i < stride; i++) {
      const a = i >= bpp ? line[i - bpp] : 0;
      const up = previous[i];
      const c = i >= bpp ? previous[i - bpp] : 0;
      let value = raw[start + 1 + i];
      if (filter === 1) value += a;
      else if (filter === 2) value += up;
      else if (filter === 3) value += (a + up) >> 1;
      else if (filter === 4) {
        const p = a + up - c;
        const pa = Math.abs(p - a);
        const pb = Math.abs(p - up);
        const pc = Math.abs(p - c);
        value += pa <= pb && pa <= pc ? a : pb <= pc ? up : c;
      }
      line[i] = value & 0xff;
    }
    for (let x = 0; x < width; x++) {
      const o = (y * width + x) * 4;
      pixels[o] = line[x * bpp];
      pixels[o + 1] = line[x * bpp + 1];
      pixels[o + 2] = line[x * bpp + 2];
      pixels[o + 3] = bpp === 4 ? line[x * bpp + 3] : 255;
    }
    previous.set(line);
  }
  return { width, height, colorType, pixels, before, after };
}

const CrcTable = Array.from({ length: 256 }, (_, n) => {
  let c = n;
  for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
  return c >>> 0;
});

function crc32(buffer) {
  let crc = 0xffffffff;
  for (const byte of buffer) crc = CrcTable[(crc ^ byte) & 0xff] ^ (crc >>> 8);
  return (crc ^ 0xffffffff) >>> 0;
}

function chunk(type, payload) {
  const length = Buffer.alloc(4);
  length.writeUInt32BE(payload.length);
  const body = Buffer.concat([Buffer.from(type, 'ascii'), payload]);
  const crc = Buffer.alloc(4);
  crc.writeUInt32BE(crc32(body));
  return Buffer.concat([length, body, crc]);
}

function paeth(a, b, c) {
  const p = a + b - c;
  const pa = Math.abs(p - a);
  const pb = Math.abs(p - b);
  const pc = Math.abs(p - c);
  return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
}

// Each row takes the filter with the smallest absolute residual sum.
function encodePng({ width, height, colorType, pixels, before, after }) {
  const bpp = colorType === 6 ? 4 : 3;
  const stride = width * bpp;
  const raw = Buffer.alloc((stride + 1) * height);
  let previous = new Uint8Array(stride);
  const line = new Uint8Array(stride);
  const candidate = new Uint8Array(stride);
  for (let y = 0; y < height; y++) {
    for (let x = 0; x < width; x++) {
      const o = (y * width + x) * 4;
      for (let k = 0; k < bpp; k++) line[x * bpp + k] = pixels[o + k];
    }
    let bestScore = Infinity;
    const row = y * (stride + 1);
    for (let filter = 0; filter <= 4; filter++) {
      let score = 0;
      for (let i = 0; i < stride; i++) {
        const a = i >= bpp ? line[i - bpp] : 0;
        const b = previous[i];
        const c = i >= bpp ? previous[i - bpp] : 0;
        const predictor = filter === 1 ? a : filter === 2 ? b
          : filter === 3 ? (a + b) >> 1 : filter === 4 ? paeth(a, b, c) : 0;
        const value = (line[i] - predictor) & 0xff;
        candidate[i] = value;
        score += value < 128 ? value : 256 - value;
      }
      if (score < bestScore) {
        bestScore = score;
        raw[row] = filter;
        raw.set(candidate, row + 1);
      }
    }
    previous = Uint8Array.from(line);
  }
  const header = Buffer.alloc(13);
  header.writeUInt32BE(width, 0);
  header.writeUInt32BE(height, 4);
  header[8] = 8;
  header[9] = colorType;
  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    chunk('IHDR', header),
    ...before,
    chunk('IDAT', deflateSync(raw, { level: 9 })),
    ...after,
    chunk('IEND', Buffer.alloc(0)),
  ]);
}

// Share of visible pixels that read as gold.
export function goldShare(image) {
  let visible = 0;
  let gold = 0;
  const { pixels } = image;
  for (let o = 0; o < pixels.length; o += 4) {
    if (pixels[o + 3] < 40) continue;
    visible++;
    if (goldWeight(pixels[o], pixels[o + 1], pixels[o + 2]) > 0.5) gold++;
  }
  return visible === 0 ? 0 : gold / visible;
}

function recolor(image, magentaFromGreen) {
  const { pixels } = image;
  for (let o = 0; o < pixels.length; o += 4) {
    if (pixels[o + 3] === 0) continue;
    const r = pixels[o] / 255;
    const g = pixels[o + 1] / 255;
    const b = pixels[o + 2] / 255;
    const [h, s, v] = toHsv(r, g, b);
    let weight = band(h, GoldHue, GoldEdge) * smooth(...GoldSaturation, s) *
      smooth(...GoldValue, v);
    let target = null;
    if (weight > 0) {
      target = fromHsv(ChromeHue, Math.min(s, ChromeSaturation), v);
    } else if (magentaFromGreen) {
      weight = band(h, GreenHue, 10) * smooth(0.2, 0.35, s);
      if (weight > 0) target = fromHsv(MagentaHue, s, v);
    }
    if (target === null) continue;
    // Blend through the pixel's own gray so partial weights desaturate
    // instead of passing through olive between gold and cyan.
    const [mr, mg, mb] = weight < 0.5
      ? [r + (v - r) * 2 * weight, g + (v - g) * 2 * weight,
        b + (v - b) * 2 * weight]
      : [v + (target[0] - v) * (2 * weight - 1),
        v + (target[1] - v) * (2 * weight - 1),
        v + (target[2] - v) * (2 * weight - 1)];
    pixels[o] = Math.round(255 * mr);
    pixels[o + 1] = Math.round(255 * mg);
    pixels[o + 2] = Math.round(255 * mb);
  }
}

function source(name) {
  return execFileSync('git', ['show', `${SourceRevision}:${ThemePath}/${name}.png`],
    { cwd: RepoRoot, maxBuffer: 64 * 1024 * 1024 });
}

const audit = process.argv.includes('--audit');
for (const name of [...Targets, 'IconCoin']) {
  const path = join(RepoRoot, ThemePath, `${name}.png`);
  if (audit || name === 'IconCoin') {
    const image = decodePng(readFileSync(path));
    console.log(`${name}: gold ${(100 * goldShare(image)).toFixed(1)}%`);
    continue;
  }
  const image = decodePng(source(name));
  const before = goldShare(image);
  recolor(image, name === 'IconPlay');
  writeFileSync(path, encodePng(image));
  console.log(`${name}: gold ${(100 * before).toFixed(1)}% -> ${(100 * goldShare(image)).toFixed(1)}%`);
}
