using UnityEngine;
using System.IO;

public class SaveData : MonoBehaviour
{
 

    private const string FileName = "savegame.json";
    private const string PlayerPrefsKey = "SaveGameData";

    [System.Serializable]


    public class GameSave
    {
        public bool tutorialCompleted;
        // Add other fields as needed
    }

    public void SaveTutorialCompleted(bool completed)
    {
        GameSave data = LoadGameSave();
        data.tutorialCompleted = completed;
        string json = JsonUtility.ToJson(data);

#if UNITY_WEBGL
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
#else
        string path = Path.Combine(Application.persistentDataPath, FileName);
        File.WriteAllText(path, json);
#endif
    }

    public bool LoadTutorialCompleted()
    {
        GameSave data = LoadGameSave();
        return data.tutorialCompleted;
    }

    private GameSave LoadGameSave()
    {
#if UNITY_WEBGL
        if (PlayerPrefs.HasKey(PlayerPrefsKey))
        {
            string json = PlayerPrefs.GetString(PlayerPrefsKey);
            return JsonUtility.FromJson<GameSave>(json);
        }
#else
        string path = Path.Combine(Application.persistentDataPath, FileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameSave>(json);
        }
#endif
        return new GameSave();
    }
}