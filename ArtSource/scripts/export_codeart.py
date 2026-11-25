"""Render semua PixelSpec di ArtSource/codeart/ lewat skill codeart2d lalu salin frame ke Assets.

Gratis dan lokal: hanya render_pixelspec.py (tanpa model gambar). Gagal (exit 1) kalau ada QA yang gagal
atau spec tanpa tujuan di DEST. Frame disalin sesuai urutan "frames" di spec menjadi <nama>_<i>.png.

    python ArtSource/scripts/export_codeart.py
"""
import json
import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SPECS = ROOT / "ArtSource" / "codeart"
OUT = SPECS / "out"
RENDER = Path.home() / ".claude" / "skills" / "codeart2d" / "scripts" / "render_pixelspec.py"
ART = ROOT / "Assets" / "Runecrypt" / "Art"

DEST = {
    "brazier_hint": ART / "Crypt",
    "pelita_sesat": ART / "Enemies",
    "pelita_eyes": ART / "Enemies",
    "biarawan_beku": ART / "Enemies",
    "biarawan_eyes": ART / "Enemies",
    "ice_shard": ART / "Enemies",
    "ice_crystal": ART / "Enemies",
    "pelita_death": ART / "Enemies",
    "biarawan_death": ART / "Enemies",
}


def export(spec_path: Path) -> bool:
    name = spec_path.name.removesuffix(".pixelspec.json")
    if name not in DEST:
        print(f"[gagal] {name}: tidak ada tujuan di DEST")
        return False
    out = OUT / name
    shutil.rmtree(out, ignore_errors=True)
    r = subprocess.run([sys.executable, str(RENDER), "--spec", str(spec_path), "--output-dir", str(out),
                        "--preview-scale", "8", "--strict-qc"], capture_output=True, text=True)
    meta = out / "codeart-meta.json"
    if r.returncode != 0 or not meta.exists():
        print(f"[gagal] {name}: render exit {r.returncode}\n{r.stdout[-800:]}{r.stderr[-800:]}")
        return False
    # Renderer hanya menulis frame pertama dari frame yang identik; duplikat disalin dari frame itu.
    first_with = {}
    sources = []
    for f in json.loads(spec_path.read_text(encoding="utf-8"))["frames"]:
        key = json.dumps(f["layers"], sort_keys=True)
        sources.append(first_with.setdefault(key, f["name"]))
    DEST[name].mkdir(parents=True, exist_ok=True)
    for i, frame in enumerate(sources):
        shutil.copyfile(out / "base" / "frames" / f"{frame}.png", DEST[name] / f"{name}_{i}.png")
    frames = sources
    print(f"[ok] {name}: {len(frames)} frame -> {DEST[name].relative_to(ROOT)}")
    return True


def main() -> int:
    results = [export(p) for p in sorted(SPECS.glob("*.pixelspec.json"))]
    return 0 if results and all(results) else 1


if __name__ == "__main__":
    sys.exit(main())
