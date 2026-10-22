import os
from pathlib import Path

project_root = Path(r"c:\CodeKuliah\prospector-journey\Assets")
guids_to_find = {
    'a2a0d931eebd6a1479e56fc82c3b78db': 'XR Origin (XR Rig)',
    '1a0fcbc1bc065984ca21139c0f33b62e': 'Gold_Pan',
    'a9b6f002ed24e2b41a299c7565b5efab': 'tool-shovel',
    '47de6033ceef29c4a9191b9d596088a0': 'tool-shovel-upgraded',
    '58d0a4ac86f2348deb02f3880c71378e': 'XR Interaction Simulator',
}

found = {}
for meta_path in project_root.rglob("*.meta"):
    try:
        with open(meta_path, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
            for g, label in guids_to_find.items():
                if g in content:
                    found[g] = (label, str(meta_path)[:-5])
    except Exception as e:
        pass

for g, info in found.items():
    print(f"GUID {g} ({info[0]}): {info[1]}")
