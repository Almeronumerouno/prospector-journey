import sys
import re
from pathlib import Path

scene_path = Path(r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Room\Room scene.unity")

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

docs = content.split("--- !u!")

prefabs = []
mono_scripts = {} # guid -> path if possible

for doc in docs[1:]:
    header_line = doc.split("\n", 1)[0]
    parts = header_line.split("&")
    if len(parts) < 2: continue
    class_id = parts[0].strip()
    file_id = parts[1].strip()
    
    if class_id == "1001":
        # PrefabInstance
        # Extract source prefab guid/fileID
        source_match = re.search(r"m_SourcePrefab:\s*\{fileID:\s*([^\}]+),\s*guid:\s*([^\}]+),", doc)
        # Extract m_Name modifications
        name_mods = re.findall(r"propertyPath:\s*m_Name\s*\n\s*value:\s*(.*)", doc)
        # Extract root or any transforms
        prefabs.append({
            "file_id": file_id,
            "source": source_match.groups() if source_match else None,
            "names": name_mods,
            "raw": doc[:1000]
        })

print(f"Found {len(prefabs)} PrefabInstances:")
for p in prefabs:
    src = p['source']
    print(f"Prefab [{p['file_id']}]: source={src}, names={p['names']}")
