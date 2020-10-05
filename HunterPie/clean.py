# Because Nuget is DUMB

shit_folders = [
    "cs", "de", "es", "fr", "it", "ja", "ko", "pl", "pt-BR", "ru", "tr", "zh-Hans", "zh-Hant"
]
ignore_dlls = [
    "Newtonsoft.Json.dll"
]
clean = [
    ".pdb"
]

import os
import shutil

for shit in os.listdir():
    if (shit.endswith(".dll") and shit not in ignore_dlls):
        shutil.move(shit, f"libs\\{shit}")
    if (shit in shit_folders):
        shutil.rmtree(shit)
    if (shit[-4:] in clean):
        os.remove(shit)
