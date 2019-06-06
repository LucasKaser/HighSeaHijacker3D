using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMove : MonoBehaviour
{
    NavMeshAgent Agent;
    public GameObject Player;
    public float chaseDistance = 10.0f;
    public float attackDistance = 2.0f;
    public float attackTimer = 0.0f;
    private Vector3 Home;
    // Start is called before the first frame update
    void Start()
    {
        Home = transform.position;
        Agent = GetComponent<NavMeshAgent>();

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = Player.transform.position - transform.position;
        if (direction.magnitude <= chaseDistance && direction.magnitude >= attackDistance)
        {
            Agent.destination = Player.transform.position;
        }
        else if (direction.magnitude <= attackDistance)
        {
            //fire or swing
            //direction = transform.position;
            Agent.destination = transform.position;
            attackTimer += Time.deltaTime;
        }
        else 
        {
            Agent.destination = Home;
        }
        if (attackTimer >= 1.5f)
        {
            Player.GetComponent<Health>().HP -= 1;
            attackTimer = 0f;
        }
    }
}
