using System;
using System.Collections;
using Firebase.Auth;
using UnityEngine;

public class FirebaseAuthManager : MonoBehaviour
{
    [SerializeField] private UIManagerFireBase uiManager;
    [SerializeField] private FirebaseUserManager userManager;

    private bool isListeningAuthState;

    private void Start()
    {
        StartCoroutine(WaitForFirebaseAndInitialize());
    }

    private IEnumerator WaitForFirebaseAndInitialize()
    {
        while (!FirebaseBootstrap.IsReady)
        {
            yield return null;
        }

        FirebaseBootstrap.Auth.StateChanged += OnAuthStateChanged;
        isListeningAuthState = true;

        CheckCurrentUser();
    }

    private void OnDestroy()
    {
        if (isListeningAuthState && FirebaseBootstrap.Auth != null)
        {
            FirebaseBootstrap.Auth.StateChanged -= OnAuthStateChanged;
        }
    }

    private void OnAuthStateChanged(object sender, EventArgs e)
    {
        CheckCurrentUser();
    }

    private void CheckCurrentUser()
    {
        if (!FirebaseBootstrap.IsReady)
        {
            return;
        }

        FirebaseUser user = FirebaseBootstrap.Auth.CurrentUser;

        if (user == null)
        {
            SessionDataFirebase.Clear();

            if (uiManager != null)
            {
                uiManager.ShowAuthPanel();
            }

            return;
        }

        SessionDataFirebase.FirebaseUser = user;

        if (userManager != null)
        {
            userManager.LoadProfile(user.UserId);
        }
    }

    public async void Register(string email, string password, string username)
    {
        if (!FirebaseBootstrap.IsReady)
        {
            uiManager.ShowStatus("Firebase aún no está listo.", MessageTypeFireBase.Error);
            return;
        }

        try
        {
            AuthResult result = await FirebaseBootstrap.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = result.User;

            FirebaseUserProfileData profile = new FirebaseUserProfileData(
                user.UserId,
                email,
                username
            );

            string json = JsonUtility.ToJson(profile);

            await FirebaseBootstrap.RootReference
                .Child("users")
                .Child(user.UserId)
                .SetRawJsonValueAsync(json);

            SessionDataFirebase.FirebaseUser = user;
            SessionDataFirebase.CurrentProfile = profile;

            uiManager.ShowStatus("Registro exitoso.", MessageTypeFireBase.Success);
            userManager.LoadProfile(user.UserId);
        }
        catch (Exception ex)
        {
            uiManager.ShowStatus("Error en registro.", MessageTypeFireBase.Error);
            Debug.LogError("Register error: " + ex);
        }
    }

    public async void Login(string email, string password)
    {
        if (!FirebaseBootstrap.IsReady)
        {
            uiManager.ShowStatus("Firebase aún no está listo.", MessageTypeFireBase.Error);
            return;
        }

        try
        {
            AuthResult result = await FirebaseBootstrap.Auth.SignInWithEmailAndPasswordAsync(email, password);
            FirebaseUser user = result.User;

            SessionDataFirebase.FirebaseUser = user;

            uiManager.ShowStatus("Login exitoso.", MessageTypeFireBase.Success);
            userManager.LoadProfile(user.UserId);
        }
        catch (Exception ex)
        {
            uiManager.ShowStatus("Error en login.", MessageTypeFireBase.Error);
            Debug.LogError("Login error: " + ex);
        }
    }

    public async void SendPasswordReset(string email)
    {
        if (!FirebaseBootstrap.IsReady)
        {
            uiManager.ShowStatus("Firebase aún no está listo.", MessageTypeFireBase.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            uiManager.ShowStatus("Escribe tu correo.", MessageTypeFireBase.Error);
            return;
        }

        try
        {
            await FirebaseBootstrap.Auth.SendPasswordResetEmailAsync(email);
            uiManager.ShowStatus("Correo de recuperación enviado.", MessageTypeFireBase.Success);
        }
        catch (Exception ex)
        {
            uiManager.ShowStatus("No se pudo enviar recuperación.", MessageTypeFireBase.Error);
            Debug.LogError("Reset password error: " + ex);
        }
    }

    public void Logout()
    {
        if (FirebaseBootstrap.Auth != null)
        {
            FirebaseBootstrap.Auth.SignOut();
        }

        SessionDataFirebase.Clear();
        uiManager.ShowAuthPanel();
        uiManager.ShowStatus("Sesión cerrada.", MessageTypeFireBase.Info);
    }
}