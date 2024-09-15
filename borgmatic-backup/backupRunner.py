import os
import sys
import json
from config import COMPILED_CONFIG_DIR
from pprint import pprint
import subprocess
from utils import format_file_size
import time
from notifier import EmailSender
from email.mime.text import MIMEText

PATH_TO_BORGMATIC = "/usr/local/bin/borgmatic"
backup_name = sys.argv[1]
backup_name = backup_name.rstrip(".yml")

def get_config_location():
    return os.path.join(COMPILED_CONFIG_DIR, f"{backup_name}.yml")
     
def run_borg_backup():
    config_location = get_config_location()
    start_time = time.time()
    backup = subprocess.run([PATH_TO_BORGMATIC, "--verbosity", "1", "--files", "--config", config_location])
    time_elapsed = time.time() - start_time
    return {
        "time_elapsed": time_elapsed,
        "backup_successful": backup.returncode == 0
        }

def format_seconds(seconds: float):
    return time.strftime('%H:%M:%S', time.gmtime(seconds))

def get_backup_report():
    # Check the backup
    config_location = get_config_location()

    raw_backup_info = subprocess.check_output([PATH_TO_BORGMATIC, "info", "--json", "--config", config_location]).decode("utf-8")

    backup_info = json.loads(raw_backup_info)[0].get('cache').get('stats')

    return {
        "total_size": backup_info.get('total_size'),
        "compressed_size": backup_info.get('total_csize'),
        "deduplicated_size": backup_info.get('unique_csize')
    }

def do_backup():
    backup_report = run_borg_backup()
    backup_stats = get_backup_report()

    backup_successful = backup_report.get("backup_successful")
    backup_duration = format_seconds(backup_report.get("time_elapsed"))
    size_on_disk = format_file_size(backup_stats.get('deduplicated_size'))

    email_body = MIMEText(f"""
                            <html>

<body>
    <h1 style="color: {"#059669" if backup_successful else "#DC2626"}">{backup_name.title()} backup {"successful" if backup_successful else "failed"}</h1>
    {
        f'''<table style="display: inline-block; font-size: large;">
        <tr>
            <td style="width: 145px;">
                Duration:
            </td>
            <td>
                {backup_duration}
            </td>
        </tr>
        <tr>
            <td>
                Size on disk:
            </td>
            <td>
                {size_on_disk}
            </td>
        </tr>
    </table>''' if backup_successful else ""}
</body>

</html>
            """, 'html')

    EmailSender().send_email(f"{backup_name.title()} backup {'successful' if backup_successful else 'failed'}", email_body)
    

do_backup()
