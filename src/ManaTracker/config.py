from __future__ import annotations
import json
import logging
from pathlib import Path
import yaml

logger = logging.getLogger(__name__)

REQUIRED_CONFIG_FILES = {
    # "cfg.yaml",
    # "items.json",
    # "items.png",
    "mem.yaml",
}

rel_cfg_dirs = [
    "../../cfg/", # Dev
    "../../../cfg/", # Build
]


class Config:

    def __init__(self, args):
        self.game: str = args.game.lower()

        self.host: str = args.host
        self.port: int = args.port
        self.debug: bool = args.debug

        self.state_csv = args.csv

        # Verify game config exists in config directory and load
        if args.cfg is not None:
            cust_cfg_dir = Path(args.cfg).resolve()

            self._is_game_cfg(self.game, cust_cfg_dir)
            self.cfg_dir = cust_cfg_dir
        else:
            # Try default paths
            for rel_path in rel_cfg_dirs:
                full_path = Path(rel_path).resolve()
                if full_path.exists():
                    try:
                        self._is_game_cfg(self.game, full_path)
                        break
                    except:
                        continue
            self.cfg_dir = full_path

        if args.verbose:
                print(f"Config Directory: {cust_cfg_dir}")

        self.game_cfg_dir = self.cfg_dir / self.game
        self._layout_cfg_file = self.game_cfg_dir / 'cfg.yaml'
        self._items_cfg_file = self.game_cfg_dir / 'items.json'

        self._items = None
        self._layout = None
        self._sheet = self.game_cfg_dir / 'items.png'
        
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
    
    @property
    def sheet(self):
        return self._sheet

    def _is_game_cfg(self, game: str, cfg_dir: Path | None = None):
        cfg_dir = cfg_dir
        if not cfg_dir.exists():
            raise ValueError(f'The config directory could not be found: {cfg_dir.resolve()}')
        game_cfg_dir = cfg_dir / game
        if not game_cfg_dir.exists():
            raise ValueError(f'A game config matching "{game}" could not be found in config directory {cfg_dir.resolve()}')
        missing = REQUIRED_CONFIG_FILES - {f.name for f in game_cfg_dir.iterdir()}
        if missing:
            raise ValueError(f'The game config is missing required config files: {missing}')


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
        self.sheet_file:         str = layout_config.get('SHEET_FILE', 'items.png')
        self.sheet_cols:         int = layout_config.get('SHEET_COLS', None)
        self.sheet_rows:         int = layout_config.get('SHEET_ROWS', None)
        self.sheet_item_size:    int = layout_config.get('SHEET_ITEM_SIZE', None)
        self.sheet_width:        int = layout_config.get('SHEET_WIDTH', None)
        self.sheet_height:       int = layout_config.get('SHEET_HEIGHT', None)
