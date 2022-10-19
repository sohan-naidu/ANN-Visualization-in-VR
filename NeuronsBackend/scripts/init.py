import shutil
from convertToONNX import convertToONNX as cto

class init:
    def reset(self):
        shutil.copy("../input/input.h5", "../output/")
        converter = cto("input.h5", "../output")
        converter.convert()