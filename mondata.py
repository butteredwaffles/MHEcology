import sqlite3
import os
import copy
import requests
from io import BytesIO
from PIL import Image

def get_monsters():
    ecodb = sqlite3.connect("Databases/ecology.db")
    ecodb.row_factory = sqlite3.Row
    cursor = ecodb.cursor()
    cursor.execute("SELECT * FROM MonsterEcology")
    return cursor.fetchall()

def check_images_exist(monsters):
    updated_mon = []
    for monster in monsters:
        url = monster['image_url']
        # get rid of quotations, they cause problems
        image_name = url.split('/')[7].replace("%27", "") 
        
        if not os.path.exists("static/images/icons/" + image_name):
            try:
                r = requests.get(url)
                Image.open(BytesIO(r.content)).save("static/images/icons/" + image_name)
                print(f"Downloading icon for {monster['name']}.")
            except Exception:
                print(f"Could not load an icon for {monster['name']}.")
        newmon = dict(monster)
        newmon['image_url'] = image_name
        updated_mon.append(newmon)
    return updated_mon
    
        