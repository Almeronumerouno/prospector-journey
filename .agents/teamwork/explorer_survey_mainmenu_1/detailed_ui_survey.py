import re
import json

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
            "class_id": class_id,
            "type": comp_type,
            "go_id": go_match.group(1) if go_match else None,
            "body": block_body
        }

# Find VR_MainMenu_Canvas
canvas_go_id = None
for gid, gdata in game_objects.items():
    if gdata["name"] == "VR_MainMenu_Canvas":
        canvas_go_id = gid
        break

def get_transform_for_go(gid):
    for tid, tdata in transforms.items():
        if tdata["go_id"] == gid:
            return tid, tdata
    return None, None

results = []

def parse_node(tid, depth=0):
    tdata = transforms.get(tid)
    if not tdata:
        return
    gid = tdata["go_id"]
    gdata = game_objects.get(gid, {"name": "Unknown", "isActive": True})
    
    node = {
        "depth": depth,
        "name": gdata["name"],
        "go_id": gid,
        "transform_id": tid,
        "isActive": gdata["isActive"],
        "rect": {},
        "components": []
    }
    
    # Parse RectTransform
    tbody = tdata["body"]
    sd = re.search(r'm_SizeDelta:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tbody)
    ap = re.search(r'm_AnchoredPosition:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tbody)
    amin = re.search(r'm_AnchorMin:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tbody)
    amax = re.search(r'm_AnchorMax:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tbody)
    piv = re.search(r'm_Pivot:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', tbody)
    scl = re.search(r'm_LocalScale:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+),\s*z:\s*([\d\.\-]+)\}', tbody)
    rot = re.search(r'm_LocalRotation:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+),\s*z:\s*([\d\.\-]+),\s*w:\s*([\d\.\-]+)\}', tbody)
    pos = re.search(r'm_LocalPosition:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+),\s*z:\s*([\d\.\-]+)\}', tbody)
    
    node["rect"] = {
        "sizeDelta": [float(sd.group(1)), float(sd.group(2))] if sd else None,
        "anchoredPosition": [float(ap.group(1)), float(ap.group(2))] if ap else None,
        "anchorMin": [float(amin.group(1)), float(amin.group(2))] if amin else None,
        "anchorMax": [float(amax.group(1)), float(amax.group(2))] if amax else None,
        "pivot": [float(piv.group(1)), float(piv.group(2))] if piv else None,
        "scale": [float(scl.group(1)), float(scl.group(2)), float(scl.group(3))] if scl else None,
        "localPosition": [float(pos.group(1)), float(pos.group(2)), float(pos.group(3))] if pos else None
    }
    
    for cid in gdata.get("components", []):
        if cid == tid:
            continue
        c = components.get(cid)
        if not c:
            continue
        ctype = c["type"]
        cbody = c["body"]
        
        comp_dict = {"id": cid, "type": ctype}
        
        script_guid = re.search(r'm_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([a-f0-9]+)', cbody)
        if script_guid:
            comp_dict["script_guid"] = script_guid.group(2)
            
        if ctype == "Canvas":
            rm = re.search(r'm_RenderMode:\s*(\d+)', cbody)
            cam = re.search(r'm_Camera:\s*\{fileID:\s*(\d+)', cbody)
            comp_dict["renderMode"] = int(rm.group(1)) if rm else None
            comp_dict["camera"] = cam.group(1) if cam else None
        elif ctype == "CanvasGroup":
            alpha = re.search(r'm_Alpha:\s*([\d\.]+)', cbody)
            interactable = re.search(r'm_Interactable:\s*(\d)', cbody)
            raycasts = re.search(r'm_BlocksRaycasts:\s*(\d)', cbody)
            comp_dict["alpha"] = float(alpha.group(1)) if alpha else None
            comp_dict["interactable"] = int(interactable.group(1)) if interactable else None
            comp_dict["blocksRaycasts"] = int(raycasts.group(1)) if raycasts else None
        elif comp_dict.get("script_guid") == "fe87c0e1cc204ed48ad3b37840f39efc": # Image
            comp_dict["name"] = "Image"
            color = re.search(r'm_Color:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            sprite = re.search(r'm_Sprite:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', cbody)
            mat = re.search(r'm_Material:\s*\{fileID:\s*(-?\d+)(?:,\s*guid:\s*([a-f0-9]+))?', cbody)
            ray = re.search(r'm_RaycastTarget:\s*(\d)', cbody)
            img_type = re.search(r'm_Type:\s*(\d+)', cbody)
            comp_dict["color"] = [float(color.group(1)), float(color.group(2)), float(color.group(3)), float(color.group(4))] if color else None
            comp_dict["sprite"] = sprite.group(2) if sprite and sprite.group(2) else None
            comp_dict["material"] = mat.group(2) if mat and mat.group(2) else None
            comp_dict["raycastTarget"] = int(ray.group(1)) if ray else None
            comp_dict["imageType"] = int(img_type.group(1)) if img_type else None
        elif comp_dict.get("script_guid") == "f4688fdb7df04437aeb418b961361dc5": # TextMeshProUGUI
            comp_dict["name"] = "TextMeshProUGUI"
            txt = re.search(r'm_text:\s*(.*)', cbody)
            fsz = re.search(r'm_fontSize:\s*([\d\.]+)', cbody)
            col = re.search(r'm_fontColor:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            align = re.search(r'm_HorizontalAlignment:\s*(\d+)', cbody)
            valign = re.search(r'm_VerticalAlignment:\s*(\d+)', cbody)
            char_spacing = re.search(r'm_characterSpacing:\s*([\d\.\-]+)', cbody)
            line_spacing = re.search(r'm_lineSpacing:\s*([\d\.\-]+)', cbody)
            comp_dict["text"] = txt.group(1).strip() if txt else ""
            comp_dict["fontSize"] = float(fsz.group(1)) if fsz else None
            comp_dict["fontColor"] = [float(col.group(1)), float(col.group(2)), float(col.group(3)), float(col.group(4))] if col else None
            comp_dict["horizontalAlignment"] = int(align.group(1)) if align else None
            comp_dict["verticalAlignment"] = int(valign.group(1)) if valign else None
            comp_dict["characterSpacing"] = float(char_spacing.group(1)) if char_spacing else None
            comp_dict["lineSpacing"] = float(line_spacing.group(1)) if line_spacing else None
        elif comp_dict.get("script_guid") == "4e29b1a8efbd4b44bb3f3716e73f07ff": # Button
            comp_dict["name"] = "Button"
            norm = re.search(r'm_NormalColor:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            high = re.search(r'm_HighlightedColor:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            press = re.search(r'm_PressedColor:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            sel = re.search(r'm_SelectedColor:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            inter = re.search(r'm_Interactable:\s*(\d)', cbody)
            comp_dict["interactable"] = int(inter.group(1)) if inter else None
            comp_dict["normalColor"] = [float(norm.group(1)), float(norm.group(2)), float(norm.group(3)), float(norm.group(4))] if norm else None
            comp_dict["highlightedColor"] = [float(high.group(1)), float(high.group(2)), float(high.group(3)), float(high.group(4))] if high else None
            comp_dict["pressedColor"] = [float(press.group(1)), float(press.group(2)), float(press.group(3)), float(press.group(4))] if press else None
            comp_dict["selectedColor"] = [float(sel.group(1)), float(sel.group(2)), float(sel.group(3)), float(sel.group(4))] if sel else None
        elif comp_dict.get("script_guid") == "e19747de3f5aca642ab2be37e372fb86": # Outline
            comp_dict["name"] = "Outline"
            col = re.search(r'm_EffectColor:\s*\{r:\s*([\d\.]+),\s*g:\s*([\d\.]+),\s*b:\s*([\d\.]+),\s*a:\s*([\d\.]+)\}', cbody)
            dist = re.search(r'm_EffectDistance:\s*\{x:\s*([\d\.\-]+),\s*y:\s*([\d\.\-]+)\}', cbody)
            comp_dict["effectColor"] = [float(col.group(1)), float(col.group(2)), float(col.group(3)), float(col.group(4))] if col else None
            comp_dict["effectDistance"] = [float(dist.group(1)), float(dist.group(2))] if dist else None
        elif comp_dict.get("script_guid") == "59f8146938fff824cb5fd77236b75775": # VerticalLayoutGroup
            comp_dict["name"] = "VerticalLayoutGroup"
            pad = re.search(r'm_Padding:\n\s*m_Left:\s*([\d\-]+)\n\s*m_Right:\s*([\d\-]+)\n\s*m_Top:\s*([\d\-]+)\n\s*m_Bottom:\s*([\d\-]+)', cbody)
            sp = re.search(r'm_Spacing:\s*([\d\.\-]+)', cbody)
            ali = re.search(r'm_ChildAlignment:\s*(\d+)', cbody)
            comp_dict["padding"] = [int(pad.group(1)), int(pad.group(2)), int(pad.group(3)), int(pad.group(4))] if pad else None
            comp_dict["spacing"] = float(sp.group(1)) if sp else None
            comp_dict["childAlignment"] = int(ali.group(1)) if ali else None
        elif comp_dict.get("script_guid") == "38f8717e7c47e084eb925967947877ac":
            comp_dict["name"] = "VRButtonAudioFeedback"
        elif comp_dict.get("script_guid") == "db18e8806cc0e4b4ba3958644fcb627e":
            comp_dict["name"] = "MainMenuVRController"
        
        node["components"].append(comp_dict)
    
    results.append(node)
    
    for ctid in tdata["children"]:
        parse_node(ctid, depth + 1)

if canvas_go_id:
    ctid, _ = get_transform_for_go(canvas_go_id)
    if ctid:
        parse_node(ctid, 0)

with open(r"c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_1\detailed_ui_hierarchy.json", 'w', encoding='utf-8') as f:
    json.dump(results, f, indent=2)

print(f"Dumped {len(results)} UI nodes into detailed_ui_hierarchy.json")
