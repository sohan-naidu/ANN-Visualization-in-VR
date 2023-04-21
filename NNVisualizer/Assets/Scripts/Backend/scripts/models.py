import os
import tf2onnx
import pandas as pd
import json
import h5py
import numpy as np
import copy
from keras import models
from logger import logger


class Directories:
    EPOCHS_DIR = "\\..\\epochs\\"
    OUTPUT_DIR = "\\..\\output\\"
    INPUT_DIR = "\\..\\input\\"


class FileExtensions:
    JSON = ".json"
    H5 = ".h5"
    ONNX = ".onnx"


class ConvertToONNX:
    @staticmethod
    def convert(filename):
        try:
            os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.OUTPUT_DIR)
            model = models.load_model(filename + FileExtensions.H5)
            os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.EPOCHS_DIR)
            tf2onnx.convert.from_keras(model, output_path=filename + FileExtensions.ONNX)
            logger.info(f"Converted {filename} to ONNX")
        except Exception as e:
            logger.exception(f"Failed to convert {filename} to ONNX : {e}")


class Train:
    def __init__(self):
        self.__x_train, self.__y_train = self.get_data_split()
        self.model = None

    @staticmethod
    def get_data_split():
        os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.INPUT_DIR)
        data = json.loads(open("ip" + FileExtensions.JSON, "r").read())
        dataset_path = data["datasetPath"]
        df = pd.read_csv(dataset_path)
        targets = data["targets"]
        predictors = [column for column in df.columns if column not in targets][1:]
        x = df[predictors].values
        y = df[targets].values
        return x, y
        # __x_train, _, __y_train, _ = train_test_split(x, y, test_size=0.3, random_state=42)
        # return __x_train, __y_train

    def train(self, current_name, init, cur):
        prev = int(current_name.replace("epoch_", ""))
        if prev == 0:
            if init:
                prev_name = "initial"
            else:
                prev_name = "epoch_0"
        else:
            if cur:
                prev_name = current_name
            else:
                prev_name = "epoch_" + str(prev - 5)

        os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.OUTPUT_DIR)
        self.model = models.load_model(prev_name + FileExtensions.H5)
        # Change based on regression or classification
        # self.model.compile(loss = 'binary_crossentropy', optimizer = 'adam', metrics = ['RootMeanSquaredError'])
        self.model.compile(loss='mean_squared_error', optimizer='adam', metrics=['RootMeanSquaredError'])

        if init:
            history = self.model.fit(self.__x_train, self.__y_train, epochs=1, verbose=0, shuffle=True)
        else:
            history = self.model.fit(self.__x_train, self.__y_train, epochs=5, verbose=0, shuffle=True)
        self.model.save(current_name + FileExtensions.H5, include_optimizer=True)
        ConvertToONNX.convert(current_name)
        os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.EPOCHS_DIR)
        with open("metrics.json", "r") as f:
            old_data = json.load(f)
        history.history["loss"] = round(history.history["loss"][0], 6)
        history.history["root_mean_squared_error"] = round(history.history["root_mean_squared_error"][0], 6)
        new_data = old_data
        new_data.append(history.history)
        with open("metrics.json", "w") as f:
            json.dump(new_data, f, indent=4)


