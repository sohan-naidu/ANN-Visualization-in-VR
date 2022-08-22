import h5py
import numpy as np

hf = h5py.File('D:\\Unity\\UnityProjects\\Review 1\\Assets\\Scripts\\Capstone\\Sohan-Review1\\weights', 'r+')


arr = np.array(hf['dense_1/dense_1/kernel:0'])
print(arr)
newarr = np.random.normal(-0.3, 0.3, size=(arr.shape[0], 1))

updatedarr = np.append(arr, newarr, axis = 1)

del hf['dense_1/dense_1/kernel:0']
newdata = hf.create_dataset("dense_1/dense_1/kernel:0", (5, 6), maxshape = (100, 100), data = updatedarr)

del hf['dense_1/dense_1/bias:0']
newdata = hf.create_dataset("dense_1/dense_1/bias:0", (6, ), maxshape = (100, ))

del hf['dense_2/dense_2/kernel:0']
newdata = hf.create_dataset("dense_2/dense_2/kernel:0", (6, 1), maxshape = (100, 100))

hf.close()