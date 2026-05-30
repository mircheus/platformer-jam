using System.IO;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<string> completedQuests = new List<string>();

    public float playerX;
    public float playerY;
    public float playerZ;
}

public class SaveSystemController : MonoBehaviour
{
    private string path;

    private void Awake()
    {
        path = Application.persistentDataPath + "/save.json";
    }

    public void Save(GameObject player, QuestSystemController questSystem)
    {
        Debug.Log("SAVE CALLED");

        SaveData data = new SaveData();

        // =========================
        // PLAYER POSITION (TEMP DISABLED)
        // =========================
        /*
        Vector3 pos = player.transform.position;
        data.playerX = pos.x;
        data.playerY = pos.y;
        data.playerZ = pos.z;
        */

        // квесты
        foreach (var quest in questSystem.GetAllQuests())
        {
            if (quest.CurrentState == QuestState.Completed)
            {
                data.completedQuests.Add(quest.QuestID);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("Saving to: " + path);
        Debug.Log("Game Saved: " + path);
    }

    public void Load(GameObject player, QuestSystemController questSystem)
    {
        if (!File.Exists(path))
        {
            Debug.Log("No save found, starting new game");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // =========================
        // PLAYER POSITION (TEMP DISABLED)
        // =========================
        /*
        player.transform.position = new Vector3(
            data.playerX,
            data.playerY,
            data.playerZ
        );
        */

        // квесты
        foreach (var quest in questSystem.GetAllQuests())
        {
            if (data.completedQuests.Contains(quest.QuestID))
            {
                quest.CurrentState = QuestState.Completed;
            }
            else
            {
                quest.CurrentState = QuestState.Inactive;
            }
        }

        Debug.Log("Game Loaded");
    }

    private void OnApplicationQuit()
    {
        // можно вызвать автосейв через Singleton или ссылки
    }
}