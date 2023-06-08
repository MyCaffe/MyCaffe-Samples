#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/framework/runtime:4.8-windowsservercore-ltsc2019
WORKDIR /
COPY ""C:/Data/Data/SS_Projects/Intelligence/_drops/1.12.0.30/MyCaffe.app.setup.exe" .
ARG source
WORKDIR /Program Files/SignalPop/MyCaffe
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["C:\\Program Files\\SignalPop\\MyCaffe\\CuDnnSimpleConnection.exe"]