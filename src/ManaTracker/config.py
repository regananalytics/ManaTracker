import json
import os
from pathlib import Path
import yaml

import ManaTracker

ROOT_DIR = Path(ManaTracker.__file__).parent
CONF_DIR = ROOT_DIR

MANA_LAYOUT_FILE = os.getenv('MANA_LAYOUT_FILE', ROOT_DIR.parent / 'config.yaml')
MANA_ASSET_MAP = os.getenv('MANA_ASSET_MAP', ROOT_DIR / 'assets' / 'items.json')

layout_config: dict = yaml.load(
    open(MANA_LAYOUT_FILE, 'r'), 
    Loader=yaml.SafeLoader
)

asset_map = json.load(open(MANA_ASSET_MAP, 'r'))

class AppConf:
    host:               str =  layout_config.get("HOST", "localhost")
    port:               int =  layout_config.get("PORT", 8080)
    debug:             bool =  layout_config.get("DEBUG", False)

class LayoutConf:
    # Update Interval
    interval:           int =   layout_config.get('UPDATE_INTERVAL', 1000)
    # App Layout
    background_color:   str =   layout_config.get('BACKGROUND_COLOR', 'black')
    background_padding: str =   layout_config.get('BACKGROUND_PADDING', '10px')
    # Item Grid
    item_grid:         list =   layout_config.get('ITEM_LAYOUT', [])
    labeled_items:      list =  layout_config.get("LABELED_ITEMS", [])
    custom_labels:      dict =  layout_config.get('CUSTOM_LABELS', {})
    # Item Properties
    inactive_opacity:   float = layout_config.get('INACTIVE_OPACITY', 0.5)
    inactive_gray:      int =   layout_config.get('INACTIVE_GRAY', 50)
    icon_size:          int =   layout_config.get("ICON_SIZE", 64)
    icon_padding:       str =   layout_config.get("ICON_PADDING", '0px')
    label_pos_bottom:   str =   layout_config.get('ICON_LABEL_POS_BOTTOM', '0px')
    label_pos_right:    str =   layout_config.get('ICON_LABEL_POS_RIGHT', '0px')
    label_color:        int =   layout_config.get('ICON_LABEL_COLOR', 'white')



class Config:
    app:    AppConf = AppConf()
    layout: LayoutConf = LayoutConf()
    assets: dict = asset_map

CONFIG = Config()