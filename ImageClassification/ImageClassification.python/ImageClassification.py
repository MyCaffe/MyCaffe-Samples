# == Image Classification Sample ==
# IMPORTANT: This sample requires the MyCaffe AI Platform version 0.11.0.65 or greater.
# This sample requires:
#   * Installing and running the MyCaffe AI Platform which is located at
#     https://github.com/MyCaffe/MyCaffe/releases
# Once installed, make sure to create the database and load the MNIST dataset
# which are completed by doing the following:
#    a.) Install Microsoft SQL Express (or SQL)
#    b.) Run the MyCaffe Test Application
#    c.) Create the Database - select the 'Database | Create Database' menu.
#    d.) Load MNIST - select the 'Database | Load MNIST' menu.
# After loading MNIST, you will need to then do the following:
#    e.) Create a 64-bit Python environment.
#    f.) Install PythonNet - run 'pip install pythonnet'

import clr
clr.AddReference('C:\Program Files\SignalPop\MyCaffe\MyCaffe.dll')
from MyCaffe import *
clr.AddReference('C:\Program Files\SignalPop\MyCaffe\MyCaffe.basecode.dll')
from MyCaffe.basecode import *
clr.AddReference('C:\Program Files\SignalPop\MyCaffe\MyCaffe.db.image.dll')
from MyCaffe.db.image import *

def onWriteLine(sender, e):
    print(e.Message)

log = Log('test')
log.OnWriteLine += onWriteLine
cancel = CancelEvent()
settings = SettingsCaffe()

# Load all images into memory before training
settings.ImageDbLoadMethod = IMAGEDB_LOAD_METHOD.LOAD_ALL
# Use GPU ID = 0
settings.GpuIds = '0'

# Load the descriptors from their respective files
# (installed by MyCaffe Test Application install)
file = open("C:\\ProgramData\\MyCaffe\\test_data\\models\\mnist\\lenet_solver.prototxt", "r")
strSolver = file.read()
file = open("C:\\ProgramData\\MyCaffe\\test_data\\models\\mnist\\lenet_train_test.prototxt", "r")
strModel = file.read()

# Load the MNIST dataset descriptor.
factory = DatasetFactory()
ds = factory.LoadDataset('MNIST')

# Create a test project with the dataset and descriptors
project = ProjectEx('Test')
project.SetDataset(ds)
project.ModelDescription = strModel
project.SolverDescription = strSolver

# Create the MyCaffeControl (with the 'float' base type)
strCudaPath = "C:\\Program Files\\SignalPop\\MyCaffe\\cuda_11.0\\CudaDnnDll.11.0.dll"
mycaffe = MyCaffeControl[float](settings, log, cancel, None, None, None, None, strCudaPath)

# Load the project, using the TRAIN phase
mycaffe.Load(Phase.TRAIN, project)

# Train the model for 5000 iterations
# (which uses the internal solver and internal training net)
nIterations = 5000
mycaffe.Train(nIterations)

# Test the model for 100 iterations
# (which uses the internal testing net)
nIterations = 100
dfAccuracy = mycaffe.Test(nIterations)

# Report the testing accuracy.
log.WriteLine('Accuracy = ' + str(dfAccuracy))

mycaffe.Dispose()
