import re

SCENE_PATH = 'Assets/Prospector-journey/Tutorial/Tutorial scene.unity'
with open(SCENE_PATH, 'r', encoding='utf-8') as f:
    text = f.read()

terms = ['ProspectorInventory', 'Prospector_Inventory', 'tool-shovel', 'Gold_Pan', 'shovel', 'Gold_pan', 'gold_pan', 'InventoryCanvas', 'ToastNotification', 'Stored_Items_Container', 'XRRestingProp']

print("=== CHECKING INVENTORY & TOOLS IN TUTORIAL SCENE ===")
for t in terms:
    count = len(re.findall(re.escape(t), text, re.IGNORECASE))
    print(f"  '{t}': {count} occurrences")

