from __future__ import annotations
import json
import logging
from pathlib import Path
import yaml

import ManaTracker

logger = logging.getLogger(__name__)


class Config:

    def __init__(self, game: str, cfg_dir: str | None = None):
        self.game = game
        self._cfg_dir = cfg_dir or Path(ManaTracker.__file__).parent / '..' / '..' / 'cfg'
        self._game_cfg = self._cfg_dir / self.game

        self._layout_cfg_file = self._game_cfg / 'cfg.yaml'
        self._items_cfg_file = self._game_cfg / 'items.json'

        self._items = None
        self._layout = None
        
    @property
    def items(self):
        if self._items is None:
            item_cfg = json.load(open(self._items_cfg_file, 'r'))
            self._items: dict = item_cfg
        return self._items

    @property
    def layout(self):
        if self._layout is None:
            layout_cfg = {}
            try:
                if self._layout_cfg_file.is_file():
                    layout_cfg = yaml.load(open(self._layout_cfg_file, 'r'), Loader=yaml.SafeLoader)
            except Exception as e:
                logger.warning(
                    f"Could not load layout config file: {self._items_cfg_file} "
                    f"Using default layout configuration. "
                    f'Exception raised: {e}'
                )
            self._layout = LayoutConfig(layout_cfg)
        return self._layout


class LayoutConfig:
    def __init__(self, layout_config: dict):
        self.title:               str = layout_config.get('TITLE', 'DEFAULT CONFIG')
        # Update Interval
        self.interval:           int = layout_config.get('UPDATE_INTERVAL', 1000)
        # App Layout
        self.background_color:   str = layout_config.get('BACKGROUND_COLOR', 'black')
        self.background_padding: str = layout_config.get('BACKGROUND_PADDING', '10px')
        # Item Grid
        self.item_grid:         list = layout_config.get('ITEM_LAYOUT', [])
        self.labeled_items:     list = layout_config.get("LABELED_ITEMS", [])
        self.custom_labels:     dict = layout_config.get('CUSTOM_LABELS', {})
        # Item Properties
        self.inactive_opacity: float = layout_config.get('INACTIVE_OPACITY', 0.5)
        self.inactive_gray:      int = layout_config.get('INACTIVE_GRAY', 50)
        self.icon_size:          int = layout_config.get("ICON_SIZE", 64)
        self.icon_padding:       str = layout_config.get("ICON_PADDING", '0px')
        self.label_pos_bottom:   str = layout_config.get('ICON_LABEL_POS_BOTTOM', '0px')
        self.label_pos_right:    str = layout_config.get('ICON_LABEL_POS_RIGHT', '0px')
        self.label_color:        int = layout_config.get('ICON_LABEL_COLOR', 'white')
