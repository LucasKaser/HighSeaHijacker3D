using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonstorAnim : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public GameObject capsule;
    // Update is called once per frame
    void Update()
    {
        if (capsule.GetComponent<Rigidbody>().velocity.x <=5 && capsule.GetComponent<Rigidbody>().velocity.z <= 5)
        {
            GetComponent<Animator>().SetBool("Idle", true);
        }
        else if (capsule.GetComponent<Rigidbody>().velocity.x >=5 && capsule.GetComponent<Rigidbody>().velocity.z >= 5)
        {
            GetComponent<Animator>().SetBool("Idle", false);
        }
    }
}
