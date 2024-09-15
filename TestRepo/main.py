from flask import Flask

app = Flask(__name__)

@app.route('/')
def greeting():
    return "Hello World!"

@app.route('/index')
def index():
    return "This is the index page"

app.run(host="0.0.0.0")
