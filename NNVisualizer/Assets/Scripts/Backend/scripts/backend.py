import argparse
from init import Initialize
from neurons import Neurons
from layers import Layers
from train import Train
from reset import Reset

class Backend:
    def parse(self):
        parser = argparse.ArgumentParser(description = "Backend argument parser.")
        parser.add_argument("-i", action = 'store_true', help = 'Set this flag to initialize. Creates new model and trains it for 50 epochs.')
        parser.add_argument("-r", action = 'store_true', help = 'Set this flag to reset and clear epochs and output folder.')
        args, unknown = parser.parse_known_args()
        currentEpoch = 0
    
        if(vars(args)['r']):
            Reset().reset()

        elif(vars(args)['i']):
            Initialize().initialize()
            for _ in range(11):
                curname = "epoch_" + str(currentEpoch)
                trainer = Train()
                trainer.train(curname)
                currentEpoch += 5
        else:
            parser.add_argument('op', type = str, help = 'Operation that needs to be done in backend.')
            parser.add_argument('x', type = int, help = 'Layer index.')
            parser.add_argument('y', type = int, help = 'Number of neurons/Neuron position.')
            parser.add_argument("epoch", type = int, help = "Epoch number from which the onnx files need to be rewritten")
            args = parser.parse_args()
            currentEpoch = args.epoch
            curname = "epoch_" + str(currentEpoch)
            if(args.op == "add" or args.op == "del"):
                Neurons(args.op, args.x, args.y, curname).modify()
            else:
                Layers(args.op, args.x, args.y, curname).modify()
            for i in range(11):
                curname = "epoch_" + str(currentEpoch)
                trainer = Train()
                if(i == 0):
                    trainer.train(curname, True)
                else:
                    trainer.train(curname)
                currentEpoch += 5


if __name__ == '__main__':
    Backend().parse()