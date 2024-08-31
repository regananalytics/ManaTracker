from pathlib import Path
from PIL import Image

ICON_PAGE = Path(__file__).parent / "icon_page.png"

ICON_ENUM = [*range(1, 8), *range(10, 89), *range(90, 130)]

def gen_icons(page_path: Path, rows: int, cols: int):
    page = Image.open(page_path)

    page_width, page_height = page.size
    width = page_width // cols
    height = page_height // rows

    n = 0
    for i in range(rows):
        for j in range(cols):
            top = i * height
            left = j * width
            right = left + width
            bottom = top + height
            icon = page.crop((left, top, right, bottom))
            icon.save(page_path.parent / f'{ICON_ENUM[n]}.png')
            n += 1
            

if __name__ == "__main__":
    gen_icons(ICON_PAGE, 18, 7)
