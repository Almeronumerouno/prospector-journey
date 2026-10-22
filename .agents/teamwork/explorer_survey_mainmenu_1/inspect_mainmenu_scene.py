import re

scene_path = r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Mainmenu\Mainmenu scene.unity"

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

pattern = re.compile(r'--- !u!(\d+) &(\d+)\n([a-zA-Z0-9_]+):\n(.*?)(?=\n--- !u!|\Z)', re.DOTALL)

game_objects = {}
transforms = {}
components = {}

for match in pattern.finditer(content):
    class_id = match.group(1)
    file_id = match.group(2)
    comp_type = match.group(3)
    block_body = match.group(4)
    
    if comp_type == "GameObject":
        name_match = re.search(r'm_Name:\s*(.*)', block_body)
        name = name_match.group(1).strip() if name_match else "Unknown"
        is_active = True
        act_match = re.search(r'm_IsActive:\s*(\d)', block_body)
        if act_match and act_match.group(1) == '0':
            is_active = False
        comps = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', block_body)
        game_objects[file_id] = {
            "name": name,
            "isActive": is_active,
            "components": comps,
            "raw": block_body
        }
    elif comp_type in ("Transform", "RectTransform"):
        go_match = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', block_body)
        father_match = re.search(r'm_Father:\s*\{fileID:\s*(\d+)\}', block_body)
        m_children_match = re.search(r'm_Children:(.*?)(?=\nm_Father|\Z)', block_body, re.DOTALL)
        children = re.findall(r'-\s*\{?fileID:\s*(\d+)\}?', m_children_match.group(1)) if m_children_match else []
        
        transforms[file_id] = {
            "type": comp_type,
            "go_id": go_match.group(1) if go_match else None,
            "father_id": father_match.group(1) if father_match else None,
            "children": children,
            "body": block_body
        }
    else:
        go_match = re.search(r'm_GameObject:\s*\{fileID:\s*(\d+)\}', block_body)
        components[file_id] = {
            "type": comp_type,
            "go_id": go_match.group(1) if go_match else None,
            "body": block_body
        }

canvas_go_id = None
for gid, gdata in game_objects.items():
    if gdata["name"] == "VR_MainMenu_Canvas":
        canvas_go_id = gid
        break

lines = []
lines.append(f"Parsed {len(game_objects)} GameObjects, {len(transforms)} Transforms, {len(components)} Components.")
lines.append(f"VR_MainMenu_Canvas ID: {canvas_go_id}")

def get_transform_for_go(gid):
    for tid, tdata in transforms.items():
        if tdata["go_id"] == gid:
            return tid, tdata
    return None, None

def print_tree(tid, indent=0):
    tdata = transforms.get(tid)
    if not tdata:
        return
    gid = tdata["go_id"]
    gdata = game_objects.get(gid, {"name": "Unknown", "isActive": True})
    
    comp_types = []
    for cid in gdata.get("components", []):
        if cid == tid:
            continue
        c = components.get(cid)
        if c:
            comp_types.append(c["type"])
            
    prefix = "  " * indent
    active_str = "" if gdata.get("isActive", True) else " [INACTIVE]"
    lines.append(f"{prefix}- {gdata['name']}{active_str} (GO:{gid}, Tr:{tdata['type']}) Comps: {', '.join(comp_types)}")
    
    for cid in gdata.get("components", []):
        c = components.get(cid)
        if not c:
            continue
        ctype = c["type"]
        cbody = c["body"]
        if ctype in ("Image", "RawImage"):
            color = re.search(r'm_Color:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            sprite = re.search(r'm_Sprite:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', cbody)
            mat = re.search(r'm_Material:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', cbody)
            col_str = f"RGBA({color.group(1)}, {color.group(2)}, {color.group(3)}, {color.group(4)})" if color else "NoColor"
            spr_str = f"fileID:{sprite.group(1)}, guid:{sprite.group(2)}" if sprite else "None"
            mat_str = f"fileID:{mat.group(1)}, guid:{mat.group(2)}" if mat else "default"
            lines.append(f"{prefix}    [{ctype}] Color: {col_str}, Sprite: {spr_str}, Mat: {mat_str}")
        elif "TextMeshPro" in ctype or (ctype == "MonoBehaviour" and "m_text:" in cbody):
            text_match = re.search(r'm_text:\s*(.*)', cbody)
            size_match = re.search(r'm_fontSize:\s*([\d\.]+)', cbody)
            color_match = re.search(r'm_fontColor:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            font_match = re.search(r'm_fontAsset:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', cbody)
            txt = text_match.group(1).strip() if text_match else ""
            sz = size_match.group(1) if size_match else "?"
            col = f"RGBA({color_match.group(1)}, {color_match.group(2)}, {color_match.group(3)}, {color_match.group(4)})" if color_match else ""
            fnt = f"guid:{font_match.group(2)}" if font_match and font_match.group(2) else "None"
            lines.append(f"{prefix}    [TMP] text='{txt}', size={sz}, color={col}, font={fnt}")
        elif ctype == "Outline":
            dist = re.search(r'm_EffectDistance:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', cbody)
            col = re.search(r'm_EffectColor:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            lines.append(f"{prefix}    [Outline] Color: {col.group(0) if col else ''}, Dist: {dist.group(0) if dist else ''}")
        elif ctype in ("MeshRenderer", "MeshFilter"):
            mesh = re.search(r'm_Mesh:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', cbody)
            mats = re.findall(r'-\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', cbody)
            lines.append(f"{prefix}    [{ctype}] Mesh: {mesh.group(0) if mesh else ''}, Mats: {mats}")
        elif ctype == "Canvas":
            renmode = re.search(r'm_RenderMode:\s*(\d+)', cbody)
            lines.append(f"{prefix}    [Canvas] RenderMode: {renmode.group(1) if renmode else '?'}")
        elif ctype == "CanvasScaler":
            lines.append(f"{prefix}    [CanvasScaler]")

    if tdata["type"] == "RectTransform":
        sd = re.search(r'm_SizeDelta:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tdata["body"])
        ap = re.search(r'm_AnchoredPosition:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tdata["body"])
        amin = re.search(r'm_AnchorMin:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tdata["body"])
        amax = re.search(r'm_AnchorMax:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tdata["body"])
        scl = re.search(r'm_LocalScale:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+),\s*z:\s*([\d\.\-]+)\}', tdata["body"])
        if sd and ap:
            lines.append(f"{prefix}    [RectTransform] Pos=({ap.group(1)}, {ap.group(2)}), Size=({sd.group(1)}, {sd.group(2)}), Min=({amin.group(1)}, {amin.group(2)}), Max=({amax.group(1)}, {amax.group(2)}), Scale=({scl.group(1) if scl else '1'}, {scl.group(2) if scl else '1'}, {scl.group(3) if scl else '1'})")

    for child_tid in tdata["children"]:
        print_tree(child_tid, indent + 1)

if canvas_go_id:
    ctid, _ = get_transform_for_go(canvas_go_id)
    if ctid:
        print_tree(ctid, 0)

with open(r"c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_1\tree_dump_utf8.txt", 'w', encoding='utf-8') as out:
    out.write("\n".join(lines))

print("Dumped to tree_dump_utf8.txt")
