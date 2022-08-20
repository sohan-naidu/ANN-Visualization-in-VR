import onnx_graphsurgeon as gs
import onnx 

graph = gs.import_onnx(onnx.load("D:/Unity/UnityProjects/Barracuda Testing/Assets/Scripts/onnxmodel.onnx"))

print("LOADED")

del_node = [node for node in graph.nodes if node.op == "MatMul"][0]
input_nodes = del_node.i()
input_nodes.outputs = del_node.outputs
del_node.outputs.clear()

mod_node = [node for node in graph.nodes if node.op == "Relu"][0]
mod_node.op = "LeakyRelu"
mod_node.attrs["alpha"] = 0.02

graph.cleanup()
onnx.save(gs.export_onnx(graph), "updatedmodel.onnx")


print("DONE")