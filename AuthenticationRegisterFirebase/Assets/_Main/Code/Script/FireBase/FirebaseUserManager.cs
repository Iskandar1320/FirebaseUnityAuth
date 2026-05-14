using System;
using Firebase.Database;
using UnityEngine;

public class FirebaseUserManager : MonoBehaviour
{
    [SerializeField] private UIManagerFireBase uiManager;

    public async void LoadProfile(string uid)
    {
        if (!FirebaseBootstrap.IsReady)
        {
            uiManager.ShowStatus("Firebase aún no está listo.", MessageTypeFireBase.Error);
            return;
        }

        try
        {
            DataSnapshot snapshot = await FirebaseBootstrap.RootReference
                .Child("users")
                .Child(uid)
                .GetValueAsync();

            if (snapshot == null || !snapshot.Exists)
            {
                uiManager.ShowStatus("Perfil no encontrado.", MessageTypeFireBase.Error);
                uiManager.ShowAuthPanel();
                return;
            }

            FirebaseUserProfileData profile = SnapshotToProfile(snapshot);

            SessionDataFirebase.CurrentProfile = profile;

            uiManager.ShowHomePanel(profile.username, profile.lastScore);
            uiManager.ShowStatus("Perfil cargado.", MessageTypeFireBase.Info);
        }
        catch (Exception ex)
        {
            uiManager.ShowStatus("Error cargando perfil.", MessageTypeFireBase.Error);
            Debug.LogError("Load profile error: " + ex);
        }
    }

    public async void SaveScore(int newScore)
    {
        if (!SessionDataFirebase.IsLoggedIn() || SessionDataFirebase.CurrentProfile == null)
        {
            uiManager.ShowStatus("Debes iniciar sesión.", MessageTypeFireBase.Error);
            uiManager.ShowAuthPanel();
            return;
        }

        int newHighScore = Mathf.Max(newScore, SessionDataFirebase.CurrentProfile.highScore);

        SessionDataFirebase.CurrentProfile.lastScore = newScore;
        SessionDataFirebase.CurrentProfile.highScore = newHighScore;

        string uid = SessionDataFirebase.FirebaseUser.UserId;
        string json = JsonUtility.ToJson(SessionDataFirebase.CurrentProfile);

        try
        {
            await FirebaseBootstrap.RootReference
                .Child("users")
                .Child(uid)
                .SetRawJsonValueAsync(json);

            uiManager.ShowHomePanel(
                SessionDataFirebase.CurrentProfile.username,
                SessionDataFirebase.CurrentProfile.lastScore
            );

            uiManager.ShowStatus("Score guardado.", MessageTypeFireBase.Success);
        }
        catch (Exception ex)
        {
            uiManager.ShowStatus("Error guardando score.", MessageTypeFireBase.Error);
            Debug.LogError("Save score error: " + ex);
        }
    }

    private FirebaseUserProfileData SnapshotToProfile(DataSnapshot snapshot)
    {
        FirebaseUserProfileData profile = new FirebaseUserProfileData();

        profile.uid = GetString(snapshot, "uid");
        profile.email = GetString(snapshot, "email");
        profile.username = GetString(snapshot, "username");
        profile.highScore = GetInt(snapshot, "highScore");
        profile.lastScore = GetInt(snapshot, "lastScore");

        return profile;
    }

    private string GetString(DataSnapshot snapshot, string childName)
    {
        DataSnapshot child = snapshot.Child(childName);

        if (child == null || child.Value == null)
        {
            return "";
        }

        return child.Value.ToString();
    }

    private int GetInt(DataSnapshot snapshot, string childName)
    {
        DataSnapshot child = snapshot.Child(childName);

        if (child == null || child.Value == null)
        {
            return 0;
        }

        return Convert.ToInt32(child.Value);
    }
}