using UnityEngine;
using AppKit;

public class MackerelTest : MonoBehaviour
{
    [Header("Enter your APIKEY")]
    public string apikey = "";

    [Range(0f, 100f)]
    public float testNumber;

    Mackerel mackerel;

    void OnEnable()
    {
        mackerel = Mackerel.Instance;
        mackerel.showLog = true;
        if (string.IsNullOrEmpty(apikey))
        {
            Debug.LogError("Enter your APIKEY");
        }
        else
        {
            mackerel.Initialize(apikey);
        }
    }

    void Update ()
    {
        mackerel.QueueMetrics("custom.framerate", 1.0f / Time.deltaTime);
        mackerel.QueueMetrics("custom.testnumber", testNumber);
    }
}
