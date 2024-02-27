using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveData
{
    public bool[] isActive;
    public int[] highScores;
    public int[] stars;
}

public class GameData : MonoBehaviour
{
    public static GameData gameData;
    public SaveData saveData;
    // Start is called before the first frame update
    void Awake()
    {
        if(gameData == null)
        {
            DontDestroyOnLoad(this.gameObject);
            gameData = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        Load();
    }
    private void Start()
    {
       
    }
    private void OnDisable()
    {
        Save();
    }
    public void Save()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/player.txt",FileMode.Create);
        SaveData data = new SaveData();
        data = saveData;
        formatter.Serialize(file, data);
        file.Close();
        Debug.Log("Saved");
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/player.txt"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file=File.Open(Application.persistentDataPath+"/player.txt",FileMode.Open);
            saveData=formatter.Deserialize(file) as SaveData;
            file.Close();
            Debug.Log("Loaded");
        }
        else
        {
            saveData= new SaveData();
            saveData.isActive = new bool[100];
            saveData.stars = new int[100];
            saveData.highScores = new int[100];
            saveData.isActive[0] = true;

        }

    }

    private void OnApplicationQuit()
    {
        Save();
    }

    // Update is called once per frame
    void Update()
    {
       
        
    }
}
