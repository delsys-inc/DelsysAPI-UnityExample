# Delsys API RF integration with Unity

## Description of the Sample App using Delsys API in RF mode:
1. When you run the app after building the project from Unity, click Scan. The app will then scan for Trigno Sensors and then connect to it. 
2. Select the sensors by clicking "Select" button. 
3. Click "Start" to start data streaming and "Stop" to stop data streaming
4. The app will save a csv file of the data collected during the run.

## Requirements
1. Unity version 2019.1.5f1 or greater
2. Latest Delsys API
3. Trigno sensors and Base Station running on latest firmware 

## Few things to note before building the project:
1. Make sure you always have the latest DelsysAPI, you can download the latest API package from [here](http://data.delsys.com/DelsysServicePortal/api/index.html) (Skip to step 8 if you already have the latest API package)
2. For this tutorial, we'll be using the NuGet for Unity plugin. This is not required, but it simplifies obtaining the API's dependencies.
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/AssetStore.jpg" />
3. Install NuGet if you don't have already from the Asset Store.
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/NuGet.jpg" />
4. Install Stateless and Portable.Licensing NuGet packages.
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/Stateless.jpg" />
5. Extract each NuGet package -- the API, Stateless, and Portable.Licensing -- with a zip extractor.
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/Extract.jpg" />
6. Copy each dll (Stateless from the lib/net45 folder, Portable.Licensing, and all of the Delsys API dlls) to the Assets -> Plugins folder of your Unity project.
7. Delete the NuGet packages and NuGet config files to ensure no duplicate dependencies. Your Plugins folder should now look something like this:
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/Final.PNG" />
8. Make sure the configuration settings are as shown below. You can access them by going to
File -> Build Settings -> Player Settings
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/ConfigurationSettings.png" />
9. From File -> Build Settings, select Architecture as x86 from the drop down menu:
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/BuildSettings.png" />
10. It is advisable to run the standalone exe file from a seperate folder for eg: BuildVersions as shown below. You can do so by going to Build Settings, click Build -> Make a new folder and then save the build output in that location.
<img src="https://github.com/delsys-inc/DelsysAPIUnityIntegration/blob/master/Screenshots/BuildVersions.png"  />
11. Also make sure that any dll files in the project have x86 checked under Inspector -> Platform Settings


## Author
[SarthakJag](https://github.com/SarthakJag)



