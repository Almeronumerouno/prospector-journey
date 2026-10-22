import re
from pathlib import Path

scene_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Room\Room scene.unity")

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

docs = content.split("--- !u!")

print(f"Total YAML documents in Room scene: {len(docs)}")

# Parse documents into map: file_id -> {class_id, doc}
scene_objects = {}
for d in docs[1:]:
    header = d.split("\n", 1)[0]
    parts = header.split("&")
    if len(parts) >= 2:
        class_id = parts[0].strip()
        fid = parts[1].strip()
        scene_objects[fid] = {
            "class_id": class_id,
            "raw": d
        }

# Find all GameObjects
gos = {}
transforms = {}
prefabs = {}
monos = {}

for fid, obj in scene_objects.items():
    cid = obj["class_id"]
    raw = obj["raw"]
    if cid == "1": # GameObject
        name_m = re.search(r"m_Name:\s*(.*)", raw)
        name = name_m.group(1).strip() if name_m else "Unknown"
        comp_fids = re.findall(r"component:\s*\{fileID:\s*([^\}]+)\}", raw)
        gos[fid] = {
            "name": name,
            "components": comp_fids,
            "raw": raw
        }
    elif cid in ["4", "224"]: # Transform or RectTransform
        go_m = re.search(r"m_GameObject:\s*\{fileID:\s*([^\}]+)\}", raw)
        father_m = re.search(r"m_Father:\s*\{fileID:\s*([^\}]+)\}", raw)
        children = re.findall(r"-\s*\{fileID:\s*([^\}]+)\}", raw)
        transforms[fid] = {
            "go_fid": go_m.group(1).strip() if go_m else None,
            "father_fid": father_m.group(1).strip() if father_m else None,
            "raw": raw
        }
    elif cid == "1001": # PrefabInstance
        src_m = re.search(r"m_SourcePrefab:\s*\{fileID:\s*([^\}]+),\s*guid:\s*([^\}]+)", raw)
        prefabs[fid] = {
            "source": src_m.groups() if src_m else None,
            "raw": raw
        }
    elif cid == "114": # MonoBehaviour
        name_m = re.search(r"m_Name:\s*(.*)", raw)
        script_m = re.search(r"m_Script:\s*\{fileID:\s*([^\}]+),\s*guid:\s*([^\}]+)", raw)
        go_m = re.search(r"m_GameObject:\s*\{fileID:\s*([^\}]+)\}", raw)
        monos[fid] = {
            "name": name_m.group(1).strip() if name_m else "",
            "script": script_m.groups() if script_m else None,
            "go_fid": go_m.group(1).strip() if go_m else None,
            "raw": raw
        }

print(f"Parsed: {len(gos)} GameObjects, {len(transforms)} Transforms, {len(prefabs)} PrefabInstances, {len(monos)} MonoBehaviours")

# Find GUIDs of scripts
guid_to_name = {}
for meta in Path(r"c:\CodeKuliah\prospector-journey\Assets").rglob("*.cs.meta"):
    try:
        txt = meta.read_text(encoding='utf-8', errors='ignore')
        g_m = re.search(r"guid:\s*([a-f0-9]+)", txt)
        if g_m:
            guid_to_name[g_m.group(1)] = meta.stem
    except Exception:
        pass

print(f"Found {len(guid_to_name)} C# script GUIDs")

# Let's inspect PrefabInstances in Room scene
print("\n=== PREFAB INSTANCES IN ROOM SCENE ===")
for pfid, pdata in prefabs.items():
    src = pdata["source"]
    # Check if there is a name modification
    name_mods = re.findall(r"propertyPath:\s*m_Name\s*\n\s*value:\s*(.*)", pdata["raw"])
    print(f"Prefab [{pfid}]: source={src}, name_mods={name_mods}")

# Search for XR Origin, Prospector, Shovel, Pan, Door
print("\n=== GAMEOBJECTS OF INTEREST ===")
keywords = ["xr", "origin", "prospector", "inventory", "shovel", "pan", "door", "testhelper", "rig"]
for gfid, gdata in gos.items():
    name = gdata["name"]
    if any(k in name.lower() for k in keywords):
        print(f"GO [{gfid}]: '{name}' (Components: {len(gdata['components'])})")
        for cfid in gdata["components"]:
            if cfid in monos:
                m = monos[cfid]
                sname = guid_to_name.get(m["script"][1] if m["script"] else "", "UnknownScript")
                print(f"   - MonoBehaviour [{cfid}]: script={sname} (guid={m['script'][1] if m['script'] else ''})")
            elif cfid in scene_objects:
                print(f"   - Component [{cfid}]: class {scene_objects[cfid]['class_id']}")
