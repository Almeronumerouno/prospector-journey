import re
from pathlib import Path

prefab_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Tutorial\XR Origin (XR Rig).prefab")
with open(prefab_path, 'r', encoding='utf-8', errors='ignore') as f:
    text = f.read()

blocks = text.split("--- !u!")
objs = {}
for b in blocks[1:]:
    h = b.split("\n", 1)[0].split("&")
    if len(h) >= 2:
        objs[h[1].strip()] = b

print(f"Total objects in prefab: {len(objs)}")

# Map gameobjects and transforms
gos = {}
trs = {}
for fid, b in objs.items():
    lines = b.splitlines()
    header = lines[0]
    class_id = header.split()[0]
    if class_id == "1": # GameObject
        m_name = re.search(r"m_Name:\s*(.*)", b)
        name = m_name.group(1).strip() if m_name else "Unknown"
        comp_ids = re.findall(r"component:\s*\{fileID:\s*([^\}]+)\}", b)
        gos[fid] = {"name": name, "components": comp_ids, "block": b}
    elif class_id in ["4", "224"]: # Transform
        m_go = re.search(r"m_GameObject:\s*\{fileID:\s*([^\}]+)\}", b)
        m_fa = re.search(r"m_Father:\s*\{fileID:\s*([^\}]+)\}", b)
        children = re.findall(r"-\s*\{fileID:\s*([^\}]+)\}", b)
        trs[fid] = {
            "go": m_go.group(1).strip() if m_go else None,
            "father": m_fa.group(1).strip() if m_fa else None,
            "children": children,
            "block": b
        }

# Print hierarchy tree
def print_tree(tr_id, depth=0):
    tr_info = trs.get(tr_id)
    if not tr_info: return
    go_id = tr_info["go"]
    go_info = gos.get(go_id)
    go_name = go_info["name"] if go_info else f"Unknown_GO_{go_id}"
    indent = "  " * depth
    print(f"{indent}- GO: '{go_name}' (Tr: {tr_id}, GO: {go_id})")
    
    # Print components on this GameObject
    if go_info:
        for cid in go_info["components"]:
            cb = objs.get(cid, "")
            clines = cb.splitlines()
            c_header = clines[0] if clines else "Unknown"
            c_type = clines[1].replace(":", "").strip() if len(clines) > 1 else ""
            c_script = ""
            if "MonoBehaviour" in c_type:
                sm = re.search(r"m_EditorClassIdentifier:\s*(.*)", cb)
                if sm and sm.group(1).strip():
                    c_script = sm.group(1).strip()
                else:
                    sm2 = re.search(r"m_Script:\s*\{fileID:\s*([^\}]+),\s*guid:\s*([^\}]+)", cb)
                    if sm2:
                        c_script = f"guid:{sm2.group(2)}"
            print(f"{indent}    * [{c_type}] {c_script}")

    for child_tr in tr_info["children"]:
        print_tree(child_tr, depth + 1)

# Find root transform (father == 0)
root_trs = [tid for tid, tdata in trs.items() if tdata["father"] == "0"]
print("\n=== PREFAB HIERARCHY TREE ===")
for rt in root_trs:
    print_tree(rt)
