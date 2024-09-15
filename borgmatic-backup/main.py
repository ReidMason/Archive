from borgmaticConfigHandler import compile_configs
from crontabBuilder import *
import os
from config import RAW_CONFIG_DIR, LOG_PATH, BORG_CACHE_DIR, SEND_ADDRESS, PASSWORD, RECIPIENT
from utils import ensure_directory_exists, fix_perms
from notifier import EmailSender
import json

from notifier import EmailSender
from email.mime.text import MIMEText


def ensure_required_directories_exist():
    print("Checking required directories")
    ensure_directory_exists(RAW_CONFIG_DIR)
    ensure_directory_exists(LOG_PATH)
    ensure_directory_exists(BORG_CACHE_DIR)
    init_example_config()

def init_example_config():
    example_conf_location = os.path.join(RAW_CONFIG_DIR, "_example.yml")
    os.system(f"generate-borgmatic-config -d {example_conf_location}")
    fix_perms(example_conf_location)

def create_config_file():
    with open("/config.json", "w") as f:
        config = {
            "send_address": SEND_ADDRESS,
            "password": PASSWORD,
            "recipient": RECIPIENT
        }
        json.dump(config, f)

if __name__ == "__main__":
    create_config_file()
    ensure_required_directories_exist()
    compile_configs()