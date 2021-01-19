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

try:
    os.mkdir("libs")
except FileExistsError:
    pass
    
for shit in os.listdir():
    if (shit.endswith(".dll") and shit not in ignore_dlls):
        shutil.move(shit, f"libs/{shit}")
    if (shit in shit_folders):
        shutil.rmtree(shit)
    if (shit[-4:] in clean):
        os.remove(shit)

for dll in os.listdir("libs"):
    if not dll.endswith(".dll"):
        os.remove(os.path.join("libs", dll))
