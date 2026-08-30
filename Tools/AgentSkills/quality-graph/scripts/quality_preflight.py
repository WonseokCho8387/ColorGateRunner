#!/usr/bin/env python3
"""Emit a compact environment decision for a configured quality graph."""

from __future__ import annotations

import argparse
import datetime as dt
import json
import pathlib
import subprocess
import sys
import tempfile
from typing import Any


MODES = ("inspect", "source-edit", "batch-validate", "visual-qc", "finalize")


def run_git(workspace: pathlib.Path, *args: str) -> str:
    completed = subprocess.run(
        ["git", *args],
        cwd=workspace,
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    return completed.stdout.strip() if completed.returncode == 0 else ""


def normalize_requested_paths(
    workspace: pathlib.Path, raw_paths: list[str]
) -> tuple[list[str], list[str]]:
    normalized: list[str] = []
    errors: list[str] = []
    workspace = workspace.resolve()
    for raw_path in raw_paths:
        candidate = pathlib.Path(raw_path)
        absolute = (candidate if candidate.is_absolute() else workspace / candidate).resolve()
        try:
            relative = absolute.relative_to(workspace)
        except ValueError:
            errors.append(f"Requested path is outside the workspace: {raw_path}")
            continue
        normalized.append(relative.as_posix())
    return normalized, errors


def find_closed_required_paths(
    requested_paths: list[str], protected_prefixes: list[str]
) -> list[str]:
    folded_prefixes = [item.replace("\\", "/").rstrip("/").casefold() for item in protected_prefixes]
    matches: list[str] = []
    for requested_path in requested_paths:
        folded_path = requested_path.casefold()
        if any(
            folded_path == prefix or folded_path.startswith(prefix + "/")
            for prefix in folded_prefixes
        ):
            matches.append(requested_path)
    return matches


def validate_player_artifact(
    workspace: pathlib.Path,
    evidence_path: pathlib.Path | None,
    profiles: list[dict[str, Any]],
) -> bool:
    if evidence_path is None or not evidence_path.exists():
        return False
    evidence_path = evidence_path.resolve()
    for profile in profiles:
        root = (workspace / profile["root"]).resolve()
        kind = profile.get("kind")
        if kind == "directory":
            if evidence_path != root or not evidence_path.is_dir():
                continue
            required_files = profile.get("required_files", [])
            if all((evidence_path / item).is_file() for item in required_files):
                return True
        elif kind == "file":
            try:
                evidence_path.relative_to(root)
            except ValueError:
                continue
            extensions = {item.casefold() for item in profile.get("extensions", [])}
            if evidence_path.is_file() and evidence_path.suffix.casefold() in extensions:
                return True
    return False


def evaluate(
    mode: str,
    application_open: bool,
    policy: dict[str, Any],
    closed_required_paths: list[str] | None = None,
    backend: str = "not-applicable",
    evidence_exists: bool = False,
    application_name: str = "stateful application",
    graphics_api: str | None = None,
    resolution: str | None = None,
) -> tuple[bool, list[str]]:
    reasons: list[str] = []
    closed_modes = set(policy.get("require_application_closed", []))
    open_modes = set(policy.get("require_application_open", []))

    if mode in closed_modes and application_open:
        reasons.append(f"{application_name} must be closed for this mode.")
    if mode in open_modes and not application_open:
        reasons.append(f"{application_name} must be open for this mode.")
    if mode == "visual-qc" and backend == "graphics-editor" and not application_open:
        reasons.append(f"A graphics-capable {application_name} must be open for this mode.")
    if mode == "visual-qc" and backend == "player-artifact" and not evidence_exists:
        reasons.append("The requested evidence does not match a configured Player artifact.")
    if mode == "visual-qc" and not graphics_api:
        reasons.append("Visual QC requires the observed graphics API.")
    if mode == "visual-qc" and not resolution:
        reasons.append("Visual QC requires the observed resolution.")
    if mode == "source-edit" and application_open and closed_required_paths:
        joined = ", ".join(closed_required_paths)
        reasons.append(f"{application_name} must be closed before editing: {joined}")
    return not reasons, reasons


def self_test() -> int:
    policy = {
        "require_application_closed": ["batch-validate", "finalize"],
    }
    cases = [
        ("inspect", True, "not-applicable", False, None, None, True),
        ("inspect", False, "not-applicable", False, None, None, True),
        ("source-edit", True, "not-applicable", False, None, None, True),
        ("batch-validate", True, "headless", False, None, None, False),
        ("batch-validate", False, "headless", False, None, None, True),
        ("visual-qc", True, "graphics-editor", False, "DX11", "540x960", True),
        ("visual-qc", False, "graphics-editor", False, "DX11", "540x960", False),
        ("visual-qc", False, "player-artifact", True, "WebGL", "540x960", True),
        ("visual-qc", False, "player-artifact", False, "WebGL", "540x960", False),
        ("visual-qc", True, "graphics-editor", False, None, "540x960", False),
        ("visual-qc", True, "graphics-editor", False, "DX11", None, False),
        ("finalize", True, "not-applicable", False, None, None, False),
        ("finalize", False, "not-applicable", False, None, None, True),
    ]
    failures = []
    for mode, application_open, backend, evidence_exists, graphics_api, resolution, expected in cases:
        actual, _ = evaluate(
            mode,
            application_open,
            policy,
            backend=backend,
            evidence_exists=evidence_exists,
            application_name="Test App",
            graphics_api=graphics_api,
            resolution=resolution,
        )
        if actual != expected:
            failures.append(
                f"{mode}/{application_open}/{backend}: expected {expected}, got {actual}"
            )
    with tempfile.TemporaryDirectory() as temp_directory:
        workspace = pathlib.Path(temp_directory).resolve()
        relative, relative_errors = normalize_requested_paths(
            workspace, ["Project/Settings/config.json"]
        )
        case_variant, case_errors = normalize_requested_paths(
            workspace, ["project/settings/config.json"]
        )
        absolute, absolute_errors = normalize_requested_paths(
            workspace, [str(workspace / "Project" / "Settings" / "config.json")]
        )
        for label, paths, errors in (
            ("relative", relative, relative_errors),
            ("case", case_variant, case_errors),
            ("absolute", absolute, absolute_errors),
        ):
            if errors or not find_closed_required_paths(paths, ["Project/Settings"]):
                failures.append(f"protected path {label} was not detected")

        webgl = workspace / "Builds" / "WebGL"
        webgl.mkdir(parents=True)
        (webgl / "index.html").write_text("test", encoding="utf-8")
        arbitrary = workspace / "notes.md"
        arbitrary.write_text("test", encoding="utf-8")
        profiles = [
            {
                "kind": "directory",
                "root": "Builds/WebGL",
                "required_files": ["index.html"],
            }
        ]
        if not validate_player_artifact(workspace, webgl, profiles):
            failures.append("configured Player directory was rejected")
        if validate_player_artifact(workspace, arbitrary, profiles):
            failures.append("arbitrary evidence file was accepted")

    if failures:
        print("\n".join(failures), file=sys.stderr)
        return 1
    print(f"Quality preflight self-test passed: {len(cases)}/{len(cases)}")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--config", type=pathlib.Path)
    parser.add_argument("--mode", choices=MODES)
    parser.add_argument("--format", choices=("text", "json"), default="text")
    parser.add_argument("--path", action="append", default=[])
    parser.add_argument(
        "--backend",
        choices=("not-applicable", "headless", "graphics-editor", "player-artifact"),
    )
    parser.add_argument("--evidence-path", type=pathlib.Path)
    parser.add_argument("--graphics-api")
    parser.add_argument("--resolution")
    parser.add_argument("--write-manifest", action="store_true")
    parser.add_argument("--self-test", action="store_true")
    args = parser.parse_args()

    if args.self_test:
        return self_test()
    if args.config is None or args.mode is None:
        parser.error("--config and --mode are required unless --self-test is used")

    config_path = args.config.resolve()
    config = json.loads(config_path.read_text(encoding="utf-8"))
    if config.get("schema_version") != 1:
        raise ValueError("Unsupported quality-graph schema_version")

    workspace = (config_path.parent / config["workspace_root"]).resolve()
    project = (workspace / config["project_root"]).resolve()
    application = config.get("stateful_application", {})
    application_name = application.get("name", "stateful application")
    lock_paths = [(project / item).resolve() for item in application.get("lock_paths", [])]
    existing_locks = [str(path) for path in lock_paths if path.exists()]
    application_open = bool(existing_locks)

    default_backends = config.get("default_backends", {})
    backend = args.backend or default_backends.get(args.mode, "not-applicable")
    evidence_path = args.evidence_path.resolve() if args.evidence_path else None
    evidence_exists = validate_player_artifact(
        workspace,
        evidence_path,
        config.get("player_artifacts", []),
    )

    requested_paths, path_errors = normalize_requested_paths(workspace, args.path)
    closed_required_paths = find_closed_required_paths(
        requested_paths,
        config.get("closed_required_paths", []),
    )
    allowed, reasons = evaluate(
        args.mode,
        application_open,
        config.get("state_policy", {}),
        closed_required_paths,
        backend,
        evidence_exists,
        application_name,
        args.graphics_api,
        args.resolution,
    )
    reasons.extend(path_errors)
    allowed = allowed and not path_errors

    status_lines = run_git(workspace, "status", "--porcelain").splitlines()
    revision = run_git(workspace, "rev-parse", "HEAD")
    branch = run_git(workspace, "branch", "--show-current")
    manifest = {
        "schema_version": 1,
        "observed_utc": dt.datetime.now(dt.timezone.utc).isoformat(),
        "project": config["project_name"],
        "project_kind": config["project_kind"],
        "revision": revision,
        "branch": branch,
        "dirty_file_count": len(status_lines),
        "mode": args.mode,
        "project_copy": config.get("project_copy", "primary"),
        "backend": backend,
        "graphics_api": args.graphics_api,
        "resolution": args.resolution,
        "stateful_application": application_name,
        "application_open": application_open,
        "application_lock_count": len(existing_locks),
        "requested_paths": requested_paths,
        "player_artifact_valid": evidence_exists,
        "allowed": allowed,
        "reasons": reasons,
    }

    if args.write_manifest:
        artifact_root = (workspace / config["artifact_root"]).resolve()
        artifact_root.mkdir(parents=True, exist_ok=True)
        output = artifact_root / "latest-preflight.json"
        output.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
        manifest["manifest_path"] = str(output)

    if args.format == "json":
        print(json.dumps(manifest, indent=2))
    else:
        decision = "ALLOW" if allowed else "BLOCK"
        print(f"Quality Graph preflight: {decision}")
        print(f"Project: {manifest['project']} ({manifest['project_copy']})")
        print(f"Mode: {args.mode}; backend: {backend}")
        print(f"Revision: {revision or 'unavailable'}; dirty files: {len(status_lines)}")
        print(
            f"{application_name} open: {application_open}; "
            f"locks: {len(existing_locks)}"
        )
        for reason in reasons:
            print(f"Reason: {reason}")
    return 0 if allowed else 2


if __name__ == "__main__":
    raise SystemExit(main())