class Neurons:
    def __init__(self, op, x, y, current_name):
        self.x = x
        self.y = y
        self.op = op
        self.current_name = current_name

    def modify(self):
        os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.OUTPUT_DIR)
        hf = h5py.File(self.current_name + FileExtensions.H5, 'r+')
        architecture = json.loads(hf.attrs.get("model_config"))
        all_groups = []
        hf.visit(all_groups.append)
        groups = [all_groups[group] for group in range(len(all_groups)) if "model" in all_groups[group]
                  and ("bias" in all_groups[group] or "kernel" in all_groups[group])]

        index = 2 * self.x + 1
        if index < 0 or index >= len(groups):
            raise IndexError("Invalid layer.")
        if index == len(groups) - 1:
            raise IndexError("Cannot modify output layer.")

        old_kernel = np.array(hf[groups[index]])
        old_bias = np.array(hf[groups[index - 1]])
        if self.op == "add":
            for _ in range(self.y):
                new_kernel = np.random.rand(old_kernel.shape[0], 1)
                new_bias = np.random.rand(1)
                old_kernel = np.append(old_kernel, new_kernel, axis=1)
                old_bias = np.append(old_bias, new_bias)
            # Updating architecture, 1-indexed
            architecture['config']['layers'][self.x + 1]['config']['units'] += self.y

        else:
            old_kernel = np.delete(old_kernel, self.y, axis=1)
            old_bias = np.delete(old_bias, self.y, axis=0)
            architecture['config']['layers'][self.x + 1]['config']['units'] -= 1
        # ith kernel
        del hf[groups[index]]
        hf.create_dataset(groups[index], data=old_kernel)
        # ith bias
        del hf[groups[index - 1]]
        hf.create_dataset(groups[index - 1], data=old_bias)
        # (i + 1)th kernel
        if index + 2 < len(groups):
            old_next_kernel = np.array(hf[groups[index + 2]])
            prev = hf[groups[index + 2]].shape[1]
            if self.op == "add":
                for _ in range(self.y):
                    new_next_kernel = np.random.rand(1, prev)
                    old_next_kernel = np.append(old_next_kernel, new_next_kernel, axis=0)
            else:
                old_next_kernel = np.delete(old_next_kernel, self.y, axis=0)
            del hf[groups[index + 2]]
            hf.create_dataset(groups[index + 2], data=old_next_kernel)

        updated_architecture = json.dumps(architecture)
        hf.attrs.modify("model_config", updated_architecture)
        hf.close()


