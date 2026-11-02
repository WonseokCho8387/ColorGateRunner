"""Package an already successful WebGL Player; never edits Unity inputs."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import zipfile


def package(build, destination):
    build = build.resolve(strict=True)
    destination = destination.resolve()
    if not (build / "index.html").is_file():
        raise ValueError("index.html must be at the build root")
    if destination.is_relative_to(build):
        raise ValueError("ZIP and manifest must be outside the build directory")
    if destination.exists() or destination.with_suffix(".manifest.json").exists():
        raise ValueError("Preserve the previous package before packaging again")
    files = []
    for path in sorted(build.rglob("*")):
        if path.is_symlink():
            raise ValueError(f"Symlink not allowed: {path}")
        if not path.is_file():
            continue
        relative = path.relative_to(build)
        if any(part.endswith("_DoNotShip") for part in relative.parts):
            continue
        name = relative.as_posix()
        name.encode("utf-8", errors="strict")
        files.append((name, path))
    names = {name for name, _ in files}
    if len({name.casefold() for name in names}) != len(names):
        raise ValueError("Case-colliding asset paths")
    sizes = [path.stat().st_size for _, path in files]
    if not files or len(files) > 1000 or sum(sizes) > 500_000_000 or max(sizes) > 200_000_000:
        raise ValueError("itch.io extracted file count or size limit exceeded")
    html = (build / "index.html").read_text(encoding="utf-8")
    urls = re.findall(r'(?:src\s*=\s*|(?:dataUrl|frameworkUrl|codeUrl|symbolsUrl)\s*:\s*)[\"\']([^\"\']+)', html)
    for url in urls:
        if url.startswith(("/", "http:", "https:")) or ".." in Path(url).parts:
            raise ValueError(f"Non-relative release asset: {url}")
        if url not in names:
            raise ValueError(f"Missing or case-mismatched release asset: {url}")
    destination.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(destination, "x", compression=zipfile.ZIP_DEFLATED) as archive:
        for name, path in files:
            archive.write(path, name)
    with zipfile.ZipFile(destination) as archive:
        if archive.testzip() is not None or set(archive.namelist()) != names:
            raise ValueError("ZIP verification failed")
    report = {
        "build_directory": str(build), "zip": str(destination),
        "file_count": len(files), "extracted_bytes": sum(sizes),
        "largest_file_bytes": max(sizes), "zip_bytes": destination.stat().st_size,
        "zip_sha256": hashlib.sha256(destination.read_bytes()).hexdigest(),
        "files": [{"path": name, "bytes": path.stat().st_size,
                   "sha256": hashlib.sha256(path.read_bytes()).hexdigest()} for name, path in files],
        "hosting_verified": False,
        "note": "Packaging checks do not certify itch.io iframe, browser/device behavior or privacy readiness."
    }
    destination.with_suffix(".manifest.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    return report


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("build", type=Path)
    parser.add_argument("destination", type=Path)
    args = parser.parse_args()
    report = package(args.build, args.destination)
    print(json.dumps({key: value for key, value in report.items() if key != "files"}, indent=2))
