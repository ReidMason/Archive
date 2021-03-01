import os

IS_DEV = False

RAW_CONFIG_DIR = ("" if IS_DEV else "/") + "data/configs/"
COMPILED_CONFIG_DIR = ("" if IS_DEV else "/") + "compiled_configs"
BACKUP_CRON_PATH = "backup-cron"
LOG_PATH = ("" if IS_DEV else "/") + "data/logs/"
BORG_CACHE_DIR = ("" if IS_DEV else "/") + "data/cache/"

PUID = int(os.getenv('PUID', 99))
GUID = int(os.getenv('GUID', 100))

SEND_ADDRESS = os.getenv("SENDER_EMAIL")
PASSWORD = os.getenv("SENDER_EMAIL_PASSWORD")
RECIPIENT = os.getenv("RECIPIENT")