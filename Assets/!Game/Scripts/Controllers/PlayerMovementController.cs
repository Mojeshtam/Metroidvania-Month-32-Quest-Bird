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

    //Jump Variables
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallTimeReleaseSpeed;
    private int _numberOfJumpsUsed;

    //Apex Vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    //Jump Buffer Vars
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    //Coyote Time Vars
    private float _coyoteTimer;


    private void Awake()
    {
        //I don't know why we arent hardcoding this in the tutorial
        _isFacingRight = true;

        _rb = GetComponent<Rigidbody2D>();
    }

    //For variable updating ONLY
    //Physics calculations MUST be in fixed update
    private void Update()
    {
        CountTimers();
        //Checks are not physics based so we will do them in update
        JumpChecks();
    }

    //We use fixed update because physics and collisions can get weird if we are basing our updates on client side frame rates.
    //This makes it so slow and fast computers have the same physics
    private void FixedUpdate()
    {
        //We perform the check to use the right accel/decel
        CollisionChecks();
        Jump();

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

    #region Jump

    private void JumpChecks()
    {
        //BUGS HERE
        //What gets changed when jump button is pressed
        if(CustomPlayerInputManager.JumpWasPressed)
        {
            _jumpBufferTimer = MoveVals.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        //What gets changed when jump button is released
        if (CustomPlayerInputManager.JumpWasReleased)
        {
            if(_jumpBufferTimer > 0)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveVals.TimeForJumpCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallTimeReleaseSpeed = VerticalVelocity;
                }
            }
        }

        //Initiate the jump
        //First jump on ground or with coyote time
        if (_jumpBufferTimer > 0 && !_isJumping && (_isGrounded || _coyoteTimer > 0))
        {
            InitiateJump(1);

            //Bunny hop behavior
            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallTimeReleaseSpeed = VerticalVelocity;
            }
        }

        //Double Jump Behavior
        //With Coyote time
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveVals.JumpAmount)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        //Without Coyote time
        //This just means we cant get 2 jumps after jumping off a ledge when we don't have coyote time
        else if(_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveVals.JumpAmount - 1)
        {
            _isFastFalling = false;
            InitiateJump(2);
        }

        //Landing behavior
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;

            VerticalVelocity = Physics2D.gravity.y;
            //VerticalVelocity = 0f;
        }
    }

    private void InitiateJump(int numJumpsUsed)
    {
        if(!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numJumpsUsed;
        VerticalVelocity = MoveVals.InitialJumpVelocity;
    }

    private void Jump()
    {
        //Apply jump gravity
        if(_isJumping)
        {
            //Bonk
            if(_hitHead)
            {
                //Interupt the jump when we bonk
                _isFastFalling = true;
            }

            //Gravity on ascending portion of jump
            if(VerticalVelocity >= 0f)
            {
                //This part just lets us control and sustain the peak of the jump

                //Find where our apex point is 
                _apexPoint = Mathf.InverseLerp(MoveVals.InitialJumpVelocity, 0f, VerticalVelocity);

                //Check that we are greater than our threshold
                if(_apexPoint > MoveVals.ApexThreshold)
                {
                    if(!_isPastApexThreshold)
                    {
                        _isPastApexThreshold=true;
                        _timePastApexThreshold = 0f;
                    }

                    if(_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if(_timePastApexThreshold < MoveVals.ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }
                //Gravity on ascending portion of jump but we arent past the apex
                else
                {
                    VerticalVelocity += MoveVals.Gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }
            //Gravity on descending portion of jump
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveVals.Gravity * MoveVals.PlayerGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }

            else if (VerticalVelocity < 0f)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
            }
        }
        
        //Jump Cut Logic
        if(_isFastFalling)
        {
            if(_fastFallTime >= MoveVals.TimeForJumpCancel)
            {
                VerticalVelocity += MoveVals.Gravity * MoveVals.PlayerGravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if(_fastFallTime < MoveVals.TimeForJumpCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallTimeReleaseSpeed, 0f, (_fastFallTime / MoveVals.TimeForJumpCancel));
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        //Normal Falling Gravity
        if (!_isGrounded && !_isJumping)
        {
            if(!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += MoveVals.Gravity * Time.fixedDeltaTime;
        }

        //Clamp Fall Speed when terminal velocity is reached and max ascension speed
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveVals.MaxFallSpeed, 50f);
        _rb.linearVelocity = new Vector2(_rb.linearVelocityX, VerticalVelocity);
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        //Consider changing to fixed delta time maybe?
        _jumpBufferTimer -= Time.deltaTime;

        if(!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = MoveVals.JumpCoyoteTime;
        }
    }

    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveVals.GroundDetectionRayLength);

        //This scans beneath the players feet and returns things we collide with on the designated "ground layer"
        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveVals.GroundDetectionRayLength, LayerMask.GetMask("Ground"));
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

    private void HitHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveVals.HeadWidth, MoveVals.HeadDetectionRayLength);

        //This scans above the players head and returns things we collide with on the designated "ground layer"
        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveVals.HeadDetectionRayLength, MoveVals.GroundLayer);
        if (_headHit.collider != null)
        {
            _hitHead = true;
        }
        else
        {
            _hitHead = false;
        }

        #region Debug Visuals
        //This just lets us view the ray cast we make for tuning purposes
        if (MoveVals.DebugShowHeadHitBox)
        {
            float headWidth = MoveVals.HeadWidth;

            Color rayColor;
            if (_hitHead)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveVals.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * MoveVals.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveVals.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);
        }
        #endregion
    }

    private void CollisionChecks()
    {
        IsGrounded();
        HitHead();
    }

    #endregion
}
