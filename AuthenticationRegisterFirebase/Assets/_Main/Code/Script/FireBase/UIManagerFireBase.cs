using System.Collections;
using TMPro;
using UnityEngine;

public enum MessageTypeFireBase
{
    Info,
    Success,
    Error
}

public class UIManagerFireBase : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject authPanel;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject rankingPanel;

    [Header("Auth UI")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_Text statusText;

    [Header("Home UI")]
    [SerializeField] private TMP_Text usernameHomeText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Project UI")]
    [SerializeField] private TMP_Text projectNameText;
    [SerializeField] private string projectLabel = "Alejandro Velasquez Rave";

    [Header("Managers")]
    [SerializeField] private FirebaseAuthManager authManager;
    [SerializeField] private FirebaseUserManager userManager;
    [SerializeField] private FirebaseRankingManager rankingManager;

    private Coroutine statusCoroutine;

    private void Awake()
    {
        if (projectNameText != null)
        {
            projectNameText.text = projectLabel;
        }

        ShowAuthPanel();
    }

    public void OnClickLogin()
    {
        string email = emailInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Completa correo y contraseña.", MessageTypeFireBase.Error);
            return;
        }

        authManager.Login(email, password);
    }

    public void OnClickRegister()
    {
        string email = emailInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Completa correo y contraseña.", MessageTypeFireBase.Error);
            return;
        }

        string username = GenerateUsernameFromEmail(email);
        authManager.Register(email, password, username);
    }

    public void OnClickResetPassword()
    {
        string email = emailInputField.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            ShowStatus("Escribe tu correo para recuperar contraseña.", MessageTypeFireBase.Error);
            return;
        }

        authManager.SendPasswordReset(email);
    }

    public void OnClickLogout()
    {
        authManager.Logout();
    }

    public void OnClickAddScore()
    {
        if (SessionDataFirebase.CurrentProfile == null)
        {
            ShowStatus("No hay usuario logueado.", MessageTypeFireBase.Error);
            return;
        }

        SessionDataFirebase.CurrentProfile.lastScore += 50;
        UpdateScoreText(SessionDataFirebase.CurrentProfile.lastScore);
    }

    public void OnClickResetScore()
    {
        if (SessionDataFirebase.CurrentProfile == null)
        {
            ShowStatus("No hay usuario logueado.", MessageTypeFireBase.Error);
            return;
        }

        SessionDataFirebase.CurrentProfile.lastScore = 0;
        UpdateScoreText(0);
    }

    public void OnClickSaveScore()
    {
        if (SessionDataFirebase.CurrentProfile == null)
        {
            ShowStatus("No hay usuario logueado.", MessageTypeFireBase.Error);
            return;
        }

        userManager.SaveScore(SessionDataFirebase.CurrentProfile.lastScore);
    }

    public void OnClickShowRanking()
    {
        rankingManager.LoadRanking();
    }

    public void OnClickBackToHome()
    {
        if (SessionDataFirebase.CurrentProfile != null)
        {
            ShowHomePanel(
                SessionDataFirebase.CurrentProfile.username,
                SessionDataFirebase.CurrentProfile.lastScore
            );
        }
        else
        {
            ShowAuthPanel();
        }
    }

    public void ShowAuthPanel()
    {
        authPanel.SetActive(true);
        homePanel.SetActive(false);
        rankingPanel.SetActive(false);
    }

    public void ShowHomePanel(string username, int score)
    {
        authPanel.SetActive(false);
        homePanel.SetActive(true);
        rankingPanel.SetActive(false);

        usernameHomeText.text = username;
        UpdateScoreText(score);
    }

    public void ShowRankingPanel()
    {
        authPanel.SetActive(false);
        homePanel.SetActive(false);
        rankingPanel.SetActive(true);
    }

    public void UpdateScoreText(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void ShowStatus(string message, MessageTypeFireBase type)
    {
        if (statusCoroutine != null)
        {
            StopCoroutine(statusCoroutine);
        }

        statusCoroutine = StartCoroutine(SetStatusCoroutine(message, type));
    }

    private IEnumerator SetStatusCoroutine(string message, MessageTypeFireBase type)
    {
        statusText.gameObject.SetActive(true);
        statusText.text = message;

        switch (type)
        {
            case MessageTypeFireBase.Success:
                statusText.color = Color.green;
                break;

            case MessageTypeFireBase.Error:
                statusText.color = Color.red;
                break;

            case MessageTypeFireBase.Info:
            default:
                statusText.color = Color.white;
                break;
        }

        yield return new WaitForSeconds(1.5f);

        statusText.gameObject.SetActive(false);
        statusCoroutine = null;
    }

    private string GenerateUsernameFromEmail(string email)
    {
        int atIndex = email.IndexOf("@");

        if (atIndex > 0)
        {
            return email.Substring(0, atIndex);
        }

        return email;
    }
}