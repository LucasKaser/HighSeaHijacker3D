using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileShooting : MonoBehaviour
{
    Camera camera;
    public GameObject prefab;
    public float bulletSpeed;
    public float rayLength = 50.0f;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            RaycastHit hit;
            Vector3 destination;
            if(Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, 50))
            {
                destination = hit.point;
            }
            else
            {
                destination = camera.transform.position + rayLength * camera.transform.forward;
            }
            Vector3 direction = destination - transform.position;
            direction.Normalize();
            GameObject projectile = Instantiate(prefab, transform.position, Quaternion.identity);
            projectile.GetComponent<Rigidbody>().velocity = direction * bulletSpeed;
            Destroy(projectile, 5);
        }
    }
}
