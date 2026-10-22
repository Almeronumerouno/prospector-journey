import re
from pathlib import Path

scene_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Room\Room scene.unity")

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

blocks = content.split("--- !u!")

objects = {}
for b in blocks[1:]:
    header_line = b.split("\n", 1)[0]
    parts = header_line.split("&")
    if len(parts) >= 2:
        class_id = parts[0].strip()
        fid = parts[1].strip()
        objects[fid] = (class_id, b)

ids_to_check = [
    "1901275658", "1901275664", "859651293", "1681052505", "432481279"
]

for fid in ids_to_check:
    if fid in objects:
        cid, text = objects[fid]
        print(f"=== Object [{fid}] Class [{cid}] ===")
        print(text[:1200])
        print("------------------------------------------")
    else:
        print(f"Object [{fid}] not found directly in scene objects")
