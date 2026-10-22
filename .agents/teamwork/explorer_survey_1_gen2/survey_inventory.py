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

gos = {}
trs = {}
for fid, b in scene_objs.items():
    lines = b.splitlines()
    header = lines[0]
    class_id = header.split()[0]
    if class_id == "1":
        m_name = re.search(r"m_Name:\s*(.*)", b)
        name = m_name.group(1).strip() if m_name else "Unknown"
        comp_ids = re.findall(r"component:\s*\{fileID:\s*([^\}]+)\}", b)
        gos[fid] = {"name": name, "components": comp_ids, "raw": b}
    elif class_id in ["4", "224"]:
        m_go = re.search(r"m_GameObject:\s*\{fileID:\s*([^\}]+)\}", b)
        m_fa = re.search(r"m_Father:\s*\{fileID:\s*([^\}]+)\}", b)
        children = re.findall(r"-\s*\{fileID:\s*([^\}]+)\}", b)
        trs[fid] = {
            "go": m_go.group(1).strip() if m_go else None,
            "father": m_fa.group(1).strip() if m_fa else None,
            "children": children,
            "raw": b
        }

# Find [Prospector_Inventory]
inv_go_id = "1787724447"
inv_tr_id = "1787724450"

print("=== [Prospector_Inventory] Full Hierarchy Tree & Components ===")

def print_inv_tree(tr_id, depth=0):
    tr_info = trs.get(tr_id)
    if not tr_info:
        print(f"{'  '*depth}[Missing Transform {tr_id}]")
        return
    go_id = tr_info["go"]
    go_info = gos.get(go_id)
    name = go_info["name"] if go_info else f"Unknown_GO_{go_id}"
    indent = "  " * depth
    print(f"{indent}- GO: '{name}' (Tr: {tr_id}, GO: {go_id})")
    
    if go_info:
        for cid in go_info["components"]:
            cb = scene_objs.get(cid, "")
            clines = cb.splitlines()
            c_class = clines[0].split()[0]
            c_type = clines[1].split(":")[0].strip() if len(clines) > 1 else ""
            print(f"{indent}    * [{c_type} (class {c_class}, fid {cid})]")
            
            # If MonoBehaviour, print details
            if c_class == "114":
                for line in clines:
                    if any(k in line for k in ["m_Script:", "m_EditorClassIdentifier:", "enableLazyFollow:", "followDistance:", "heightOffset:", "deadzoneAngle:", "followSpeed:", "heightSpeed:", "maxSlots:", "maxPitchAngle:", "interactionManager:", "rightNearFar:", "leftNearFar:", "inventoryPanel:", "storedItemsContainer:", "toastPanel:", "toastText:", "storeHeldButton:", "storeHeldButtonText:", "closeButton:", "statusText:", "openClip:", "closeClip:", "storeClip:", "retrieveClip:", "errorClip:", "hoverClip:", "clickClip:"]):
                        print(f"{indent}        {line.strip()}")
            elif c_class in ["223", "224", "225"]:
                for line in clines:
                    if any(k in line for k in ["m_RenderMode:", "m_Camera:", "m_ReferencePixelsPerUnit:", "m_Alpha:", "m_Interactable:", "m_BlocksRaycasts:", "m_SizeDelta:", "m_AnchoredPosition:"]):
                        print(f"{indent}        {line.strip()}")

    for c_tr in tr_info["children"]:
        print_inv_tree(c_tr, depth + 1)

print_inv_tree(inv_tr_id)
