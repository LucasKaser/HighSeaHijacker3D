using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestsOnPlayer : MonoBehaviour
{
    /*int KeyLocRand = 11;
    public GameObject Key0;
    public GameObject Key1;
    public GameObject Key2;
    public GameObject Key3;
    public GameObject Key4;*/
    public bool trolleyGo = true;
    //find the trolley stop
    public bool choosePath = false;
    //choose a path to your house
    public bool goToGate = false;
    //find the gate house
    public bool goToPower = false;
    //find the power station
    public bool turnOnPower;
    //turn on the power
    public GameObject SwitchOn;
    public bool goToGate2 = false;
    //go back to the gate house
    public bool openGate = false;
    //open the gate
    public GameObject GateSwitchOn;
    public bool findSGate = false;
    //find the gate in the slums
    public bool findKey = false;
    //search for the key in the slums;
    public bool findSGate2 = false;
    //return to the slum gate
    public bool goToHouse = false;
    //enter the suburbs
    public bool haveKey = false;
    public GameObject Pgate;
    public GameObject Sgate;
    //GameObjects that are texts
    public Text QuestText;
    public string T1 = "Search for the Bus Stop to get home";
    public string T2 = "Go to the Park or the Slums.";
    public string TP1 = "Find the Gate to get into the suburbs.";
    public string TP2 = "The gate is locked, find the power station to turn on the gate controls.";
    public string TP3 = "find the gate control center to open the gate.";
    public string TS1 = "Find the gate from the slums to the suburbs.";
    public string TS2 = "Find the key to the slums gate.";
    public string TS3 = "Go back to the slum gate.";
    public string T3 = "Search for and enter your house. (HINT: The porch light is on)";

    // Start is called before the first frame update
    void Start()
    {
        //trolleyGo = true;
        QuestText.GetComponent<Text>().text = T1;
        //Debug.Log(SaveScript.allData.QuestText);
        switch (SaveScript.allData.QuestText)
        {
            case "T1":
                QuestText.GetComponent<Text>().text = T1;
                break;
            case "T2":
                QuestText.GetComponent<Text>().text = T2;
                break;
            case "TP1":
                QuestText.GetComponent<Text>().text = TP1;
                break;
            case "TP2":
                QuestText.GetComponent<Text>().text = TP2;
                break;
            case "TP3":
                QuestText.GetComponent<Text>().text = TP3;
                break;
            case "TS1":
                QuestText.GetComponent<Text>().text = TS1;
                break;
            case "TS2":
                QuestText.GetComponent<Text>().text = TS2;
                break;
            case "TS3":
                QuestText.GetComponent<Text>().text = TS3;
                break;
            case "T3":
                QuestText.GetComponent<Text>().text = T3;
                break;
        }
        trolleyGo = SaveScript.allData.trolleyGo;
        choosePath = SaveScript.allData.choosePath;
        goToGate = SaveScript.allData.goToGate;
        findSGate = SaveScript.allData.findSGate;
        goToPower = SaveScript.allData.goToPower;
        turnOnPower = SaveScript.allData.turnOnPower;
        goToGate2 = SaveScript.allData.goToGate2;
        openGate = SaveScript.allData.openGate;
        findKey = SaveScript.allData.findKey;
        findSGate2 = SaveScript.allData.findSGate2;
        goToHouse = SaveScript.allData.goToHouse;
}

    // Update is called once per frame
    void Update()
    {
        
        if (PlayerPrefs.GetInt("Loading") == 1)
        {
            //Debug.Log(SaveScript.allData.QuestText);
            
            switch (SaveScript.allData.QuestText)
            {
                case "T1":
                    QuestText.GetComponent<Text>().text = T1;
                    break;
                case "T2":
                    QuestText.GetComponent<Text>().text = T2;
                    break;
                case "TP1":
                    QuestText.GetComponent<Text>().text = TP1;
                    break;
                case "TP2":
                    QuestText.GetComponent<Text>().text = TP2;
                    break;
                case "TP3":
                    QuestText.GetComponent<Text>().text = TP3;
                    break;
                case "TS1":
                    QuestText.GetComponent<Text>().text = TS1;
                    break;
                case "TS2":
                    QuestText.GetComponent<Text>().text = TS2;
                    break;
                case "TS3":
                    QuestText.GetComponent<Text>().text = TS3;
                    break;
                case "T3":
                    QuestText.GetComponent<Text>().text = T3;
                    break;
            }
            trolleyGo = SaveScript.allData.trolleyGo;
            choosePath = SaveScript.allData.choosePath;
            goToGate = SaveScript.allData.goToGate;
            findSGate = SaveScript.allData.findSGate;
            goToPower = SaveScript.allData.goToPower;
            turnOnPower = SaveScript.allData.turnOnPower;
            goToGate2 = SaveScript.allData.goToGate2;
            openGate = SaveScript.allData.openGate;
            findKey = SaveScript.allData.findKey;
            findSGate2 = SaveScript.allData.findSGate2;
            goToHouse = SaveScript.allData.goToHouse;
        }
        if (gameObject.GetComponent<ObjectInteraction>().SKey == true)
        {
            haveKey = true;
        }
        if (gameObject.GetComponent<ObjectInteraction>().SKey == false)
        {
            haveKey = false;
        }
        if (SwitchOn.activeSelf == true && turnOnPower == true)
        {
            turnOnPower = false;
            SaveScript.allData.turnOnPower = false;
            goToGate2 = true;
            SaveScript.allData.goToGate2 = true;
        }
        if (GateSwitchOn.activeSelf == true && openGate == true)
        {
            openGate = false;
            SaveScript.allData.openGate = false;
            goToHouse = true;
            SaveScript.allData.goToHouse = true;
        }
        if(trolleyGo == true)
        {
            QuestText.GetComponent<Text>().text = T1;
            //Debug.Log("Search for the Bus Stop to get home.");
            SaveScript.allData.QuestText = "T1";
        }
        if(choosePath == true)
        {
            QuestText.GetComponent<Text>().text = T2;
            //Debug.Log("Go to the Park or the Slums.");
            Sgate.SetActive(false);
            Pgate.SetActive(false);
            SaveScript.allData.QuestText = "T2";
        }
        if (goToGate == true)
        {
            QuestText.GetComponent<Text>().text = TP1;
            //Debug.Log("Find the Gate to get into the suburbs.");
            SaveScript.allData.QuestText = "TP1";
        }
        if (goToPower == true)
        {
            QuestText.GetComponent<Text>().text = TP2;
            //Debug.Log("The gate is locked, find the power station to turn on the gate controls.");
            SaveScript.allData.QuestText = "TP2";
        }
        if (goToGate2 == true)
        {
            QuestText.GetComponent<Text>().text = TP3;
            //Debug.Log("find the gate control center to open the gate.");
            SaveScript.allData.QuestText = "TP3";
        }
        if (findSGate == true)
        {
            QuestText.GetComponent<Text>().text = TS1;
            //Debug.Log("Find the gate from the slums to the suburbs.");
            SaveScript.allData.QuestText = "TS1";
        }
        if (findKey == true)
        {
            QuestText.GetComponent<Text>().text = TS2;
            //Debug.Log("Find the key to the slums gate.");
            SaveScript.allData.QuestText = "TS2";
        }
        if (findSGate2 == true)
        {
            QuestText.GetComponent<Text>().text = TS3;
            //Debug.Log("Go back to the slum gate.");
            SaveScript.allData.QuestText = "TS3";
        }
        if (goToHouse == true)
        {
            QuestText.GetComponent<Text>().text = T3;
            //Debug.Log("Search for and enter your house. (HINT: The porch light is on)");
            SaveScript.allData.QuestText = "T3";
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "TrolleyTrigger" && trolleyGo == true)
        {
            
            trolleyGo = false;
            SaveScript.allData.trolleyGo = false;
            choosePath = true;
            SaveScript.allData.choosePath = true;
            //found trolley
        }
        if (other.gameObject.name == "PathChosenPark" && choosePath == true)
        {
            choosePath = false;
            SaveScript.allData.choosePath = false;
            goToGate = true;
            SaveScript.allData.goToGate = true;
            Pgate.SetActive(true);
            //decide to go to forest
        }
        if(other.gameObject.name == "PathChosenSlums" && choosePath == true)
        {
            choosePath = false;
            SaveScript.allData.choosePath = false;
            findSGate = true;
            SaveScript.allData.findSGate = true;
            Sgate.SetActive(true);
            //decide to go to slums
        }
        if (other.gameObject.name == "SubGate" && goToGate2 == false && goToGate == true)
        {
            goToGate = false;
            SaveScript.allData.goToGate = false;
            goToPower = true;
            SaveScript.allData.goToPower = true;
            //find power house
        }
        if (other.gameObject.name == "PowerStation" && goToPower == true)
        {
            goToPower = false;
            SaveScript.allData.goToPower = false;
            turnOnPower = true;
            SaveScript.allData.turnOnPower = true;
            //turned on power, find gate control
        }
        if (other.gameObject.name == "GateSwitch" && goToGate2 == true)
        {
            goToGate2 = false;
            SaveScript.allData.goToGate2 = false;
            openGate = true;
            SaveScript.allData.openGate = true;
            //gate is now open
        }
        if (other.gameObject.name == "SGate" && findSGate == true)
        {
            /*if (gameObject.GetComponent<ObjectInteraction>().SKey == false)
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
            findSGate = false;
            SaveScript.allData.findSGate = false;
            findKey = true;
            SaveScript.allData.findKey = true;
            //gate is locked, find key
        }
        if (haveKey == true && findKey == true)
        {
            findKey = false;
            SaveScript.allData.findKey = false;
            findSGate2 = true;
            SaveScript.allData.findSGate2 = true;
            //key is found, return to gate
        }
        if (other.gameObject.name == "SGate" && findSGate2 == true)
        {
            findSGate2 = false;
            SaveScript.allData.findSGate2 = false;
            goToHouse = true;
            SaveScript.allData.goToHouse = true;
            //gate is open, go to house
        }
    }
}
