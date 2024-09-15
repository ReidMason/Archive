# Twitter api
Just some very basic examples of accessing the Twitter api using Python.

## Just Twitter api
The folder "Just Twitter Api" contains two folders that are examples of two different ways of purely accessing the Twitter api using Python no web page or other bells and whistles.

### Without library
Inside the folder `Without library` the file `twitter_auth_without_library.py` is an example of getting the most recent tweet from Boris Johnson.\
This doesn't use any Twitter specific libraries, it only uses the Oauth library to send your credentials to Twitter.\
Running this file will print out the text of Boris Johnson's latest Tweet.
You might want to use this instead of the library because it has a slightly smaller overhead because there are less libraries to install. It will return the data in JSON format so if you're more familiar with that then this method might be preferred. Finally you would be interfacing directly with the Twitter API's endpoints so you would likely be using the Twitter API documentation rather than a third parties documentation.

To run this code clone the library and head to the `Without library` folder and open a command prompt there. Run the following command to install the required libraries using pip.
```
pip install -r requirements.txt
```
This will install all the requirements through pip. The `-r` argument just lets you pass in a requirements file to the install command.\
These libraries can be uninstalled once you're done using the program with the next command.
```
pip uninstall -y -r requirements.txt
```
The `-y` argument just automatically answers yes to all of the prompts asking if you're sure you want to remove the library to save you from having to type yes a bunch of times.

Then fill in your Twitter API credentials and run the code using this next command.
```
python twitter_auth_without_library.py
```

### With library
Inside the folder `With library` the file `twitter_auth_with_library.py` is an example of getting the most recent tweet from Boris Johnson.\
This uses a library for interfacing with the Twitter api.\
Running this file will print out the text of Boris Johnson's latest Tweet.
You might want to use the library because it will let you interface with the data using standard Python objects rather than JSON though both data types work just fine, it's personal preference. Though this does have a slightly larger overhead as there are more requirements to install but this really doesn't make much difference. To use this you would also be looking at a third party documentation rather than Twitters own documentation.
The Twitter library being used is [python-twitter](https://pypi.org/project/python-twitter/) it has [documentation here](https://python-twitter.readthedocs.io/en/latest/).

To run this code clone the library and head to the `With library` folder and open a command prompt there. Run the following command to install the required libraries using pip.
```
pip install -r requirements.txt
```
This will install all the requirements through pip. The `-r` argument just lets you pass in a requirements file to the install command.\
These libraries can be uninstalled once you're done using the program with the next command.
```
pip uninstall -y -r requirements.txt
```
The `-y` argument just automatically answers yes to all of the prompts asking if you're sure you want to remove the library to save you from having to type yes a bunch of times.

Then fill in your Twitter API credentials and run the code using this next command.
```
python twitter_auth_with_library.py
```

## Flask with Twitter Api
The folder "Flask with Twitter Api" contains an example of accessing the Twitter API and then putting the results on a webpage created using the Flask framework. This isn't necessary if all you're looking to do at the moment is get data from Twitter.

### Running the code
Once you've cloned the repo you will need to install all the required libraries through pip. These requirements are listed inside the `requirements.txt` file. You can install these by opening a terminal in the `Flask with Twitter Api` directory and running the following command.
```
pip install -r requirements.txt
```
This will install all the requirements through pip. The `-r` argument just lets you pass in a requirements file to the install command.\
These libraries can be uninstalled once you're done using the program with the next command.
```
pip uninstall -y -r requirements.txt
```
The `-y` argument just automatically answers yes to all of the prompts asking if you're sure you want to remove the library to save you from having to type yes a bunch of times.

Inside this folder is a `templates` folder that contains all of the HTML files needed to make the website work.\
The `main.py` file is used to start the application. This will start the local web server that you will be able to access through your browser at [http://127.0.0.1:5000/](http://127.0.0.1:5000/)\
Before you can run this you just need to fill in your Twitter API credentials in `main.py`. Then you should be set to run the dev server.
```
python main.py
```