import os
import json
import pandas as pd
from keras import Sequential
from keras.layers import Dense
from convertToONNX import ConvertToONNX as cto
from globals import *
from keras import models

class Initialize:
    def initialize(self):
        os.chdir( os.path.dirname(os.path.abspath(__file__)) + INPUT_DIR)
        data = json.loads(open(JSON, "r").read())
        datasetPath = data["datasetPath"]
        targets = data["targets"]
        layerCount = data['layerCount']
        neuronsCount = data['neuronsCount']

        df = pd.read_csv(datasetPath)
        df.columns = df.columns.str.lower()
        columnCount = len(df.columns) - 1
        targetCount = len(targets)
        inputDim = columnCount - targetCount
        if(layerCount == 1):
            raise ValueError("Can't have a single layer.")
        model = Sequential()
        for layer in range(layerCount):
            if(layer == 0):
                model.add(Dense(units = neuronsCount[layer], input_dim = inputDim, kernel_initializer='random_normal', activation='tanh'))
            else:
                model.add(Dense(units = neuronsCount[layer], kernel_initializer = 'random_normal', activation = 'tanh'))
        model.add(Dense(units = 1, kernel_initializer = 'random_normal', activation = 'tanh'))

        # Change based on regression/classification
        # model.compile(loss='binary_crossentropy', optimizer='adam')
        model.compile(loss='mean_squared_error', optimizer='adam')

        #history = self.model.fit(self.x_train, self.y_train, epochs = 5, verbose = 2, shuffle = True)
        os.chdir(os.path.dirname(os.path.abspath(__file__)) + OUTPUT_DIR)
        model.save("initial" + H5, include_optimizer = True)
        #model.save("epoch_0" + H5)
        #models.save_model(model, "epoch_0" + H5, include_optimizer=True)
        #cto("initial").convert()
        os.chdir(os.path.dirname(os.path.abspath(__file__)) + EPOCHS_DIR)
        with open('metrics.json', 'w') as f:
            #json.dump({"metrics" : []}, f, indent = 4)
            json.dump([], f, indent = 4)