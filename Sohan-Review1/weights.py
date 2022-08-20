from ast import Constant
import onnx_graphsurgeon as gs
import onnx 

graph = gs.import_onnx(onnx.load("D:/Unity/UnityProjects/Barracuda Testing/Assets/Scripts/onnxmodel.onnx"))

node = [node for node in graph.nodes if node.op == "MatMul"][0]

ip = node.inputs[1]

#print(help(Constant))

print(ip.values[0][0])

ip.values[0][0] = 0

print(ip.values[0][0])


graph.cleanup()
onnx.save(gs.export_onnx(graph), "weights.onnx")