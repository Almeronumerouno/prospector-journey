import re

scene_path = r"c:\CodeKuliah\prospector-journey\Assets\Prospector-journey\Mainmenu\Mainmenu scene.unity"

with open(scene_path, "r", encoding="utf-8") as f:
    content = f.read()

blocks = re.split(r'\n--- !u!', content)
elements = {}
for block in blocks:
    if not block.strip(): continue
    m = re.match(r'^(\d+) &(\d+)', block)
    if m: elements[int(m.group(2))] = (m.group(1), block)

# List all GameObjects with TMP components
print("--- ALL TMP_Text in Scene ---")
for fid, (tid, text) in elements.items():
    if tid == '1': # GameObject
        m_name = re.search(r'm_Name: (.*)', text)
        gname = m_name.group(1).strip() if m_name else "<unknown>"
        comp_ids = [int(c) for c in re.findall(r'- component: \{fileID: (\d+)\}', text)]
        for cid in comp_ids:
            if cid in elements:
                ctid, ctext = elements[cid]
                if 'm_fontSize:' in ctext:
                    m_fs = re.search(r'm_fontSize: ([\d\.]+)', ctext)
                    m_text = re.search(r'm_text: (.*)', ctext)
                    m_col = re.search(r'm_fontColor: \{(.*?)\}', ctext)
                    print(f"GO: '{gname}' (ID: {fid}) -> fontSize: {m_fs.group(1) if m_fs else '?'} | text: '{m_text.group(1) if m_text else '?'}'")
