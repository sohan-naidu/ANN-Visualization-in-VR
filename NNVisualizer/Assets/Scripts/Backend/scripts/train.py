from keras import models
from sklearn.model_selection import train_test_split
import os
import pandas as pd
import json
from convertToONNX import ConvertToONNX as cto
from globals import *

class Train():
    def __init__(self):
        self.x_train, self.y_train = self.get()

    def get(self):
        os.chdir( os.path.dirname(os.path.abspath(__file__)) + INPUT_DIR)
        #os.chdir(INPUT_DIR)
        data = json.loads(open(JSON, "r").read())
        datasetPath = data["datasetPath"]
        df = pd.read_csv(datasetPath)
        targets = data["targets"]
        predictors = [column for column in df.columns if column not in targets][1 : ]
        x = df[predictors].values
        y = df[targets].values
        return x,y
        #x_train, _, y_train, _ = train_test_split(x, y, test_size=0.3, random_state=42)
        #return x_train, y_train

    def train(self, curname, flag = False):
        prev = int(curname.replace("epoch_", ""))
        if(prev == 0):
            flag = True
        prevname = "epoch_" + str(prev - 5)
        if(flag):
            prevname = "initial"
        self.model = models.load_model(OUTPUT_DIR + prevname + H5) 

        # Change based on regression or classification
        # self.model.compile(loss = 'binary_crossentropy', optimizer = 'adam', metrics = ['RootMeanSquaredError'])
        self.model.compile(loss = 'mean_squared_error', optimizer = 'adam', metrics = ['RootMeanSquaredError'])
        
        if(flag):
            history = self.model.fit(self.x_train, self.y_train, epochs = 1, verbose = 2, shuffle = True)
        else:
            history = self.model.fit(self.x_train, self.y_train, epochs = 5, verbose = 2, shuffle = True)

        os.chdir(OUTPUT_DIR)
        self.model.save(curname + H5, include_optimizer = True)
        cto(curname).convert()

        os.chdir(EPOCHS_DIR)
        with open("metrics.json", "r") as f:
            old_data = json.load(f)
        history.history["loss"] = round(history.history["loss"][0], 6)
        history.history["root_mean_squared_error"] = round(history.history["root_mean_squared_error"][0], 6)
        old = old_data
        old.append(history.history)
        old_data = old
        print(old_data)
        with open("metrics.json", "w") as f:
            json.dump(old_data, f, indent = 4)