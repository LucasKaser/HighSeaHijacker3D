using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveScript : MonoBehaviour
{

    string savePath;
    int oilAmnt = 0;
    public static AllSaveData allData;
    float timer = 0;
    // Start is called before the first frame update
    void Awake()
    {
        allData = new AllSaveData();
        savePath = Application.persistentDataPath + "/" + gameObject.name + "myData.dat";
        //Debug.Log(savePath);
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerPrefs.GetInt("Loading") == 1)
        {
            timer += Time.deltaTime;
            if(timer > 0.2f)
            {
                PlayerPrefs.SetInt("Loading", 0);
            }
        }
        oilAmnt = gameObject.GetComponent<LampScript>().OilAmnt;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            allData.Save();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            allData = allData.Load();
            //Debug.Log(allData.QuestText);
        }
    }
    
}
[System.Serializable]
public class AllSaveData
{
    public bool flippedswitch = false;
    public bool powerswitch = true;
    public bool gate = true;
    public bool flippedswitch2 = false;
    public bool gateswitch = true;
    public bool SKey = false;
    public int oilAmnt = 2;
    public bool Sgate = true;
    public bool trolleyGo = true;
    public bool choosePath = false;
    public bool goToGate = false;
    public bool findSGate = false;
    public bool goToPower = false;
    public bool turnOnPower = false;
    public bool goToGate2 = false;
    public bool openGate = false;
    public bool findKey = false;
    public bool findSGate2 = false;
    public bool goToHouse = false;
    public string QuestText = "T1";
    string savePath = "AllSaveData";
    public AllSaveData()
    {
        savePath = Application.persistentDataPath + "/" + savePath + "myData.dat";
    }
    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (!File.Exists(savePath))
        {
            file = File.Create(savePath);
        }
        else
        {
            file = File.Open(savePath, FileMode.Open);
        }
        AllSaveData data = this;
        //Debug.Log(data.QuestText);
        bf.Serialize(file, data);
        file.Close();
    }
    public AllSaveData Load()
    {
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(savePath))
        {
            FileStream file = File.Open(savePath, FileMode.Open);
            AllSaveData data = (AllSaveData)bf.Deserialize(file);
            PlayerPrefs.SetInt("Loading", 1);
            file.Close();
            return data;
            
        }
        else
        {
            return new AllSaveData();
        }
    }
}

