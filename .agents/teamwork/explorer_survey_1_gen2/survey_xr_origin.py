import re
from pathlib import Path

scene_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Room\Room scene.unity")
with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    scene_text = f.read()

# Also let's inspect the source prefab of XR Origin
xr_prefab_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Tutorial\XR Origin (XR Rig).prefab")
with open(xr_prefab_path, 'r', encoding='utf-8', errors='ignore') as f:
    prefab_text = f.read()

print(f"XR Origin prefab size: {len(prefab_text)} chars")

# Let's extract PrefabInstance 859651291 from Room scene
blocks = scene_text.split("--- !u!")
for b in blocks:
    if "&859651291" in b.split("\n", 1)[0]:
        print("=== PrefabInstance 859651291 (Room scene) ===")
        print(b)
        break
