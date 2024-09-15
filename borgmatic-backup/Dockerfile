FROM python:slim

RUN apt-get update && apt-get -y install cron borgbackup 
WORKDIR /app
ADD . /app
RUN pip install -r requirements.txt

ENV BORG_CACHE_DIR="/data/cache/"

# Copy backup-cron file to the cron.d directory
# COPY backup-cron /etc/cron.d/backup-cron
# Give execution rights on the cron job
# RUN chmod 0644 /etc/cron.d/backup-cron
# Apply cron job
# RUN crontab /etc/cron.d/backup-cron
# Create the log file to be able to run tail
RUN touch /var/log/cron.log

# # Run the command on container startup
CMD python3 main.py && \
    cp backup-cron /etc/cron.d/backup-cron && \
    chmod 0644 /etc/cron.d/backup-cron && \
    crontab /etc/cron.d/backup-cron && \
    cron && \
    tail -f /var/log/cron.log