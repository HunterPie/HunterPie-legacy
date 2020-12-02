"""
    Script to scan all HunterPie localization files, find missing translation strings and add them automatically
    Author: Haato
"""

import xml.etree.ElementTree as XML
import os

class TranslationMerger():
    def __init__(self):
        self.BaseStrings: XML.ElementTree = XML.parse("en-us.xml")
        self.CurrentLocalizationFile: dict = {}

    def OpenAndFindMissing(self):
        for loc_file in os.listdir():
            # Skip non xml files
            if (not loc_file.endswith("xml")): continue
            if (loc_file == "en-us.xml"): continue

            print(f"\n==== {loc_file} ====")
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
                        print(f"- MISSING NODE: {elem.tag}")
                else:
                    if (self.CurrentLocalizationFile.get(elem.attrib["ID"]) == None):
                        print(f"- {elem.attrib['ID']}")

if __name__ == "__main__":
    Merger = TranslationMerger()
    Merger.OpenAndFindMissing()