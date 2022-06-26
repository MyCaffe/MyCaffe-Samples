# MyCaffe Multi-Class Classification Loss Sample
MyCaffe Multi-Class Classification sample solves a simple 3-class classification problem where the model learns to determine whether a given point falls within 
one of three blobs of dots (created with Python SciKit-Learn make_blobs()) shown below.  NOTE: This sample requires MyCaffe version 1.11.6.46 or later.

![Dataset](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/blob_dataset.png)

Note the labels for this dataset are set to 0, 1, or 2.

The input shape to each of the models discussed below is: 

```
 Data shape  (batch, 1, 1, 2), 
 Label shape (batch, 1, 1, 1) 
```

where the label contains each actual label value of 0, 1 or 2.

All models were trained for 300 epochs and as shown below, even though all models eventually get to a similar accuracy, at the given training epoch level the models had the following performance.

- Sigmoid Cross Entropy Loss: 79.38%
- Hinge Loss: 79.06%
- Softmax Loss: 78.13%
- Softmax Cross Entropy Loss: 77.19%

Unlike in the 2-class sample, in this 3-class sample we use the MyCaffe [Accuracy layer](https://www.signalpop.com/onlinehelp/mycaffe/html/class_my_caffe_1_1layers_1_1_accuracy_layer.html) to calculate all accuracies for each model.

## Hinge Loss
The Hinge Loss calculates the loss by incorporating a margin or distance into the loss function.  See [Understanding Hinge Loss and the SVM Cost Function](https://programmathically.com/understanding-hinge-loss-and-the-svm-cost-function/) for more information.

![Hinge Loss Model](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/hinge_loss_model.png)

The results of running the Hinge Loss model over 300 epochs are as follows.

![Hinge Loss Results](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/hinge_loss_results.png)

## Cross Entropy Loss

The Cross Entropy Loss calculates the loss using a cross entropy calculation preceded by either a Sigmoid or Softmax function.  For a great discussion on cross entropy loss variants see [Understanding Categorical Cross-Entropy Loss, Binary Cross-Entropy Loss, Softmax Loss, Logistic Loss, Focal Loss and all those confusing names](https://gombru.github.io/2018/05/23/cross_entropy_loss/) by Raúl Gómez. 

### Sigmoid Cross Entropy Loss

The Sigmoid Cross Entropy Loss sample used the following model.

![Sigmoid Cross Entropy Loss Model](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/sigmoidce_loss_model.png)

The results of running the Sigmoid Cross Entropy Loss model over 300 epochs are as follows.

![Sigmoid Cross Entropy Loss Results](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/sigmoidce_loss_results.png)

### Softmax Cross Entropy Loss

The Softmax Cross Entropy Loss sample used the following model.

![Softmax Cross Entropy Loss Model](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/softmaxce_loss_model.png)

The results of running the Softmax Cross Entropy Loss model over 300 epochs are as follows.

![Softmax Cross Entropy Loss Results](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/softmaxce_loss_results.png)

## Softmax Loss

The Softmax Loss sample uses the following model.

![Softmax Loss Model](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/softmax_loss_model.png)

The results of running the Softmax Loss model over 300 epochs are as follows.

![Softmax Loss Results](https://github.com/MyCaffe/MyCaffe-Samples/blob/master/Loss/MultiClassClassificationLoss/MultiClassClassificationLoss/Documents/softmax_loss_results.png)
