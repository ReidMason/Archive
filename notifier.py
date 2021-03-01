import smtplib
import sys
from datetime import datetime
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
import os
import json

class EmailSender:
    def __init__(self):
        with open("/config.json", "r") as f:
            config = json.load(f)
            self.SEND_ADDRESS = config.get('send_address')
            self.PASSWORD = config.get('password')
            self.RECIPIENT = config.get('recipient')

    def send_email(self, subject: str, body: MIMEText):
        # Create the connection to the mail server
        with smtplib.SMTP('smtp.gmail.com', 587) as server:
            server.starttls()
            server.login(self.SEND_ADDRESS, self.PASSWORD)

            msg = self.generate_message(subject)
            msg.attach(self.generate_message_body(body))

            server.sendmail(self.SEND_ADDRESS, self.RECIPIENT, msg.as_string())


    def generate_message_body(self, body: MIMEText):
        # Add the message body
        msg_body = MIMEMultipart('alternative')
        msg_body.attach(body)
        return msg_body

    def generate_message(self, subject: str):
        # Create the header for the email with the subject to and from etc
        msg = MIMEMultipart('related')
        msg['Subject'] = f"{subject} {datetime.now().strftime('%d-%m-%Y %H:%M:%S')}"
        msg['From'] = self.SEND_ADDRESS
        msg['To'] = self.RECIPIENT
        return msg


if __name__ == '__main__':
    email_subject = sys.argv[1]
    email_body = MIMEText(sys.argv[2])

    EmailSender().send_email(email_subject, email_body)