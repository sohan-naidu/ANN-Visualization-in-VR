from ipaddress import ip_interface
from platform import node
from xml.etree.ElementTree import tostring
import onnx_graphsurgeon as gs
import onnx 
import numpy as np
import re 

graph = gs.import_onnx(onnx.load("D:/Unity/UnityProjects/Review 1/Assets/Scripts/Capstone/Sohan-Review1/ANN.onnx"))

print("LOADED")

nodes = [node for node in graph.nodes]

print(nodes)

for n in range(len(nodes)):
    for ip in nodes[n].inputs:
        ip.name = re.sub(":", "", ip.name)
    for ip in nodes[n].outputs:
        ip.name = re.sub(":", "", ip.name)    

print(nodes)


#nodes.inputs[1].name = re.sub(":", "", nodes.inputs[1].name)
#print(nodes.inputs[1].name)

graph.cleanup()
onnx.save(gs.export_onnx(graph), "D:\\Unity\\UnityProjects\\Review 1\\Assets\\Scripts\\Capstone\\Sohan-Review1\\ANN.onnx")







'''nodes = [node.name for node in graph.nodes]
print(nodes)

for i in range(len(nodes)):
    nodes[i] = "node" + str(i)

i = 0
for node in graph.nodes:
    node.name = nodes[i]
    i += 1

graph.cleanup()
onnx.save(gs.export_onnx(graph), "D:\\Unity\\UnityProjects\\Review 1\\Assets\\Scripts\\Capstone\\Sohan-Review1\\udpatedANN.onnx")'''


'''matmul = [node for node in graph.nodes if node.op == "MatMul"][0]

copynode = matmul.inputs[1].copy()

ip = matmul.outputs

new = gs.Constant(name = "new", values = np.ones(shape = (7, 6)))

for i in range((copynode.shape[0])):
    for j in range(copynode.shape[1]):
        new.values[i][j] = copynode.values[i][j]



#print(new.shape)

matmul.inputs[1] = new

graph.cleanup()
onnx.save(gs.export_onnx(graph), "D:\\Unity\\UnityProjects\\Review 1\\Assets\\Scripts\\Capstone\\Sohan-Review1\\udpatedANN.onnx")
print("OK")
#print(copynode.values)'''



