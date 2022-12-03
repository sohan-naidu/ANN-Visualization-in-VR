using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

class EpochMetrics {
    public float loss, root_mean_squared_error;
}
public class UpdateMetrics : MonoBehaviour {
    private TMP_Text text;
    private string value;
    List<EpochMetrics> metrics;
    private int currentEpoch;
    private int prevEpoch;
    private string pathToJson;
    string unformattedString = "";
    private bool done = false;

    public GameObject NNSpawner;

    public void Init()
    {
        pathToJson = Application.dataPath + "/Scripts/backend/epochs/metrics.json";
        text = GetComponent<TMP_Text>();
        metrics = new List<EpochMetrics>();
        using (StreamReader r = new StreamReader(pathToJson)) {
            string json = r.ReadToEnd();
            // json is of the form [{},{},...,{}]
            metrics = JsonConvert.DeserializeObject<List<EpochMetrics>>(json);
        }
        prevEpoch = 0;
        var curr = metrics[0];
        string[] Metrics = { "Loss", "RMSE" };
        for (int i = 0; i < Metrics.Length; i++) {
            unformattedString += "<align=left>" + Metrics[i] + "<line-height=0>\n<align=right>: {" + 2 * i + ":0.000000}->{" + ( 2 * i + 1 ) + ":0.000000}<line-height=1em>\n";
        }
        done = true;
        //Debug.Log(unformattedString);
    }

    // Update is called once per frame
    void Update()
    {
        currentEpoch = NNSpawner.GetComponent<NeuronInstantiator>().currentEpoch;
        if (done && prevEpoch != currentEpoch) {
            var curr = metrics[currentEpoch / 5];
            var prev = metrics[prevEpoch / 5];
            //text.text = string.Format(unformattedString, curr.loss, curr.accuracy, curr.rmse);
            text.text = string.Format(unformattedString, prev.loss, curr.loss, prev.root_mean_squared_error, curr.root_mean_squared_error);
            //text.text = string.Format("Loss    :{0}\nAccuracy:{1}\nRMSE    :{2}", curr.loss, curr.accuracy, curr.rmse);
            prevEpoch = currentEpoch;
        }
    }
}
