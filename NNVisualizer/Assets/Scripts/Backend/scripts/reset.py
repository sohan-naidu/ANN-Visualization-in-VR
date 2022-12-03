import os
import glob
from globals import EPOCHS_DIR, OUTPUT_DIR

class Reset:
    def reset(self):
        files = glob.glob(EPOCHS_DIR + "*")
        for file in files:
            os.remove(file)
        files = glob.glob(OUTPUT_DIR + "*")
        for file in files:
            os.remove(file)
