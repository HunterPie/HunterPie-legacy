"""
    Script to scan all HunterPie localization files, find missing translation strings and add them automatically
    Author: Haato
"""

# TODO: Rewrite all this in a more readable way

import xml.etree.ElementTree as XML
import os
import requests
import dotenv
import time

dotenv.load_dotenv(".env")

class TranslationMerger():
    def __init__(self):
        self.BaseStrings: XML.ElementTree = XML.parse("en-us.xml")
        self.CurrentLocalizationFile: dict = {}
        self.missing = []
        self.name = ""

    def OpenAndFindMissing(self):
        for loc_file in os.listdir():
            self.missing.clear()
            # Skip non xml files
            if (not loc_file.endswith("xml")): continue
            if (loc_file == "en-us.xml"): continue

            self.name = loc_file
            currentLocFile = XML.parse(loc_file)
            self.CurrentLocalizationFile = {}

            for elem in currentLocFile.iter():
                if (elem.attrib.get("Name") == None): 
                    self.CurrentLocalizationFile[elem.tag] = "TAG"
                else:
                    self.CurrentLocalizationFile[elem.attrib["ID"]] = elem.attrib["Name"]
            Comment = []
            Content = []
            for elem in self.BaseStrings.iter():
                elem: XML.Element = elem
                if (elem.attrib.get("Name") == None): 
                    if (self.CurrentLocalizationFile.get(elem.tag) == None):
                        self.missing.append(f"- MISSING NODE: {elem.tag}\n")
                else:
                    if (self.CurrentLocalizationFile.get(elem.attrib["ID"]) == None):
                        self.missing.append(f"- {elem.attrib['ID']}\n")
            self.SendWebhook()
            time.sleep(2)

    def SendWebhook(self):
        payload = {
            "username": "HunterPie - Localization",
            "content": "",
            "embeds": [
                {
                    "title": self.name,
                    "description": f"```diff\n{''.join(self.missing)[0: 2010]}```"
                }
            ]
        }
        webhook = requests.post(os.environ["Webhook"], json = payload)
        print(webhook)

if __name__ == "__main__":
    Merger = TranslationMerger()
    Merger.OpenAndFindMissing()