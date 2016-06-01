using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace AppKit
{
    /// <summary>
    /// Mackerel post metrics wrapper<br>
    /// https://mackerel.io/ja/api-docs/entry/host-metrics#post
    /// </summary>
    public class Mackerel : MonoBehaviour
    {
        public static float TIMEOUT = 10f;
        public bool showLog;


        Dictionary<string, float> _metrics;
        string _apiKey;
        string _id;


        #region lifecycle
        IEnumerator Start()
        {
            float lastTime = Time.fixedTime;
            while (Application.isPlaying)
            {
                float wait = 20f - (Time.fixedTime - lastTime);
                yield return new WaitForSeconds(Mathf.Max(wait, 0));
                yield return StartCoroutine(SendAllMetrics());
                lastTime = Time.fixedTime;
            }
        }
        #endregion

        #region public
        public void Initialize(string apiKey, string id="")
        {
            _metrics = new Dictionary<string, float>();
            _apiKey = apiKey;
            _id = string.IsNullOrEmpty(id) ? LoadId() : id;
        }

        public void QueueMetrics(string metrics, float value)
        {
            if (_metrics.ContainsKey(metrics))
            {
                _metrics[metrics] = value;
            }
            else
            {
                _metrics.Add(metrics, value);
            }
        }
        #endregion

        #region private

        string LoadId()
        {
            if (Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer)
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var idPath = Path.GetFullPath(Path.Combine(desktop, "../Library/mackerel-agent/id"));
                if (File.Exists(idPath))
                {
                    return File.ReadAllText(idPath);
                }

            }
            if (Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                string idPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
                string[] components =
                {
                    "Macerel",
                    "mackerel-agent",
                    "id"
                };
                foreach (var path in components)
                {
                    idPath = Path.Combine(idPath, path);
                }

                if (File.Exists(idPath))
                {
                    return File.ReadAllText(idPath);
                }
            }
            return "";
        }

        IEnumerator SendAllMetrics()
        {
            if (_metrics.Count == 0)
            {
                yield break;
            }

            // make json
            int epoch = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            StringBuilder st = new StringBuilder("[");
            int index = 0;
            foreach (var kv in _metrics)
            {
                st.Append('{');
                st.AppendFormat("\"hostId\":\"{0}\",\"name\":\"{1}\",\"time\":{2},\"value\":{3}",
                    _id, kv.Key, epoch, kv.Value);
                st.Append('}');
                index++;
                if (index != _metrics.Count)
                {
                    st.Append(',');
                }
            }
            _metrics.Clear();
            st.Append(']');
            if (showLog)
            {
                Debug.LogFormat("[Mackerel] Send {0}", st);
            }

            // to www requiest
            const string uri = "https://mackerel.io" + "/api/v0/tsdb";
            var headers = new Dictionary<string, string>();
            headers.Add("X-Api-Key", _apiKey);
            headers.Add("Content-Type", "application/json");
            using (WWW www = new WWW(uri, Encoding.UTF8.GetBytes(st.ToString()), headers))
            {
                // Simple timeout
                float start = Time.unscaledTime;
                while (!www.isDone)
                {
                    if (TIMEOUT < Time.unscaledTime - start)
                    {
                        www.Dispose();
                        Debug.LogWarning("timeout");
                        yield break;
                    }
                    yield return null;
                }

                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.LogWarning(www.error);
                }
                if (showLog)
                {
                    Debug.LogFormat("[Mackerel] Response {0}", www.text);
                }
            }
        }
        #endregion

        #region Singleton
        static Mackerel _instance;

        public static Mackerel Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(typeof(Mackerel).ToString());
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<Mackerel>();
                }
                return _instance;
            }
        }
        #endregion
    }

}
