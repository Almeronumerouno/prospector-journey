import re
import os
import glob

SCENE_PATH = 'Assets/Prospector-journey/Tutorial/Tutorial scene.unity'

# Build a fast mapping of guid to asset path
guid_map = {}
for meta in glob.glob('Assets/**/*.meta', recursive=True):
    try:
        with open(meta, 'r', encoding='utf-8', errors='ignore') as f:
            for line in f:
                if line.startswith('guid:'):
                    guid = line.strip().split()[1]
                    guid_map[guid] = meta[:-5]
                    break
    except:
        pass

def parse_yaml_blocks(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        text = f.read()
    
    docs = text.split('--- !u!')
    obj_by_id = {}
    for doc in docs[1:]:
        lines = doc.strip().splitlines()
        header = lines[0]
        parts = header.split('&')
        type_id = parts[0].strip()
        file_id = parts[1].split()[0]
        is_stripped = 'stripped' in header
        
        content = '\n'.join(lines[1:])
        obj_by_id[file_id] = {
            'type': type_id,
            'content': content,
            'lines': lines[1:],
            'stripped': is_stripped
        }
    return obj_by_id

scene_objs = parse_yaml_blocks(SCENE_PATH)

# Look at Environment2
env2_id = '1763126900'
env2_content = scene_objs[env2_id]['content']
print("=== ENVIRONMENT2 ===")
print("Components on Environment2:")
for l in scene_objs[env2_id]['lines']:
    if 'component:' in l:
        print(" ", l)

# Find Transform of Environment2
env2_tf_id = None
for l in scene_objs[env2_id]['lines']:
    m = re.search(r'component:\s*\{fileID:\s*(\d+)\}', l)
    if m:
        cid = m.group(1)
        if cid in scene_objs and scene_objs[cid]['type'] == '4':
            env2_tf_id = cid
            break

print(f"Environment2 Transform ID: {env2_tf_id}")
if env2_tf_id and env2_tf_id in scene_objs:
    tf_c = scene_objs[env2_tf_id]['content']
    children = re.findall(r'-\s*\{fileID:\s*(\d+)\}', tf_c)
    print(f"Total children Transforms under Environment2: {len(children)}")

# Check what each child transform belongs to!
# In Unity scenes, child transforms can be stripped transforms of PrefabInstances or direct Transforms.
child_tf_set = set(children if env2_tf_id else [])

# Let's inspect all PrefabInstances
print("\n=== PREFAB INSTANCES IN SCENE ===")
prefab_instances = []
for fid, data in scene_objs.items():
    if data['type'] == '1001':
        content = data['content']
        source_m = re.search(r'm_SourcePrefab:\s*\{fileID:\s*\d+,\s*guid:\s*([a-f0-9]+)', content)
        guid = source_m.group(1) if source_m else ''
        asset = guid_map.get(guid, f"Unknown GUID {guid}")
        
        # Check m_Father
        father_m = re.search(r'target:\s*\{fileID:\s*\d+,\s*guid:\s*[a-f0-9]+,\s*type:\s*\d+\}\s*propertyPath:\s*m_Father\s*value:\s*\{fileID:\s*(\d+)\}', content)
        father_id = father_m.group(1) if father_m else '0'
        
        # Check root transform file ID in scene if present (stripped Transform)
        # Often m_Modifications has target: {fileID: <tf_id>...} propertyPath: m_RootOrder or m_LocalPosition
        root_tf_m = re.search(r'target:\s*\{fileID:\s*(\d+),\s*guid:\s*' + guid + r',\s*type:\s*\d+\}\s*propertyPath:\s*m_RootOrder', content)
        
        # Check name modification
        name_m = re.search(r'propertyPath:\s*m_Name\s*value:\s*(.*)', content)
        name_override = name_m.group(1) if name_m else ''
        
        is_under_env2 = (father_id == env2_tf_id)
        
        prefab_instances.append({
            'fid': fid,
            'guid': guid,
            'asset': asset,
            'father_id': father_id,
            'is_under_env2': is_under_env2,
            'name_override': name_override
        })

print(f"Total PrefabInstances: {len(prefab_instances)}")
under_env2 = [p for p in prefab_instances if p['is_under_env2']]
root_level = [p for p in prefab_instances if not p['is_under_env2']]

print(f"Children under Environment2: {len(under_env2)}")
print(f"Root level PrefabInstances: {len(root_level)}")

from collections import Counter
under_env2_counts = Counter([p['asset'] for p in under_env2])
print("\n--- Children of Environment2 by Asset ---")
for asset, count in under_env2_counts.most_common():
    print(f"  {count:3d}x {os.path.basename(asset)}")

print("\n--- Root Level PrefabInstances ---")
for p in root_level:
    print(f"  ID {p['fid']}: {os.path.basename(p['asset'])} (father_id={p['father_id']}) {p['name_override']}")

