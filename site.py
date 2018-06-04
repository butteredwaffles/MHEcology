from flask import Flask, render_template, render_template_string
import mondata

app = Flask(__name__)

@app.route('/')
def index():
    monsters = mondata.check_images_exist(mondata.get_monsters())
    return render_template('index.html', monsters=monsters)

@app.route('/<name>')
def eco_page(name):
    monsters = mondata.check_images_exist(mondata.get_monsters())
    monster = [monstert for monstert in monsters if monstert['name'] == name]
    if monster:
        return render_template('eco.html', monster=monster[0])
    return render_template_string("<h1>Could not find that monster!</h1>")

if __name__ == "__main__":
    app.run()