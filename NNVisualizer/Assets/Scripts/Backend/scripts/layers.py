import h5py
import json
import numpy as np
import copy
import os

class layers():
    def __init__(self, op, x, y):
        self.op = op
        self.x = x
        self.y = y
        
    def modify(self):
        hf = h5py.File(os.path.dirname(os.path.abspath(__file__)) + '/../output/input.h5', 'r+')
        arch = json.loads(hf.attrs.get("model_config"))
        all = []
        hf.visit(all.append)

        groups = [all[i] for i in range(len(all)) if "model" in all[i]]
        groups = groups[1 : -1]          # Ignores model_weights and top_level_model_weights

        layerGroups = self.getGroups(groups, 4)

        groups = [all[i] for i in range(len(all)) if "optimizer" in all[i]]
        groups = groups[2 : -1]  # Ignores optimizer_weights, optimizer_weights/optimizer and iter:0

        optimizerGroups = self.getGroups(groups, 7)

        #  Architecture part 
        allLayers = arch['config']['layers']
        layers = allLayers[1 : ]

        groupData = None
        optimizerData = None
        for i in range(self.x, len(layerGroups)): # Name change and data copy
            if(i == self.x):
                prev = np.array(hf[layerGroups[i - 1][3]]).shape[1] # Collects shape of previous layer's kernel
                groupData = copy.deepcopy(layerGroups[i])  # Collects only name of current group cause new group will have this name
                optimizerData = copy.deepcopy(optimizerGroups[i])
                newDict = copy.deepcopy(layers[self.x])
                newDict['config']['units'] = self.y
                newDict['config']['activation'] = 'relu'
                groupData.append(prev)
                optimizerData.append(prev)

            old = layers[i]['config']['name']
            new = self.updateName(layers[i]['config']['name'])

            layers[i]['config']['name'] = new
            for j in range(len(layerGroups[i])):                 # -1 cause last element in groups is in int (prev layer shape)
                layerGroups[i][j] = layerGroups[i][j].replace(old, new)
            for j in range(len(optimizerGroups[i])):
                optimizerGroups[i][j] = optimizerGroups[i][j].replace(old, new)
        self.modifygroups(hf, layerGroups, groupData)
        self.modifyopt(hf, optimizerGroups, optimizerData)

        layers.insert(self.x, newDict)
        allLayers[1 : ] = layers
        arch['config']['layers'] = allLayers
        updarch = json.dumps(arch)
        hf.attrs.modify("model_config", updarch)
        hf.close()

    def getGroups(self, groups, jump):
        current = []
        arr = []
        for i in range(0, len(groups), jump):
            for j in range(i, i + jump):
                arr.append(groups[j])
            current.append(arr)
            arr = []
        return current


    def updateName(self, oldName):
        name, number = "", ""
        for j in range(len(oldName)):
            if(oldName[j].isdigit()):
                number += oldName[j]
            else:
                name += oldName[j]
        num = int(number)
        num += 1
        number = str(num)
        newName = name + number
        return newName

    def modifygroups(self, hf, layerGroups, groupData):

        model_weights = list(hf.keys())[0] # model_weights
        allLayers = list(hf[model_weights].keys())
        layers = allLayers[ : -1]   # Ignore top_level_model_weights
        saved = []

        for i in range(self.x, len(layers)): # Deletion of old layers and saving weights
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
        hf.create_dataset(groupData[2], data = np.random.rand(self.y, ))  # should be bias
        hf.create_dataset(groupData[3], data = np.zeros([groupData[4], self.y]))   # should be kernel
        kernelName = allLayers[0] + "_" + str(self.x) + "/kernel:0"
        biasName = allLayers[0] + "_" + str(self.x) + "/bias:0"
        newGroup.attrs.__setitem__("weight_names", (kernelName, biasName))

        # Readding previously deleted layers with updated names

        for i in range(self.x, len(layerGroups)):
            newGroup = hf.create_group(layerGroups[i][0])
            hf.create_group(layerGroups[i][1])
            if(i == self.x):
                hf.create_dataset(layerGroups[i][2], data = saved[i - self.x][0])  # Bias
                hf.create_dataset(layerGroups[i][3], data = np.zeros([self.y, saved[i - self.x][1].shape[1]]))
            else:
                hf.create_dataset(layerGroups[i][2], data = saved[i - self.x][0]) 
                hf.create_dataset(layerGroups[i][3], data = saved[i - self.x][1]) 
            kernelName = allLayers[0] + "_" + str(i + 1) + "/kernel:0"
            biasName = allLayers[0] + "_" + str(i + 1) + "/bias:0"
            newGroup.attrs.__setitem__("weight_names", (kernelName, biasName))

        last = layerGroups[len(layerGroups) - 1][0].replace("model_weights/", "")
        layer_names = list(hf[model_weights].attrs.__getitem__('layer_names'))
        layer_names.append(last)
        hf[model_weights].attrs.__setitem__('layer_names', layer_names)

    def modifyopt(self, hf, optimizerGroups, optimizerData):

        optimizerWeights = list(hf.keys())[1] # optimizer_weights
        optimizer = list(hf[optimizerWeights].keys())[0]  # Optimizier (Adam)
        allLayers = list(hf[optimizerWeights][optimizer].keys())
        layers = allLayers[ : -1] # Get hidden layers that need to be changed upto output layer
        saved = []

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
        hf.create_group(optimizerData[1])   # dense_i/bias
        hf.create_dataset(optimizerData[2], data = np.random.rand(self.y, ))  # bias/m:0
        hf.create_dataset(optimizerData[3], data = np.random.rand(self.y, ))  # bias/v:0
        hf.create_group(optimizerData[4])    # dense_i/kernel
        hf.create_dataset(optimizerData[5], data = np.random.rand(optimizerData[7], self.y))   #dense_i/kernel/m:0
        hf.create_dataset(optimizerData[6], data = np.random.rand(optimizerData[7], self.y))  # dense_i/kernel/v:0
        

        for i in range(self.x, len(optimizerGroups)):
            hf.create_group(optimizerGroups[i][0])
            hf.create_group(optimizerGroups[i][1])
            hf.create_group(optimizerGroups[i][4])
            if(i == self.x):
                hf.create_dataset(optimizerGroups[i][2], data = np.random.rand(optimizerData[7], ))    # Bias m:0
                hf.create_dataset(optimizerGroups[i][3], data = np.random.rand(optimizerData[7], ))    # Bias v:0
                hf.create_dataset(optimizerGroups[i][5], data = np.random.rand(self.y, optimizerData[7]))   # Kernel m:0
                hf.create_dataset(optimizerGroups[i][6], data = np.random.rand(self.y, optimizerData[7]))   # Kernel m:0
            else:
                hf.create_dataset(optimizerGroups[i][2], data = saved[i - self.x][0])
                hf.create_dataset(optimizerGroups[i][3], data = saved[i - self.x][1])
                hf.create_dataset(optimizerGroups[i][5], data = saved[i - self.x][2])
                hf.create_dataset(optimizerGroups[i][6], data = saved[i - self.x][3])

        arr = list(hf[optimizerWeights].attrs.__getitem__("weight_names"))
        last = optimizerGroups[len(optimizerGroups) - 1][0].replace("optimizer_weights/Adam/", "")
        biasName = optimizer + "/" + last + "/" + "bias/"
        kernelName = optimizer + "/" + last + "/" + "kernel/"
        pos = int((len(arr) - 1)/2)
        arr.insert(pos + 1, biasName + "m:0")
        arr.insert(pos + 1, kernelName + "m:0")
        arr.extend([kernelName + "v:0", biasName + "v:0"])
        hf[optimizerWeights].attrs.__setitem__("weight_names", arr)