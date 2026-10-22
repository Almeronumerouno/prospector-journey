import re

SCENE_PATH = 'Assets/Prospector-journey/Tutorial/Tutorial scene.unity'
with open(SCENE_PATH, 'r', encoding='utf-8') as f:
    text = f.read()

docs = text.split('--- !u!')

village_props = [
    'House_1', 'House_2', 'House_3', 'Bench', 'Crate', 'Fence', 
    'Pine Tree_1', 'Pine Tree_2', 'Lantern', 'Flower Pot'
]

print("=== VILLAGE PROPS POSITIONS IN TUTORIAL SCENE ===")
for d in docs:
    for vp in village_props:
        if f'{vp}.prefab' in d: # or check by name modification
            pass
    # check name modification
    nm = re.search(r'propertyPath:\s*m_Name\s*value:\s*(.*)', d)
    name = nm.group(1) if nm else ''
    for vp in village_props:
        if name.startswith(vp):
            # extract pos
            px = re.search(r'm_LocalPosition\.x\s*value:\s*([^\n]+)', d)
            py = re.search(r'm_LocalPosition\.y\s*value:\s*([^\n]+)', d)
            pz = re.search(r'm_LocalPosition\.z\s*value:\s*([^\n]+)', d)
            x = px.group(1) if px else '0'
            y = py.group(1) if py else '0'
            z = pz.group(1) if pz else '0'
            ry = re.search(r'm_LocalEulerAnglesHint\.y\s*value:\s*([^\n]+)', d)
            r = ry.group(1) if ry else '0'
            print(f"{name:15s} at ({float(x):6.2f}, {float(y):6.2f}, {float(z):6.2f}) rotY={float(r):5.1f}")

