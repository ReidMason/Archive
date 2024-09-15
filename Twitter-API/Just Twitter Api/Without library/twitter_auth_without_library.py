from requests_oauthlib import OAuth1
import requests
import json

# Create the auth
auth = OAuth1('API key',
              'API secret key',
              'Access token',
              'Access token secret', )

# Send the request with the auth
# This request is asking for Boris Johnsons tweets and it is only requesting one of them which will be the most recent
r = requests.get('https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=BorisJohnson&count=1',
                 auth = auth)

# The content of the request is where our data is located
# We run the .decode on it to convert it from a bytes type to a string
data = r.content.decode("utf-8")

# This data will be in json format so we can load it into a Python data structure using the built in json library
# .loads will read a string and convert it into a Python usable data structure
# In this instance it will be a list of dictionaries
data = json.loads(data)

# Because it's a list we need to isolate one entry
# For this case the list is only one element long because we only requested one tweet, so we grab the first element
data = data[0]

# Now we get the text of the tweet
tweet_text = data.get('text')

# Print the tweet text, simple!
print(tweet_text)
