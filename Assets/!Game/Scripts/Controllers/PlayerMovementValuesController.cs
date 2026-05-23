using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementValuesController : ScriptableObject
{
    //These values here are values we can dynamically update to alter the feel of player movement
    //This builds an accessible template of values we can use for player movement
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
    public float HeadDetectionRayLength = 0.02f;
    //Head size for collisions
    [Range(0f, 1f)] public float HeadWidth = 0.75f;



    //This section is for jump logic and variables
    [Header("Jump")]
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeUntilJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float PlayerGravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 5)] public int JumpAmount = 2;

    //How long you have to control the jump height
    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeForJumpCancel = 0.027f;

    //How long the peak of the jump is sustained
    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;

    //How long you have to press the jump button to perform a frame perfect back to back jump
    //Basically holding down the jump key
    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;

    //How much time you still have to jump after walking off a ledge
    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;


    //This section is to show debug visualizations and collision test information for developers
    [Header("Debug")]
    public bool DebugShowIsGroundedBox = false;
    public bool DebugShowHeadHitBox = false;

    [Header("Jump Debug")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    //Gravity mechanics based on GDC's Math for Game Programmers: Building a Better Jump
    public float Gravity { get; private set; }

    public float InitialJumpVelocity { get; private set; }

    public float AdjustedJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        //Ordinarily we can fix physics based Jump Height accurcacy by increasing the physics time step (How many times we calculate physics per second)
        //But that is an expensive change and we can do just fine by manually compensating for inaccuracies
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        //We can derive the equation for gravity g = -2h/t sub h (h is half of the period) by using derivates and integration
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(TimeUntilJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeUntilJumpApex;
    }

}
