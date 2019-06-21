# Delsys API RF integration with Unity

## Description of the Sample App using Delsys API in RF mode:
1. When you run the app after building the project from Unity, click Scan. The app will then scan for Trigno Sensors and then connect to it. 
2. Select the sensors by clicking "Select" button. 
3. Click "Start" to start data streaming and "Stop" to stop data streaming
4. The app will save a csv file of the data collected during the run.

## Requirements
1. Unity version 2019.1.5f1 or greater
2. Latest Delsys API
3. Trigno sensors and Basestation running on latest firmware

## Few things to note before building the project:
1. Make sure the configuration settings are as shown below. You can access them by going to
File -> Build Settings -> Player Settings
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/ConfigurationSettings.png" width="700" height="200" />
2. From File -> Build Settings, select Architecture as x86 from the drop down menu:
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/BuildSettings.png" width="700" height="200" />
3. It is advisable to run the standalone exe file from a seperate folder for eg: BuildVersions as shown below. You can do so by going to Build Settings, click Build -> Make a new folder and then save the build output in that location.
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/BuildVersions.png" width="700" height="200" />
4. Also make sure that any dll files in the project have x86 checked under Inspector -> Platform Settings

# License


## Author
[SarthakJag](https://github.com/SarthakJag)



