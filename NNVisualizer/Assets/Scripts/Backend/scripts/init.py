import os
import json
import pandas as pd
import argparse
from keras import Sequential
from keras.layers import Dense
from convertToONNX import convertToONNX as cto

class initialize:
    def __init__(self):
        self.JSON = "ip.json"

    def initialize(self):
        os.chdir('../input/')
        #df = pd.read_csv(self.dataset)
        data = json.loads(open(self.JSON, "r").read())
        #columnCount = len(df.columns)
        #targetCount = len(data['targets'])
        #inputDim = columnCount - targetCount
        
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
                model.add(Dense(units = neuronsCount[layer], input_dim = inputDim, kernel_initializer='ones', activation='relu'))
            else:
                model.add(Dense(units = neuronsCount[layer], kernel_initializer='ones', activation='relu'))
        model.compile(loss='mean_squared_error', optimizer='adam')
        model.save(os.path.dirname(os.path.abspath(__file__)) + '/../output/epoch_0.h5', include_optimizer = True)
        cto("epoch_0", os.path.dirname(os.path.abspath(__file__)) + '/../output/').convert()
        #print(os.path.dirname(os.path.abspath(__file__)))
        #return model

'''if __name__ == '__main__':
    parser = argparse.ArgumentParser(description = "Backend initialization argument parser.")
    #parser.add_argument('dataset', type = str, help = 'Dataset filename (csv)')
    parser.add_argument('json', type = str, help = 'JSON filename')
    args = parser.parse_args()
    start = initialize(args.json)
    model = start.initialize()
    #model.save(os.path.dirname(os.path.abspath(__file__)) + '/../output/model.h5')'''