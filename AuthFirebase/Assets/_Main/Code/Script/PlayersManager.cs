using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayersManager : MonoBehaviour
{
    private Dictionary<int, Character> characterCache = new Dictionary<int, Character>();
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    [Header("API URLs")]
    [SerializeField] private string playersUrl = "https://my-json-server.typicode.com/Iskandar1320/ApiSistemas_01_/jugadores";
    [SerializeField] private string URL_API = "https://rickandmortyapi.com/api/character";

    [Header("Referencias")]
    [SerializeField] private ApiClient api;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerIdText;
    [SerializeField] private CardUI[] cardSlots = new CardUI[5];

    private Jugador[] jugadores;
    private int currentIndex = 0;
    private Coroutine showRoutine;
    private bool isLoading;

    void Start()
    {
        StartCoroutine(GetPlayersDB());
    }

    public void NextPlayer()
    {
        if (jugadores == null || jugadores.Length == 0) return;
        if (isLoading) return; // opcional: bloqueo mientras carga

        currentIndex = (currentIndex + 1) % jugadores.Length;

        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowPlayer());
    }

    public void PrevPlayer()
    {
        if (jugadores == null || jugadores.Length == 0) return;
        if (isLoading) return;

        currentIndex = (currentIndex - 1 + jugadores.Length) % jugadores.Length;

        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowPlayer());
    }

    IEnumerator GetPlayersDB()
    {
        string json = null;

        yield return StartCoroutine(api.GetJson(
            playersUrl,
            onSuccess: (txt) => json = txt,
            onError: (err) => Debug.LogError("Players API error: " + err)
        ));

        if (string.IsNullOrEmpty(json)) yield break;

        string wrappedJson = "{\"jugadores\":" + json + "}";

        JugadorList list = JsonUtility.FromJson<JugadorList>(wrappedJson);
        jugadores = list.jugadores;

        currentIndex = 0;
        yield return StartCoroutine(ShowPlayer());
    }
    IEnumerator ShowPlayer()
    {
        isLoading = true;
        Jugador jugador = jugadores[currentIndex];

            playerNameText.text = $"{jugador.nombre}" ;//(ID {jugador.id})";
            playerIdText.text = jugador.id.ToString();

        // CargaVisual??
        /*for (int i = 0; i < cardSlots.Length; i++)
            if (cardSlots[i]) cardSlots[i].SetLoading();*/

        int count = Mathf.Min(cardSlots.Length, jugador.cartas.Length);

        // Fill
        for (int i = 0; i < count; i++)
        {
            int characterId = jugador.cartas[i];
            yield return FillCard(cardSlots[i], characterId);
            yield return new WaitForSeconds(0.15f);
        }
        isLoading = false;
    }
    IEnumerator FillCard(CardUI slot, int characterId)
    {
        Character ch;

        if (characterCache.TryGetValue(characterId, out ch))
        {
            // ya lo tengo, no pido JSON
        }
        else
        {
            string json = null;

            yield return api.GetJson(
                $"{URL_API}/{characterId}",
                onSuccess: (txt) => json = txt,
                onError: (err) => Debug.LogError("Character error: " + err)
            );

            if (string.IsNullOrEmpty(json)) yield break;

            ch = JsonUtility.FromJson<Character>(json);
            characterCache[characterId] = ch; // ✅ guardo en cache
        }
        Texture2D tex;

        if (textureCache.TryGetValue(ch.image, out tex))
        {
            // ya la tengo, no descargo imagen
        }
        else
        {
            tex = null;

            yield return api.GetTexture(
                ch.image,
                onSuccess: (t) => tex = t,
                onError: (err) => Debug.LogError("Image error: " + err)
            );

            if (tex == null) yield break;

            textureCache[ch.image] = tex; // ✅ guardo en cache
        }
        slot.SetData(ch.name, ch.species, ch.status, tex);
    }

    [System.Serializable]
    private class JugadorList
    {
        public Jugador[] jugadores;
    }

    [System.Serializable]
    private class Jugador
    {
        public int id;
        public string nombre;
        public int[] cartas;
    }

    [System.Serializable]
    private class Character
    {
        public int id;
        public string name;
        public string species;
        public string status;
        public string image;
    }
}