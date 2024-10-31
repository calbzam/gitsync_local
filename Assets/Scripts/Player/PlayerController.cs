using System;
using UnityEngine;
using UnityEngine.InputSystem;


// PlayerController.cs and ScriptableStats.cs EDITED from TarodevController on GitHub
// github: https://github.com/Matthew-J-Spencer/Ultimate-2D-Controller/tree/main
// license: https://github.com/Matthew-J-Spencer/Ultimate-2D-Controller/blob/main/LICENSE
namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
    /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        private InputControls input;
        private bool JumpHolding, JumpTriggered, JumpTriggeredPrev;

        private Transform playerTransform;

        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time = 1f; // 1f > 0 + 0.1:  prevent character from jumping without input at scene start

        private void Awake()
        {
            input = new InputControls();
            JumpTriggered = JumpTriggeredPrev = false;

            playerTransform = gameObject.GetComponent<Transform>();
            _rb = gameObject.GetComponent<Rigidbody2D>();
            _col = gameObject.GetComponent<CapsuleCollider2D>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void OnEnable()
        {
            input.Enable();
        }

        private void Start()
        {
            Physics2D.IgnoreLayerCollision(0, 3); // default layer <--> player layer
        }

        private void OnDisable()
        {
            input.Disable();
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();

            CheckDead();
        }

        private void GatherInput()
        {
            JumpHolding = input.Player.Jump.IsPressed();
            JumpTriggered = !JumpTriggeredPrev && JumpHolding;
            JumpTriggeredPrev = JumpHolding;

            _frameInput = new FrameInput
            {
                JumpDown = JumpTriggered,
                JumpHeld = JumpHolding,
                Move = input.Player.Movement.ReadValue<Vector2>()
            };

            // unneeded as arrow keys are automatically snapped
            //if (_stats.SnapInput)
            //{
            //    _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
            //    _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            //}

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();

            ApplyMovement();
        }

        #region Collisions

        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        //_col.bounds.center: (x=0.00, y=2.30, z=0.00)
        //_col.size: (x=0.50, y=1.26)
        //_col.direction: Vertical

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;


            // Ground and Ceiling
            //bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            //bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _stats.GroundCheckCapsuleSize, _col.direction, 0, Vector2.down, _stats.GrounderDistance, _stats.GroundLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _stats.GroundCheckCapsuleSize, _col.direction, 0, Vector2.up, _stats.GrounderDistance, _stats.GroundLayer);

            // Hit a Ceiling: cancel jumping from there
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        private bool _jumpToConsume = false;
        private bool _bufferedJumpUsable = false;
        private bool _endedJumpEarly = false;
        private bool _coyoteUsable = false;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && (_time < _timeJumpWasPressed + _stats.JumpBuffer);
        private bool CanUseCoyote => _coyoteUsable && !_grounded && (_time < _frameLeftGrounded + _stats.CoyoteTime);

        private void HandleJump()
        {
            //Debug.Log(_bufferedJumpUsable + " && ( " + _time + " < " + _timeJumpWasPressed + " + " + _stats.JumpBuffer + " )");

            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;

            _frameVelocity.y = _stats.JumpPower;
            //_rb.AddForce(Vector2.up * _stats.JumpPower, ForceMode2D.Impulse);
            //_frameVelocity = _rb.velocity;
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f) // on ground and falling
            {
                _frameVelocity.y = _stats.GroundingForce;
                //_frameVelocity.y = 0;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_frameVelocity.y > 0)
                {
                    if (_endedJumpEarly) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                    else inAirGravity *= _stats.JumpUpGravityModifier;
                }
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }

            //if (_frameVelocity.y > 0)
            //{
            //    _rb.gravityScale = _stats.JumpUpGravityModifier;
            //}
            //else
            //{
            //    _rb.gravityScale = 1f;
            //}
        }

        #endregion

        #region Respawn

        private void CheckDead()
        {
            if (playerTransform.position.y < _stats.deadPositionY)
                playerTransform.position = _stats.respawnPoint;
        }

        #endregion

        private void ApplyMovement() => _rb.velocity = _frameVelocity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}
