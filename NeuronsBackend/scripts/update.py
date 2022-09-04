import sys
import h5py
import numpy as np

def modify(index, flag):
    old = np.array(hf[b[index]])
    oldbias = np.array(hf[b[index - 1]])
    if(flag):
        new = np.random.normal(-0.3, 0.3, size = (old.shape[0]))
        upd = np.insert(old, y, new, axis = 1)
        newbias = np.zeros(1)
        updbias = np.insert(oldbias, y, newbias, axis = 0)
    else:
        upd = np.delete(old, y, axis = 1)
        updbias = np.delete(oldbias, y, axis = 0)
    #ith kernel
    del hf[b[index]]
    hf.create_dataset(b[index], (upd.shape[0], upd.shape[1]), data = upd)
    #ith bias
    del hf[b[index - 1]]
    hf.create_dataset(b[index - 1], (upd.shape[1],), data = updbias)
    #(i + 1)th kernel
    if(index + 2 < len(b)):
        oldnextkernel = np.array(hf[b[index + 2]])
        prev = hf[b[index + 2]].shape[1]
        if(flag):
            newnextkernel = np.zeros(shape = (1, prev))
            updnextkernel = np.insert(oldnextkernel, y, newnextkernel, axis = 0)
        else:
            updnextkernel = np.delete(oldnextkernel, y, axis = 0)
        del hf[b[index + 2]]
        hf.create_dataset(b[index + 2], (upd.shape[1], prev), data = updnextkernel)

[_, op, x, y] = sys.argv
x = int(x)
y = int(y)

hf = h5py.File('../output/input.h5', 'r+')
groups = []
hf.visit(groups.append)
b = [groups[i] for i in range(len(groups)) if "model" in groups[i] and ("bias" in groups[i] or "kernel" in groups[i])]

index = 2*x + 1
if(index < 0 or index >= len(b)):
    raise IndexError("Invalid layer.")
if(index == len(b) - 1):
    raise IndexError("Cannot modify output layer.")

if(op == "add"):
    try:
        modify(index, 1)
        hf.close()
        print("Successfully added a neuron to layer " + str(x) + " at position " + str(y) + ".")
    except IndexError:
        print("Invalid layer.")

elif(op == "del"):
    try:
        modify(index, 0)
        hf.close()
        print("Successfully deleted a neuron in layer " + str(x) + " at position " + str(y) + ".")
    except Exception as e:
        print("Invalid position." + str(e))

else:
    raise Exception("Invalid operator.")