using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
public class LampScript : MonoBehaviour
{

    /*check list
     lamp on = bool on (turn on by left click)
     timer start when lamp on
     when timer at x sec lamp oil -= 1
     reclick left click then off*/
    public int OilAmnt = 2;
    public float LightTimer = 0;
    public float SpendTime = 3f;
    public bool LampOn = false;
    public GameObject lightCylinder;
    public int VisibleOil = 0;
    public Text OilText;
    public AudioClip click;
    // Start is called before the first frame update
    void Start()
    {
        OilAmnt = SaveScript.allData.oilAmnt;
        VisibleOil = OilAmnt - 1;
        OilText.GetComponent<Text>().text = "Oil left: " + VisibleOil;
        //Debug.Log(SaveScript.allData.oilAmnt);
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerPrefs.GetInt("Loading") == 1)
        {
            OilAmnt = SaveScript.allData.oilAmnt;
            Debug.Log(OilAmnt);
            OilText.GetComponent<Text>().text = "Oil left: " + VisibleOil;
        }
        SaveScript.allData.oilAmnt = OilAmnt;
        VisibleOil = OilAmnt ;
        OilText.GetComponent<Text>().text = "Oil left: " + VisibleOil;
        /*if (VisibleOil <= 0)
        {
            VisibleOil = 0;
        }*/
        //turn lamp on
        if (Input.GetKeyDown(KeyCode.Mouse0) && LampOn == false && OilAmnt >= 1)
        {
            GetComponent<AudioSource>().PlayOneShot(click);
            LampOn = true;
            OilAmnt -= 1;
            SaveScript.allData.oilAmnt = OilAmnt;
            
        }
        //turn lamp off
        else if (Input.GetKeyDown(KeyCode.Mouse0) && LampOn == true)
        {
            GetComponent<AudioSource>().PlayOneShot(click);
            LampOn = false;
        }
        //stop timer and turn off light
        if (LampOn == false)
        {
            LightTimer = 0;
            lightCylinder.SetActive(false);
            lightCylinder.GetComponent<NavMeshObstacle>().radius = 0.05f;
        }
        //start timer and turn light 
        if (LampOn == true)
        {
            LightTimer += Time.deltaTime;
            lightCylinder.SetActive(true);
            if (LightTimer >= SpendTime)
            {
                //Debug.Log("OverTime");
                LightTimer = 0f;
                OilAmnt -= 1;
            }
            if (OilAmnt < 0)
            {
                LampOn = false;
            }
            if (lightCylinder.GetComponent<NavMeshObstacle>().radius <= .5)
            {
                lightCylinder.GetComponent<NavMeshObstacle>().radius += Time.deltaTime;
            }
             
        }
        if (OilAmnt < 0)
        {
            LampOn = false;
        }
        if (OilAmnt == -1)
        {
            OilAmnt = 0;
        }
        
    }
    public void CollectedOil(int amnt)
    {
        OilAmnt += amnt;
    }
}
