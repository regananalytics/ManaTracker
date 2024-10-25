from collections import deque
import csv
from pathlib import Path
import re

from ManaTracker.config import Config

HISTORY_LEN = 1000


class CSVWriter:
    def __init__(self, file_name: Path | str, fieldnames: list | set):
        self.file_name = Path(file_name)
        self.fieldnames = fieldnames

        self.file = open(self.file_name, 'a', newline='')
        self.writer = csv.DictWriter(self.file, fieldnames=self.fieldnames)

        if not self.file_name.exists():
            self.writer.writeheader()

    def append_row(self, data_dict: dict):
        if set(data_dict.keys()) != set(self.fieldnames):
            raise ValueError("Keys of data_dict do not match the expected fieldnames")
        self.writer.writerow(data_dict)

    def close(self):
        self.file.close()


class State:

    def __init__(self, config: Config):
        self.config = config
        self.csv_out = config.state_csv

        self._csv: CSVWriter = None
        self._got_update: bool = False
        self._message_history: deque = deque(maxlen=HISTORY_LEN)

    
    async def callback(self, message: dict):
        # Process Message from ManaServer
        self._got_update = True
        self._message_history.appendleft(message)


    def write_to_csv(self, message: dict):
        if self.csv_out is not None:
            if self._csv is None:
                fields = set(message.keys())
                self._csv = CSVWriter(self.csv_out, fields)
            self._csv.append_row(message)


class RE1State(State):

    def __init__(self, config: Config):
        super().__init__(config)
        self.item_enums = sorted(
             [
                item
                for row in self.config.layout.item_grid
                for item in row
            ]
        )
        self.inventory = ItemContainer()
        self.item_box = ItemContainer()


    def get_item_state(self, enum) -> bool:
        if not self._got_update:
            return True
        return (
            self.inventory.items[enum] is not None 
            or self.item_box.items[enum] is not None
        )
    

    def get_inventory_quantity(self, enum) -> int:
        return self.inventory.items[enum]
    


    async def callback(self, message: dict):
        self._got_update = True

        inventory_map = {}
        item_box_map = {}
        for k,v in message.items():
            inventory_match = re.search(r'Inventory\[(\d+)\]\.Item', k)
            if inventory_match and v > 0:
                slot = int(inventory_match.group(1))
                inventory_map[slot] = [v, 0]
                continue
            inventory_match = re.search(r'Inventory\[(\d+)\]\.Quantity', k)
            if inventory_match:
                slot = int(inventory_match.group(1))
                if slot in inventory_map:
                    inventory_map[slot][1] = v
                continue
            item_box_match = re.search(r'ItemBox\[(\d+)\]\.Item', k)
            if item_box_match and v > 0:
                slot = int(item_box_match.group(1))
                item_box_map[slot] = [v, 0]
                continue
            item_box_match = re.search(r'ItemBox\[(\d+)\]\.Quantity', k)
            if item_box_match:
                slot = int(item_box_match.group(1))
                if slot in item_box_map:
                    item_box_map[slot][1] = v
                continue
        
        self.inventory.update(list(inventory_map.values()))
        self.item_box.update(list(item_box_map.values()))


class ItemContainer:
    def __init__(self, nitems=256):
        self.nitems = nitems
        self.items = [0]*nitems

    def update(self, map: list[list[int]]):
        self.items = [None]*self.nitems
        for item_quantity in map:
            if self.items[item_quantity[0]] is None:
                self.items[item_quantity[0]] = item_quantity[1]
            else:
                self.items[item_quantity[0]] += item_quantity[1]
