import smtplib
import sys
from datetime import datetime
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText

from config import PASSWORD, RECIPIENT, SEND_ADDRESS


class EmailSender:
    def send_email(self, subject: str, body: MIMEText):
        # Create the connection to the mail server
        with smtplib.SMTP('smtp.gmail.com', 587) as server:
            server.starttls()
            server.login(SEND_ADDRESS, PASSWORD)

            msg = self.generate_message(subject)
            msg.attach(self.generate_message_body(body))

            server.sendmail(SEND_ADDRESS, RECIPIENT, msg.as_string())

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
        msg['From'] = SEND_ADDRESS
        msg['To'] = RECIPIENT
        return msg


if __name__ == '__main__':
    email_subject = sys.argv[1]
    email_body = MIMEText(sys.argv[2])

    EmailSender().send_email(email_subject, email_body)