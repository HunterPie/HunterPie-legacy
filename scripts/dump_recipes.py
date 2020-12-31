from requests import request
import pandas
import xml
from io import StringIO
import math

item_data_url: str = "https://raw.githubusercontent.com/gatheringhallstudios/MHWorldData/master/source_data/items/item_base.csv"
crafting_data_url: str = "https://raw.githubusercontent.com/gatheringhallstudios/MHWorldData/master/source_data/items/item_combination_list.csv"

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
        if (row['subcategory'] in ('account', 'appraisal', 'trade') or row['category'] in ('hidden')):
            continue

        if (math.isnan(row['carry_limit'])):
            continue

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
        return type(material) == str and material in item_data

    buffer = StringIO(raw_csv)

    df = pandas.read_csv(buffer)

    for idx, row in df.iterrows():
        if (row['result'] not in item_data):
            continue
        
        output_id = item_data[row['result']]
        mats_required = (
            item_data[row['first']] if isMaterialValid(row['first']) else 0,
            item_data[row['second']] if isMaterialValid(row['second']) else 0
        )
        output_amount = row['quantity']
        crafting_data[output_id] = {
            "mats": mats_required,
            "output": output_amount
        }

    return True

if __name__ ==  "__main__":
    request_item_data()
    request_crafting_data()
    print(crafting_data)