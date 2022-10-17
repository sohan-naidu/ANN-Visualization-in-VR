from keras import models
import os
import tf2onnx

class convertToONNX:
    def __init__(self, fileName, source):
        self.fileName = fileName
        self.source = source

    def convert(self):
        try:
            os.chdir(self.source)
            model = models.load_model(self.fileName)
            tf2onnx.convert.from_keras(model, output_path = "../input/tontyfive.onnx")
            print("Successfully converted to ONNX.")
        except Exception as e:
            print("Failed.\n" + str(e))