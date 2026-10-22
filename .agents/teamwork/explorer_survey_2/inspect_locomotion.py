import re
import os

PREFAB_PATH = 'Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab'
with open(PREFAB_PATH, 'r', encoding='utf-8') as f:
    text = f.read()

docs = text.split('--- !u!')

# Map fileID to doc
doc_map = {}
for d in docs[1:]:
    lines = d.strip().splitlines()
    header = lines[0]
    fid = header.split('&')[1].split()[0]
    tid = header.split('&')[0].strip()
    doc_map[fid] = {'type': tid, 'doc': d, 'lines': lines}

print("=== LOCOMOTION COMPONENTS ON PREFAB ===")
# Find Locomotion children
# ContinuousMove, SnapTurn, ContinuousTurn, Jump, Grab Move, Teleportation, Climb
loco_ids = ['5167925059111895691', '58445280694286476', '1470279098769358944', '1470279098769358945', '3904471525139099824', '3533369827395663398', '2190828208922718286', '2626757739553014894']

for fid in loco_ids:
    if fid in doc_map:
        d = doc_map[fid]
        name = ''
        for l in d['lines']:
            if l.strip().startswith('m_Name:'):
                name = l.strip().split(':', 1)[1].strip()
                break
        print(f"\nGameObject: {name} (ID: {fid})")
        # Find components
        comps = re.findall(r'component:\s*\{fileID:\s*(-?\d+)\}', d['doc'])
        for cid in comps:
            if cid in doc_map:
                c = doc_map[cid]
                ctype = c['type']
                if ctype == '114':
                    script_id = ''
                    for l in c['lines']:
                        if 'm_EditorClassIdentifier:' in l:
                            script_id = l.strip().split(':', 1)[1].strip()
                        elif 'm_Script:' in l and not script_id:
                            script_id = l.strip()
                    print(f"  Component 114: {script_id}")
                    # print enabled status
                    for l in c['lines']:
                        if l.strip().startswith('m_Enabled:'):
                            print(f"    {l.strip()}")
                        if 'moveSpeed' in l or 'turnSpeed' in l or 'enableTurnAround' in l or 'smoothMotion' in l:
                            print(f"    {l.strip()}")
                else:
                    print(f"  Component Type {ctype}")

print("\n=== CONTROLLER AND CAMERA CONFIGURATION ===")
target_gos = ['1767192433', '202364687', '1670256624', '1680501586'] # Main Camera, Left Controller, Right Controller, Camera Offset
for fid in target_gos:
    if fid in doc_map:
        d = doc_map[fid]
        name = ''
        for l in d['lines']:
            if l.strip().startswith('m_Name:'):
                name = l.strip().split(':', 1)[1].strip()
                break
        print(f"\nGameObject: {name} (ID: {fid})")
        comps = re.findall(r'component:\s*\{fileID:\s*(-?\d+)\}', d['doc'])
        for cid in comps:
            if cid in doc_map:
                c = doc_map[cid]
                ctype = c['type']
                if ctype == '114':
                    for l in c['lines'][:10]:
                        if 'm_EditorClassIdentifier:' in l or 'm_Script:' in l or 'm_Enabled:' in l:
                            print(f"  114: {l.strip()}")
                elif ctype == '20':
                    print("  Camera Component")
                elif ctype == '4':
                    for l in c['lines']:
                        if 'm_LocalPosition:' in l:
                            print(f"  Transform pos: {l.strip()}")
