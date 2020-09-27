import flask
import pymongo

from flask import request
from pymongo import MongoClient

app = flask.Flask(__name__)
app.config["DEBUG"] = True

client = MongoClient("mongodb://admin:123456##@localhost") 
db = client.logDispatcher

@app.route('/upload', methods=['POST'])
def upload():

    if not request.json or not 'destination' in request.json or not 'logs' in request.json:
        abort(400)

    coll = db[request.json["destination"]]
    for log in request.json['logs']:  
        coll.insert(log)

    return f'Inserted {len(request.json["logs"])} record logs'

app.run(host='0.0.0.0', port=3000)
