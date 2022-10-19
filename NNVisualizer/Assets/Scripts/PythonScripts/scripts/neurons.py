import h5py
import numpy as np
import json
import convertToONNX as cto
import os

class neurons():
    def __init__(self, op, x, y):
        self.x = x
        self.y = y
        self.op = op

    def modify(self, flag):
    
        hf = h5py.File(os.path.dirname(os.path.abspath(__file__)) + '/../output/input.h5', 'r+')
        arch = json.loads(hf.attrs.get("model_config"))

        all = []
        hf.visit(all.append)

        groups = [all[i] for i in range(len(all)) if "model" in all[i] and ("bias" in all[i] or "kernel" in all[i])]
        
        index = 2*(self.x) + 1
        if(index < 0 or index >= len(groups)):
            raise IndexError("Invalid layer.")
        if(index == len(groups) - 1):
            raise IndexError("Cannot modify output layer.")

        old = np.array(hf[groups[index]])
        oldbias = np.array(hf[groups[index - 1]])
        if(flag):
            new = np.zeros(shape = old.shape[0]).reshape(old.shape[0], 1)
            newbias = np.zeros(1)
            for _ in range(self.y):   
                old = np.append(old, new, axis = 1)
                oldbias = np.append(oldbias, newbias)
            
            arch['config']['layers'][self.x + 1]['config']['units'] += self.y # Updating architecture, 1-indexed
        else:
            upd = np.delete(old, self.y, axis = 1)
            updbias = np.delete(oldbias, self.y, axis = 0)
            arch['config']['layers'][self.x + 1]['config']['units'] -= 1
        #ith kernel
        del hf[groups[index]]
        hf.create_dataset(groups[index], data = old)
        #ith bias
        del hf[groups[index - 1]]
        hf.create_dataset(groups[index - 1], data = oldbias)
        #(i + 1)th kernel
        if(index + 2 < len(groups)):
            oldnextkernel = np.array(hf[groups[index + 2]])
            
            prev = hf[groups[index + 2]].shape[1]
            if(flag):
                newnextkernel = np.zeros(shape = (1, prev))
                for _ in range((self.y)):
                    oldnextkernel = np.append(oldnextkernel, newnextkernel, axis = 0)
            else:
                updnextkernel = np.delete(oldnextkernel, self.y, axis = 0)
            del hf[groups[index + 2]]
            hf.create_dataset(groups[index + 2], data = oldnextkernel)
            
        updarch = json.dumps(arch)
        hf.attrs.modify("model_config", updarch)
        hf.close()