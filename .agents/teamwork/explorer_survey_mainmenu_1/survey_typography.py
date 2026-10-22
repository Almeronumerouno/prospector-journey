import json

with open(r"c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_1\detailed_ui_hierarchy.json", "r", encoding="utf-8") as f:
    nodes = json.load(f)

print(f"{'GameObject':<28} | {'Font Size':<10} | {'Color':<24} | {'Text':<35}")
print("-" * 105)
for n in nodes:
    for c in n["components"]:
        if c.get("name") == "TextMeshProUGUI":
            col = c.get("fontColor")
            col_str = f"({col[0]:.2f}, {col[1]:.2f}, {col[2]:.2f}, {col[3]:.2f})" if col else "None"
            txt = c.get("text", "").replace("\n", " ")
            if len(txt) > 33: txt = txt[:30] + "..."
            print(f"{n['name']:<28} | {str(c.get('fontSize')):<10} | {col_str:<24} | {txt:<35}")
