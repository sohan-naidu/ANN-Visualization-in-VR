import os
import json
import pandas as pd
from models import Directories, FileExtensions
from keras import Sequential
from keras.layers import Dense
from logger import logger


def remove(path):
    try:
        os.chdir(path)
        files = os.listdir(path)
        for file in files:
            logger.info(f"Deleting file {file}")
            os.remove(file)
    except FileNotFoundError as e:
        logger.exception(e)


def reset():
    for path in [Directories.EPOCHS_DIR, Directories.OUTPUT_DIR]:
        remove(os.path.dirname(os.path.abspath(__file__)) + path)


def initialize():
    os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.INPUT_DIR)
    data = json.loads(open("ip" + FileExtensions.JSON, "r").read())
    dataset_path = data["datasetPath"]
    targets = data["targets"]
    layers_count = data['layerCount']
    neurons_count = data['neuronsCount']

    df = pd.read_csv(dataset_path)
    df.columns = df.columns.str.lower()
    column_count = len(df.columns) - 1
    target_count = len(targets)
    input_dim = column_count - target_count
    if layers_count == 1:
        raise ValueError("Can't have a single layer.")
    model = Sequential()
    for layer in range(layers_count):
        if layer == 0:
            model.add(Dense(units=neurons_count[layer], input_dim=input_dim, kernel_initializer='random_normal',
                            activation='tanh'))
        else:
            model.add(Dense(units = neurons_count[layer], kernel_initializer='random_normal', activation='tanh'))
    model.add(Dense(units=1, kernel_initializer='random_normal', activation='tanh'))

    # Change based on regression/classification
    # model.compile(loss='binary_crossentropy', optimizer='adam')
    model.compile(loss='mean_squared_error', optimizer='adam')

    #history = self.model.fit(self.x_train, self.y_train, epochs = 5, verbose = 2, shuffle = True)
    os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.OUTPUT_DIR)
    model.save("initial" + FileExtensions.H5, include_optimizer=True)
    logger.info("Saved initial model")
    #model.save("epoch_0" + H5)
    #models.save_model(model, "epoch_0" + H5, include_optimizer=True)
    #cto("initial").convert()
    os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.EPOCHS_DIR)
    with open('metrics.json', 'w') as f:
        #json.dump({"metrics" : []}, f, indent = 4)
        json.dump([], f, indent=4)
    logger.info("Saved metrics file")
