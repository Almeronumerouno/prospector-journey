import re

scene_path = r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Mainmenu\Mainmenu scene.unity"

with open(scene_path, "r", encoding="utf-8") as f:
    content = f.read()

# Parse YAML blocks
blocks = re.split(r'\n--- !u!', content)
print(f"Total blocks: {len(blocks)}")

gameobjects = {} # fileID -> name
transforms = {}  # fileID -> {'go': fileID, 'father': fileID, 'children': []}
components = {}  # fileID -> {'type': type_str, 'go': fileID, 'raw': str}

for block in blocks:
    if not block.strip():
        continue
    header_match = re.match(r'^(\d+) &(\d+)', block)
    if not header_match:
        continue
    type_id, file_id = header_match.groups()
    file_id = int(file_id)
    
    if type_id == '1': # GameObject
        name_match = re.search(r'm_Name: (.*)', block)
        name = name_match.group(1).strip() if name_match else "<no name>"
        gameobjects[file_id] = name
    elif type_id in ('4', '224'): # Transform / RectTransform
        go_match = re.search(r'm_GameObject: \{fileID: (\d+)\}', block)
        go_id = int(go_match.group(1)) if go_match else None
        father_match = re.search(r'm_Father: \{fileID: (\d+)\}', block)
        father_id = int(father_match.group(1)) if father_match else None
        children = [int(c) for c in re.findall(r'- \{fileID: (\d+)\}', block.split('m_Children:')[1].split('m_Father:')[0])] if 'm_Children:' in block else []
        transforms[file_id] = {'go': go_id, 'father': father_id, 'children': children}

print(f"Found {len(gameobjects)} GameObjects and {len(transforms)} Transforms.")

# Find VR_MainMenu_Canvas transform
canvas_go_id = None
for gid, name in gameobjects.items():
    if name == 'VR_MainMenu_Canvas':
        canvas_go_id = gid
        break

print(f"VR_MainMenu_Canvas GameObject ID: {canvas_go_id}")

canvas_tr_id = None
for tid, tdata in transforms.items():
    if tdata['go'] == canvas_go_id:
        canvas_tr_id = tid
        break

print(f"VR_MainMenu_Canvas Transform ID: {canvas_tr_id}")

def print_tree(tr_id, depth=0):
    tdata = transforms.get(tr_id)
    if not tdata:
        return
    go_id = tdata['go']
    name = gameobjects.get(go_id, f"<unknown GO {go_id}>")
    print("  " * depth + f"- {name} (GO: {go_id}, TR: {tr_id})")
    for child_tr_id in tdata['children']:
        print_tree(child_tr_id, depth + 1)

print("\n--- VR_MainMenu_Canvas Hierarchy ---")
if canvas_tr_id:
    print_tree(canvas_tr_id)
