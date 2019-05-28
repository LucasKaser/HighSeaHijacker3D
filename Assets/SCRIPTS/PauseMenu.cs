using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour {
    public float Tm = 1.0f;
    public GameObject oilbroke;
    // Use this for initialization
    void Start() {
        Time.timeScale = Tm;
    }

    // Update is called once per frame
    void Update() {
        
            if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 0)
                {
                Resume();
                
            }
            else 
            {
                //Show Menu Buttons, Pause game
                GetComponent<Canvas>().enabled = true;
                Time.timeScale = 0;
                oilbroke.SetActive(false);


            }

        }
    }
    public void Resume()
    {
        GetComponent<Canvas>().enabled = false;
        Time.timeScale = Tm;
        oilbroke.SetActive(true);

    }
    public void quit()
    {
        Application.Quit();

    }
}
