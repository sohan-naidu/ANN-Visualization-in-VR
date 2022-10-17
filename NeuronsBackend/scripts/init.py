from lib2to3.pytree import convert
import shutil
from convertToONNX import convertToONNX as cto

shutil.copy("../input/input.h5", "../output/")
converter = cto("input.h5", "../output")
converter.convert()