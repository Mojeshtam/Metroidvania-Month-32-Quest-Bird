using System;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float cameraSize;
    //X is left to right, Y is bottom to top
    [SerializeField] UnityEngine.Vector2 sceneSize;
    //how high/low the player can move before the camera starts moving
    [SerializeField] float heightBuffer;
    [SerializeField] float panSpeed;
    private GameObject player;
    private float width;
    private float height;
    private float aspect;
    private float posx;
    private float posy;
    private float playerx, playery;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        GetComponent<Camera>().orthographicSize = cameraSize;
        aspect = GetComponent<Camera>().aspect;
        height = cameraSize;
        width = height * aspect;
        //offset the z axis so it can see
        transform.position = new UnityEngine.Vector3(player.transform.position.x, player.transform.position.y, -10f);
        
    }

    // Update is called once per frame
    void Update()
    {
        playerx = player.transform.position.x;
        playery = player.transform.position.y;
        Debug.Log(transform.position.x - width);
        //x vals
        if((playerx - width) <= 0.001 || (playerx + width) >= sceneSize.x)
        {
            //if on a left or right edge, set to left or right bound
            posx = (playerx - width) <= 0.001 ? width : sceneSize.x - width;
        }
        else
        {
            //ifanywhere else, follow the player
            posx = playerx;
        }
        //y vals
        if((playery - height) <= 0.001 || (playery + height) >= sceneSize.y)
        {
            //if on upper or lower edge, set to upper or lower bound
            posy = (playery - height) <= 0.001 ? height : sceneSize.y - height;
        }
        else
        {
            //
            posy = Math.Abs(transform.position.y - playery) <= heightBuffer ? transform.position.y : playery;
        }
        UnityEngine.Vector3 targetPosition = new UnityEngine.Vector3(posx, posy, -10f);

        transform.position = UnityEngine.Vector3.Lerp(transform.position, targetPosition, panSpeed * Time.deltaTime);
    }
}
