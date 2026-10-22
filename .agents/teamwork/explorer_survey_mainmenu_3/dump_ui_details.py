import re

scene_path = r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Mainmenu\Mainmenu scene.unity"

with open(scene_path, "r", encoding="utf-8") as f:
    content = f.read()

blocks = re.split(r'\n--- !u!', content)

# Map fileID to (type_id, block_text)
elements = {}
for block in blocks:
    if not block.strip():
        continue
    m = re.match(r'^(\d+) &(\d+)', block)
    if m:
        elements[int(m.group(2))] = (m.group(1), block)

target_names = [
    'VR_MainMenu_Canvas', 'Board_Background', 'Gold_Frame', 'Inner_Board',
    'Title_Text', 'Subtitle_Text', 'Btn_StartExpedition', 'Btn_Tutorial', 'Btn_Settings',
    'Settings_Panel', 'Section_Title', 'Frame_GameOptions', 'Row_TurnType', 'Row_LocomotionType',
    'Btn_SaveAndExit'
]

# Find GOs
target_gos = {}
for fid, (tid, text) in elements.items():
    if tid == '1': # GameObject
        m_name = re.search(r'm_Name: (.*)', text)
        if m_name and m_name.group(1).strip() in target_names:
            name = m_name.group(1).strip()
            # get components
            comp_ids = [int(c) for c in re.findall(r'- component: \{fileID: (\d+)\}', text)]
            target_gos[name] = (fid, comp_ids)

for name, (goid, comp_ids) in target_gos.items():
    print(f"\n================ GameObject: {name} (ID: {goid}) ================")
    for cid in comp_ids:
        if cid in elements:
            ctid, ctext = elements[cid]
            # print component type and interesting fields
            first_lines = ctext.split('\n')[:2]
            print(f"  Component type {ctid}: {first_lines[0]}")
            
            # If RectTransform (224)
            if ctid == '224':
                m_size = re.search(r'm_SizeDelta: \{(.*?)\}', ctext)
                m_pos = re.search(r'm_AnchoredPosition: \{(.*?)\}', ctext)
                print(f"    RectTransform: pos={m_pos.group(1) if m_pos else '?'} size={m_size.group(1) if m_size else '?'}")
            
            # If Image (114)
            if 'm_Color:' in ctext:
                m_col = re.search(r'm_Color: \{(.*?)\}', ctext)
                m_sprite = re.search(r'm_Sprite: \{(.*?)\}', ctext)
                m_mat = re.search(r'm_Material: \{(.*?)\}', ctext)
                print(f"    Color: {m_col.group(1) if m_col else '?'}")
                if m_sprite: print(f"    Sprite: {m_sprite.group(1)}")
                if m_mat: print(f"    Material: {m_mat.group(1)}")
            
            # If TextMeshProUGUI
            if 'm_fontSize:' in ctext:
                m_fs = re.search(r'm_fontSize: ([\d\.]+)', ctext)
                m_text = re.search(r'm_text: (.*)', ctext)
                m_font = re.search(r'm_fontAsset: \{(.*?)\}', ctext)
                print(f"    TMP: fontSize={m_fs.group(1) if m_fs else '?'} text='{m_text.group(1) if m_text else '?'}'")
