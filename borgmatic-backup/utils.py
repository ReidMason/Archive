import os
from config import PUID, GUID, COMPILED_CONFIG_DIR
import math

def remove_file_extension(filename):
    return os.path.splitext(filename)[0]

def ensure_directory_exists(dir_path):
    if not os.path.exists(dir_path):
        os.makedirs(dir_path)
        fix_perms(dir_path)

def fix_perms(filepath):
    try:
        os.chmod(filepath, 0o770)
    except PermissionError:
        print(f"Unable to chmod {filepath}")

    try:
        os.chown(filepath, PUID, GUID)
    except PermissionError:
        print(f"Unable to chown {filepath}")


def format_file_size(size_bytes):
   if size_bytes == 0:
       return "0B"
   size_name = ("B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB")
   i = int(math.floor(math.log(size_bytes, 1024)))
   p = math.pow(1024, i)
   s = round(size_bytes / p, 2)
   return "%s %s" % (s, size_name[i])