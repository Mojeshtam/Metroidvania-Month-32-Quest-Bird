using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
    [Header("References")]
    public PlayerMovementValuesController MoveVals;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    //We don't even really use this lol
    private Rigidbody2D _rb;

    //Movement Variables
    private Vector2 _moveVelocity;
    //We use this for direction calculation in movement and sprite stuff
    private bool _isFacingRight;

    //Collision checkers
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _hitHead;

    private void Awake()
    {
        //I don't know why we arent hardcoding this in the tutorial
        _isFacingRight = true;

        _rb = GetComponent<Rigidbody2D>();
    }

    //We use fixed update because physics and collisions can get weird if we are basing our updates on client side frame rates.
    //This makes it so slow and fast computers have the same physics
    private void FixedUpdate()
    {
        //We perform the check to use the right accel/decel
        CollisionChecks();

        //Call movement depending on the player's grounded state
        if(_isGrounded)
        {
            Move(MoveVals.GroundAcceleration, MoveVals.GroundDeceleration, CustomPlayerInputManager.Movement);
        }
        else
        {
            Move(MoveVals.AirAcceleration, MoveVals.AirDeceleration, CustomPlayerInputManager.Movement);
        }
    }

    #region Movement
    //This region just lets us group code to keep things neat


    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if(moveInput != Vector2.zero)
        {
            //This is the acceleration code

            TurnCheck(moveInput);

            //We will be manually lerping our movement for smooth movement
            //I would not normally do this since lerping can be a pain in the ahh
            Vector2 targetVelocity = Vector2.zero;

            //This is setting the new velocity we want to lerp to over time
            if (CustomPlayerInputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveVals.MaxRunSpeed;
            }
            else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveVals.MaxWalkSpeed;
            }

            //This translates to "adjust my velocity closer to the target by these values per time between frame"
            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocityY);

        }
        else if (moveInput == Vector2.zero)
        {
            //This is the deceleration code
            //This translates to "adjust my velocity closer to zero so we slow down by these values per time between frame"
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocityY);
        }
    }

    //We use this function to check turns
    //Making it a function reduces clutter and increases readability
    private void TurnCheck(Vector2 moveInput)
    {
        if(_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if(!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    //Turns the player
    //Using rotate here is eh but it's not terrible
    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveVals.GroundDetectionRayLength);

        //This scans beneath the players feet and returns things we collide with on the designated "ground layer"
        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveVals.GroundDetectionRayLength, MoveVals.GroundLayer);
        if(_groundHit.collider != null)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }

        #region Debug Visuals
        //This just lets us view the ray cast we make for tuning purposes
        if (MoveVals.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if(_isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveVals.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveVals.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveVals.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }

    private void CollisionChecks()
    {
        IsGrounded();
    }

    #endregion
}
