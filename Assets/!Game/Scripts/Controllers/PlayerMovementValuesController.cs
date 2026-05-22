using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementValuesController : ScriptableObject
{
    //These values here are values we can dynamically update to alter the feel of player movement
    //Note that some of the functionality here required modifying other scripts
    //Make sure to add functionality before adding new values here
    [Header("Walk")]
    [Range(1f, 100f)] public float MaxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;

    [Header("Run")]
    [Range(1f, 100f)] public float MaxRunSpeed = 20f;

    //This section is tailored towards raycasting behavior
    //This just means that you can control the value of how sensitive the collision detection is
    [Header("Grounded/Collision Checks")]
    //This just tells the script what objects are ground objects
    public LayerMask GroundLayer;
    //This determines the size of the ray that shoots out directly beneath the player feet to detect collisions
    public float GroundDetectionRayLength = 0.02f;
    //This is the same thing as the previous ray but for crouching behaviors
    public float HeadTetectionRayLength = 0.02f;
    //Head size for collisions
    [Range(0f, 1f)] public float HeadWidth = 0.75f;

    [Header("Debug")]
    public bool DebugShowIsGroundedBox = false;


}
