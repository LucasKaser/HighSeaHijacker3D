using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkSound : MonoBehaviour
{
    public AudioClip Footsteps;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && !Camera.main.GetComponent<AudioSource>().isPlaying)
        {
            //Debug.Log("foo");
            Camera.main.GetComponent<AudioSource>().PlayOneShot(Footsteps);
        }else if(!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
        {
            //Debug.Log("bar");
            Camera.main.GetComponent<AudioSource>().Stop();
        }
    }
}
