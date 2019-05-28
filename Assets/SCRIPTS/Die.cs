using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.AI;

public class Die : MonoBehaviour
{
    public GameObject Jumpscare;
    public GameObject JumpscareImage;
    public bool Jumpscareactive = false;
    public AudioClip jump;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Jumpscareactive)
        {
            if (JumpscareImage.transform.localScale.magnitude < 8)
            {
                JumpscareImage.transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
            } 
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Enemy")
        {
            GetComponent<AudioSource>().PlayOneShot(jump);
            gameObject.GetComponent<RigidbodyFirstPersonController>().movementSettings.ForwardSpeed = 0;
            gameObject.GetComponent<RigidbodyFirstPersonController>().movementSettings.BackwardSpeed = 0;
            gameObject.GetComponent<RigidbodyFirstPersonController>().movementSettings.StrafeSpeed = 0;
            gameObject.GetComponent<RigidbodyFirstPersonController>().mouseLook.XSensitivity = 0;
            gameObject.GetComponent<RigidbodyFirstPersonController>().mouseLook.YSensitivity = 0;
            collision.gameObject.transform.parent.GetComponent<NavMeshAgent>().enabled = false;
            collision.gameObject.transform.parent.GetComponent<NavMoveLight>().enabled = false;
            collision.gameObject.transform.parent.GetComponent<Animator>().SetBool("Idle", true);
            Jumpscare.GetComponent<Canvas>().enabled = true;
            Jumpscareactive = true;
            Camera.main.transform.LookAt(collision.gameObject.transform.position);
            StartCoroutine(jumpscare());
          //  SceneManager.LoadScene("LoseScene");
        }
    }
    IEnumerator jumpscare()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("LoseScene");

    }

}
