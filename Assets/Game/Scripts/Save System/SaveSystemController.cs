using System.IO;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class NPCProgressData
{
    public string npcID;
    public int progress;
}

[System.Serializable]
public class SaveData
{
    public List<string> completedQuests = new List<string>();

    public List<NPCProgressData> npcProgress = new List<NPCProgressData>();

    public float playerX;
    public float playerY;
    public float playerZ;
}

public class SaveSystemController : MonoBehaviour
{
    private static string SavePath => Application.persistentDataPath + "/save.json";

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

        // прогресс диалогов NPC
        foreach (var npc in FindObjectsByType<NPCInteractable>(FindObjectsSortMode.None))
        {
            data.npcProgress.Add(new NPCProgressData
            {
                npcID = npc.ObjectID,
                progress = npc.Progress
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Saving to: " + SavePath);
        Debug.Log("Game Saved: " + SavePath);
    }

    public void Load(GameObject player, QuestSystemController questSystem)
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save found, starting new game");
            return;
        }

        string json = File.ReadAllText(SavePath);
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

        // прогресс диалогов NPC
        foreach (var npc in FindObjectsByType<NPCInteractable>(FindObjectsSortMode.None))
        {
            foreach (var entry in data.npcProgress)
            {
                if (entry.npcID == npc.ObjectID)
                {
                    npc.SetProgress(entry.progress);
                    break;
                }
            }
        }

        Debug.Log("Game Loaded");
    }

    public static void ClearSaves()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No save file to clear: " + SavePath);
            return;
        }

        File.Delete(SavePath);

        Debug.Log("Save cleared: " + SavePath);
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Save System/Clear Saves")]
    private static void ClearSavesMenuItem()
    {
        ClearSaves();
    }
#endif

    private void OnApplicationQuit()
    {
        // можно вызвать автосейв через Singleton или ссылки
    }
}