using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class UserInput : MonoBehaviour {
    //Before application begins, this has to be filled appropriately
    public string datasetFolderPath;
    public List<string> targetVariables;
    private int layerCount;
    public List<int> neuronsPerLayer;

    void Start()
    {
        layerCount = neuronsPerLayer.Count;
        string pythonInitFilename = Application.dataPath + "/Scripts/Backend/scripts/backend.py";
        //args depending on what sohan needs
        string outputJson = JsonConvert.SerializeObject(new
        {
            datasetPath = this.datasetFolderPath + "/dataset.csv",
            targets = this.targetVariables,
            layerCount = this.layerCount,
            neuronsCount = this.neuronsPerLayer
        });
        Debug.Log(outputJson);
        File.WriteAllText(datasetFolderPath + "/ip.json", outputJson);
        string args = string.Format("{0} -i", pythonInitFilename);
        runPythonProcess(args);
    }

    private void runPythonProcess(string cmd)
    {
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo = new System.Diagnostics.ProcessStartInfo();
        //check if works
        //specific to my system
        //p.StartInfo.FileName = "C:/My_Files/Dev/Anaconda3/envs/minimal_ds/python.exe";
        p.StartInfo.FileName = "C:\\My_Files\\Dev\\Python39\\python.exe";
        //p.StartInfo.RedirectStandardError = true;

        //cmd has pythonFileName already attached at the beginning
        p.StartInfo.Arguments = cmd;
        p.StartInfo.UseShellExecute = false;

        p.StartInfo.RedirectStandardError = true;
        Debug.Log("Sohan's part called\nCmd: " + p.StartInfo.FileName + " " + cmd);

        p.Start();

        string stringErrorOutput = p.StandardError.ReadToEnd();
        Debug.Log(stringErrorOutput);

        p.WaitForExit();
        UnityEditor.AssetDatabase.Refresh();
        GameObject.Find("Metrics Text").GetComponent<UpdateMetrics>().Init();
    }
}
