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
            model = models.load_model(self.fileName + ".h5")
            tf2onnx.convert.from_keras(model, output_path = os.path.dirname(os.path.abspath(__file__)) + "/../epochs/" + self.fileName + ".onnx")
        except Exception as e:
            print("Failed.\n" + str(e))