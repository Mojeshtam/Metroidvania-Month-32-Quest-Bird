using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float cameraSize;
    //X is left to right, Y is bottom to top
    [SerializeField] UnityEngine.Vector2 sceneSize;
    private GameObject player;
    private float width;
    private float height;
    private float aspect;
    private float posx;
    private float posy;
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
        Debug.Log(transform.position.x - width);
        //check if it is on an edge, else make it follow the player
        if((player.transform.position.x - width) <= 0.001 || (player.transform.position.x + width) >= sceneSize.x)
        {
            posx = (player.transform.position.x - width) <= 0.001 ? width : sceneSize.x - width;
        }
        else
        {
            posx = player.transform.position.x;
        }
        if((player.transform.position.y - height) <= 0.001 || (player.transform.position.y + height) >= sceneSize.y)
        {
            posy = (player.transform.position.y - height) <= 0.001 ? height : sceneSize.y - height;
        }
        else
        {
            posy = player.transform.position.y;
        }
        transform.position = new UnityEngine.Vector3(posx, posy, -10f);
    }
}
