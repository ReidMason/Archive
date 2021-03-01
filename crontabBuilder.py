import os
from crontab import CronTab
from config import BACKUP_CRON_PATH

def ensure_crontab_exists():
    print("Creating backup-cron file")
    if not os.path.exists(BACKUP_CRON_PATH):
        open(BACKUP_CRON_PATH, 'a').close()

def remove_crontab():
    print(f"Removing old crontab at {BACKUP_CRON_PATH}")
    if os.path.exists(BACKUP_CRON_PATH):
        os.remove(BACKUP_CRON_PATH)

def esure_path_exists(filepath):
    if not os.path.exists(filepath):
        os.makedirs(filepath)

def add_cron_job(time, command, log_path):
    ensure_crontab_exists()
    
    with CronTab(user='root', tabfile=BACKUP_CRON_PATH) as cron:
        job = cron.new(command=f'{command} > "{log_path}" 2>&1')
        job.setall(time)
    print('Added task to cron file')

