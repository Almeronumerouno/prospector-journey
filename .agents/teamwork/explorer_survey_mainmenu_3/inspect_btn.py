import re

scene_path = r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Mainmenu\Mainmenu scene.unity"
with open(scene_path, "r", encoding="utf-8") as f: content = f.read()

blocks = re.split(r'\n--- !u!', content)
elements = {}
for block in blocks:
    if not block.strip(): continue
    m = re.match(r'^(\d+) &(\d+)', block)
    if m: elements[int(m.group(2))] = (m.group(1), block)

# Check Btn_StartExpedition (GO: 1231874296) and its parent Main_Buttons_Panel
btn_id = 1231874296
for fid in [1759804371, 1231874296, 661137788]:
    tid, text = elements.get(fid, (None, None))
    print(f"\nObject {fid}:")
    print(text[:400])
