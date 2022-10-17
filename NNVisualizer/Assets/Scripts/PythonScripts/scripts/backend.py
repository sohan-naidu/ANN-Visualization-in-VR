from convertToONNX import convertToONNX as cto
from neurons import neurons
from layers import layers
import sys
import os

class backend:
    def __init__(self, op, x, y):
        self.op = op
        self.x = x
        self.y = y

    def do(self):
        neuron = neurons(self.op, self.x, self.y)
        converter = cto("input.h5", os.path.dirname(os.path.abspath(__file__)) + '/../output/')
        layer = layers(self.op, self.x, self.y)
        if(self.op == "add"):
            #if(self.op == "add"):
            neuron.modify(1)
            #else:
                #neuron.modify(0)
        else:
            layer.modify()
            pass
        
        converter.convert()

if __name__ == '__main__':
    [_, op, x, y] = sys.argv
    x = int(x)
    y = int(y) 
    back = backend(op, x, y)
    back.do()