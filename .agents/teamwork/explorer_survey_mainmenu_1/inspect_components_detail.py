import re

scene_path = r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Mainmenu\Mainmenu scene.unity"

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

pattern = re.compile(r'--- !u!(\d+) &(\d+)\n([a-zA-Z0-9_]+):\n(.*?)(?=\n--- !u!|\Z)', re.DOTALL)

game_objects = {}
components = {}

for match in pattern.finditer(content):
    class_id = match.group(1)
    file_id = match.group(2)
    comp_type = match.group(3)
    block_body = match.group(4)
    
    if comp_type == "GameObject":
        name_match = re.search(r'm_Name:\s*(.*)', block_body)
        name = name_match.group(1).strip() if name_match else "Unknown"
        comps = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', block_body)
        game_objects[file_id] = {
            "name": name,
            "components": comps,
            "raw": block_body
        }
    else:
        components[file_id] = {
            "class_id": class_id,
            "type": comp_type,
            "body": block_body
        }

target_names = [
    "VR_MainMenu_Canvas",
    "Board_Background",
    "Gold_Frame",
    "Inner_Board",
    "Header_Section",
    "Title_Text",
    "Subtitle_Text",
    "Gold_Divider",
    "Panels_Content",
    "Main_Buttons_Panel",
    "Btn_StartExpedition",
    "Btn_Settings",
    "Settings_Panel",
    "Section_Title",
    "Frame_GameOptions",
    "Row_TurnType",
    "Row_LocomotionType",
    "Btn_SaveAndExit"
]

report_lines = []

for gid, gdata in game_objects.items():
    if gdata["name"] in target_names:
        report_lines.append(f"\n==================================================")
        report_lines.append(f"GameObject: {gdata['name']} (ID: {gid})")
        report_lines.append(f"==================================================")
        for cid in gdata["components"]:
            cdata = components.get(cid)
            if not cdata:
                continue
            ctype = cdata["type"]
            cbody = cdata["body"]
            
            # Identify script if MonoBehaviour
            script_info = ""
            if ctype == "MonoBehaviour":
                script_m = re.search(r'm_Script:\s*\{fileID:\s*(-?\d+),\s*guid:\s*([a-f0-9]+)', cbody)
                if script_m:
                    script_info = f" (Script GUID: {script_m.group(2)})"
            
            report_lines.append(f"  Component: {ctype}{script_info} (ID: {cid})")
            
            # Extract key properties
            for key in ["m_Color", "m_Sprite", "m_Material", "m_text", "m_fontSize", "m_fontColor", "m_fontAsset", 
                        "m_EffectColor", "m_EffectDistance", "m_SizeDelta", "m_AnchoredPosition", 
                        "m_AnchorMin", "m_AnchorMax", "m_Pivot", "m_Alpha", "m_Interactable", "m_BlocksRaycasts"]:
                km = re.search(rf'{key}:\s*(.*)', cbody)
                if km:
                    report_lines.append(f"    {key}: {km.group(1).strip()}")

with open(r"c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_1\components_detail.txt", 'w', encoding='utf-8') as f:
    f.write("\n".join(report_lines))

print("Dumped components_detail.txt successfully.")
