using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyLocation : MonoBehaviour
{
    int KeyLocRand = 11;
    public GameObject Key0;
    public GameObject Key1;
    public GameObject Key2;
    public GameObject Key3;
    public GameObject Key4;
    public bool keyspawned = false;

    // Start is called before the first frame update
    void Start()
    {
       /* if (gameObject.GetComponent<ObjectInteraction>().SKey == false && SaveScript.allData.QuestText == "TS2")
        {
            KeyLocRand = Random.Range(0, 5);
            Debug.Log(KeyLocRand);
            switch (KeyLocRand)
            {
                case 0:
                    Key0.SetActive(true);
                    break;
                case 1:
                    Key1.SetActive(true);
                    break;
                case 2:
                    Key2.SetActive(true);
                    break;
                case 3:
                    Key3.SetActive(true);
                    break;
                case 4:
                    Key4.SetActive(true);
                    break;
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<ObjectInteraction>().SKey == false && SaveScript.allData.QuestText == "TS2" && SaveScript.allData.SKey == false && keyspawned == false)
        {
            KeyLocRand = Random.Range(0, 5);
            Debug.Log(KeyLocRand);
            switch (KeyLocRand)
            {
                case 0:
                    Key0.SetActive(true);
                    break;
                case 1:
                    Key1.SetActive(true);
                    break;
                case 2:
                    Key2.SetActive(true);
                    break;
                case 3:
                    Key3.SetActive(true);
                    break;
                case 4:
                    Key4.SetActive(true);
                    break;    
            }
            keyspawned = true;
        }
    }
}
