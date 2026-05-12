using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text speciesText;
    [SerializeField] TMP_Text statusText;
    [SerializeField] RawImage image;

    public void SetData(string name, string species, string status, Texture2D tex)
    {
        nameText.text = name;
        speciesText.text = species;
        statusText.text = status;
        image.texture = tex;
    }
    /*public void SetLoading()
    {
        if (nameText) nameText.text = "Cargando...";
        if (speciesText) speciesText.text = "";
        if (statusText) statusText.text = "";
        if (image) image.texture = null;
    }*/
}