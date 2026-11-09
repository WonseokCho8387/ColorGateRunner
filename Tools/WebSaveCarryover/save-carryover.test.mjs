// Run: node --test Tools/WebSaveCarryover/save-carryover.test.mjs
import { test } from "node:test";
import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { createRequire } from "node:module";
import { fileURLToPath } from "node:url";
import path from "node:path";

const here = path.dirname(fileURLToPath(import.meta.url));
const require = createRequire(import.meta.url);
const carry = require(path.join(here, "../../ColorGateRunner/Assets/WebGLTemplates/ColorGateRunnerPortrait/save-carryover.js"));

const md5 = (text) => createHash("md5").update(text, "utf8").digest("hex");
const bytes = (text) => Int8Array.from(Buffer.from(text, "utf8"));
const save = (lastWrite, extra = {}) => bytes(JSON.stringify({
  SchemaVersion: 3, SaveRevision: 5, Profile: { ProfileId: "p" },
  CampaignProgress: { Records: [] }, LastWriteUtc: lastWrite, ...extra,
}));
const file = (contents, timestamp = new Date("2026-11-01T00:00:00Z")) =>
  ({ timestamp, mode: 33206, contents });
const dir = () => ({ timestamp: new Date("2026-11-01T00:00:00Z"), mode: 16877 });

const A = "a".repeat(32), B = "b".repeat(32), T = "c".repeat(32);

test("md5 matches node crypto, including multi-block and non-ASCII text", () => {
  for (const text of ["", "abc", "x".repeat(55), "y".repeat(56), "z".repeat(200), "랭킹 Guest#12"]) {
    assert.equal(carry.md5Hex(text), md5(text), JSON.stringify(text.slice(0, 10)));
  }
});

test("the folder name matches what Unity used on itch.io and locally", () => {
  assert.equal(carry.folderForUrl("https://html-classic.itch.zone/html/20364020/index.html"),
    "34e9dec25f5f8c24e674fc27940a08b4");
  assert.equal(carry.folderForUrl("http://127.0.0.1:8765/html/111/index.html?a=/b/c"),
    "d5732eb8dc75828ec5eec54bed38510d");
  assert.equal(carry.folderForUrl("http://127.0.0.1:8765/html/111/index.html?x=1#h"),
    "d5732eb8dc75828ec5eec54bed38510d");
  assert.equal(carry.folderForUrl("http://127.0.0.1:8765/html/111/"),
    "d5732eb8dc75828ec5eec54bed38510d");
});

test("a folder that already has a save is kept", () => {
  const records = [
    { key: `/idbfs/${A}/product-save.json`, value: file(save("2026-11-09T00:00:00.0000000Z")) },
    { key: `/idbfs/${T}/product-save.json`, value: file(bytes("not json")) },
  ];
  assert.deepEqual(carry.chooseSource(records, T), { kept: true });
});

test("a folder with only a backup or temp file also counts as having a save", () => {
  for (const name of ["product-save.json.bak", "product-save.json.tmp"]) {
    const keys = [`/idbfs/${A}/product-save.json`, `/idbfs/${T}/${name}`];
    assert.equal(carry.hasSave(keys, T), true, name);
    const records = keys.map((key) => ({ key, value: file(save("2026-11-09T00:00:00.0000000Z")) }));
    assert.deepEqual(carry.chooseSource(records, T), { kept: true }, name);
  }
  assert.equal(carry.hasSave([`/idbfs/${T}/PlayerPrefs`, `/idbfs/${T}`], T), false);
});

test("only other folders' save files are read as candidates", () => {
  const keys = [
    `/idbfs/${A}/product-save.json`, `/idbfs/${A}/PlayerPrefs`, `/idbfs/${A}/product-save.json.bak`,
    `/idbfs/${B}/product-save.json`, `/idbfs/${T}/PlayerPrefs`, `/idbfs/${"f".repeat(32)}/data.bin`,
  ];
  assert.deepEqual(carry.candidateKeys(keys, T), [`/idbfs/${A}/product-save.json`, `/idbfs/${B}/product-save.json`]);
});

test("the newest valid save is chosen; other files and broken saves are skipped", () => {
  const records = [
    { key: `/idbfs/${A}/product-save.json`, value: file(save("2026-11-08T10:00:00.0000000Z")) },
    { key: `/idbfs/${B}/product-save.json`, value: file(save("2026-11-08T12:00:00.0000000Z")) },
    { key: `/idbfs/${"d".repeat(32)}/product-save.json`, value: file(bytes("{broken")) },
    { key: `/idbfs/${"e".repeat(32)}/product-save.json`, value: file(bytes('{"SchemaVersion":1}')) },
    { key: `/idbfs/${"f".repeat(32)}/save.json`, value: file(save("2027-01-01T00:00:00.0000000Z")) },
    { key: `/idbfs/${T}/PlayerPrefs`, value: file(bytes("fresh")) },
  ];
  assert.deepEqual(carry.chooseSource(records, T), { folder: B });
  assert.equal(carry.chooseSource(records.slice(2), T), null);
  assert.equal(carry.chooseSource([], T), null);
});

test("equal write times fall back to the newer file", () => {
  const same = "2026-11-08T10:00:00.0000000Z";
  const records = [
    { key: `/idbfs/${A}/product-save.json`, value: file(save(same), new Date("2026-11-08T10:00:00Z")) },
    { key: `/idbfs/${B}/product-save.json`, value: file(save(same), new Date("2026-11-08T11:00:00Z")) },
  ];
  assert.deepEqual(carry.chooseSource(records, T), { folder: B });
});

test("the copy writes the folder, save, backup and PlayerPrefs only", () => {
  const records = [
    { key: `/idbfs/${A}`, value: dir() },
    { key: `/idbfs/${A}/product-save.json`, value: file(save("x")) },
    { key: `/idbfs/${A}/product-save.json.bak`, value: file(save("w")) },
    { key: `/idbfs/${A}/PlayerPrefs`, value: file(bytes("prefs")) },
    { key: `/idbfs/${A}/product-save.json.tmp`, value: file(bytes("tmp")) },
    { key: `/idbfs/${A}/Other/file.bin`, value: file(bytes("other")) },
  ];
  const writes = carry.planCopy(records, A, T);
  assert.deepEqual(writes.map((w) => w.key), [
    `/idbfs/${T}`,
    `/idbfs/${T}/product-save.json`,
    `/idbfs/${T}/product-save.json.bak`,
    `/idbfs/${T}/PlayerPrefs`,
  ]);
  assert.equal(writes[0].value.mode, 16877);
  assert.equal(writes[1].value, records[1].value);
});

test("without IndexedDB the game still starts", async () => {
  assert.equal(await carry.run(null, "https://x/html/1/index.html", 50), "unavailable");
  const failing = { open() { throw new Error("denied"); } };
  const original = console.warn;
  console.warn = () => {};
  try {
    assert.equal(await carry.run(failing, "https://x/html/1/index.html", 50), "failed");
  } finally {
    console.warn = original;
  }
});
