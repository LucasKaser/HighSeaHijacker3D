using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinCinematic : MonoBehaviour
{
    public float LightTime = 0f;
    public GameObject WinCanvas;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LightTime += Time.deltaTime;
        Vector3 rot = GetComponent<Transform>().rotation.eulerAngles;
        rot.x += 0.01f;
        GetComponent<Transform>().rotation = Quaternion.Euler(rot);
        if(LightTime >= 25f)
        {
            WinCanvas.SetActive(true);
        }
    }
}
