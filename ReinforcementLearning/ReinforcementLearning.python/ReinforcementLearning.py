# == Reinforcement Learning Sample ==
# IMPORTANT: This sample requires the MyCaffe AI Platform version 1.12.1.82 or greater.
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
#
# NOTE: The MyCaffe Test Application must be running when using this script, for the
# test application hosts the Cart-Pole gym used.

import clr # required pythonnet
clr.AddReference('C:\Program Files\SignalPop\MyCaffe\MyCaffe.dll')
from MyCaffe import *
from MyCaffe.common import *
clr.AddReference('C:\Program Files\SignalPop\MyCaffe\MyCaffe.basecode.dll')
from MyCaffe.basecode import *
clr.AddReference('C:\Program Files\SignalPop\MyCaffe\MyCaffe.gym.python.dll')
from MyCaffe.gym.python import *
from random import randint
from random import random

gym = MyCaffePythonGym()
# Init1 = the amount of force to use on each move.
# Init2 = 0 for non additive force, 1 for additive force.
gym.Initialize('Cart-Pole', 'Init1=10;Init2=0')


#----------------------------------------------------------------------------------------
# Classes
#----------------------------------------------------------------------------------------

class MemoryItem:
    def __init__(self, state, x, action, aprob, reward):
        self.state = state
        self.x = x
        self.action = action
        self.aprob = aprob
        self.reward = reward

    def dlogps(self):
        fY = 0
        if (self.action == 0):
            fY = 1;
        return fY - self.aprob

class MemoryItemCollection:
    def __init__(self):
        self.rgItems = []

    def Count(self):
        return len(self.rgItems)

    def Clear(self):
        self.rgItems = []
 
    def Add(self, state, x, action, aprob, reward):
        mi = MemoryItem(state, x, action, aprob, reward)
        self.rgItems.append(mi)

    def GetDiscountedRewards(self, fGamma, bAllowReset):
        nCount = len(self.rgItems)
        rgDiscountedR = [None] * nCount
        fRunningAdd = 0

        for t in reversed(range(nCount)):
            if (bAllowReset and self.rgItems[t].reward):
                fRunningAdd = 0

            fRunningAdd = fRunningAdd * fGamma + self.rgItems[t].reward
            rgDiscountedR[t] = fRunningAdd
        return rgDiscountedR

    def GetPolicyGradients(self):
        nCount = len(self.rgItems)
        rgPolicyGradients = [None] * nCount

        for i in range(nCount):
            rgPolicyGradients[i] = self.rgItems[i].dlogps()
        return rgPolicyGradients;

    def GetData(self):
        rgData = []
        for item in self.rgItems:
            d = Datum(item.state.Data)
            rgData.append(d)
        return rgData

class Brain:
    def onGetLoss(self, sender, e):
        if (self.skipLoss):
            return
        nCount = self.blobPolicyGradient.count()
        hPolicyGrad = self.blobPolicyGradient.mutable_gpu_data
        hBottomDiff = e.Bottom[0].mutable_gpu_diff
        hDiscountedR = self.blobDiscountedR.gpu_data

        # Calculate the acutal loss
        fSumSq = self.blobPolicyGradient.sumsq_data()
        fMean = fSumSq

        e.Loss = fMean
        # Apply gradients to bottom directly.
        e.EnableLossUpdate = False

        # Modulate the gradient with the advantage (Policy Gradient happens here)
        self.mycaffe.Cuda.mul(nCount, hPolicyGrad, hDiscountedR, hPolicyGrad)
        self.mycaffe.Cuda.copy(nCount, hPolicyGrad, hBottomDiff)
        self.mycaffe.Cuda.mul_scalar(nCount, -1.0, hBottomDiff)

    def __init__(self, mycaffe, nMiniBatch):
        self.mycaffe = mycaffe
        self.skipLoss = False
        self.nMiniBatch = nMiniBatch
        # Get the internal, solver and key layers used
        self.net = mycaffe.GetInternalNet(Phase.TRAIN)
        self.solver = mycaffe.GetInternalSolver()
        self.memData = self.net.FindLayer('MEMORYDATA', None)
        self.memLoss = self.net.FindLayer('MEMORY_LOSS', None)
        self.memLoss.OnGetLoss += self.onGetLoss
        self.blobDiscountedR = mycaffe.CreateBlob('discountedR');
        self.blobPolicyGradient = mycaffe.CreateBlob('policygrad');

    def Act(self, data):
        self.memData.AddDatum(data.Data, 1, True, True)
        self.skipLoss = True
        results = self.net.Forward()
        self.skipLoss = False

        for res in results:
            if (res.type != BLOB_TYPE.LOSS):
                rgfAprob = res.update_cpu_data()
                break;
    
        fAprob = rgfAprob[0]
        return 0 if (random() < fAprob) else 1, fAprob

    def Reshape(self, memory):
        nNum = memory.Count()
        self.blobDiscountedR.Reshape(nNum, 1, 1, 1)
        self.blobPolicyGradient.Reshape(nNum, 1, 1, 1)

    def SetDiscountedR(self, rg):
        fMean = self.blobDiscountedR.mean(rg)
        fStd = self.blobDiscountedR.std(fMean, rg)
        self.blobDiscountedR.SetData(rg)
        self.blobDiscountedR.NormalizeData(fMean, fStd)

    def SetPolicyGradients(self, rg):
        self.blobPolicyGradient.SetData(rg)

    def SetData(self, memory):
        rgData = memory.GetData()
        self.memData.AddDatumVector(rgData, None, 1, True, True)

    def Train(self, nIteration):
        mycaffe.Log.Enable = False
        # Accumulate grad over batch
        self.solver.Step(1, TRAIN_STEP.NONE, False, False, True, True) 

        if (nIteration % self.nMiniBatch == 0):
            self.solver.ApplyUpdate(nIteration)
            self.net.ClearParamDiffs()
        mycaffe.Log.Enable = True

    def CleanUp(self):
        self.blobDiscountedR.Dispose()
        self.blobPolicyGradient.Dispose()
        

