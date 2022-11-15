import shutil
from convertToONNX import convertToONNX as cto
import os
import glob

class reset:
    def reset(self):
        files = glob.glob("../epochs/*")
        for file in files:
            os.remove(file)
            #print(file)
        files = glob.glob("../output/*")
        for file in files:
            os.remove(file)
