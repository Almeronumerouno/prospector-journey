import re
from pathlib import Path

scene_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Room\Room scene.unity")

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Let's inspect PrefabInstance 859651291 (XR Origin)
# Find the exact doc block for 859651291
blocks = content.split("--- !u!")

xr_instance_block = None
for b in blocks:
    if "&859651291" in b.split("\n", 1)[0]:
        xr_instance_block = b
        break

print("=== XR Origin (XR Rig) PrefabInstance [859651291] ===")
if xr_instance_block:
    # Print added components and modifications
    print("--- Added Components ---")
    added_comps = re.findall(r"m_AddedComponents:\s*(.*?)(?=m_AddedGameObjects|\nm_[A-Z]|\Z)", xr_instance_block, re.DOTALL)
    if added_comps:
        print(added_comps[0].strip())
    
    print("\n--- Added GameObjects ---")
    added_gos = re.findall(r"m_AddedGameObjects:\s*(.*?)(?=m_RemovedComponents|\nm_[A-Z]|\Z)", xr_instance_block, re.DOTALL)
    if added_gos:
        print(added_gos[0].strip())

    print("\n--- Property Modifications (Sample) ---")
    prop_mods = re.findall(r"- target: \{fileID: ([^\}]+), guid: ([^\}]+)[^\}]*\}\s+propertyPath: ([^\n]+)\s+value: ([^\n]+)", xr_instance_block)
    for target_fid, target_guid, prop, val in prop_mods:
        if any(k in prop for k in ["bodyRadius", "stepOffset", "skinWidth", "m_Height", "m_Radius", "m_Center", "Collision", "Controller", "m_RootOrder", "m_LocalPosition", "m_LocalRotation"]):
            print(f"Mod: {prop} = {val} (target {target_fid})")

# Also find any standalone components in the scene referencing XR Origin or whose GameObject is part of XR Origin
print("\n=== Objects referencing or related to PlayerBodyCollision / CharacterController in Room Scene ===")
for b in blocks:
    if "PlayerBodyCollision" in b or "CharacterController" in b:
        header = b.split("\n", 2)[:2]
        print("\nBlock header:", header)
        print(b[:1500])
