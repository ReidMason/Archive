from PIL import Image
from image_helper import tile_is_blank


class GameTile:
    def __init__(self, image: Image.Image) -> None:
        self.image = image
        self.is_blank = tile_is_blank(self.image)
