using System.Collections;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour {
    public static void RunCoroutine(IEnumerator coroutine, bool dontDestroyOnLoad) {
        var go = new GameObject("runner");

        if(dontDestroyOnLoad) {
            DontDestroyOnLoad(go);
        }

        var runner = go.AddComponent<CoroutineRunner>();

        runner.StartCoroutine(runner.MonitorRunning(coroutine));
    }

    IEnumerator MonitorRunning(IEnumerator coroutine) {
        while(coroutine.MoveNext()) {
            yield return coroutine.Current;
        }

        if(Application.isPlaying) {
            Destroy(gameObject);
        }
        else {
            DestroyImmediate(gameObject);
        }
    }
}