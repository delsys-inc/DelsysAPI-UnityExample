using DelsysAPI.Channels.Transform;
using DelsysAPI.Configurations;
using DelsysAPI.Configurations.DataSource;
using DelsysAPI.Contracts;
using DelsysAPI.DelsysDevices;
using DelsysAPI.Events;
using DelsysAPI.Pipelines;
using DelsysAPI.Transforms;
using DelsysAPI.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnityExample : MonoBehaviour
{
    /// <summary>
    /// Data structure for recording every channel of data.
    /// </summary>
    List<List<double>> Data = new List<List<double>>();
    public Button ScanButton;
    public Button StartButton;
    public Button StopButton;
    public Button SelectButton;
    public Button PairButton;
    IDelsysDevice DeviceSource = null;
    int TotalLostPackets = 0;
    int TotalDataPoints = 0;
    public Text APIStatusText;
    Pipeline RFPipeline;
    ITransformManager TransformManager;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Entered Start Function.");

        //Finding references to all the buttons in the scene
        ScanButton = GameObject.FindGameObjectWithTag ("ScanButton").GetComponent<Button>();
        ScanButton.onClick.AddListener((UnityEngine.Events.UnityAction) this.clk_Scan);
        
        StartButton = GameObject.FindGameObjectWithTag ("StartButton").GetComponent<Button>();
        StartButton.onClick.AddListener((UnityEngine.Events.UnityAction) this.clk_Start);

        StopButton = GameObject.FindGameObjectWithTag ("StopButton").GetComponent<Button>();
        StopButton.onClick.AddListener((UnityEngine.Events.UnityAction) this.clk_Stop);

        SelectButton = GameObject.FindGameObjectWithTag ("SelectButton").GetComponent<Button>();
        SelectButton.onClick.AddListener((UnityEngine.Events.UnityAction) this.clk_Select);

        PairButton = GameObject.FindGameObjectWithTag ("PairButton").GetComponent<Button>();
        PairButton.onClick.AddListener((UnityEngine.Events.UnityAction) this.clk_Pair);
        
        ScanButton.enabled = true; //Enabling only the Scan button for now.
        StartButton.enabled = false;
        StopButton.enabled = false;
        SelectButton.enabled = false;
        PairButton.enabled = false;
        
        CopyUSBDriver(); // Copying the SiUSBXp.dll file if not present
        InitializeDataSource(); //Initializing the Delsys API Data source

    }

    // Update is called once per frame
    void Update()
    {
        //do nothing
    }

    public void CopyUSBDriver()
    {
        string unityAssetPath = Application.streamingAssetsPath + "/SiUSBXp.dll";
        string adjacentToExePath = Application.dataPath + "/../SiUSBXp.dll";
        if (!File.Exists(adjacentToExePath))
        {
            File.Copy(unityAssetPath, adjacentToExePath);
        }
    }

    /// <summary>
    /// Dumping all the debug statements from DelsysAPI into the Unity's Log file, see https://docs.unity3d.com/Manual/LogFiles.html for more details.
    /// </summary>
    /// <returns> None </returns>
    public void TraceWriteline(string s, object[] args)
    {
        for(int i=0; i< args.Count();i++){
            s = s + "; " + args[i];
        }
        Debug.Log("Delsys API:- " + s);
        
    }
   
    #region Initialization
    public void InitializeDataSource()
    {
        //Load your key & license either through reflection as shown in the User Guide, or by hardcoding it to these strings.
        string key = "";
        string license = "";

        APIStatusText.text = "Creating device source . . . ";
        var deviceSourceCreator = new DeviceSourcePortable(key, license);
        deviceSourceCreator.SetDebugOutputStream(TraceWriteline);
        DeviceSource = deviceSourceCreator.GetDataSource(SourceType.TRIGNO_RF);
        APIStatusText.text  = "Device source created.";
        DeviceSource.Key = key;
        DeviceSource.License = license;
        APIStatusText.text = "Loading data source . . . ";
        try
        {
            LoadDataSource(DeviceSource);
        }
        catch(Exception exception)
        {
            APIStatusText.text = "Something went wrong: " + exception.Message;
            return;
        }
        APIStatusText.text = "Data source loaded and ready to Scan.";
    }

    public void LoadDataSource(IDelsysDevice ds)
    {
        PipelineController.Instance.AddPipeline(ds);

        RFPipeline = PipelineController.Instance.PipelineIds[0];
        TransformManager = PipelineController.Instance.PipelineIds[0].TransformManager;
        
        RFPipeline.TrignoRfManager.ComponentScanComplete += ComponentScanComplete;
        RFPipeline.CollectionStarted += CollectionStarted;
        RFPipeline.CollectionDataReady += CollectionDataReady;
        RFPipeline.CollectionComplete += CollectionComplete;
        RFPipeline.TrignoRfManager.ComponentAdded += ComponentAdded;
        RFPipeline.TrignoRfManager.ComponentLost += ComponentLost;
        RFPipeline.TrignoRfManager.ComponentRemoved += ComponentRemoved;        
    }

    #endregion

    #region Button Click events: clk_Scan, clk_Select, clk_Start, clk_Stop, clk_Pair
    public void clk_Scan()
    {
        foreach(var comp in RFPipeline.TrignoRfManager.Components)
        {
            RFPipeline.TrignoRfManager.DeselectComponentAsync(comp);
        }
        APIStatusText.text = "Scanning . . .";
        RFPipeline.Scan();
    }

    public void clk_Select(){
        SelectSensors();
    }

    public void clk_Start()
    {
        // The pipeline must be reconfigured before it can be started again.      
        bool success = ConfigurePipeline();
        if(success){
            Debug.Log("Starting data streaming....");
            APIStatusText.text = "Starting data streaming....";
            RFPipeline.Start(); 
            StopButton.enabled = true; 
        }
        else{
            Debug.Log("Configuration failed. Cannot start streaming!!");  
            APIStatusText.text = "Fatal error!";
        }
       
    }

    public void clk_Stop()
    {
        RFPipeline.Stop();
    }

    public void clk_Pair()
    {
        APIStatusText.text = "Awaiting a sensor pair . . .";
        RFPipeline.TrignoRfManager.AddTrignoComponent(RFPipeline.TrignoRfManager.Components.Count);
    }

    #endregion

    public void SelectSensors()
    {
        APIStatusText.text = "Selecting all sensors . . .";

        // Select every component we found and didn't filter out.
        foreach (var component in RFPipeline.TrignoRfManager.Components)
        {
            bool success = RFPipeline.TrignoRfManager.SelectComponentAsync(component).Result;
            if(success){
                APIStatusText.text = "Sensor selected!";
            }
            else{
                APIStatusText.text = "Could not select sensor!!";
            }
        }       
        StartButton.enabled = true;
    }

    /// <summary>
    /// Configures the input and output of the pipeline.
    /// </summary>
    /// <returns></returns>
    private bool ConfigurePipeline()
    {
        var inputConfiguration = new TrignoDsConfig();

        if (PortableIoc.Instance.CanResolve<TrignoDsConfig>())
        {
            PortableIoc.Instance.Unregister<TrignoDsConfig>();
        }

        PortableIoc.Instance.Register(ioc => inputConfiguration);

        foreach (var somecomp in RFPipeline.TrignoRfManager.Components.Where(x => x.State == SelectionState.Allocated))
        {
            bool success = somecomp.Configuration.SelectSampleMode(somecomp.DefaultMode);
            if(success){
                APIStatusText.text = "Default mode selected: " + somecomp.DefaultMode;
                Debug.Log("Mode selected: " + somecomp.DefaultMode);
            }
            else{
                APIStatusText.text = "Default mode failed: " + somecomp.DefaultMode; 
                Debug.Log("Mode selection failed");
            }           
            if (somecomp.Configuration == null)
            {
                APIStatusText.text = "null config.......";
                return false;
            }
        }

        try
        {
            Debug.Log("Applying Input configurations");
            bool success_1 = RFPipeline.ApplyInputConfigurations(inputConfiguration);
            if(success_1){
                 APIStatusText.text =  "Applied input configuration";
                 Debug.Log("Applied input configuration");
            }
            else{
                 APIStatusText.text = "Input configurations failed";
                 Debug.Log("Input configurations failed");
            }
        }
        catch (Exception exception)
        {
            APIStatusText.text = exception.Message;
        }
        RFPipeline.RunTime = int.MaxValue;

        var transformTopology = GenerateTransforms();
        Debug.Log("Generated Transforms");
        Debug.Log("Applying Output configurations.....");
        bool success_2 = RFPipeline.ApplyOutputConfigurations(transformTopology);
        if(success_2){
            APIStatusText.text = "Applied Output configurations";
            Debug.Log("Applied Output configurations");
            return true;
        }
        else{
            APIStatusText.text = "Output configurations failed!";
            Debug.Log("Output configurations failed!");
            return false;
        }        
    }
    
    /// <summary>
    /// Generates the Raw Data transform that produces our program's output.
    /// </summary>
    /// <returns>A transform configuration to be given to the API pipeline.</returns>
    public OutputConfig GenerateTransforms()
    {
        RFPipeline.TransformManager.TransformList.Clear();
        //Create the transforms for the first time.
        int sensorNum = 0;
        int channelNum = 0;
        for (int i = 0; i < RFPipeline.TrignoRfManager.Components.Count; i++)
        {            
            if (RFPipeline.TrignoRfManager.Components[i].State == SelectionState.Allocated)
            {
                var tmp = RFPipeline.TrignoRfManager.Components[i];
                sensorNum++;
                channelNum += tmp.TrignoChannels.Count;
            }
        }

        if (TransformManager.TransformList.Count == 0)
        {
            var t = new TransformRawData(channelNum, channelNum);
            TransformManager.AddTransform(t);
        }

        //channel configuration happens each time transforms are armed.
        var t0 = TransformManager.TransformList[0];
        var outconfig = new OutputConfig();
        outconfig.NumChannels = channelNum;
        int channelIndex = 0;

        foreach (var component in RFPipeline.TrignoRfManager.Components)
        {
            if (component.State == SelectionState.Allocated)
            {
                for (int k = 0; k < component.TrignoChannels.Count; k++)
                {
                    var chin = component.TrignoChannels[k];
                    var chout = new ChannelTransform(chin.FrameInterval, chin.SamplesPerFrame, Units.VOLTS);
                    TransformManager.AddInputChannel(t0, chin);
                    TransformManager.AddOutputChannel(t0, chout);
                    outconfig.MapOutputChannel(channelIndex, chout);
                    channelIndex++;
                }
            }
        }

        return outconfig;

    }


    #region Collection Callbacks -- Data Ready, Colleciton Started, and Collection Complete
    public void CollectionDataReady(object sender, ComponentDataReadyEventArgs e)
    {
        APIStatusText.text = "Received data packet with average value: " + e.Data.First().Data.Average().ToString();
        int lostPackets = 0;
        int dataPoints = 0;

        // Check each data point for if it was lost or not, and add it to the sum totals.
        for (int j = 0; j < e.Data.Count(); j++)
        {
            var channelData = e.Data[j];
            Data[j].AddRange(channelData.Data);
            dataPoints += channelData.Data.Count;
            for (int i = 0; i < channelData.Data.Count; i++)
            {
                if (e.Data[j].IsLostData[i])
                {
                    lostPackets++;
                }
            }
        }
        TotalLostPackets += lostPackets;
        TotalDataPoints += dataPoints;
    }

    private void CollectionStarted(object sender, DelsysAPI.Events.CollectionStartedEvent e)
    {
        APIStatusText.text = "CollectionStarted event triggered!";
        var comps = PipelineController.Instance.PipelineIds[0].TrignoRfManager.Components;

        // Refresh the counters for display.
        TotalDataPoints = 0;
        TotalLostPackets = 0;

        // Recreate the list of data channels for recording
        int totalChannels = 0;
        for (int i = 0; i < comps.Count; i++)
        {
            for (int j = 0; j < comps[i].TrignoChannels.Count; j++)
            {
                if (Data.Count <= totalChannels)
                {
                    Data.Add(new List<double>());
                }
                else
                {
                    Data[totalChannels] = new List<double>();
                }
                totalChannels++;
            }
        }        
    }

    private void CollectionComplete(object sender, DelsysAPI.Events.CollectionCompleteEvent e)
    {
        for (int i = 0; i < Data.Count; i++)
        {
            using (StreamWriter channelOutputFile = new StreamWriter("./channel" + i + "_data.csv"))
            {
                foreach (var pt in Data[i])
                {
                    channelOutputFile.WriteLine(pt.ToString());
                }
            }
        }
        APIStatusText.text = "CollectionComplete event triggered!";
        RFPipeline.DisarmPipeline().Wait();
    }

    #endregion

    #region Component Events: Scan complete, Component Added, Lost, Removed
    private void ComponentScanComplete(object sender, DelsysAPI.Events.ComponentScanCompletedEventArgs e)
    {
        APIStatusText.text = "Scan Complete!";        
        SelectButton.enabled = true;
        PairButton.enabled = true;
    }

    public void ComponentAdded(object sender, ComponentAddedEventArgs e)
    {
        APIStatusText.text = "Connected to sensor " + e.Component.Id.ToString();
    }

    public void ComponentLost(object sender, ComponentLostEventArgs e)
    {
        int sensorStickerNumber = RFPipeline.TrignoRfManager.Components.Where(sensor => sensor.Id == e.Component.Id).First().PairNumber;
        Console.WriteLine("It appears sensor " + sensorStickerNumber + " has lost connection. Please power cycle this sensor.");
        APIStatusText.text = "It appears sensor " + sensorStickerNumber + " has lost connection";
    }

    public void ComponentRemoved(object sender, ComponentRemovedEventArgs e)
    {

    }
   
    #endregion

}
