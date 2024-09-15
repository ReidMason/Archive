import yaml
import os
import glob
from config import COMPILED_CONFIG_DIR, RAW_CONFIG_DIR, LOG_PATH, BORG_CACHE_DIR, PUID, GUID, IS_DEV
from crontabBuilder import remove_crontab, add_cron_job
from utils import remove_file_extension
from pprint import pprint

def ensure_compiled_config_dir_exists():
    if not os.path.exists(COMPILED_CONFIG_DIR):
        os.makedirs(COMPILED_CONFIG_DIR)

def config_is_valid(filename: str):
    return filename.endswith(".yml") and not filename.startswith("_")

def get_raw_configs():
    print("Getting raw configs")
    return [x for x in os.listdir(RAW_CONFIG_DIR) if config_is_valid(x)] 

def clear_compiled_configs():
    print("Clearing old compiled configs")
    for file in glob.glob(os.path.join(COMPILED_CONFIG_DIR, '*.yml')):
        os.remove(file)

def compile_config(filename: str):
    print(f"Compiling config for {remove_file_extension(filename)}")
    ensure_compiled_config_dir_exists()

    with open(os.path.join(RAW_CONFIG_DIR, filename), 'r') as f:
        try:
            config_data = yaml.safe_load(f)
            if not IS_DEV:
                config_data = add_chown_hooks(config_data)

            run_time = config_data.pop('run_time')
            create_cron_job(filename, run_time)

        except yaml.YAMLError as ex:
            print(ex)
            return

    compiled_config_path = os.path.join(COMPILED_CONFIG_DIR, filename)
    save_yaml(config_data, compiled_config_path)
    os.system(f'borgmatic init --encryption repokey --config "{compiled_config_path}"')

def add_chown_hooks(config_data):
    location = config_data.get('location', {})
    repositories = location.get('repositories', [])

    # Add a chown and chmod command for all repositories
    chown_commands = [f'chown -R {PUID}:{GUID} "{x}"' for x in repositories]
    chown_commands = chown_commands + [f'chmod -R 770 "{x}"' for x in repositories]
    # Also add commands for the cache directory
    chown_commands.append(f'chown -R {PUID}:{GUID} "{BORG_CACHE_DIR}"')
    chown_commands.append(f'chmod -R 770 "{BORG_CACHE_DIR}"')
    # Also add commands for logs
    chown_commands.append(f'chown -R {PUID}:{GUID} "{LOG_PATH}"')
    chown_commands.append(f'chmod -R 770 "{LOG_PATH}"')
        
    hooks = config_data.get('hooks', {})
    after_everything = hooks.get('after_everything', []) + chown_commands
    hooks['after_everything'] = after_everything
    config_data['hooks'] = hooks

    return config_data

def create_cron_job(config_filename, run_time):
    command = build_backup_command(config_filename)
    log_path = os.path.join(LOG_PATH, f"{remove_file_extension(config_filename)}.log")
    add_cron_job(run_time, command, log_path)

def save_yaml(data: dict, filepath: str):
    with open(filepath, 'w') as f:
            yaml.dump(data, f, default_flow_style=False)

def build_backup_command(config_filename):
    return f'python3 /app/backupRunner.py "{config_filename}"'

def compile_configs():
    print("Compiling initial configs")
    clear_compiled_configs()
    remove_crontab()

    configs = get_raw_configs()

    for config in configs:
        compile_config(config)
        
        