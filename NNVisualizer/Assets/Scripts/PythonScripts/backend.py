from convertToONNX import convertToONNX as cto
from neurons import neurons
from layers import layers
import sys

class backend:

    def __init__(self, a, op, x, y):
        self.a = a
        self.op = op
        self.x = x
        self.y = y

    def do(self):
        neuron = neurons(self.op, self.x, self.y)
        converter = cto("input.h5", "../input")
        layer = layers(self.op, self.x, self.y)
        if(self.a == "n"):
            if(self.op == "add"):
                neuron.modify(1)
            else:
                neuron.modify(0)
        else:
            layer.modify()
        
        converter.convert()

if __name__ == '__main__':
    [_, a, op, x, y] = sys.argv
    x = int(x)
    y = int(y) 
    back = backend(a, op, x, y)
    back.do()