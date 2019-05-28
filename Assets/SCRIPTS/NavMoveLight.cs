using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMoveLight : MonoBehaviour
{
    NavMeshAgent Agent;
    public GameObject Player;
    public float chaseDistance = 10.0f;
    private Vector3 Home;
    public float timer = 5.0f;
    public AudioClip Danger;
    public float soundtime = 0.0f;
    public GameObject vignette;
    public bool playerchase = false;
   // public Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        Home = transform.position;
        Agent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    Vector3 homedistance;
    void Update()
    {
        soundtime += Time.deltaTime;
        homedistance = Home - transform.position;
        if (homedistance.magnitude <= 1)
        {
            GetComponent<Animator>().SetBool("Idle", true);
        }
        if (homedistance.magnitude >= 1)
        {
            GetComponent<Animator>().SetBool("Idle", false);
        }
        RaycastHit hit;
        /*if (Physics.Raycast(gameObject.transform.position, gameObject.transform.forward, out hit))
        {
            Debug.Log(hit.transform.gameObject.tag);
            if (hit.transform.gameObject.tag == "Light")
            {
                Agent.destination = Home;
            }
        }*/

         Vector3 direction = Player.transform.position - transform.position;

        //GetComponent<LineRenderer>().SetPosition(0, transform.position);
        if (direction.magnitude <= chaseDistance && Physics.Raycast(gameObject.transform.position, direction, out hit))
        {
            //Debug.Log(hit.transform.gameObject.tag);
            //checks to see if the player is obstructed by a light
            //GetComponent<LineRenderer>().SetPosition(1, hit.point);
            if (hit.transform.gameObject.tag == "Light")
            {
                //if so then monster returns home
                //vignette.GetComponent<Canvas>().enabled = (false);
                if (vignette.transform.localScale.magnitude < 19 && Player.GetComponent<GettingChased>().vignetteing == false)
                {
                    vignette.transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
                }
                Agent.destination = Home;
                playerchase = false;
                
                //timer allows time for monster to return home before beginning to follow the player if in range
                timer = 0.0f;
                timer += Time.deltaTime;
                soundtime = 30000000000.0f;
            }
            else if (hit.transform.gameObject.tag == "Player")
            {
                playerchase = true;
                //vignette.GetComponent<Canvas>().enabled = (true);
                if (vignette.transform.localScale.magnitude > 6 && Player.GetComponent<GettingChased>().vignetteing == true)
                {
                    vignette.transform.localScale += new Vector3(-0.1f, -0.1f, -0.1f);
                }
                Agent.destination = Player.transform.position;

                if (soundtime >= 30000000000.0f)
                {
                    Player.GetComponent<AudioSource>().PlayOneShot(Danger);
                    soundtime = 0;
                }
            }
        }
        /*else if (!(direction.magnitude <= chaseDistance && Physics.Raycast(gameObject.transform.position, gameObject.transform.forward, out hit)) && direction.magnitude <= chaseDistance)
        {
            //if there are no objects in view, and the player is in range, chase the player
            Agent.destination = Player.transform.position;
            Debug.Log("ooga");
        }*/



        //Debug.Log(Agent.destination);
        
    }
}
