from keras import models
import os
import tf2onnx
from globals import OUTPUT_DIR, EPOCHS_DIR, H5, ONNX

class ConvertToONNX:
    def __init__(self, fileName):
        self.fileName = fileName

    def convert(self):
        try:
            os.chdir(os.path.dirname(os.path.abspath(__file__)) + OUTPUT_DIR)
            model = models.load_model(self.fileName + H5)
            os.chdir(os.path.dirname(os.path.abspath(__file__)) + EPOCHS_DIR)
            tf2onnx.convert.from_keras(model, output_path = self.fileName + ONNX)
        except Exception as e:
            print("Failed.\n" + str(e))