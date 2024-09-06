from dash import Dash, dcc, html, Input, Patch, Output
from pathlib import Path
import threading

from ManaTracker.config import Config
from ManaTracker.client import run_client
from ManaTracker.items import Items
from ManaTracker.state import State


class ManaTracker:

    def __init__(self, args):
        self.config = Config(args)
        self.items = Items(self.config)

        self.app: App = self._init_app()
        self.server = self.app.server
        self.state = State(self.config)

    def run(self, *args, **kwargs):
        self.app.run(
            self.config.host, 
            self.config.port, 
            self.config.debug, 
            self.state, 
            *args, **kwargs
        )

    def _init_app(self):
        app = App(f'ManaTracker.{self.config.game.upper()}', assets_folder=self.config.game_cfg_dir)
        app.title = f'ManaTracker - {self.config.layout.title}'

        items = []
        layout_div = []
        for row_items in self.config.layout.item_grid:
            row_div = []
            for item in row_items:
                row_div.append(
                    html.Div(
                        [
                            # Image
                            self._item_img(item),
                            # Label
                            html.Div(
                                (
                                    self.config.layout.custom_labels.get(item, None) 
                                    or self.config.items[str(item)]['text']
                                    if item in self.config.layout.labeled_items
                                    else ''
                                ),
                                id=f'item_label_{item}',
                                style = {
                                    'position': 'absolute',
                                    'bottom': self.config.layout.label_pos_bottom,
                                    'right': self.config.layout.label_pos_right,
                                    'color': self.config.layout.label_color,
                                },
                            ),
                        ], 
                        style = {
                            'position': 'relative', 
                            'display': 'inline-block', 
                            'padding': self.config.layout.icon_padding,
                        },
                    )
                ),
                items.append(item)
            layout_div.append(html.Div(row_div))
        layout_div.append(
            dcc.Interval(
                id="grid-update",
                interval=int(self.config.layout.interval),
                n_intervals=0,
            )
        )

        app.layout = html.Div(
            layout_div,
            style={
                'backgroundColor': self.config.layout.background_color,
                'padding': self.config.layout.background_padding,
            }
        )

        _img_styles = [
            {
                'opacity': self.config.layout.inactive_opacity,
                'filter': f'grayscale({self.config.layout.inactive_gray}%)',
            },
            {
                'opacity': 100,
                'filter': 'grayscale(0%)',
            },
        ]

        @app.callback(
            [Output(f"item_img_{item}", "style") for item in items],
            Input("grid-update", "n_intervals"),
        )
        def item_image_state(interval):
            patches = []
            for i in items:
                p = Patch()
                s = self.state.get_item_state(i)
                p.update(_img_styles[s])
                patches.append(p)
            return patches
        
        return app


    def _item_img(self, item):
        if item > 0:
            x_offset, y_offset = self.items.get_icon_loc(item)
            return html.Div(
                id=f"item_img_{item}",
                style={
                    'width': f'{self.items.display_width}px',
                    'height': f'{self.items.display_height}px',
                    'background-image': f'url(assets/{self.config.layout.sheet_file})',
                    'background-position': f'-{x_offset}px -{y_offset}px',
                    'background-size': f'{self.items.sheet_width}px {self.items.sheet_height}px',
                    'background-repeat': 'no-repeat',
                    'display': 'inline-block',
                    'background-size': f'{self.items.scale_width}px {self.items.scale_height}px',
                    'opacity': 100,
                    'filter': f'grayscale({0}%)',
                }
            )
        else:
            return html.Div(
                id=f"item_img_{item}",
                style={
                    'width': f'{self.items.display_width}px',
                    'height': f'{self.items.display_height}px',
                }
            )


class App(Dash):

    def run(self, host, port, debug, state, *args, **kwargs):
        client_thread = threading.Thread(
            target=run_client, 
            args=[state],
            daemon=True
        )
        client_thread.start()
        super().run(host=host, port=port, debug=debug, *args, **kwargs)
