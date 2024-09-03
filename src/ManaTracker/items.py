from ManaTracker.config import Config

class Items:

    def __init__(self, config: Config):
        self.display_width = config.layout.icon_size
        self.display_height = config.layout.icon_size

        self.sheet_rows = config.layout.sheet_rows
        self.sheet_cols = config.layout.sheet_cols
        self.sheet_item_size = config.layout.sheet_item_size
        self.sheet_width = config.layout.sheet_width
        self.sheet_height = config.layout.sheet_height

        self.items = config.items

        self.scale_width = self.sheet_width * (self.display_width / self.sheet_item_size)
        self.scale_height = self.sheet_height * (self.display_height / self.sheet_item_size)

    def get_icon_loc(self, enum: int):
        row, col = self.items[str(enum)]['page']
        x_offset = ((col - 1) * self.sheet_item_size) * (self.display_width / self.sheet_item_size)
        y_offset = ((row - 1) * self.sheet_item_size) * (self.display_height / self.sheet_item_size)
        return x_offset, y_offset
