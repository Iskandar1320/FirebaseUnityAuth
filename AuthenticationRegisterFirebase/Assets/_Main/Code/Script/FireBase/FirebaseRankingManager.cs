using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using UnityEngine;

public class FirebaseRankingManager : MonoBehaviour
{
    [SerializeField] private UIManagerFireBase uiManager;

    [Header("Ranking UI")]
    [SerializeField] private Transform rankingContainer;
    [SerializeField] private RankingItemUIFirebase rankingItemPrefab;

    [Header("Ranking Settings")]
    [SerializeField] private int maxRankingItems = 10;

    public async void LoadRanking()
    {
        if (!FirebaseBootstrap.IsReady)
        {
            uiManager.ShowStatus("Firebase aún no está listo.", MessageTypeFireBase.Error);
            return;
        }

        if (!SessionDataFirebase.IsLoggedIn())
        {
            uiManager.ShowStatus("Debes iniciar sesión.", MessageTypeFireBase.Error);
            uiManager.ShowAuthPanel();
            return;
        }

        try
        {
            DataSnapshot snapshot = await FirebaseBootstrap.RootReference
                .Child("users")
                .GetValueAsync();

            List<FirebaseUserProfileData> profiles = new List<FirebaseUserProfileData>();

            if (snapshot != null && snapshot.Exists)
            {
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    FirebaseUserProfileData profile = SnapshotToProfile(childSnapshot);
                    profiles.Add(profile);
                }
            }

            List<FirebaseUserProfileData> sortedProfiles = profiles
                .OrderByDescending(profile => profile.highScore)
                .Take(maxRankingItems)
                .ToList();

            DrawRanking(sortedProfiles);

            uiManager.ShowRankingPanel();
            uiManager.ShowStatus("Ranking cargado.", MessageTypeFireBase.Success);
        }
        catch (Exception ex)
        {
            uiManager.ShowStatus("Error cargando ranking.", MessageTypeFireBase.Error);
            Debug.LogError("Load ranking error: " + ex);
        }
    }

    private void DrawRanking(List<FirebaseUserProfileData> profiles)
    {
        ClearRanking();

        for (int i = 0; i < profiles.Count; i++)
        {
            RankingItemUIFirebase item = Instantiate(rankingItemPrefab, rankingContainer);
            item.Setup(
                i + 1,
                profiles[i].username,
                profiles[i].highScore
            );
        }
    }

    private void ClearRanking()
    {
        for (int i = rankingContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(rankingContainer.GetChild(i).gameObject);
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