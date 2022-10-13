from keras import models
import os
import tf2onnx
import shutil

def convert(fileName):
    try:
        os.chdir("input")
        shutil.copy(fileName, "../output/")
        model = models.load_model(fileName)
        outputPath = "../output/input.onnx"
        tf2onnx.convert.from_keras(model, output_path = outputPath)
        print("Successfully converted to ONNX.")

    except Exception as e:
        print("Failed.\n" + str(e))