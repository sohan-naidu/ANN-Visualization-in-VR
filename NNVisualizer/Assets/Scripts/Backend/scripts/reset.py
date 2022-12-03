import os
import glob
from globals import EPOCHS_DIR, OUTPUT_DIR, INPUT_DIR

class Reset:
    def reset(self):
        os.chdir( os.path.dirname(os.path.abspath(__file__)))
        files = glob.glob(EPOCHS_DIR + "*")
        for file in files:
            os.remove(file)
        files = glob.glob(OUTPUT_DIR + "*")
        for file in files:
            os.remove(file)
