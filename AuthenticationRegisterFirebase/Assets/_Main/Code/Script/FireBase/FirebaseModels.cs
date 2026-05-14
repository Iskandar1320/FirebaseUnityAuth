using System;

[Serializable]
public class FirebaseUserProfileData
{
    public string uid;
    public string email;
    public string username;
    public int highScore;
    public int lastScore;

    public FirebaseUserProfileData() { }

    public FirebaseUserProfileData(string uid, string email, string username)
    {
        this.uid = uid;
        this.email = email;
        this.username = username;
        this.highScore = 0;
        this.lastScore = 0;
    }
}