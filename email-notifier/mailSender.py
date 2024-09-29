import smtplib
from datetime import datetime
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from config import SENDER_ADDRESS, SENDER_PASSWORD, RECIPIENTS, MAIL_SERVER, MAIL_PORT


class EmailSender:
    def send_email(self, subject: str, body: MIMEText):
        # Create the connection to the mail server
        with smtplib.SMTP(MAIL_SERVER, MAIL_PORT) as server:
            server.starttls()
            server.login(SENDER_ADDRESS, SENDER_PASSWORD)

            msg = self.generate_message(subject)
            msg.attach(self.generate_message_body(body))

            server.sendmail(SENDER_ADDRESS, RECIPIENTS, msg.as_string())

    @staticmethod
    def generate_message_body(body: MIMEText):
        # Add the message body
        msg_body = MIMEMultipart('alternative')
        msg_body.attach(body)
        return msg_body

    @staticmethod
    def generate_message(subject: str):
        # Create the header for the email with the subject to and from etc
        msg = MIMEMultipart('related')
        msg['Subject'] = f"{subject} {datetime.now().strftime('%d-%m-%Y %H:%M:%S')}"
        msg['From'] = SENDER_ADDRESS
        msg['To'] = ",".join(RECIPIENTS)
        return msg
