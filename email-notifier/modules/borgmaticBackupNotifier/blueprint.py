import os
import time
import utils
from email.mime.text import MIMEText
from flask import Blueprint, request, jsonify
from mailSender import EmailSender

borgmatic_backup_notifier = Blueprint('borgmatic_backup_notifier', __name__)
ENDPOINT_PREFIX = "borgmatic_backup_notifier"


@borgmatic_backup_notifier.route(f'/{ENDPOINT_PREFIX}/send_notification_email', methods = ["POST"])
def borgmatic():
    if not utils.verify_auth_token(request.headers):
        return "Access denied, invalid auth token.", 403

    # Backup encountered an error
    if isinstance(request.json, dict) and request.json.get("error", False):
        data = request.json
        backup_name = utils.get_filename_from_path(data.get('config_path'))

        data = {
            "backup_successful": False,
            "backup_name"      : backup_name,
            "message"          : data.get('message').replace("\n", "<br />\n"),
            "stderr"           : data.get('stderr').replace("\n", "<br />\n")
        }

    # Backup was successful
    else:
        data = request.json[0]
        backup_name = utils.get_filename_from_path(data.get('repository').get('location'))
        backup_duration = data.get('archive').get('duration')
        total_size = data.get('cache').get('stats').get('unique_csize')

        data = {
            "backup_successful"        : True,
            "backup_name"              : os.path.basename(os.path.normpath(data.get('repository').get('location'))),
            "backup_duration_formatted": time.strftime('%H:%M:%S', time.gmtime(backup_duration)),
            "formatted_size"           : utils.format_file_size(total_size)
        }

    # Build out the email and send it
    template = utils.load_email_template("BorgBackup.html")
    email_body = MIMEText(template.render(data = data), 'html')

    EmailSender().send_email(f"Backup complete: {backup_name}", email_body)

    return "Borgmatic backup email notification sent."
