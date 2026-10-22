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

def inspect_prefab_instance(pfid, label):
    print(f"\n==========================================")
    print(f"PrefabInstance: {label} [{pfid}]")
    print(f"==========================================")
    b = scene_objs.get(pfid)
    if not b:
        print(f"PrefabInstance {pfid} not found!")
        return
    print(b)

inspect_prefab_instance("2122980739", "tool-shovel")
inspect_prefab_instance("2018871572", "Gold_Pan")
