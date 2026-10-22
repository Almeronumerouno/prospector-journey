import sys
import re
from pathlib import Path

scene_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Room\Room scene.unity")

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Split into documents
docs = content.split("--- !u!")

print(f"Total YAML blocks: {len(docs)}")

# Let's map fileID -> {class_id, type_name, lines, dict_data}
objects = {}

for doc in docs[1:]:
    header_line = doc.split("\n", 1)[0]
    parts = header_line.split("&")
    if len(parts) < 2:
        continue
    class_id = parts[0].strip()
    file_id = parts[1].strip()
    
    rest = doc.split("\n", 1)[1] if "\n" in doc else ""
    type_name = rest.split(":", 1)[0].strip() if ":" in rest else "Unknown"
    
    objects[file_id] = {
        "class_id": class_id,
        "type_name": type_name,
        "raw": doc
    }

# Find all GameObjects
game_objects = {}
for fid, obj in objects.items():
    if obj["type_name"] == "GameObject":
        name_match = re.search(r"m_Name:\s*(.*)", obj["raw"])
        name = name_match.group(1).strip() if name_match else "Unknown"
        
        # find components
        comp_fids = re.findall(r"component:\s*\{fileID:\s*([^\}]+)\}", obj["raw"])
        game_objects[fid] = {
            "name": name,
            "components": comp_fids,
            "raw": obj["raw"]
        }

print(f"Found {len(game_objects)} GameObjects.")

# Let's find specific gameObjects of interest
keywords = ["XR Origin", "Prospector", "Inventory", "shovel", "Pan", "Door", "RoomInteractionTestHelper"]
for fid, go in game_objects.items():
    for kw in keywords:
        if kw.lower() in go["name"].lower():
            print(f"GameObject [{fid}]: {go['name']} with {len(go['components'])} components")
            for cfid in go["components"]:
                cobj = objects.get(cfid, {})
                print(f"   -> Component [{cfid}]: {cobj.get('type_name')} (class {cobj.get('class_id')})")
