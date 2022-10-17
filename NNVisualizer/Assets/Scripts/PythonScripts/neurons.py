import h5py
import numpy as np
import json
import convertToONNX as cto

class neurons():
    def __init__(self, op, x, y):
        self.x = x
        self.y = y
        self.op = op
        if(self.op == "add"):
            self.modify(1)
        else:
            self.modify(0)

    def modify(self, flag):
    
        hf = h5py.File('../output/input.h5', 'r+')
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
            new = np.random.normal(-0.3, 0.3, size = (old.shape[0]))
            upd = np.insert(old, self.y, new, axis = 1)
            newbias = np.zeros(1)
            updbias = np.insert(oldbias, self.y, newbias, axis = 0)
            arch['config']['layers'][self.x + 1]['config']['units'] += 1 # Updating architecture, 1-indexed
        else:
            upd = np.delete(old, self.y, axis = 1)
            updbias = np.delete(oldbias, self.y, axis = 0)
            arch['config']['layers'][self.x + 1]['config']['units'] -= 1
        #ith kernel
        del hf[groups[index]]
        hf.create_dataset(groups[index], (upd.shape[0], upd.shape[1]), data = upd)
        #ith bias
        del hf[groups[index - 1]]
        hf.create_dataset(groups[index - 1], (upd.shape[1],), data = updbias)
        #(i + 1)th kernel
        if(index + 2 < len(groups)):
            oldnextkernel = np.array(hf[groups[index + 2]])
            prev = hf[groups[index + 2]].shape[1]
            if(flag):
                newnextkernel = np.zeros(shape = (1, prev))
                updnextkernel = np.insert(oldnextkernel, self.y, newnextkernel, axis = 0)
            else:
                updnextkernel = np.delete(oldnextkernel, self.y, axis = 0)
            del hf[groups[index + 2]]
            hf.create_dataset(groups[index + 2], (upd.shape[1], prev), data = updnextkernel)
            
        updarch = json.dumps(arch)
        hf.attrs.modify("model_config", updarch)
        hf.close()