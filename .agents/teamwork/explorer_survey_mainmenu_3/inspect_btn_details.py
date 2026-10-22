import re

scene_path = r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Mainmenu\Mainmenu scene.unity"
with open(scene_path, "r", encoding="utf-8") as f: content = f.read()

blocks = re.split(r'\n--- !u!', content)
elements = {}
for block in blocks:
    if not block.strip(): continue
    m = re.match(r'^(\d+) &(\d+)', block)
    if m: elements[int(m.group(2))] = (m.group(1), block)

print("--- Main_Buttons_Panel components ---")
for cid in [1759804372, 1759804373]:
    tid, text = elements.get(cid, ('?', '?'))
    print(f"ID {cid} (Type {tid}):\n{text}")

print("--- Btn_StartExpedition components ---")
for cid in [1231874297, 1231874301, 1231874300, 1231874299, 1231874298]:
    tid, text = elements.get(cid, ('?', '?'))
    print(f"ID {cid} (Type {tid}):\n{text}")
