import re
from pathlib import Path

root = Path(r"c:\CodeKuliah\prospector-journey")

guids_to_check = [
    '326bfa1eeb8ba014bb4cdb3ff2832049',
    '8a35f6cfbfba9b548aaa00d52cfe8a50',
    '4d2dd7193bf540644aa6c6c52d7b24c0',
    'cb2acaab03b675c4fb63607539da19b0',
    '83e4e6cca11330d4088d729ab4fc9d9f',
    'a2a0d931eebd6a1479e56fc82c3b78db',
    '1a0fcbc1bc065984ca21139c0f33b62e',
    'a9b6f002ed24e2b41a299c7565b5efab',
    '0cd44c1031e13a943bb63640046fad76',
    'dc42784cf147c0c48a680349fa168899',
    '7951c64acb0fa62458bf30a60089fe2d',
]

results = {}

# Search Assets and Library/PackageCache
search_dirs = [root / "Assets", root / "Packages", root / "Library" / "PackageCache"]

for sdir in search_dirs:
    if not sdir.exists(): continue
    for meta in sdir.rglob("*.meta"):
        try:
            txt = meta.read_text(encoding='utf-8', errors='ignore')
            for g in guids_to_check:
                if g in txt:
                    results[g] = str(meta)
        except Exception:
            pass

for g in guids_to_check:
    print(f"GUID {g} -> {results.get(g, 'NOT FOUND')}")
