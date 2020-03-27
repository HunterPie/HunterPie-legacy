import hashlib
import json
import os

class Hasher():
    def __init__(self):
        self.files = os.listdir()
        self.hashes = {}

    def removeFiles(self):
        _files = [
            "config.json",
            "hash.py",
            ".git",
			"Update.exe",
            "hashes.json",
            "map_backup"
        ]
        for f in _files:
            if f in self.files:
                self.files.remove(f)

    def tryHash(self, file):
        normalFile = True
        if (file.endswith("dll") or file.endswith("exe") or file.endswith("png") or file.endswith("cn.xml")):
            normalFile = False
        with open(file, "rb",) as f:
            if normalFile:
                bytes = f.read().replace(b"\r", b"")
            else:
                bytes = f.read()
            self.hashes[file] = hashlib.sha256(bytes).hexdigest()

    def hashFiles(self):
        for file in self.files:
            if os.path.isdir(file):
                for subfile in os.listdir(file):
                    if os.path.isdir(f"{file}\\{subfile}"):
                        for sub_subfile in os.listdir(f"{file}\\{subfile}"):
                            if os.path.isdir(f"{file}\\{subfile}\\{sub_subfile}"):
                                for img in os.listdir(f"{file}\\{subfile}\\{sub_subfile}"):
                                    self.tryHash(f"{file}\\{subfile}\\{sub_subfile}\\{img}")
                                    continue
                    else:
                        self.tryHash(f"{file}/{subfile}")
                continue
            else:
                self.tryHash(file)
    
    def dumpHashes(self):
        with open("hashes.json", "w") as output:
            json.dump(self.hashes, output, indent=4)
            output.close()

    def run(self):
        self.removeFiles()
        self.hashFiles()
        self.dumpHashes()

if __name__ == "__main__":
    hasher = Hasher()
    hasher.run()