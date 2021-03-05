FROM python:slim

RUN apt-get update && apt-get install -y build-essential libssl-dev libffi-dev python-dev

WORKDIR /app
ADD . /app

# Generate new cert for local ssl
RUN openssl genrsa -out cert.key 2048
RUN openssl req -new -key cert.key -out cert.csr -batch
RUN openssl x509 -req -days 365 -in cert.csr -signkey cert.key -out cert.crt

RUN ls

RUN pip3 install -r requirements.txt

CMD ["uwsgi", "--master", "--https", "0.0.0.0:5004,cert.crt,cert.key", "-w", "wsgi:app"]