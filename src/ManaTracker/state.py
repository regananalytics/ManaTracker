from collections import deque
import csv
from pathlib import Path

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
        self.item_states = {k:ItemState(k) for k in self.item_enums}

    def get_item_state(self, enum) -> bool:
        return 1 if not self._got_update or self.item_states[enum].state else 0
    
    async def callback(self, message: dict):
        super().callback(message)
        # Get inventory
        frame_counter = [v for k,v in message.items() if "FrameCounter" in k]
        frame = frame_counter[0] if frame_counter != [] else None
        if frame == 0:
            self.reset()
        inventory = {
            k:v for k,v in message.items()
            if 'Inventory' in k and 'Item' in k
        }
        for key, enum in inventory.items():
            if enum == 0:
                continue
            if enum in self.item_enums:
                key_num = int(key.split('[')[1].split(']')[0])
                if key_num < 8:
                    quantity = message[f'Inventory[{key_num}].Quantity']
                    self.item_states[enum].quantity = quantity

    def reset(self):
        for state in self.item_states.values():
            state.reset()


class ItemState:

    def __init__(self, enum: int):
        self.enum = enum
        self.quantity: int = 0
    
    @property
    def state(self) -> bool:
        return self.quantity > 0

    def reset(self):
        self.quantity = 0
