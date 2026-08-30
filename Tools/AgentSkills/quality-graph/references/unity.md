# Unity Adapter

Use this reference only for Unity repositories.

## State contract

- `inspect`: an interactive Editor may remain open, but do not infer that
  in-memory Scenes are saved.
- `source-edit`: C# and external tooling may be edited while the Editor is open
  when project rules allow it. Scene, prefab, material, package, and
  ProjectSettings authority must be explicit.
- `batch-validate`: require the primary project Editor and its lock to be
  absent before launching batch mode against that project.
- `visual-qc`: require a graphics-capable Editor or built Player. Record the
  graphics backend and resolution. Headless results are not visual evidence.
- `finalize`: save all intended assets, exit Play Mode, close the Editor, then
  recheck the repository from disk.

An isolated project copy may be used while the primary Editor is open only
when the report labels it as isolated and confirms the revision, imported
assets, settings, and generated content it actually tested.

## Visual failure classes

Inspect rendered artifacts for magenta error materials, missing or default
materials, detached effects, absent runtime objects, UI occlusion, stale
generated Scenes, screen-space direction, timing sequences, and device-specific
layout. Test user-visible left/right rather than imported model names.

## Builder authority

When a Builder owns generated content, edit the Builder rather than its output.
Run the Builder according to the repository contract, validate idempotence,
then perform post-Builder checks. A passing code test does not prove that the
currently opened Scene contains the generated result.

## Protected local data

Treat Editor PlayerPrefs, local saves, identity, Package caches, Library,
Temp, Logs, and build outputs according to the repository policy. Never call a
validation copy equivalent to the primary project without labeling it.
