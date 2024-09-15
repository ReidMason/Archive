# MyAnimeList backup
A program to backup users anime lists from MyAnimeLists locally.\
This is designed to run in a docker container on unraid but can be run as a python app if the config file is edited to remove the use of environment variables.

## This project contains
* Docker deployment
* Unraid integration
* Web scraping
* Scheduling code to run
* Xml templates
* Json file manipulation

## What I learned
* How to create docker applications
* How to write a docker container for Unraid
* How to schedule code to run using [Schedule](https://pypi.org/project/schedule/)


## Installation to unraid
This is the easiest and recommended way to install.\
Simply add this repo to the templates list and create the container using the new template.

## Alternate installation
Clone the repo and edit `config.py` replacing the environment variable references.
Example:
```python
import os

# Setup environment variables
NUM_BACKUPS = 5
USERNAMES = ['Username1', 'Username2']

BACKUP_PATH = 'backups'
```
Then just run the following commands.
```
$ pip install -r requirements.txt
$ python main.py
```