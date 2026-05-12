using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    public IEnumerator GetJson(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            onError?.Invoke(www.error);
        else
            onSuccess?.Invoke(www.downloadHandler.text);
    }

    public IEnumerator GetTexture(string url, System.Action<Texture2D> onSuccess, System.Action<string> onError)
    {
        UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
            onError?.Invoke(uwr.error);
        else
            onSuccess?.Invoke(DownloadHandlerTexture.GetContent(uwr));
    }
}