using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class TryRestAPI : MonoBehaviour
{
    [Header("API")]
    [SerializeField] private int characterId = 12;
    [SerializeField] private string URL = "https://rickandmortyapi.com/api/character";
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text speciesText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private RawImage CharacterImage;

    public void OnButtonPress()
    {
        StartCoroutine(GetInfo());
    }
    void Start()
    {
        //StartCoroutine(GetInfo());
    }

    IEnumerator GetInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get(URL + "/" + characterId);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            if (www.responseCode == 404)
            {
                Debug.Log("Character not found");
            }
        }
        else
        {
            Character character = JsonUtility.FromJson<Character>(www.downloadHandler.text);
            nameText.text = character.name;
            speciesText.text = character.species;
            statusText.text = character.status;
            Debug.Log(character.name + " is a " + character.species);

            StartCoroutine(GetTexture(character.image));


        }
    }

    IEnumerator GetTexture(string imageUrl)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                var texture = DownloadHandlerTexture.GetContent(uwr);
                CharacterImage.texture = texture;
            }
        }
    }
  
}

[System.Serializable]
public class Character
{
    public int id;
    public string name;
    public string species;
    public string status;
    public string image;
}


