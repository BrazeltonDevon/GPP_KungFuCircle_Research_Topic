using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class EnemySubject : MonoBehaviour
{
#region _Movement
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;


    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    // char
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    //private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

   // private const float _threshold = 0.01f;

    private bool _hasAnimator;

    [SerializeField]
    private float shouldSprintDistance = 6f;

    private Animator _animator;
    private CharacterController _controller;


   //private Vector2 _move;
   //private Vector2 _look;


    private Vector3 _moveDirection;
    private bool shouldSprint = false;

    #endregion

    public int gridWeight = 1;
    public int health = 3;
    private EnemyManager _enemyManager;

    [SerializeField]
    private GameObject _player;
    private float distToTarget;

    public bool IsTargetPlayer = true;
    public bool IsTargetSlot = false;

    private FightingCircle _fightingCircle;
    private CharacterController characterController;

    [Header("States")]
    [SerializeField] private bool isPreparingAttack;
    [SerializeField] private bool isMoving = true;
    [SerializeField] private bool isRetreating;
    public bool isWaiting = false;
    public bool isInRange = false;
  

    private Coroutine Enemy_Movement;
    private Coroutine PrepareAttackCoroutine;
    private Coroutine RetreatCoroutine;

    private Vector3 moveDirection;
    [SerializeField]
    private AudioClip punchSound;
    [SerializeField]
    private ParticleSystem attackParticles;

    public bool IsRegisteredInFC = false;
    public Vector3 targetVector;

    //Events
    public UnityEvent<EnemySubject> OnDamage;
    public UnityEvent<EnemySubject> OnStopMoving;
    public UnityEvent<EnemySubject> OnRetreat;

    // Start is called before the first frame update
    void Start()
    {
        _enemyManager = GetComponentInParent<EnemyManager>();
        characterController = GetComponent<CharacterController>();

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();

        AssignAnimationIDs();
        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        // Find player GameObject

        if (!_player)
            _player = GameObject.FindGameObjectWithTag("Player");

        // get the fighting circle
        _fightingCircle = _player.GetComponent<FightingCircle>();

        var approach = _fightingCircle.GetApproachCircle();
        shouldSprintDistance = approach.radius + approach.distanceFromPlayer;

        Enemy_Movement = StartCoroutine(EnemyMovement());
    }

    // Update is called once per frame
    void Update()
    {
        if(IsTargetPlayer)
        {
            targetVector = _player.transform.position;
            IsTargetSlot = false;
        }
        if(IsTargetSlot)
        {
            targetVector = _fightingCircle.GetSlotPositionFromApproachCircle(this);
            IsTargetPlayer = false;
        }

        if(!IsTargetPlayer && !IsTargetSlot)
        {
            // stay outside of approach range
            Vector3 dir = (_player.transform.position - transform.position);
            float dirMag = dir.magnitude - (_fightingCircle.GetApproachCircle().distanceFromPlayer + _fightingCircle.GetApproachCircle().radius + 4f);

            targetVector = transform.position + dir.normalized * dirMag;
            
            //DebugDrawing.DrawCircle(targetVector, 0.5f, Color.magenta, 1f);

            // get direction to player, normalize it then times that dir by the magnitude
            // of the distance to the player - approach range

            if (Vector3.Distance(transform.position, targetVector) <= 2f)
            {
                isWaiting = true;
            }
            else
            {
                //Debug.Log("no longer waiting");
                isWaiting = false;
                isMoving = true;
                moveDirection = Vector3.forward;
            }
                
        }

        GravityFall();
        GroundedCheck();

        // if player if out of range, sprint towards them
        Vector3 dist = targetVector - transform.position;
        distToTarget = dist.magnitude;

        if (distToTarget > shouldSprintDistance || (!isInRange && IsRegisteredInFC) )
        {
            shouldSprint = true;
        }
        else
        {
            shouldSprint = false;
        }

        transform.LookAt(new Vector3(_player.transform.position.x, transform.position.y, _player.transform.position.z));
        MoveEnemy();
        
        // always look at player


    
    }
    IEnumerator EnemyMovement()
    {
        // waits until the enemy is not assigned to any action
        // and
        // waits until enemy is registered in fighting circle
        yield return new WaitUntil(() => isWaiting == true); //&& IsRegisteredInFC

        // random chance to move
        int randomChance = Random.Range(0, 2);

        if (randomChance == 1)
        {
            // random chance to move left or right
            int randomDir = Random.Range(0, 2);
            moveDirection = randomDir == 1 ? Vector3.right : Vector3.left;
            isMoving = true;
        }
        else
        {
            StopMoving();
        }

        yield return new WaitForSeconds(1);

        Enemy_Movement = StartCoroutine(EnemyMovement());
    }

    public void TakeDamage()
    {
        health--;

        if (health <= 0)
        {
            Death();
            return;
        }
    }

    void MoveEnemy()
    {
        // if not set to move, don't
        if (!isMoving) return;

        // accelerate or decelerate to target speed

        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = shouldSprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;


        // distance to player
        float inputMagnitude = 1f;

        // SET TARGET DIRECTION

        // move character
        Vector3 dir = (targetVector - transform.position).normalized;
        Vector3 pDir = Quaternion.AngleAxis(90, Vector3.up) * dir; //Vector perpendicular to direction
        Vector3 finalDirection = Vector3.zero;

        if (moveDirection == Vector3.forward || moveDirection == Vector3.zero)
            finalDirection = dir;
        if (moveDirection == Vector3.right || moveDirection == Vector3.left)
        {
            finalDirection = (pDir * moveDirection.normalized.x);
        }
        if (moveDirection == -Vector3.forward)
        {
            finalDirection = -transform.forward;
        }


        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // set animation speed based on target movement speed
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;




        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving

        _targetRotation = Mathf.Atan2(finalDirection.x, finalDirection.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
           // transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

       // _controller.Move(movedir);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }

        if (Vector3.Distance(transform.position, _player.transform.position) < 1.2f)
        {
           // StopMoving();
           if(isPreparingAttack)
            {
                Attack();
                PrepareAttack(false);
            }
 
        }


    }

    public void Death()
    {
        StopEnemyCoroutines();

        _fightingCircle.Unregister(this);

        this.enabled = false;
        characterController.enabled = false;
        _enemyManager.SetEnemyAvailiability(this, false);
        _enemyManager.RemoveEnemy(this);
        Destroy(gameObject, 1f);
    }

    public void OutOfRangeStopWaiting()
    {
        StopEnemyCoroutines();

        isRetreating = false;
        isWaiting = false;
        moveDirection = Vector3.forward;
        isMoving = true;

        IsTargetPlayer = false;
        IsTargetSlot = true;
        //targetVector = _fightingCircle.GetSlotPositionFromApproachCircle(this);

        Enemy_Movement = StartCoroutine(EnemyMovement());
    }

    void PrepareAttack(bool active)
    {
        isPreparingAttack = active;

        if(active)
        {
            attackParticles.Play();
        }
        else
        {
            attackParticles.Clear();
            attackParticles.Stop();
            StopMoving();
        }
    }

    IEnumerator PrepRetreat()
    {
        Debug.Log("PrepRetreat started");
        yield return new WaitForSeconds(1.4f);
        OnRetreat.Invoke(this);
        isRetreating = true;
        moveDirection = -Vector3.forward;

        IsTargetSlot = true;
        IsTargetPlayer = false;
        targetVector = _fightingCircle.GetSlotPositionFromApproachCircle(this);

        isMoving = true;
        Debug.Log("In PrepRetreat, waiting until character is back to slot pos");

       // yield return new WaitUntil(() => Vector3.Distance(transform.position, targetVector) > 2f);

        isRetreating = false;
       // Debug.Log("Enemy is no longer retreating");

        StopMoving();



        //Free 
        isWaiting = true;
        Enemy_Movement = StartCoroutine(EnemyMovement());
    }

    public void SetRetreat()
    {
        StopEnemyCoroutines();

        RetreatCoroutine = StartCoroutine(PrepRetreat());
    }

    public void SetAttack()
    {
        isWaiting = false;
        Debug.Log("Set Attack Active!");

        PrepareAttackCoroutine = StartCoroutine(PrepAttack());

        IEnumerator PrepAttack()
        {
            PrepareAttack(true);
            yield return new WaitForSeconds(.2f);
            moveDirection = Vector3.forward;

            Debug.Log("Preparing attack, target is now player");
            isMoving = true;

            IsTargetPlayer = true;
            IsTargetSlot = false;
            //targetVector = _player.transform.position;
        }
    }

    private void Attack()
    {
      // transform.DOMove(_player.transform.position, .3f);

        AudioSource.PlayClipAtPoint(punchSound, transform.position);
    }

    public void StopMoving()
    {
        isMoving = false;
        moveDirection = Vector3.zero;
       if (characterController.enabled)
           characterController.Move(moveDirection);
    }

    public void StopEnemyCoroutines()
    {
        PrepareAttack(false);

        if (isRetreating)
        {
            if (RetreatCoroutine != null)
                StopCoroutine(RetreatCoroutine);
        }

        if (PrepareAttackCoroutine != null)
            StopCoroutine(PrepareAttackCoroutine);

        if (Enemy_Movement != null)
            StopCoroutine(Enemy_Movement);

    }
    private void GravityFall()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }
        }

    }


    public int GetGridWeight()
    {
        return gridWeight;
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_animator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }

    #region Public Booleans

    public bool IsAttackable()
    {
        return health > 0;
    }

    public bool IsPreparingAttack()
    {
        return isPreparingAttack;
    }

    public bool IsRetreating()
    {
        return isRetreating;
    }

    #endregion

}
