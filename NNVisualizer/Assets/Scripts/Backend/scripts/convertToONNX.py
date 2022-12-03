from keras import models
import os
import tf2onnx
from globals import OUTPUT_DIR, EPOCHS_DIR, H5, ONNX

class ConvertToONNX:
    def __init__(self, fileName):
        self.fileName = fileName

    def convert(self):
        try:
            os.chdir(OUTPUT_DIR)
            model = models.load_model(self.fileName + H5)
            tf2onnx.convert.from_keras(model, output_path = EPOCHS_DIR + self.fileName + ONNX)
        except Exception as e:
            print("Failed.\n" + str(e))