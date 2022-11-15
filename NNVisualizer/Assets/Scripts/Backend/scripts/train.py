from keras import models
from sklearn.model_selection import train_test_split
import os
import pandas as pd
import json
from convertToONNX import convertToONNX as cto

'''class train:
    def __init__(self, JSON, model):
        self.JSON = JSON
        self.model = model
        #self.curname = curname
    def get(self):
        os.chdir("../input/")
        data = json.loads(open(self.JSON, "r").read())
        datasetPath = data["datasetPath"]
        df = pd.read_csv(datasetPath)
        targets = data["targets"]
        predictors = [column for column in df.columns if column not in targets][1 : ]
        x = df[predictors].values
        y = df[targets].values
        #print(x)
        x_train, _, y_train, _ = train_test_split(x, y, test_size=0.3, random_state=42)
        # df = pd.read_csv(self.dataset)
        # targets = data['targets']
        # predictors = [column for column in df.columns if column not in data['targets']]
        return x_train, y_train
        
    def train(self):
        #self.get()
        #self.model.compile()
        x_train, y_train = self.get()
        self.model.fit(x_train, y_train, batch_size = 15, epochs = 5, verbose = 1)
        return self.model
        #self.model.save(os.path.dirname(os.path.abspath(__file__)) + '/../output/'+ self.curname + ".h5")'''


class train():
    def __init__(self):
        self.JSON = "ip.json"
        self.x_train, self.y_train = self.get()

    def get(self):
        #os.chdir("../input/")
        os.chdir(os.path.dirname(os.path.abspath(__file__))+'/../input/')
        data = json.loads(open(self.JSON, "r").read())
        datasetPath = data["datasetPath"]
        df = pd.read_csv(datasetPath)
        targets = data["targets"]
        predictors = [column for column in df.columns if column not in targets][1 : ]
        x = df[predictors].values
        y = df[targets].values
        #print(x)
        x_train, _, y_train, _ = train_test_split(x, y, test_size=0.3, random_state=42)
        # df = pd.read_csv(self.dataset)
        # targets = data['targets']
        # predictors = [column for column in df.columns if column not in data['targets']]
        return x_train, y_train

    def train(self, curname, flag = False):
        # x_train, y_train = self.get()
        prev = int(curname.replace("epoch_", ""))
        prevname = "epoch_" + str(prev - 5)
        if(flag):
            prevname = curname
        self.model = models.load_model("../output/" + prevname + ".h5", compile = False)
        self.model.compile(loss='mean_squared_error', optimizer='adam')
        #print(prev)
        self.model.fit(self.x_train, self.y_train, epochs = 5, verbose = 2, shuffle = True)
        #return self.model
        self.model.save(os.path.dirname(os.path.abspath(__file__)) + '/../output/' + curname + '.h5', include_optimizer = True)
        cto(curname, os.path.dirname(os.path.abspath(__file__)) + '/../output/').convert()