#import sys
import os
import argparse
from convertToONNX import convertToONNX as cto
from neurons import neurons
from layers import layers
from init import init

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
        
        converter.convert()

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description = "Backend argument parser.")
    parser.add_argument('op', type = str, help = 'Operation that needs to be done in backend.')
    parser.add_argument('x', type = int, help = 'Layer index.')
    parser.add_argument('y', type = int, help = 'Number of neurons.')
    parser.add_argument('-r', action = 'store_true', help = 'Pass this argument to run init.py to reset model.')
    args = parser.parse_args()

    if(args.r):
        inst = init()
        inst.reset()
    
    back = backend(args.op, args.x, args.y)
    back.do()