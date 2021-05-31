<H2>Welcome to the Seq2Seq MyCaffe-Samples!</H2>

The following Sequence-to-Sequence (Seq2Seq) examples are provided using the MyCaffe AI Platform.

<b>Seq2Seq</b> provides a simple sample that learns to generate a Sin curve.  This sample is a re-write of the [LSTM-Mini-Tutorial](https://github.com/CorvusCorax/Caffe-LSTM-Mini-Tutorial) by 
Corvus/Caffe and distributed under the [GNU License](https://github.com/CorvusCorax/Caffe-LSTM-Mini-Tutorial/blob/master/LICENSE).  The MyCaffe Seq2Seq sample uses a two layer encoder/decoder
style LSTM model to learn the Sin curve.

<b>Seq2SeqImageToSin</b> sample learns generate a Sin curve based on a sequence of hand-written characters from the [MNIST](http://yann.lecun.com/exdb/mnist/) dataset.  This sample was
inspired by the 2015 article [Sequence to Sequence - Video to Text](https://arxiv.org/abs/1505.00487) by Subhashini Venugopalan, Marcus Rohrbach, Jeff Donahue, Raymond Mooney, Trevor Darrell, 
and Kate Saenko.  In addition, the 2014 article [On the Properties of Neural Machine Translation: Encoder-Decoder Approaches](https://arxiv.org/abs/1409.1259) by Kyunghyun Cho, 
Bart van Merrienboer, Dzmitry Bahdanau, and Yoshua Bengio inspired the encoder/decoder model design used in this sample.

<b>Seq2SeqChatBot</b> chat-bot sample provides a MyCaffe Sequence-to-Sequence model that encodes the inputs from a questioner and maps the probabilities to the encoding
of the chat-bot and uses attention to learn the link between the two.  This sample is a re-write of the original open-source sample Seq2SeqLearn from 
[mashmawy/Seq2SeqLearn](https://github.com/mashmawy/Seq2SeqLearn) distributed under MIT license and the chat-bot sample from
[HectorPulido/Chatbot-seq2seq-C-](https://github.com/HectorPulido/Chatbot-seq2seq-C-) also distributed under [MIT license](https://github.com/HectorPulido/Chatbot-seq2seq-C-/blob/master/LICENSE).

<b>Seq2SeqChatBot2</b> chat-bot sample that uses the new TextDataLayer to provide a MyCaffe Sequence-to-Sequence model that encodes the inputs from a questioner and maps the probabilities to the encoding
of the chat-bot and uses attention to learn the link between the two.  This sample is a re-write of the original open-source sample Seq2SeqLearn from 
[mashmawy/Seq2SeqLearn](https://github.com/mashmawy/Seq2SeqLearn) distributed under MIT license and the chat-bot sample from
[HectorPulido/Chatbot-seq2seq-C-](https://github.com/HectorPulido/Chatbot-seq2seq-C-) also distributed under [MIT license](https://github.com/HectorPulido/Chatbot-seq2seq-C-/blob/master/LICENSE).
