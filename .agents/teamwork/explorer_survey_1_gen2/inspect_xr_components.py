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

def inspect_go(go_fid, name_hint=""):
    b = objs.get(go_fid)
    if not b:
        print(f"GO {go_fid} not found")
        return
    m_name = re.search(r"m_Name:\s*(.*)", b)
    name = m_name.group(1).strip() if m_name else name_hint
    print(f"\n==========================================")
    print(f"GameObject: '{name}' [{go_fid}]")
    print(f"==========================================")
    comp_ids = re.findall(r"component:\s*\{fileID:\s*([^\}]+)\}", b)
    for cid in comp_ids:
        cb = objs.get(cid, "")
        clines = cb.splitlines()
        header = clines[0]
        class_id = header.split()[0]
        type_name = clines[1].split(":")[0].strip() if len(clines) > 1 else ""
        if class_id == "114":
            sm = re.search(r"m_EditorClassIdentifier:\s*(.*)", cb)
            ed_class = sm.group(1).strip() if sm else ""
            sm2 = re.search(r"m_Script:\s*\{fileID:\s*([^\}]+),\s*guid:\s*([^\}]+)", cb)
            guid = sm2.group(2) if sm2 else ""
            print(f"\n  * Component [{cid}]: MonoBehaviour -> {ed_class} (GUID: {guid})")
        else:
            print(f"\n  * Component [{cid}]: {type_name} (Class {class_id})")
        
        # print non-default or key properties
        for line in clines[2:]:
            if any(k in line for k in ["m_Camera:", "m_Origin:", "m_MoveSpeed", "m_TurnSpeed", "m_Enable", "m_Tracking", "m_Input", "m_Radius", "m_Height", "m_StepOffset", "m_SkinWidth", "m_Mass", "m_Drag", "m_UseGravity", "m_IsKinematic", "m_InteractionManager", "m_SelectAction", "m_ActivateAction", "m_PositionAction"]):
                print(f"      {line.strip()}")

# Inspect key gameobjects
inspect_go("1717954561962503725", "XR Origin (XR Rig)")
inspect_go("1680501586", "Camera Offset")
inspect_go("1767192433", "Main Camera")
inspect_go("202364687", "Left Controller")
inspect_go("1670256624", "Right Controller")
inspect_go("5167925059111895691", "Locomotion")
inspect_go("58445280694286476", "ContinuousMove")
inspect_go("1470279098769358944", "SnapTurn")
inspect_go("1470279098769358945", "ContinuousTurn")
