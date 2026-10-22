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

# Let's search for Cabin_Door_Slab (1042778239) and Door_Interaction_Prompt (1797127868)
door_ids = ["1042778239", "1042778240", "1042778243", "1042778242", "1042778241", "1042778245", "1042778244",
            "1797127868", "1797127871", "1797127870", "1797127869"]

print("=== CABIN DOOR & PROMPT OBJECTS ===")
for fid in door_ids:
    if fid in scene_objs:
        print(f"\n--- ID {fid} ---")
        print(scene_objs[fid])

# Let's find children of Door_Interaction_Prompt
p_tr_id = "1797127871"
ptr_block = scene_objs.get(p_tr_id, "")
children = re.findall(r"-\s*\{fileID:\s*([^\}]+)\}", ptr_block)
print(f"\nChildren of Door_Interaction_Prompt ({len(children)}): {children}")
for cfid in children:
    if cfid in scene_objs:
        print(f"\n--- Child Transform [{cfid}] ---")
        print(scene_objs[cfid])
        m_go = re.search(r"m_GameObject:\s*\{fileID:\s*([^\}]+)\}", scene_objs[cfid])
        if m_go and m_go.group(1) in scene_objs:
            print(f"--- Child GO [{m_go.group(1)}] ---")
            print(scene_objs[m_go.group(1)])
