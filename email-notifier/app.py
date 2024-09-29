from flask import Flask
from modules.borgmaticBackupNotifier.blueprint import borgmatic_backup_notifier
from config import PORT

app = Flask(__name__)

app.register_blueprint(borgmatic_backup_notifier)

if __name__ == '__main__':
    app.run(host = '0.0.0.0', port = PORT, debug = True, ssl_context = "adhoc")
