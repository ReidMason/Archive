import twitter
from flask import Flask, render_template, request

# Create the actual Flask object
app = Flask(__name__)

# Create the Twitter API object to allow us to send requests
api = twitter.Api(consumer_key = 'API key',
                  consumer_secret = 'API secret key',
                  access_token_key = 'Access token',
                  access_token_secret = 'Access token secret',
                  # This means we are asking for the full tweet not the truncated version
                  tweet_mode = 'extended')


# This is the index page for the website, the first page that will be displayed to the user
# It will be located at the root of the web address so for this example http://127.0.0.1/
@app.route('/')
def index():
    # This tells it to render the html page that is inside the templates directory
    return render_template('index.html')


# This is the same as the index page but instead the url would be http://127.0.0.1/findUserForm
# This wouldn't however return a web page unless you posted in the right kind of data
@app.route('/findUserForm', methods = ['POST'])
def find_user_form():
    # Gets the data sent in the post message and converts it into a usable dictionary
    data = dict(request.form)
    username = data.get('TwitterName')

    # Sends twitter request to the API to get a list of statuses
    # Statuses are basically tweets just the API calls them statuses
    statuses = get_user_statuses(username)
    # This contains a lot of information but for now I only want the text of the latest status
    last_tweet_content = statuses[0].full_text

    # The arguments username and last_tweet_content are data that is passed into the html file
    # The html file accesses them using jinja2 templating
    # For this example it's just simple {{ username }} in the html file
    return render_template('user.html', username = username, last_tweet_content = last_tweet_content)


def get_user_statuses(username: str) -> list:
    # This is just sending a request to the Twitter API asking for a list of users statuses
    return api.GetUserTimeline(screen_name = username)


if __name__ == '__main__':
    # Start the local dev server
    app.run()
