using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class DamnMovement : MonoBehaviour
{
    // [SerializeField] float damnSpeed;
    // [SerializeField] float distanceBuffer;
    private Rigidbody2D rb;
    private GameObject player;
    private float dir;
    private float xdist;
    private float ydist;
    private float lindist;
    // private float lastpos;
    private bool canJump;
    private bool isGrounded;
    [SerializeField] float speedY;
    [SerializeField] float speedX;
    [SerializeField] float hopCooldown;
    [SerializeField] int groundLayer;
    [SerializeField] float aggroProximity;

        //this is when I had them runnign around, going to switch to a hopping motion instead
    // void Start()
    // {
    //     rb = GetComponent<Rigidbody2D>();
    //     player = GameObject.FindGameObjectWithTag("Player");
    //     damnSpeed *=  UnityEngine.Random.Range(0.5f, 1.5f);
    //     lastpos = transform.position.x;
    // }

    // // Update is called once per frame
    // void FixedUpdate()
    // {
    //     xdist = player.transform.position.x - transform.position.x;
    //     if(Math.Abs(xdist) > distanceBuffer || Math.Abs(lastpos - transform.position.x) < 0.000001)
    //     {
    //         dir = Math.Abs(xdist)/xdist;
    //         Debug.Log(dir);
            
    //     }

    //     Debug.Log(lastpos - transform.position.x);
        
    //     rb.linearVelocityX = dir * damnSpeed;
    //     lastpos = transform.position.x;
        
    // }

    //hopping motion
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = player = GameObject.FindGameObjectWithTag("Player");
        canJump = true;
        isGrounded = true;
    }

    void Update()
    {
        xdist = player.transform.position.x - transform.position.x;
        ydist = player.transform.position.y - transform.position.y;
        lindist = (float) Math.Sqrt((xdist * xdist) + (ydist * ydist));
        if(canJump && isGrounded)
        {
            isGrounded = false;
            canJump = false;
            //if inside aggro range, jump toward player, else choose random x direction
            dir = lindist < aggroProximity ? Math.Abs(xdist)/xdist : (UnityEngine.Random.Range(0,2) == 0 ? -1 : 1);

            StartCoroutine(HopTimer());
            

        }
    }

    private IEnumerator HopTimer()
    {
        
        Hop(dir * UnityEngine.Random.Range(0.75f, 1.25f) * speedX, UnityEngine.Random.Range(0.75f, 1.25f) * speedY);
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.75f, 1.25f) * hopCooldown);
        canJump = true;
        
    }
    private void Hop(float velX, float velY)
    {
        rb.linearVelocity = new Vector2(velX, velY);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //gorund 
        if(collision.gameObject.layer == groundLayer)
        {
            isGrounded = true;
            rb.linearVelocityX = 0;
        }
    }
}
