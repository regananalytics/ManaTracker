from dash import Dash, dcc, html, Input, Output
import threading

from ManaTracker.config import CONFIG
from ManaTracker.client import run_client
from ManaTracker.state import State

state = State()

class ManaTracker(Dash):
    host: str = CONFIG.app.host
    port: int = CONFIG.app.port
    debug: bool = CONFIG.app.debug

    def run(self, *args, **kwargs):
        client_thread = threading.Thread(
            target=run_client, 
            args=[state],
            daemon=True
        )
        client_thread.start()
        super().run(
            *args, 
            host=kwargs.pop("host", None) or self.host, 
            port=kwargs.pop("port", None) or self.port, 
            debug=kwargs.pop("debug", None) or self.debug, 
            **kwargs
        )


app = ManaTracker(__name__, assets_folder='assets')
app.title = 'ManaTracker - Resident Evil Randomizer'

server = app.server

items = []
layout_div = []
for row_items in CONFIG.layout.item_grid:
    row_div = []
    for item in row_items:
        row_div.append(
            html.Div(
                [
                    # Image
                    html.Img(
                        id=f"item_image_{item}",
                        src=app.get_asset_url(f'{item}.png'),
                        height=CONFIG.layout.icon_size, 
                        width=CONFIG.layout.icon_size,
                        style={
                            'opacity': 100,
                            'filter': f'grayscale({0}%)',
                        },
                    ),
                    # Label
                    html.Div(
                        (
                            CONFIG.layout.custom_labels.get(item, None) 
                            or CONFIG.assets[str(item)]['text']
                            if item in CONFIG.layout.labeled_items
                            else ''
                        ),
                        id=f'item_label_{item}',
                        style = {
                            'position': 'absolute',
                            'bottom': CONFIG.layout.label_pos_bottom,
                            'right': CONFIG.layout.label_pos_right,
                            'color': CONFIG.layout.label_color,
                        },
                    ),
                ], 
                style = {
                    'position': 'relative', 
                    'display': 'inline-block', 
                    'padding': CONFIG.layout.icon_padding,
                },
            )
        ),
        items.append(item)
    layout_div.append(html.Div(row_div))
layout_div.append(
    dcc.Interval(
        id="grid-update",
        interval=int(CONFIG.layout.interval),
        n_intervals=0,
    )
)

app.layout = html.Div(
    layout_div,
    style={
        'backgroundColor': CONFIG.layout.background_color,
        'padding': CONFIG.layout.background_padding,
    }
)


_img_styles = [
    {
        'opacity': CONFIG.layout.inactive_opacity,
        'filter': f'grayscale({CONFIG.layout.inactive_gray}%)',
    },
    {
        'opacity': 100,
        'filter': 'grayscale(0%)',
    },
]


@app.callback(
    [Output(f"item_image_{item}", "style") for item in items],
    Input("grid-update", "n_intervals"),
)
def item_image_state(interval):
    # return [_img_styles[state.get_item_state(i)] for i in items]
    ret = []
    for i in items:
        s = state.get_item_state(i)
        ret.append(_img_styles[s])
    return ret
