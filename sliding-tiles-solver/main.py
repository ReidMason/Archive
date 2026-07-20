from driver import Driver
import mss
from image_helper import get_game_tile_images
from models.game_board import GameBoard
from models.game_tile import GameTile
from utils import get_difficulty


def get_current_game_state(sct: mss.mss, columns: int) -> GameBoard:
    boundaries, image_tiles = get_game_tile_images(sct, columns)
    game_board = GameBoard([GameTile(x) for x in image_tiles])
    game_board.set_grid_coords(boundaries)
    return game_board


def get_completed_game_board(current_game_board: GameBoard) -> GameBoard:
    completed_image_tiles = current_game_board.find_completed_image_tiles()
    return GameBoard([GameTile(x) for x in completed_image_tiles])


def main(difficulty: int):
    columns = get_difficulty(difficulty)
    puzzle_grid = f'{difficulty}x{difficulty}'
    # Open the puzzle
    puzzle_url = f'https://slidingtiles.com/en/puzzle/play/food/3665-tray-of-poultry-eggs#{puzzle_grid}'
    with Driver('data/webdrivers/geckodriver/geckodriver', fullscreen=True) as driver:
        driver.go(puzzle_url)
        driver.click('#start-solve-button')

        # Open the screenshot utility
        with mss.mss() as sct:
            # This is the current and completed board states
            game_board = get_current_game_state(sct, columns)
            complete_game_board = get_completed_game_board(game_board)

            # Click the 
            game_board.click_grid(9, 8)

            input("Waiting...")


if __name__ == '__main__':
    difficulty = 9
    main(difficulty)
