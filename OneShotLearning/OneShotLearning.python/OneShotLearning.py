# == OneShot Learning Sample ==
# IMPORTANT: This sample requires the MyCaffe AI Platform version 0.11.2.9 or greater.
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
from MyCaffe.common import *
from MyCaffe.param import *
from MyCaffe.param.beta import *
clr.AddReference('C:\Program Files\SignalPop\MyCaffe\MyCaffe.basecode.dll')
from MyCaffe.basecode import *
clr.AddReference('C:\Program Files\SignalPop\MyCaffe\MyCaffe.db.image.dll')
from MyCaffe.db.image import *
from random import randint
from random import random


#----------------------------------------------------------------------------------------
# Functions
#----------------------------------------------------------------------------------------

def onWriteLine(sender, e):
    print(e.Message)

#----------------------------------------------------------------------------------------
# Main Script
#----------------------------------------------------------------------------------------

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
file = open("C:\\ProgramData\\MyCaffe\\test_data\\models\\siamese\\mnist\\solver.prototxt", "r")
strSolver = file.read()
file = open("C:\\ProgramData\\MyCaffe\\test_data\\models\\siamese\\mnist\\train_val.prototxt", "r")
strModel = file.read()

# Change Decode to KNN (from default CENTROID)
proto = RawProto.Parse(strModel)
net_param = NetParameter.FromProto(proto)
for layer in net_param.layer:
    if (layer.type == LayerParameter.GetType("DECODE")):
        layer.decode_param.target = DecodeParameter.TARGET.KNN
        break
proto = net_param.ToProto("root")
strModel = proto.ToString()

# Load the MNIST dataset descriptor.
factory = DatasetFactory()
ds = factory.LoadDataset('MNIST')
    
# Create a test project with the dataset and descriptors
project = ProjectEx('Test')
project.SetDataset(ds)
project.ModelDescription = strModel
project.SolverDescription = strSolver

# Create the MyCaffeControl (with the 'float' base type)
strCudaPath = "C:\\Program Files\\SignalPop\\MyCaffe\\cuda_11.2\\CudaDnnDll.11.2.dll"
mycaffe = MyCaffeControl[float](settings, log, cancel, None, None, None, None, strCudaPath)

# Load the project, using the TRAIN phase
mycaffe.Load(Phase.TRAIN, project)

# Train the model for 1200 iterations
# (which uses the internal solver and internal training net)
nIterations = 1200
mycaffe.Train(nIterations)

# Test the model for 100 iterations
# (which uses the internal testing net)
nIterations = 100
dfAccuracy = mycaffe.Test(nIterations)

# Report the testing accuracy.
log.WriteLine('Accuracy = ' + str(dfAccuracy))

mycaffe.Dispose()

