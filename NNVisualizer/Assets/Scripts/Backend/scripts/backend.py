import click
from models import *
from utils import *


@click.command()
@click.option("--i", "initialize_flag", type=bool, default=False, is_flag=True,
              help='Set this flag to initialize. Creates new model and trains it for 50 epochs.')
@click.option('--r', "reset_flag", type=bool, default=False, is_flag=True,
              help='Set this flag to reset and clear epochs and output folder.')
@click.argument('op', type=str, default="pass", required=False)
@click.argument('x', type=int, default=-1, required=False)
@click.argument('y', type=int, default=-1, required=False)
@click.argument('epoch', type=int, default=-1, required=False)
def parse(initialize_flag, reset_flag, op, x, y, epoch):
    trainer = Train()
    current_epoch = 0

    if reset_flag:
        reset()

    if initialize_flag:
        initialize()
        for _ in range(5):
            current_name = "epoch_" + str(current_epoch)
            trainer.train(current_name, init=True, cur=False)
            current_epoch += 5

    current_epoch = epoch
    current_name = "epoch_" + str(current_epoch)
    if op == "add" or op == "del":
        Neurons(op, x, y, current_name).modify()
    else:
        Layers(op, x, y, current_name).modify()
    for i in range(5):
        current_name = "epoch_" + str(current_epoch)
        trainer = Train()
        if i == 0:
            trainer.train(current_name, init = False, cur = True)
        else:
            trainer.train(current_name, init = False, cur = False)
        current_epoch += 5


if __name__ == '__main__':
    parse()
