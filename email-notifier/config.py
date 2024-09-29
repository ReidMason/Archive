import os

RECIPIENTS = os.getenv("RECIPIENTS", "").split(',')
SENDER_ADDRESS = os.getenv("SENDER_ADDRESS")
SENDER_PASSWORD = os.getenv("SENDER_PASSWORD")
MAIL_SERVER = os.getenv("MAIL_SERVER")
MAIL_PORT = os.getenv("MAIL_PORT")
AUTH_TOKEN = os.getenv("AUTH_TOKEN")
PORT = os.getenv("PORT", 5004)
