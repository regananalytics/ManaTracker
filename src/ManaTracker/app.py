from dash import Dash, dcc, html, Input, Output
import threading

from ManaTracker.config import Config
from ManaTracker.client import run_client
from ManaTracker.state import State


class ManaTracker:

    def __init__(self, args):
        self.game = args.game
        self.cfg_dir = args.cfg
        self.memconf = args.mem
        self.host = args.host
        self.port = args.port
        self.debug = args.debug

        self.config = Config(self.game, self.cfg_dir)

        self.app: App = self._init_app()
        self.server = self.app.server
        self.state = State(self.config)

    def run(self, *args, **kwargs):
        self.app.run(self.host, self.port, self.debug, self.state, *args, **kwargs)

    def _init_app(self):
        self.app = App(__name__)
        self.app.title = f'ManaTracker - {self.config.layout.title}'

        items = []
        layout_div = []
        for row_items in self.config.layout.item_grid:
            row_div = []
            for item in row_items:
                row_div.append(
                    html.Div(
                        [
                            # Image
                            html.Img(
                                id=f"item_image_{item}",
                                src=self.app.get_asset_url(f'{item}.png'),
                                height=self.config.layout.icon_size, 
                                width=self.config.layout.icon_size,
                                style={
                                    'opacity': 100,
                                    'filter': f'grayscale({0}%)',
                                },
                            ),
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

        self.app.layout = html.Div(
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

        @self.app.callback(
            [Output(f"item_image_{item}", "style") for item in items],
            Input("grid-update", "n_intervals"),
        )
        def item_image_state(interval):
            # return [_img_styles[state.get_item_state(i)] for i in items]
            ret = []
            for i in items:
                s = self.state.get_item_state(i)
                ret.append(_img_styles[s])
            return ret


class App(Dash):

    def run(self, host, port, debug, state, *args, **kwargs):
        client_thread = threading.Thread(
            target=run_client, 
            args=[state],
            daemon=True
        )
        client_thread.start()
        super().run(host=host, port=port, debug=debug, *args, **kwargs)
