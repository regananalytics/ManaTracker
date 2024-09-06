from random import random
from ManaTracker.config import Config

class State:

    def __init__(self, config: Config):
        self.config = config
        self.item_enums = sorted(
             [
                item
                for row in self.config.layout.item_grid
                for item in row
            ]
        )
        self.item_states = {k:ItemState(k) for k in self.item_enums}
        self._got_update: bool = False

    def get_item_state(self, enum) -> bool:
        return 1 if not self._got_update or self.item_states[enum].state else 0
    
    async def callback(self, message: dict):
        # Get inventory
        self._got_update = True
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


class ItemState:

    def __init__(self, enum: int):
        self.enum = enum
        self.quantity: int = 0
    
    @property
    def state(self) -> bool:
        return self.quantity > 0
