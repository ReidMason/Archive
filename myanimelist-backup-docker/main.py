import os
import time
import schedule
from backup import run_backup

if __name__ == '__main__':
    # Run the backup once at startup
    run_backup()

    # Run the backup every day at 7:05 PM
    schedule.every().day.at("19:05").do(run_backup)

    while True:
        # Check if it's time to run any schedules
        schedule.run_pending()
        time.sleep(60)
