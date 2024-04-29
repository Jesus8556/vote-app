# app/app.py
from flask import Flask, render_template, request, redirect
import redis

# Conectar con Redis (asume que Redis está ejecutándose en el puerto 6379)
redis_client = redis.StrictRedis(host='redis', port=6379, decode_responses=True)

app = Flask(__name__)

@app.route('/')
def home():
    return render_template('index.html')

@app.route('/vote', methods=['POST'])
def vote():
    option = request.form.get('option')
    if option == 'dog':
        redis_client.incr('dog_votes')  # Incrementar la cuenta para perros
    elif option == 'cat':
        redis_client.incr('cat_votes')  # Incrementar la cuenta para gatos
    return redirect('/')

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)
