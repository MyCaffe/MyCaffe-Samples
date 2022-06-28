# MyCaffe Multi-Label Classification Loss Sample
The MyCaffe Multi-Label Classification sample solves problems where the model learns one or more labels per input.  In this sample, we describe 
each hand-written character in the MNIST dataset with one or more of 7 characteristics as shown below.

0. **Loop** - the character has a loop.
1. **Top Loop** - the character has a loop in the top portion.
2. **Bottom Loop** - the character has a loop in the bottom portion.
3. **Angle** - the character has an angled line in it.
4. **Top Curve** - the character has a curve in the top portion.
5. **Bottom Curve** - the character has a curve in the bottom portion.
6. **Line** - the character has a straight line in it.

This same model could also be used to learn the characteristics of the Kaggle Amazon Rainforest Satellite Image Dataset; however we have found acquiring this dataset to be
very time consuming.  So, in the meantime we have created a very simple replica of the problem using the MNIST dataset which is very readily available.  To create the MNIST
images, just run the [MyCaffe Test Application](https://github.com/MyCaffe/MyCaffe/releases) and follow the instructions below.

```
1. Select the 'Database | Load MNIST' menu
2. Check the 'Export to file only (SQL not needed)' checkbox
3. Press the 'Download' button to download the files which by default download to the '\ProgramData\MyCaffe\test_data\mnist' directory
4. Copy the 'testing' and 'training' directories from your download location to '..\MyCaffe-Samples\Loss\MultiLabelClassificationLoss\MultiLabelClassificationLoss\dataset' directory
```
**NOTE:** Even though the export above does not require SQL (or SQL Express), this sample does requires SQL (or SQL Express) for it loads the MNIST dataset with one-hot encoded labels into a special KARS dataset stored in the DNN SQL database.
Once loaded, the sample merely queries the dataset directly from SQL using the MyCaffe in-memory database.

## One-hot Encoding
The label of each MNIST image is one-hot encoded.  This means that the label is a vector of length 8 with a 1 indicating each attribute that the label meets. As shown below, there are 7 attributes used to describe each MNIST character.

![MNIST Attributes](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiLabelClassificationLoss/MultiLabelClassificationLoss/Documents/MNIST_MultiClassification.png)

For example, the number 3 has a 'top curve' and a 'bottom curve' and is therefore given the hot-vector of |0 0 0 0 1 1 0| as input to the loss layer of the model.

All models were trained for 2 epochs with a batch size of 128 and as shown below, the models had the following performance.

- Sigmoid Cross Entropy Loss: 99.32%
- Hinge Loss: 75.52%

Each model uses a VGG like structure with three blocks of convolution, pooling and dropout layers.

## Hinge Loss
The Hinge Loss calculates the loss by incorporating a margin or distance into the loss function.  See [Understanding Hinge Loss and the SVM Cost Function](https://programmathically.com/understanding-hinge-loss-and-the-svm-cost-function/) for more information.

![Hinge Loss Model](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiLabelClassificationLoss/MultiLabelClassificationLoss/Documents/hinge_loss_model2.png)

The results of running the Hinge Loss model over 2 epochs with a batch = 128 are as follows.

![Hinge Loss Results](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiLabelClassificationLoss/MultiLabelClassificationLoss/Documents/hinge_loss_results.png)

## Cross Entropy Loss

The Cross Entropy Loss calculates the loss using a cross entropy calculation preceded by either a Sigmoid or Softmax function.  For a great discussion on cross entropy loss variants see [Understanding Categorical Cross-Entropy Loss, Binary Cross-Entropy Loss, Softmax Loss, Logistic Loss, Focal Loss and all those confusing names](https://gombru.github.io/2018/05/23/cross_entropy_loss/) by Raúl Gómez. 

### Sigmoid Cross Entropy Loss

The Sigmoid Cross Entropy Loss sample used the following model.

![Sigmoid Cross Entropy Loss Model](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiLabelClassificationLoss/MultiLabelClassificationLoss/Documents/sigmoidce_loss_model.png)

The results of running the Sigmoid Cross Entropy Loss model over 2 epochs with a batch = 128 are as follows.

![Sigmoid Cross Entropy Loss Results](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiLabelClassificationLoss/MultiLabelClassificationLoss/Documents/sigmoidce_loss_results.png)