class Layers():
    def __init__(self, op, x, y, current_name):
        self.op = op
        self.x = x
        self.y = y
        self.current_name = current_name

    @staticmethod
    def get_groups(groups, jump):
        current = []
        arr = []
        for i in range(0, len(groups), jump):
            for j in range(i, i + jump):
                arr.append(groups[j])
            current.append(arr)
            arr = []
        return current

    def update_name(self, old_name):
        name, number = "", ""
        for character in range(len(old_name)):
            if old_name[character].isdigit():
                number += old_name[character]
            else:
                name += old_name[character]

        num = int(number)
        if self.op == "addL":
            num += 1
        else:
            num -= 1
        number = str(num)
        new_name = name + number
        return new_name

    def modify(self):
        os.chdir(os.path.dirname(os.path.abspath(__file__)) + Directories.OUTPUT_DIR)
        hf = h5py.File(self.current_name + FileExtensions.H5, 'r+')
        architecture = json.loads(hf.attrs.get("model_config"))
        all_groups = []
        hf.visit(all_groups.append)

        groups = [all_groups[group] for group in range(len(all_groups)) if "model" in all_groups[group]]
        groups = groups[1: -1]  # Ignores model_weights and top_level_model_weights

        layer_groups = self.get_groups(groups, 4)

        groups = [all_groups[group] for group in range(len(all_groups)) if "optimizer" in all_groups[group]]
        groups = groups[2: -1]  # Ignores optimizer_weights, optimizer_weights/optimizer and iter:0

        optimizer_groups = self.get_groups(groups, 7)

        #  Architecture part
        all_layers = architecture['config']['layers']
        layers = all_layers[1:]

        group_data = []
        optimizer_data = []

        if self.op == "addL":
            for i in range(self.x, len(layer_groups)):  # Name change and data copy
                if i == self.x:
                    prev = np.array(hf[layer_groups[i - 1][2]]).shape[
                        0]  # Collects shape of previous layer's bias == number of neurons
                    group_data = copy.deepcopy(
                        layer_groups[i])  # Collects only name of current group cause new group will have this name
                    optimizer_data = copy.deepcopy(layer_groups[i])
                    new_dict = copy.deepcopy(layers[self.x])
                    new_dict['config']['units'] = self.y
                    new_dict['config']['activation'] = 'relu'
                    group_data.append(prev)
                    optimizer_data.append(prev)

                old = layers[i]['config']['name']
                new = self.update_name(old)
                layers[i]['config']['name'] = new

                for j in range(len(layer_groups[i])):  # -1 cause last element in groups is in int (prev layer shape)
                    layer_groups[i][j] = layer_groups[i][j].replace(old, new)
                for j in range(len(optimizer_groups[i])):
                    optimizer_groups[i][j] = optimizer_groups[i][j].replace(old, new)
            self.modify_groups(hf, layer_groups, group_data)
            self.modify_optimizer(hf, optimizer_groups, optimizer_data)

            layers.insert(self.x, new_dict)
            all_layers[1:] = layers
            architecture['config']['layers'] = all_layers
            updated_architecture = json.dumps(architecture)
            hf.attrs.modify("model_config", updated_architecture)
            hf.close()

        else:
            # print(layerGroups)
            for i in range(self.x, len(layerGroups)):
                if (i == self.x):
                    prev = np.array(hf[layerGroups[i - 1][2]]).shape[0]  # Previous layer no of neurons
                    next = np.array(hf[layerGroups[i + 1][2]]).shape[0]  # Next layer no of neurons
                    optimizerData.extend([prev, next])
                    groupData = optimizerData
                    continue

                old = layers[i]['config']['name']
                new = self.update_name(layers[i]['config']['name'])
                layers[i]['config']['name'] = new

                for j in range(len(layerGroups[i])):
                    layerGroups[i][j] = layerGroups[i][j].replace(old, new)
                for j in range(len(optimizerGroups[i])):
                    optimizerGroups[i][j] = optimizerGroups[i][j].replace(old, new)

            # del layers[self.x]
            del layerGroups[self.x]
            del optimizerGroups[self.x]

            self.modify_groups(hf, layerGroups, groupData)
            self.modify_optimizer(hf, optimizerGroups, optimizerData)

            del layers[self.x]
            allLayers[1:] = layers
            arch['config']['layers'] = allLayers
            updarch = json.dumps(arch)
            hf.attrs.modify("model_config", updarch)
            hf.close()

    def modify_groups(self, hf, layerGroups, groupData):

        model_weights = list(hf.keys())[0]  # model_weights
        allLayers = list(hf[model_weights].keys())
        layers = allLayers[: -1]  # Ignore top_level_model_weights
        saved = []

        if (self.op == "addL"):
            for i in range(self.x, len(layers)):  # Deletion of old layers and saving weights
                arr = []
                subGroup = list(hf[model_weights][layers[i]].keys())[0]
                bias = list(hf[model_weights][layers[i]][subGroup].keys())[0]
                kernel = list(hf[model_weights][layers[i]][subGroup].keys())[1]
                biasValue = np.array(hf[model_weights][layers[i]][subGroup][bias])
                kernelValue = np.array(hf[model_weights][layers[i]][subGroup][kernel])
                arr.extend([biasValue, kernelValue])
                saved.append(arr)
                del hf[model_weights][layers[i]]

            # Adding the new layer

            newGroup = hf.create_group(groupData[0])
            hf.create_group(groupData[1])
            hf.create_dataset(groupData[2], data=np.random.rand(self.y, ))  # should be bias
            hf.create_dataset(groupData[3], data=np.random.rand(groupData[4], self.y))  # should be kernel
            kernelName = allLayers[0] + "_" + str(self.x) + "/kernel:0"
            biasName = allLayers[0] + "_" + str(self.x) + "/bias:0"
            newGroup.attrs.__setitem__("weight_names", (kernelName, biasName))

            # Readding previously deleted layers with updated names

            for i in range(self.x, len(layerGroups)):
                newGroup = hf.create_group(layerGroups[i][0])
                hf.create_group(layerGroups[i][1])
                if (i == self.x):
                    hf.create_dataset(layerGroups[i][2], data=saved[i - self.x][0])  # Bias
                    hf.create_dataset(layerGroups[i][3], data=np.random.rand(self.y, saved[i - self.x][1].shape[1]))
                else:
                    hf.create_dataset(layerGroups[i][2], data=saved[i - self.x][0])
                    hf.create_dataset(layerGroups[i][3], data=saved[i - self.x][1])
                kernelName = allLayers[0] + "_" + str(i + 1) + "/kernel:0"
                biasName = allLayers[0] + "_" + str(i + 1) + "/bias:0"
                newGroup.attrs.__setitem__("weight_names", (kernelName, biasName))

            last = layerGroups[len(layerGroups) - 1][0].replace("model_weights/", "")
            layer_names = list(hf[model_weights].attrs.__getitem__('layer_names'))
            layer_names.append(last)
            hf[model_weights].attrs.__setitem__('layer_names', layer_names)

        else:
            for i in range(self.x + 1, len(layers)):  # Deletion of old layers and saving weights
                arr = []
                subGroup = list(hf[model_weights][layers[i]].keys())[0]
                bias = list(hf[model_weights][layers[i]][subGroup].keys())[0]
                kernel = list(hf[model_weights][layers[i]][subGroup].keys())[1]
                biasValue = np.array(hf[model_weights][layers[i]][subGroup][bias])
                kernelValue = np.array(hf[model_weights][layers[i]][subGroup][kernel])
                arr.extend([biasValue, kernelValue])
                saved.append(arr)
                del hf[model_weights][layers[i]]

            # Deletion of layer
            del hf[model_weights][layers[self.x]]

            # Readding deleted layers with updated names
            for i in range(self.x, len(layerGroups)):
                newGroup = hf.create_group(layerGroups[i][0])
                hf.create_group(layerGroups[i][1])
                if (i == self.x):
                    hf.create_dataset(layerGroups[i][2], data=saved[i - self.x][0])  # Bias
                    hf.create_dataset(layerGroups[i][3],
                                      data=np.random.rand(groupData[0], saved[i - self.x][1].shape[1]))
                else:
                    hf.create_dataset(layerGroups[i][2], data=saved[i - self.x][0])
                    hf.create_dataset(layerGroups[i][3], data=saved[i - self.x][1])
                kernelName = allLayers[0] + "_" + str(i) + "/kernel:0"
                biasName = allLayers[0] + "_" + str(i) + "/bias:0"
                newGroup.attrs.__setitem__("weight_names", (kernelName, biasName))

            layer_names = list(hf[model_weights].attrs.__getitem__('layer_names'))
            # for i in range(self.x + 1, len(layer_names)):
            # layer_names[i] = self.update_name(layer_names[i])
            del layer_names[len(layer_names) - 1]
            hf[model_weights].attrs.__setitem__('layer_names', layer_names)

    def modify_optimizer(self, hf, optimizerGroups, optimizerData):

        optimizerWeights = list(hf.keys())[1]  # optimizer_weights
        optimizer = list(hf[optimizerWeights].keys())[0]  # Optimizier (Adam)
        allLayers = list(hf[optimizerWeights][optimizer].keys())
        layers = allLayers[: -1]  # Get hidden layers that need to be changed upto output layer
        saved = []

        if (self.op == "addL"):
            for i in range(self.x, len(layers)):
                arr = []
                bias = list(hf[optimizerWeights][optimizer][layers[i]].keys())[0]
                kernel = list(hf[optimizerWeights][optimizer][layers[i]].keys())[1]
                biasM = list(hf[optimizerWeights][optimizer][layers[i]][bias].keys())[0]
                biasV = list(hf[optimizerWeights][optimizer][layers[i]][bias].keys())[1]
                biasMValue = np.array(hf[optimizerWeights][optimizer][layers[i]][bias][biasM])
                biasVValue = np.array(hf[optimizerWeights][optimizer][layers[i]][bias][biasV])
                kernelM = list(hf[optimizerWeights][optimizer][layers[i]][kernel].keys())[0]
                kernelV = list(hf[optimizerWeights][optimizer][layers[i]][kernel].keys())[1]
                kernelMValue = np.array(hf[optimizerWeights][optimizer][layers[i]][kernel][kernelM])
                kernelVValue = np.array(hf[optimizerWeights][optimizer][layers[i]][kernel][kernelV])
                arr.extend([biasMValue, biasVValue, kernelMValue, kernelVValue])
                saved.append(arr)
                del hf[optimizerWeights][optimizer][layers[i]]

            hf.create_group(optimizerData[0])  # dense_i
            hf.create_group(optimizerData[1])  # dense_i/bias
            hf.create_dataset(optimizerData[2], data=np.random.rand(self.y, ))  # bias/m:0
            hf.create_dataset(optimizerData[3], data=np.random.rand(self.y, ))  # bias/v:0
            hf.create_group(optimizerData[4])  # dense_i/kernel
            hf.create_dataset(optimizerData[5], data=np.random.rand(optimizerData[7], self.y))  # dense_i/kernel/m:0
            hf.create_dataset(optimizerData[6], data=np.random.rand(optimizerData[7], self.y))  # dense_i/kernel/v:0

            for i in range(self.x, len(optimizerGroups)):
                hf.create_group(optimizerGroups[i][0])
                hf.create_group(optimizerGroups[i][1])
                hf.create_group(optimizerGroups[i][4])
                if (i == self.x):
                    hf.create_dataset(optimizerGroups[i][2], data=np.random.rand(optimizerData[7], ))  # Bias m:0
                    hf.create_dataset(optimizerGroups[i][3], data=np.random.rand(optimizerData[7], ))  # Bias v:0
                    hf.create_dataset(optimizerGroups[i][5],
                                      data=np.random.rand(self.y, optimizerData[7]))  # Kernel m:0
                    hf.create_dataset(optimizerGroups[i][6],
                                      data=np.random.rand(self.y, optimizerData[7]))  # Kernel m:0
                else:
                    hf.create_dataset(optimizerGroups[i][2], data=saved[i - self.x][0])
                    hf.create_dataset(optimizerGroups[i][3], data=saved[i - self.x][1])
                    hf.create_dataset(optimizerGroups[i][5], data=saved[i - self.x][2])
                    hf.create_dataset(optimizerGroups[i][6], data=saved[i - self.x][3])

            arr = list(hf[optimizerWeights].attrs.__getitem__("weight_names"))
            last = optimizerGroups[len(optimizerGroups) - 1][0].replace("optimizer_weights/Adam/", "")
            biasName = optimizer + "/" + last + "/" + "bias/"
            kernelName = optimizer + "/" + last + "/" + "kernel/"
            pos = int((len(arr) - 1) / 2)
            arr.insert(pos + 1, biasName + "m:0")
            arr.insert(pos + 1, kernelName + "m:0")
            arr.extend([kernelName + "v:0", biasName + "v:0"])
            hf[optimizerWeights].attrs.__setitem__("weight_names", arr)

        else:
            for i in range(self.x + 1, len(layers)):
                arr = []
                bias = list(hf[optimizerWeights][optimizer][layers[i]].keys())[0]
                kernel = list(hf[optimizerWeights][optimizer][layers[i]].keys())[1]
                biasM = list(hf[optimizerWeights][optimizer][layers[i]][bias].keys())[0]
                biasV = list(hf[optimizerWeights][optimizer][layers[i]][bias].keys())[1]
                biasMValue = np.array(hf[optimizerWeights][optimizer][layers[i]][bias][biasM])
                biasVValue = np.array(hf[optimizerWeights][optimizer][layers[i]][bias][biasV])
                kernelM = list(hf[optimizerWeights][optimizer][layers[i]][kernel].keys())[0]
                kernelV = list(hf[optimizerWeights][optimizer][layers[i]][kernel].keys())[1]
                kernelMValue = np.array(hf[optimizerWeights][optimizer][layers[i]][kernel][kernelM])
                kernelVValue = np.array(hf[optimizerWeights][optimizer][layers[i]][kernel][kernelV])
                arr.extend([biasMValue, biasVValue, kernelMValue, kernelVValue])
                saved.append(arr)
                del hf[optimizerWeights][optimizer][layers[i]]

            del hf[optimizerWeights][optimizer][layers[self.x]]
            # print(optimizerGroups)
            # print(optimizerData)
            for i in range(self.x, len(optimizerGroups)):
                hf.create_group(optimizerGroups[i][0])
                hf.create_group(optimizerGroups[i][1])
                hf.create_group(optimizerGroups[i][4])
                if (i == self.x):
                    hf.create_dataset(optimizerGroups[i][2], data=np.random.rand(optimizerData[1]))  # Bias m:0
                    hf.create_dataset(optimizerGroups[i][3], data=np.random.rand(optimizerData[1]))  # Bias v:0
                    hf.create_dataset(optimizerGroups[i][5],
                                      data=np.random.rand(optimizerData[0], optimizerData[1]))  # Kernel m:0
                    hf.create_dataset(optimizerGroups[i][6],
                                      data=np.random.rand(optimizerData[0], optimizerData[1]))  # Kernel v:0
                else:
                    hf.create_dataset(optimizerGroups[i][2], data=saved[i - self.x][0])
                    hf.create_dataset(optimizerGroups[i][3], data=saved[i - self.x][1])
                    hf.create_dataset(optimizerGroups[i][5], data=saved[i - self.x][2])
                    hf.create_dataset(optimizerGroups[i][6], data=saved[i - self.x][3])

            arr = list(hf[optimizerWeights].attrs.__getitem__("weight_names"))
            # layer_names = list(hf[model_weights].attrs.__getitem__('layer_names'))

            # print(arr)
            name = layers[0] + "_" + str(len(layers) - 1)
            # print(arr)
            # print("name " + name)
            '''for i in range(len(arr)):
                if(name in arr[i]):
                    del arr[i]
                    print(arr[i])'''
            arr = [arr[i] for i in range(len(arr)) if name not in arr[i]]
            # print(arr)

            hf[optimizerWeights].attrs.__setitem__("weight_names", arr)
