import sys
import h5py
import numpy as np

def modify(upd, index):
    #ith kernel
    del hf[b[index]]
    hf.create_dataset(b[index], (upd.shape[0], upd.shape[1]), data = upd)
    #ith bias
    del hf[b[index - 1]]
    hf.create_dataset(b[index - 1], (upd.shape[1],))
    #(i + 1)th kernel
    index = 2*(x + 1) + 1
    if(index < len(b)/2):
        prev = hf[b[index]].shape[1]
        del hf[b[index]]
        hf.create_dataset(b[index], (upd.shape[1], prev))

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
old = np.array(hf[b[index]])

if(op == "add"):
    new = np.random.normal(-0.3, 0.3, size = (old.shape[0]))
    try:
        upd = np.insert(old, y, new, axis = 1)
        modify(upd, index)
        hf.close()
        print("Successfully added a neuron to layer " + str(x) + " at position " + str(y) + ".")
    except IndexError:
        print("Invalid position.")

elif(op == "del"):
    try:
        upd = np.delete(old, y, axis = 1)
        modify(upd, index)
        hf.close()
        print("Successfully deleted a neuron in layer " + str(x) + " at position " + str(y) + ".")
    except IndexError:
        print("Invalid position.")

else:
    raise Exception("Invalid operator.")