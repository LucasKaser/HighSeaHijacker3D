using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGame : MonoBehaviour
{
    public GameObject Canvas;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y) && Canvas.activeSelf)
        {
            SceneManager.LoadScene("Level1");
        }
        if (Input.GetKeyDown(KeyCode.N) && Canvas.activeSelf)
        {
            Application.Quit();
        }
    }
    public void Exit()
    {
        Application.Quit();
    }
    public void Retry()
    {
        SceneManager.LoadScene("Level1");
    }
}
