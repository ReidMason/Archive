import json
import os

import requests
from datetime import datetime
from colorama import Style, Fore, init
from bs4 import BeautifulSoup
from config import BACKUP_PATH, NUM_BACKUPS
from typing import Optional

# Initialise colorama
init()


def log(text: str, fore: Optional[str] = '', back: Optional[str] = '') -> None:
    """ Logs text with a specified style using colorama styles

    :param text: The message to log
    :param fore: The foreground colour
    :param back: The background colour
    :return: None
    """
    timestamp = datetime.today().strftime('%d-%m-%Y %H:%M:%S')
    print(f"{timestamp} {fore + back + text + Style.RESET_ALL}")


def get_date() -> str:
    """ Gets the current date and time.

    :return: The current date and time. Format: d-m-y h:m:s
    """
    return str(datetime.now()).replace(':', '.')


def get_anime_list_data(username: str) -> Optional[dict]:
    """ Gets the specified users anime list

    :param username: The username of the user who's list to retrieve
    :return: A dictionary containing their anime list
    """
    log(f'Obtaining anime list data for user {username}')
    # Get the html from the webpage
    url = f'https://myanimelist.net/animelist/{username}'
    r = requests.get(url)
    soup = BeautifulSoup(r.content, 'lxml')

    # Extract the anime list from the html
    anime_list_data = soup.find('table', {'class': 'list-table'}).get('data-items')

    return json.loads(anime_list_data)


def get_recent_backup(username: str) -> dict:
    log('Loading recent backup')
    path = os.path.join(BACKUP_PATH, username)

    # Check that the backup folder exists
    if not os.path.exists(path):
        log(f"Creating backup folder for '{username}'", Fore.YELLOW)
        os.mkdir(path)
        return {}

    # Find all backup files
    backups = os.listdir(os.path.join(path))

    # When there are no backups
    if len(backups) < 1:
        return {}

    # Get the data from the most recent backup
    with open(os.path.join(path, backups[-1]), 'r') as f:
        return json.load(f)


def backup_anime_list(data: dict, username: str) -> None:
    """ Saves the new anime data as a new backup and prunes backups.

    :param data: The new anime data to save
    :param username: The username of the user the data belongs to
    :return: None
    """

    # Create save path
    filename = f"MyAnimeListBackup {get_date()} - {username}.json"
    filepath = os.path.join(BACKUP_PATH, username, filename)

    # Save the file
    log(f'Saving backup to "{filepath}"')
    with open(filepath, 'w') as f:
        json.dump(data, f)
        os.chmod(filepath, 0o777)

    # Remove old backups
    prune_old_backups(username)


def prune_old_backups(username: str) -> None:
    """ Removes backups until number of backups is back within the limit

    :param username: The username of the target user
    :return: None
    """
    log(f'Checking for old backups')
    path = os.path.join(BACKUP_PATH, username)
    backups = os.listdir(path)
    # Remove every backup over the NUM_BACKUPS limit
    for i in range(0, len(backups) - NUM_BACKUPS):
        log(f'Removing old backup')
        # Remove the last backup in the list
        os.remove(os.path.join(path, backups.pop()))
