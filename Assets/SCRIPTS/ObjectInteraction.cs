using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    Camera camera;
    public GameObject interactButton;
    /*public GameObject BBClosed;
    public GameObject BBOpen;
    public GameObject BBSwitchOff;
    public GameObject BBSwitchOn;*/
    public AudioClip Flicker;
    public GameObject powerswitch;
    bool canswitch = false;
    public GameObject gate;
    public GameObject flippedswitch;
    public bool SKey = false;
    public GameObject gateswitch;
    public GameObject flippedswitch2;
    public GameObject Sgate;
    //public GameObject AssociatedLights;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        //gate.SetActive(true);
        if(SaveScript.allData.SKey == true)
        {
            SKey = true;
        }
        else
        {
            SKey = false;
        }
        if (SaveScript.allData.Sgate == false)
        {
            Sgate.SetActive(false);
        }
        else
        {
            Sgate.SetActive(true);
        }
        if(SaveScript.allData.flippedswitch == true)
        {
            flippedswitch.SetActive(true);
        }
        else
        {
            flippedswitch.SetActive(false);
        }
        if(SaveScript.allData.powerswitch == false)
        {
            powerswitch.SetActive(false);
        }
        else
        {
            powerswitch.SetActive(true);
        }
        if(SaveScript.allData.gate == false)
        {
            gate.SetActive(false);
        }
        else
        {
            gate.SetActive(true);
        }
        if(SaveScript.allData.flippedswitch2 == true)
        {
            flippedswitch2.SetActive(true);
        }
        else
        {
            flippedswitch2.SetActive(false);
        }
        if(SaveScript.allData.gateswitch == false)
        {
            gateswitch.SetActive(false);
        }
        else
        {
            gateswitch.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (PlayerPrefs.GetInt("Loading") == 1)
        {
            if (SaveScript.allData.SKey == true)
            {
                SKey = true;
            }
            else
            {
                SKey = false;
            }
            if (SaveScript.allData.Sgate == false)
            {
                Sgate.SetActive(false);
            }
            else
            {
                Sgate.SetActive(true);
            }
            if (SaveScript.allData.flippedswitch == true)
            {
                flippedswitch.SetActive(true);
            }
            else
            {
                flippedswitch.SetActive(false);
            }
            if (SaveScript.allData.powerswitch == false)
            {
                powerswitch.SetActive(false);
            }
            else
            {
                powerswitch.SetActive(true);
            }
            if (SaveScript.allData.gate == false)
            {
                gate.SetActive(false);
            }
            else
            {
                gate.SetActive(true);
            }
            if (SaveScript.allData.flippedswitch2 == true)
            {
                flippedswitch2.SetActive(true);
            }
            else
            {
                flippedswitch2.SetActive(false);
            }
            if (SaveScript.allData.gateswitch == false)
            {
                gateswitch.SetActive(false);
            }
            else
            {
                gateswitch.SetActive(true);
            }
        }
        RaycastHit hit;
        Vector3 destination;
        
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 2))
        {
            if (hit.transform.gameObject.tag == "Interact" || Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 2) && hit.transform.gameObject.tag == "GateSwitch" || Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 2) && hit.transform.gameObject.tag == "PowerSwitch")
            {
                //destination = hit.point;
                interactButton.SetActive(true);
                //Debug.Log("HIT!!");
            }
        }
        else
        {
            interactButton.SetActive(false);
        }
        if (interactButton.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            if (hit.transform.gameObject.name == "OilCollectible")
            {
                //oil hit
                hit.transform.gameObject.GetComponent<OilScript>().Used(1);
                //lamp response
                gameObject.GetComponent<LampScript>().CollectedOil(1);
            }
            //this might be put on the individual lights and breakers
            //might need to duplicate for each set of bb and switchs and lights
            /*if (hit.transform.gameObject.name == "breakerBoxClosed")
            {
                BBOpen.SetActive(true);
                BBClosed.SetActive(false);
            }
            if (hit.transform.gameObject.name == "breakerBoxOpen")
            {
                BBOpen.SetActive(false);
                BBClosed.SetActive(true);
            }
            if(hit.transform.gameObject.name == "BBSwitchOff")
            {
                BBSwitchOff.SetActive(false);
                BBSwitchOn.SetActive(true);
                GetComponent<AudioSource>().PlayOneShot(Flicker);
            }
            if(hit.transform.gameObject.name == "BBSwitchOn")
            {
                BBSwitchOn.SetActive(false);
                BBSwitchOff.SetActive(true);
               
            }*/
            if(hit.transform.gameObject.name == "SKey")
            {
                SKey = true;
                SaveScript.allData.SKey = true;
                //1
                Destroy(hit.transform.gameObject);
                Sgate.SetActive(false);
                SaveScript.allData.Sgate = false;
                //2

            }
            if(SKey == true && hit.transform.gameObject.name == "SGate")
            {
                hit.transform.gameObject.GetComponent<SGateScript>().Open(1);
                SKey = false;
            }
        }
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 2) && hit.transform.gameObject.tag == "PowerSwitch" && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("can switch gate switch");
            GetComponent<AudioSource>().PlayOneShot(Flicker);
            canswitch = true;
            flippedswitch.SetActive(true);
            SaveScript.allData.flippedswitch = true;
            //3
            powerswitch.SetActive(false);
            SaveScript.allData.powerswitch = false;
            //4
                
        }
        if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 2) && hit.transform.gameObject.tag == "GateSwitch" && Input.GetKeyDown(KeyCode.E) && canswitch == true)
        {
            Debug.Log("switch Flipped");
            GetComponent<AudioSource>().PlayOneShot(Flicker);
            gate.SetActive(false);
            SaveScript.allData.gate = false;
            //gateswitch.GetComponent<Animator>().SetBool("flipped", true);
            //5
           // flippedswitch2.SetActive(true);
            SaveScript.allData.flippedswitch2 = true;
            //6
            gateswitch.SetActive(false);
            SaveScript.allData.gateswitch = false;
            //7

        }

    }
}
