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

print("=== Added Objects / Components in Room Scene ===")
for fid in ["1901275664", "859651293", "1681052505", "432481279", "678803731"]:
    if fid in scene_objs:
        print(f"\n--- ID {fid} ---")
        print(scene_objs[fid])
    else:
        print(f"\nID {fid} NOT in scene_objs (might be in prefab or asset)")

# Let's inspect XR Origin prefab
prefab_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Tutorial\XR Origin (XR Rig).prefab")
with open(prefab_path, 'r', encoding='utf-8', errors='ignore') as f:
    prefab_text = f.read()

prefab_blocks = prefab_text.split("--- !u!")
prefab_objs = {}
for b in prefab_blocks[1:]:
    h = b.split("\n", 1)[0].split("&")
    if len(h) >= 2:
        prefab_objs[h[1].strip()] = b

print("\n=== Objects in XR Origin Prefab ===")
for fid in ["1717954561962503725", "1717954561962503726", "6232745470614056090", "1767192433"]:
    if fid in prefab_objs:
        print(f"\n--- Prefab ID {fid} ---")
        print(prefab_objs[fid][:800])
    else:
        print(f"\nPrefab ID {fid} not found")
