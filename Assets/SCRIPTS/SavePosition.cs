using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SavePosition : MonoBehaviour
{
    string savepath;

    // Start is called before the first frame update
    void Start()
    {
        savepath = Application.persistentDataPath + "/" + gameObject.name + "myData.dat";
        //Debug.Log(savepath);
        //Load();
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerPrefs.GetInt("Loading") == 1)
        {
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Load();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Load();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Save();
        }
        
    }
    void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (!File.Exists(savepath))
        {
            file = File.Create(savepath);
        }
        else
        {
            file = File.Open(savepath, FileMode.Open);
        }
        DataObject data = new DataObject(transform.position);
        bf.Serialize(file, data);
        file.Close();
    }
    void Load()
    {
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(savepath))
        {
            FileStream file = File.Open(savepath, FileMode.Open);
            DataObject data = (DataObject)bf.Deserialize(file);
            transform.position = data.GetVector3();
        }
    }
}
[System.Serializable]
public class DataObject
{
    public float x;
    public float y;
    public float z;
    public DataObject(Vector3 position)
    {
        x = position.x;
        y = position.y;
        z = position.z;
    }
    public Vector3 GetVector3()
    {
        return new Vector3(x, y, z);
    }
}
