import twitter

# Create the Twitter API object to allow us to send requests
api = twitter.Api(consumer_key = 'API key',
                  consumer_secret = 'API secret key',
                  access_token_key = 'Access token',
                  access_token_secret = 'Access token secret',
                  # This means we are asking for the full tweet not the truncated version
                  tweet_mode = 'extended')

# This gets the timeline for the requested user
# This will be a list of tweets from Boris Johnson
timeline = api.GetUserTimeline(screen_name = 'BorisJohnson', count = 1)

# Because it's a list we need to isolate one entry
# For this case the list is only one element long because we only requested one tweet, so we grab the first element
latest_tweet = timeline[0]

# Now we get the text of the tweet
tweet_text = latest_tweet.full_text

# Print the tweet text, simple!
print(tweet_text)
