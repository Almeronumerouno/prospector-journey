import re
import os

SCENE_PATH = 'Assets/Prospector-journey/Tutorial/Tutorial scene.unity'

with open(SCENE_PATH, 'r', encoding='utf-8') as f:
    text = f.read()

# Search for any occurrence of 'Door' or 'DoorSceneTransition' or 'Prompt' in Tutorial scene
print("=== SEARCHING FOR DOOR / PROMPT / TRANSITION IN TUTORIAL SCENE ===")
matches = re.findall(r'.*?(?:Door|Transition|Prompt|Cabin).*?', text, re.IGNORECASE)
print(f"Total matching lines with Door/Transition/Prompt/Cabin: {len(matches)}")
for m in matches[:20]:
    print(" ", m.strip())

# Inspect House_1, House_2, House_3 positions and contents in Tutorial scene
house_guids = {
    'e43b87eec9181834094b5883f6858f76': 'House_1',
    '17baadd2d44e62d4e8a91e885ce4144e': 'House_2',
    'd4e0f7e6af5fde041b562e69c8777fe6': 'House_3'
}

docs = text.split('--- !u!')
print("\n=== HOUSES IN SCENE ===")
for d in docs:
    for guid, hname in house_guids.items():
        if f'guid: {guid}' in d:
            # find root order, position, rotation
            pos_x = re.search(r'm_LocalPosition\.x\s*value:\s*([^\n]+)', d)
            pos_y = re.search(r'm_LocalPosition\.y\s*value:\s*([^\n]+)', d)
            pos_z = re.search(r'm_LocalPosition\.z\s*value:\s*([^\n]+)', d)
            px = pos_x.group(1) if pos_x else '0'
            py = pos_y.group(1) if pos_y else '0'
            pz = pos_z.group(1) if pos_z else '0'
            rot_y = re.search(r'm_LocalEulerAnglesHint\.y\s*value:\s*([^\n]+)', d)
            ry = rot_y.group(1) if rot_y else '0'
            print(f"{hname} PrefabInstance:")
            print(f"  Position: ({px}, {py}, {pz})")
            print(f"  Rotation Y: {ry}")

print("\n=== INSPECTING HOUSE PREFABS INTERNALS ===")
for hname in ['House_1', 'House_2', 'House_3']:
    p = f"Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/{hname}.prefab"
    with open(p, 'r', encoding='utf-8') as f:
        ptext = f.read()
    print(f"\n{hname} prefab internal details:")
    # Find child GameObjects or meshes
    pdocs = ptext.split('--- !u!')
    for pd in pdocs:
        if pd.startswith('1 &'): # GameObject
            lines = pd.strip().splitlines()
            gname = [l for l in lines if l.strip().startswith('m_Name:')][0]
            print(f"  GO: {gname}")
