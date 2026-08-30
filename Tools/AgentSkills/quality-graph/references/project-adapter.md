# Project Adapter

An adapter connects the generic graph to one repository. Prefer a small,
versioned JSON configuration and a thin launcher over copying the whole skill.

## Required configuration

- schema version and project name;
- project kind and project root relative to the workspace;
- ignored artifact location;
- supported operating modes;
- stateful-application lock paths or probes;
- paths that require the application to be closed before external edits;
- deterministic validation commands;
- visual or black-box evidence commands when available;
- protected state and forbidden mutations.

Avoid machine-specific absolute paths. Resolve tools through environment
variables, project version files, or documented runtime discovery.

## Recommended modes

- `inspect`: read-only discovery; stateful applications may be open.
- `source-edit`: ordinary source changes; adapter decides which paths require
  closure.
- `batch-validate`: deterministic non-interactive validation; conflicting
  applications must be closed.
- `visual-qc`: a graphics-capable application or player must be available.
- `finalize`: persisted files must be flushed and the working state frozen.

## Evidence manifest

Emit a compact JSON record under an ignored artifact directory. Include UTC
time, revision, dirty-file count, requested mode, observed application state,
backend, project copy type, decision, and reasons. Do not include credentials,
full process command lines, personal identifiers, or unrelated environment
variables.

The generic preflight uses a configured stateful-application name and lock
paths; it must not hardcode Unity fields. Visual QC may use a live graphics
application or an explicitly supplied existing Player artifact.

## Portability

Keep engine-specific behavior in a dedicated reference or adapter. A web
project may replace Unity locks with development-server and browser state; a
mobile project may add emulator and signing-state checks. The maker/checker,
evidence, and stopping contracts remain unchanged.
