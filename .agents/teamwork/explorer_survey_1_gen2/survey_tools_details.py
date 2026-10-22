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

print("=== TOOL-SHOVEL ADDED OBJECTS & COMPONENTS ===")
shovel_ids = ["391156553", "375342303", "2122980745", "2122980744", "2122980743", "2122980742", "2122980748"]
# Also get gameobjects corresponding to transforms if any
for fid in shovel_ids:
    if fid in scene_objs:
        print(f"\n--- ID {fid} ---")
        print(scene_objs[fid])
        # If transform, find its gameobject
        m_go = re.search(r"m_GameObject:\s*\{fileID:\s*([^\}]+)\}", scene_objs[fid])
        if m_go and m_go.group(1) in scene_objs:
            gofid = m_go.group(1)
            print(f"--- Associated GameObject [{gofid}] ---")
            print(scene_objs[gofid])

print("\n=== GOLD_PAN ADDED OBJECTS & COMPONENTS ===")
pan_ids = ["497355702", "1763807607", "2018871579", "2018871578", "2018871577", "2018871583", "2018871576"]
for fid in pan_ids:
    if fid in scene_objs:
        print(f"\n--- ID {fid} ---")
        print(scene_objs[fid])
        m_go = re.search(r"m_GameObject:\s*\{fileID:\s*([^\}]+)\}", scene_objs[fid])
        if m_go and m_go.group(1) in scene_objs:
            gofid = m_go.group(1)
            print(f"--- Associated GameObject [{gofid}] ---")
            print(scene_objs[gofid])
