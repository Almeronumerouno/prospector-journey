import re
import os
import glob

# Collect all prefabs referenced
def inspect_prefab_colliders(prefab_path):
    if not os.path.exists(prefab_path):
        return f"File not found: {prefab_path}"
    with open(prefab_path, 'r', encoding='utf-8') as f:
        text = f.read()
    
    docs = text.split('--- !u!')
    gos = {}
    colliders = []
    renderers = []
    mesh_filters = []
    
    for doc in docs[1:]:
        lines = doc.strip().splitlines()
        header = lines[0]
        type_id = header.split('&')[0].strip()
        fid = header.split('&')[1].split()[0]
        first_key = lines[1] if len(lines) > 1 else ''
        
        if type_id == '1': # GameObject
            name = ''
            for l in lines:
                if l.strip().startswith('m_Name:'):
                    name = l.strip().split(':', 1)[1].strip()
                    break
            tag = 'Untagged'
            for l in lines:
                if l.strip().startswith('m_TagString:'):
                    tag = l.strip().split(':', 1)[1].strip()
                    break
            layer = '0'
            for l in lines:
                if l.strip().startswith('m_Layer:'):
                    layer = l.strip().split(':', 1)[1].strip()
                    break
            gos[fid] = {'name': name, 'tag': tag, 'layer': layer}
            
        elif type_id in ('65', '136', '64', '58'): # BoxCollider, CapsuleCollider, MeshCollider, SphereCollider
            col_type = {
                '65': 'BoxCollider',
                '136': 'CapsuleCollider',
                '64': 'MeshCollider',
                '58': 'SphereCollider'
            }.get(type_id, f'Collider_{type_id}')
            is_trigger = False
            for l in lines:
                if 'm_IsTrigger: 1' in l:
                    is_trigger = True
            is_convex = False
            for l in lines:
                if 'm_Convex: 1' in l:
                    is_convex = True
            colliders.append({'fid': fid, 'type': col_type, 'trigger': is_trigger, 'convex': is_convex})
            
        elif type_id in ('23', '137'): # MeshRenderer, SkinnedMeshRenderer
            renderers.append(fid)
        elif type_id == '33': # MeshFilter
            mesh_filters.append(fid)

    return {
        'gos': list(gos.values()),
        'colliders': colliders,
        'has_renderer': len(renderers) > 0,
        'has_mesh': len(mesh_filters) > 0
    }

print("=== INSPECTING ROOT PROPS & VILLAGE BUILDINGS ===")
village_prefabs = [
    'House_1', 'House_2', 'House_3', 'Bench', 'Crate', 'Fence', 
    'Pine Tree_1', 'Pine Tree_2', 'Lantern', 'Flower Pot'
]

for name in village_prefabs:
    p = f"Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/{name}.prefab"
    info = inspect_prefab_colliders(p)
    print(f"\n[{name}] ({p}):")
    if isinstance(info, dict):
        print(f"  GameObjects ({len(info['gos'])}): {info['gos'][:3]}")
        print(f"  Colliders ({len(info['colliders'])}): {info['colliders']}")
        print(f"  Has Mesh/Renderer: mesh={info['has_mesh']}, rend={info['has_renderer']}")
    else:
        print(" ", info)

print("\n" + "="*50)
print("=== INSPECTING SIMPLENATUREPACK PREFABS (ENVIRONMENT2) ===")
nature_prefabs = [
    'Ground_01', 'Ground_02', 'Ground_03',
    'Tree_01', 'Tree_02', 'Tree_03', 'Tree_04', 'Tree_05',
    'Rock_01', 'Rock_02', 'Rock_03', 'Rock_04', 'Rock_05',
    'Stump_01', 'Bush_02', 'Bush_03',
    'Grass_01', 'Grass_02', 'Flowers_01', 'Flowers_02', 'Mushroom_01', 'Mushroom_02', 'Branch_01'
]

for name in nature_prefabs:
    p = f"Assets/SimpleNaturePack/Prefabs/{name}.prefab"
    info = inspect_prefab_colliders(p)
    cols_str = ', '.join([f"{c['type']}(trig={c['trigger']},cvx={c['convex']})" for c in info['colliders']]) if info['colliders'] else "NO COLLIDERS"
    gos_summary = ", ".join([f"{g['name']}[tag={g['tag']},layer={g['layer']}]" for g in info['gos']])
    print(f"  {name:15s}: Colliders={cols_str:35s} | GOs: {gos_summary}")

