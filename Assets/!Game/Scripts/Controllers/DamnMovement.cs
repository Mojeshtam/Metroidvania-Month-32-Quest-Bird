using System;
using Unity.VisualScripting;
using UnityEngine;

public class DamnMovement : MonoBehaviour
{
    [SerializeField] float damnSpeed;
    [SerializeField] float distanceBuffer;
    private Rigidbody2D rb;
    private GameObject player;
    private float dir;
    private float xdist;
    private float lastpos;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        damnSpeed *=  UnityEngine.Random.Range(0.5f, 1.5f);
        lastpos = transform.position.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        xdist = player.transform.position.x - transform.position.x;
        if(Math.Abs(xdist) > distanceBuffer || Math.Abs(lastpos - transform.position.x) < 0.000001)
        {
            dir = Math.Abs(xdist)/xdist;
            Debug.Log(dir);
            
        }

        Debug.Log(lastpos - transform.position.x);
        
        rb.linearVelocityX = dir * damnSpeed;
        lastpos = transform.position.x;
        
    }
}
