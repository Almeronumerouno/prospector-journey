import re
from pathlib import Path

scene_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Room\Room scene.unity")
with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    scene_text = f.read()

blocks = scene_text.split("--- !u!")
scene_objs = {}
for b in blocks[1:]:
    h = b.split("\n", 1)[0].split("&")
    if len(h) >= 2:
        scene_objs[h[1].strip()] = b

for fid in ["1681052504", "1681052505", "827924896", "827924897", "432481278", "432481279", "1424857011", "1424857012"]:
    if fid in scene_objs:
        print(f"=== Object [{fid}] ===")
        print(scene_objs[fid])
