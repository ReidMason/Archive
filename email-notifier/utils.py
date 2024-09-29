import math
import os
from jinja2 import Environment, FileSystemLoader
from config import AUTH_TOKEN


def verify_auth_token(headers):
    # get the auth token
    auth_header = headers.get('Authorization')
    if auth_header:
        auth_token = auth_header.split(" ")[1]
    else:
        auth_token = ''

    return auth_token == AUTH_TOKEN


def format_file_size(size_bytes):
    if size_bytes == 0:
        return "0B"
    size_name = ("B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB")
    i = int(math.floor(math.log(size_bytes, 1024)))
    p = math.pow(1024, i)
    s = round(size_bytes / p, 2)
    return "%s %s" % (s, size_name[i])


def load_email_template(template_name):
    env = Environment(loader = FileSystemLoader('emailTemplates/'))
    return env.get_template(template_name)


def get_filename_from_path(filepath):
    return os.path.splitext(os.path.basename(os.path.normpath(filepath)))[0]
