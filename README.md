# UnityMackerel


Unity wrapper of https://mackerel.io/  


## Requirement

1. Create accont mackerel.io
2. Install mackerel-agent


## Example


```cs
using UnityEngine;
using AppKit;

public class MackerelTest : MonoBehaviour
{
    [Range(0f, 100f)]
    public float testNumber;

    Mackerel mackerel;

    void OnEnable()
    {
        mackerel = Mackerel.Instance;
        mackerel.showLog = true;
        
        // TODO enter apikey
        // https://mackerel.io/orgs/YOUR_ORGANIZATION?tab=apikeys
        mackerel.Initialize(THE_API_KEY); 
    }

    void Update ()
    {
        mackerel.QueueMetrics("custom.framerate", 1.0f / Time.deltaTime);
        mackerel.QueueMetrics("custom.testnumber", testNumber);
    }
}

```