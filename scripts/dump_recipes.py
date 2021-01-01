from requests import request
import pandas
import xml
from io import StringIO
import math
'''
item_data_url: str = "https://raw.githubusercontent.com/gatheringhallstudios/MHWorldData/master/source_data/items/item_base.csv"
crafting_data_url: str = "https://raw.githubusercontent.com/Haato3o/MHWorldData/items-data/source_data/items/item_combination_list.csv"

item_data: dict = dict()
crafting_data: dict = dict()

def request_item_data() -> bool:
    data: Response = request('GET', item_data_url)
    if (data.ok):
        return load_item_data(data.text)
    else:
        return False

def load_item_data(raw_csv: str) -> bool:
    buffer = StringIO(raw_csv)

    df = pandas.read_csv(buffer)
    
    for idx, row in df.iterrows():

        # filter items if they're not relevant for crafting
        

        

        item_data[row['name_en']] = row['id']

    return True

def request_crafting_data() -> bool:
    data = request('GET', crafting_data_url)
    if (data.ok):
        return load_crafting_data(data.text)
    else:
        return False

def load_crafting_data(raw_csv: str) -> bool:

    def isMaterialValid(material):
        #print(material)
        return type(material) == str and material in item_data

    buffer = StringIO(raw_csv)

    df = pandas.read_csv(buffer)

    for idx, row in df.iterrows():
        if (row['result'] not in item_data):
            print(row['result'])
            continue
        
        output_id = item_data[row['result']]
        mats_required = (
            item_data[row['first']] if isMaterialValid(row['first']) else 0,
            item_data[row['second']] if isMaterialValid(row['second']) else 0
        )
        output_amount = row['quantity']
        crafting_data[output_id] = {
            "mats": mats_required,
            "output": output_amount,
            "out_id": output_id,
            "out_name": row['result']
        }

    return True

def dump_to_xml():
    # Yes, I'm not gonna use some XML library cuz im lazy
    xml_lines = ['<?xml version="1.0" encoding="utf-8" ?>\n', '<Crafting>\n']

    for c_data_i in crafting_data:
        c_data = crafting_data[c_data_i]
        xml_lines.append(
            f'\t<!-- {c_data["out_name"]} -->\n'
        )
        xml_lines.append(
            f'\t<Item Output="{c_data["out_id"]}" OutMultiplier="{c_data["output"]}">\n'
        )
        for material in c_data["mats"]:
            if (material != 0):
                xml_lines.append(
                    f'\t\t<Material Id="{material}" Amount="1"/>\n'
                )
        xml_lines.append("\t</Item>\n")

    xml_lines.append('</Crafting>')

    with open('CraftingData_out.xml', "w") as f:
        f.writelines(xml_lines)
'''

items_make = pandas.read_csv("itemsmake.csv")

def dump_to_xml():
    # Yes, I'm not gonna use some XML library cuz im lazy
    xml_lines = ['<?xml version="1.0" encoding="utf-8" ?>\n', '<Crafting>\n']

    for c_data_i in items_make.iterrows():
        c_data = c_data_i[1]
        xml_lines.append(
            f'\t<Item Output="{c_data["result"]}" OutMultiplier="{c_data["quantity"]}">\n'
        )

        ignore_mats = [137]

        materials = [x for x in (c_data["first"], c_data["second"]) if x not in ignore_mats]

        for material in materials:
            if (material != 0):
                xml_lines.append(
                    f'\t\t<Material Id="{material}" Amount="1"/>\n'
                )
        xml_lines.append("\t</Item>\n")

    xml_lines.append('</Crafting>')

    with open('CraftingData_out.xml', "w") as f:
        f.writelines(xml_lines)

if __name__ ==  "__main__":
    dump_to_xml()