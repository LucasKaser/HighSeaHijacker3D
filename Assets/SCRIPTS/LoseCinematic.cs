using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseCinematic : MonoBehaviour
{
    public float timeTillFade;
    public GameObject loseCanvas;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeTillFade += Time.deltaTime;
        if (timeTillFade >= 3)
        {
            GetComponent<Light>().intensity -= 0.01f;
        }
        if (timeTillFade >= 6.5)
        {
            loseCanvas.SetActive(true);
        }
    }
}
