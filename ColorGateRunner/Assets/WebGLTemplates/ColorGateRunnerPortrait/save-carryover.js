// Carries the save over to a new upload of the web build.
//
// Unity keeps Application.persistentDataPath at /idbfs/<md5 of the page URL
// up to its last "/", without query or hash>. A host such as itch.io gives
// every upload a new URL, so a new upload starts with an empty folder. All
// folders on one site share the "/idbfs" IndexedDB database, so before Unity
// starts this copies the newest Color Gate Runner save, its backup and
// PlayerPrefs (which hold the anonymous ranking sign-in) from another folder
// into the empty one. It never overwrites a save, never deletes anything and
// only reads files named like ours. Any failure starts the game as before.
(function (root) {
  "use strict";

  var DATABASE = "/idbfs";
  var STORE = "FILE_DATA";
  var SAVE = "product-save.json";
  var COPIED = [SAVE, SAVE + ".bak", "PlayerPrefs"];
  var SAVE_KEY = /^\/idbfs\/([0-9a-f]{32})\/product-save\.json$/;
  var SAVE_FILE_KEY = /^\/idbfs\/([0-9a-f]{32})\/product-save\.json(\.bak|\.tmp)?$/;
  var DIRECTORY_MODE = 16877;

  function md5Hex(text) {
    var bytes = new TextEncoder().encode(text);
    var length = bytes.length;
    var words = new Array(((length + 8) >>> 6) * 16 + 16).fill(0);
    for (var i = 0; i < length; i++) {
      words[i >> 2] |= bytes[i] << ((i % 4) * 8);
    }
    words[length >> 2] |= 0x80 << ((length % 4) * 8);
    words[words.length - 2] = (length * 8) >>> 0;
    words[words.length - 1] = Math.floor(length / 0x20000000);

    var shifts = [7, 12, 17, 22, 5, 9, 14, 20, 4, 11, 16, 23, 6, 10, 15, 21];
    var constants = [];
    for (var k = 0; k < 64; k++) {
      constants[k] = Math.floor(Math.abs(Math.sin(k + 1)) * 0x100000000) | 0;
    }

    var a0 = 0x67452301, b0 = 0xefcdab89 | 0, c0 = 0x98badcfe | 0, d0 = 0x10325476;
    for (var block = 0; block < words.length; block += 16) {
      var a = a0, b = b0, c = c0, d = d0;
      for (var step = 0; step < 64; step++) {
        var f, g, round = step >> 4;
        if (round === 0) { f = (b & c) | (~b & d); g = step; }
        else if (round === 1) { f = (d & b) | (~d & c); g = (5 * step + 1) % 16; }
        else if (round === 2) { f = b ^ c ^ d; g = (3 * step + 5) % 16; }
        else { f = c ^ (b | ~d); g = (7 * step) % 16; }
        var sum = (a + f + constants[step] + words[block + g]) | 0;
        var shift = shifts[round * 4 + (step % 4)];
        a = d; d = c; c = b;
        b = (b + ((sum << shift) | (sum >>> (32 - shift)))) | 0;
      }
      a0 = (a0 + a) | 0; b0 = (b0 + b) | 0; c0 = (c0 + c) | 0; d0 = (d0 + d) | 0;
    }

    var hex = "";
    [a0, b0, c0, d0].forEach(function (word) {
      for (var byteIndex = 0; byteIndex < 4; byteIndex++) {
        hex += ((word >>> (byteIndex * 8)) & 0xff).toString(16).padStart(2, "0");
      }
    });
    return hex;
  }

  // The folder name Unity uses for a page URL.
  function folderForUrl(url) {
    var path = String(url).split("#")[0].split("?")[0];
    return md5Hex(path.substring(0, path.lastIndexOf("/")));
  }

  function decode(contents) {
    if (!contents || typeof contents.length !== "number") return null;
    var view = contents instanceof Uint8Array
      ? contents
      : new Uint8Array(contents.buffer || contents, contents.byteOffset || 0, contents.length);
    return new TextDecoder().decode(view);
  }

  // Our save: the Product save document, not another game's file.
  function readSave(contents) {
    try {
      var data = JSON.parse(decode(contents));
      if (!data || typeof data.SchemaVersion !== "number" ||
          typeof data.SaveRevision !== "number" ||
          !data.Profile || !data.CampaignProgress) {
        return null;
      }
      return data;
    } catch (error) {
      return null;
    }
  }

  // True when the target folder already holds a save, backup or temp file.
  // The game recovers the backup when an interrupted write left no primary.
  function hasSave(keys, targetFolder) {
    for (var i = 0; i < keys.length; i++) {
      var match = SAVE_FILE_KEY.exec(String(keys[i]));
      if (match && match[1] === targetFolder) return true;
    }
    return false;
  }

  // The keys of other folders' saves: the only values read before choosing.
  function candidateKeys(keys, targetFolder) {
    var result = [];
    for (var i = 0; i < keys.length; i++) {
      var match = SAVE_KEY.exec(String(keys[i]));
      if (match && match[1] !== targetFolder) result.push(keys[i]);
    }
    return result;
  }

  // Picks the folder to copy from, or null. records: [{ key, value }].
  function chooseSource(records, targetFolder) {
    var keys = records.map(function (record) { return record.key; });
    if (hasSave(keys, targetFolder)) return { kept: true };
    var best = null;
    for (var i = 0; i < records.length; i++) {
      var match = SAVE_KEY.exec(String(records[i].key));
      if (!match) continue;
      var data = readSave(records[i].value && records[i].value.contents);
      if (!data) continue;
      var written = typeof data.LastWriteUtc === "string" ? data.LastWriteUtc : "";
      var stamp = new Date(records[i].value.timestamp || 0).getTime() || 0;
      if (!best || written > best.written ||
          (written === best.written && stamp > best.stamp)) {
        best = { folder: match[1], written: written, stamp: stamp };
      }
    }
    return best ? { folder: best.folder } : null;
  }

  // The records to write into the target folder.
  function planCopy(records, sourceFolder, targetFolder) {
    var byKey = {};
    for (var i = 0; i < records.length; i++) byKey[String(records[i].key)] = records[i].value;
    var source = "/idbfs/" + sourceFolder;
    var target = "/idbfs/" + targetFolder;
    var directory = byKey[source] || { timestamp: new Date(), mode: DIRECTORY_MODE };
    var writes = [{ key: target, value: { timestamp: directory.timestamp, mode: directory.mode } }];
    for (var j = 0; j < COPIED.length; j++) {
      var value = byKey[source + "/" + COPIED[j]];
      if (value) writes.push({ key: target + "/" + COPIED[j], value: value });
    }
    return writes;
  }

  function request(req) {
    return new Promise(function (resolve, reject) {
      req.onsuccess = function () { resolve(req.result); };
      req.onerror = function () { reject(req.error); };
    });
  }

  function openExisting(factory) {
    return new Promise(function (resolve, reject) {
      var req = factory.open(DATABASE);
      // A brand-new database means no save anywhere; let Unity create it.
      req.onupgradeneeded = function () { req.transaction.abort(); };
      req.onsuccess = function () { resolve(req.result); };
      req.onerror = function () { resolve(null); };
      req.onblocked = function () { reject(new Error("blocked")); };
    });
  }

  async function carryOver(factory, url, state) {
    var db = await openExisting(factory);
    if (!db) return "empty";
    try {
      if (!db.objectStoreNames.contains(STORE) || state.timedOut) return "empty";
      var tx = db.transaction(STORE, "readwrite");
      state.tx = tx;
      var done = new Promise(function (resolve, reject) {
        tx.oncomplete = resolve;
        tx.onerror = function () { reject(tx.error); };
        tx.onabort = function () { reject(tx.error || new Error("aborted")); };
      });
      done.catch(function () { /* the failing request reports it */ });
      var store = tx.objectStore(STORE);
      var target = folderForUrl(url);
      var keys = await request(store.getAllKeys());
      var result = "none";
      if (hasSave(keys, target)) {
        result = "kept";
      } else {
        // Read only our save files, not everything stored on this site.
        var records = [];
        var candidates = candidateKeys(keys, target);
        for (var i = 0; i < candidates.length; i++) {
          records.push({ key: candidates[i], value: await request(store.get(candidates[i])) });
        }
        var choice = chooseSource(records, target);
        if (choice) {
          var source = "/idbfs/" + choice.folder;
          var wanted = [source].concat(COPIED.map(function (name) { return source + "/" + name; }));
          var sourceRecords = [];
          for (var j = 0; j < wanted.length; j++) {
            var value = await request(store.get(wanted[j]));
            if (value) sourceRecords.push({ key: wanted[j], value: value });
          }
          planCopy(sourceRecords, choice.folder, target).forEach(function (write) {
            store.put(write.value, write.key);
          });
          result = "copied";
        }
      }
      await done;
      return result;
    } finally {
      db.close();
    }
  }

  // Runs the carry-over, giving up after timeoutMs so the game always starts.
  // A late copy is aborted so it cannot land after Unity has read the folder.
  function run(factory, url, timeoutMs) {
    var state = { timedOut: false, tx: null };
    var work = Promise.resolve().then(function () {
      return factory ? carryOver(factory, url, state) : "unavailable";
    });
    var timeout = null;
    var timer = new Promise(function (resolve) {
      timeout = setTimeout(function () {
        state.timedOut = true;
        if (state.tx) {
          try { state.tx.abort(); } catch (error) { /* already finished */ }
        }
        resolve("timeout");
      }, timeoutMs || 3000);
    });
    return Promise.race([work, timer]).then(function (result) {
      clearTimeout(timeout);
      if (result === "copied") console.log("Save carried over from an earlier upload.");
      return result;
    }, function (error) {
      clearTimeout(timeout);
      console.warn("Save carry-over skipped: " + (error && error.message || error));
      return "failed";
    });
  }

  var api = {
    md5Hex: md5Hex,
    folderForUrl: folderForUrl,
    hasSave: hasSave,
    candidateKeys: candidateKeys,
    chooseSource: chooseSource,
    planCopy: planCopy,
    run: run
  };
  if (typeof module === "object" && module.exports) {
    module.exports = api;
  } else {
    root.ColorGateSaveCarryover = api;
  }
})(this);
