import re
import os

SCENE_PATH = 'Assets/Prospector-journey/Tutorial/Tutorial scene.unity'
PREFAB_PATH = 'Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab'

with open(SCENE_PATH, 'r', encoding='utf-8') as f:
    scene_text = f.read()

# Find PrefabInstance 441087506
docs = scene_text.split('--- !u!')
xr_instance = None
for d in docs:
    if d.startswith('1001 &441087506'):
        xr_instance = d
        break

print("=== SCENE PREFABINSTANCE MODIFICATIONS FOR XR ORIGIN ===")
if xr_instance:
    # Print modifications
    mods = re.findall(r'target:\s*\{fileID:\s*(\d+),\s*guid:\s*[a-f0-9]+,\s*type:\s*\d+\}\s*propertyPath:\s*([^\n]+)\s*value:\s*([^\n]+)?', xr_instance)
    print(f"Total property modifications: {len(mods)}")
    for m in mods:
        print(f"  target {m[0]}: {m[1]} = {m[2]}")
else:
    print("Instance 441087506 not found!")

# Now inspect prefab in detail
with open(PREFAB_PATH, 'r', encoding='utf-8') as f:
    prefab_text = f.read()

pdocs = prefab_text.split('--- !u!')
print("\n=== COMPONENTS ON XR ORIGIN (XR RIG) ROOT IN PREFAB ===")
for d in pdocs:
    if d.startswith('1 &1717954561962503725'): # Root GameObject
        print(d[:600])
    if d.startswith('143 &6232745470614056090'): # CharacterController
        print("CharacterController on Root:")
        print(d)

print("\n=== MONOBEHAVIOURS ON XR ORIGIN (XR RIG) ROOT ===")
# Root components:
# 1178791450436251564, 6520291404937146767, 5826056641483426609, 5033801203051696737
for comp_id in ['1178791450436251564', '6520291404937146767', '5826056641483426609', '5033801203051696737']:
    for d in pdocs:
        if d.startswith(f'114 &{comp_id}'):
            print(f"--- Component {comp_id} ---")
            lines = d.strip().splitlines()
            for l in lines[:15]:
                print(" ", l)

