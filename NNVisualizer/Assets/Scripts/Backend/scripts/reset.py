import os
import glob
from globals import EPOCHS_DIR, OUTPUT_DIR, INPUT_DIR

class Reset:
    def reset(self):
        os.chdir(os.path.dirname(os.path.abspath(__file__)) + EPOCHS_DIR)
        files = glob.glob("*")
        for file in files:
            os.remove(file)
        os.chdir(os.path.dirname(os.path.abspath(__file__)) + OUTPUT_DIR)
        files = glob.glob("*")
        for file in files:
            os.remove(file)
