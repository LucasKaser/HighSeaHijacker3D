using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMove : MonoBehaviour
{
    NavMeshAgent Agent;
    public GameObject Player;
    public float chaseDistance = 10.0f;
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
        if (direction.magnitude <= chaseDistance)
        {
            Agent.destination = Player.transform.position;
        }
        else
        {
            Agent.destination = Home;
        }
    }
}
