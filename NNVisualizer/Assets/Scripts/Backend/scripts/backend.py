import argparse
from init import initialize
from neurons import neurons
from layers import layers
from convertToONNX import convertToONNX as cto
from keras import models
import os
from train import train
from reset import reset
    
class backend:
    def parse(self):
        parser = argparse.ArgumentParser(description = "Backend argument parser.")
        parser.add_argument("-i", action = 'store_true')
        parser.add_argument("-r", action = 'store_true')
        args, unknown = parser.parse_known_args()
        cur = 5
    
        if(vars(args)['r']):
            reset().reset()

        elif(vars(args)['i']):
            initialize().initialize()
            for _ in range(10):
                curname = "epoch_" + str(cur)
                trainer = train()
                trainer.train(curname)
                cur += 5
        else:
            parser.add_argument('op', type = str, help = 'Operation that needs to be done in backend.')
            parser.add_argument('x', type = int, help = 'Layer index.')
            parser.add_argument('y', type = int, help = 'Number of neurons/Neuron position.')
            parser.add_argument("epoch", type = int, help = "Epoch number from which the onnx files need to be rewritten")
            args = parser.parse_args()
            cur = args.epoch
            curname = "epoch_" + str(cur)
            #backend_ = backend(args.op, args.x, args.y, curname)
            if(args.op == "add" or args.op == "del"):
                neurons(args.op, args.x, args.y, curname).modify()
            else:
                layers(args.op, args.x, args.y, curname).modify()
            for i in range(11):
                curname = "epoch_" + str(cur)
                trainer = train()
                if(i == 0):
                    trainer.train(curname, True)
                else:
                    trainer.train(curname)
                cur += 5


if __name__ == '__main__':
    backend().parse()