#----------------------------------------------------------------------------------------
# Functions
#----------------------------------------------------------------------------------------

def preprocess(sCurrent, sPrevious):
    if (sPrevious == None):
        sCurrent.Data.Zero()
    else:
        sCurrent.Data.Sub(sPrevious.Data)

def onWriteLine(sender, e):
    print(e.Message)


#----------------------------------------------------------------------------------------
# Main Script
#----------------------------------------------------------------------------------------

log = Log('test')
log.OnWriteLine += onWriteLine
cancel = CancelEvent()
settings = SettingsCaffe()

# Load on demand used for data is received from the Gym.
settings.ImageDbLoadMethod = IMAGEDB_LOAD_METHOD.LOAD_ON_DEMAND
# Use GPU ID = 0
settings.GpuIds = '0'

# Load the descriptors from their respective files
# (installed by MyCaffe Test Application install)
file = open("C:\\ProgramData\\MyCaffe\\test_data\\models\\reinforcement\\cartpole\\solver.prototxt", "r")
strSolver = file.read()
file = open("C:\\ProgramData\\MyCaffe\\test_data\\models\\reinforcement\\cartpole\\train_val_sigmoid.prototxt", "r")
strModel = file.read()

# Load the CartPole Gym dataset descriptor.
try:
    ds = gym.GetDataset(DATA_TYPE.VALUES)
except Exception as e:
    print("You must have MyCaffe version 0.12.0.60 or greater to run this sample!")
    print(".")
    print(".")
    quit()
    

# Create a test project with the dataset and descriptors
project = ProjectEx('Test')
project.SetDataset(ds)
project.ModelDescription = strModel
project.SolverDescription = strSolver

# Create the MyCaffeControl (with the 'float' base type)
strCudaPath = "C:\\Program Files\\SignalPop\\MyCaffe\\cuda_11.8\\CudaDnnDll.11.8.dll"
mycaffe = MyCaffeControl[float](settings, log, cancel, None, None, None, None, strCudaPath)

# Load the project, using the TRAIN phase
mycaffe.Load(Phase.TRAIN, project, None, None, False, None, False, True, 'RL')
# NOTE: You must run the MyCaffe Test Application which starts the Gym, otherwise the
# call below will fail.
gym.OpenUi()

# Train the model for 1000000 iterations
nIterations = 1000000
nIteration = 0
nEpisode = 0
fEpisodeReward = 0
fRunningReward = None

# Reset the state to get the initial state
actions = gym.Actions
s = gym.Reset()
sLast = None

fGamma = 0.99
bAllowDiscountReset = False
nMiniBatch = 32
memory = MemoryItemCollection()
brain = Brain(mycaffe, nMiniBatch)

# Run the main training loop
while (nIteration < nIterations):
    x = preprocess(s, sLast)
    sLast = s

    nAction, fAprob = brain.Act(s)
    s_ = gym.Step(nAction)

    fEpisodeReward += s_.Reward

    memory.Add(s, x, nAction, fAprob, s_.Reward)

    if (s_.Terminal):
        nIteration = nIteration + 1
        nEpisode = nEpisode + 1

        brain.Reshape(memory)

        # Compute the discounted reward (backwards through time)
        rgDiscountedR = memory.GetDiscountedRewards(fGamma, bAllowDiscountReset)
        brain.SetDiscountedR(rgDiscountedR)

        # Get the policy gradients.
        rgDlogp = memory.GetPolicyGradients()
        # Discounted R applied to the policy gradient within loss function,
        # just before the backward pass.
        brain.SetPolicyGradients(rgDlogp)

        # Train one iteration, which triggers the loss function
        brain.SetData(memory)
        brain.Train(nIteration);

        if (fRunningReward == None):
            fRunningReward = fEpisodeReward
        else:
            fRunningReward = fRunningReward * 0.99 + fEpisodeReward * 0.01

        print('Iteration = ' + str(nIteration) + ', Episode = ' + str(nEpisode) + ' Episode Reward = ' + str(fEpisodeReward) + ' Running Reward = ' + str(fRunningReward))

        fEpisodeReward = 0
        s = gym.Reset()
        sLast = None
        memory.Clear()
    else:
        s = s_

# Release the resources used
brain.CleanUp()
mycaffe.Dispose()
