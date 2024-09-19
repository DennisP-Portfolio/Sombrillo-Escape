using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool CanMove;
    public bool IsSafeFromJumpScare;

    [Header("Speed")]
    [SerializeField] private float _Speed = 5;
    [SerializeField] private float _MaxSpeed = 20;
    [SerializeField] private bool _Decelerate = false;
    [SerializeField] private float _Deceleration = 5f;
    [SerializeField] private LayerMask _SlopedGround;

    [Header ("Jump")]
    public bool IsOnPlatform = false;
    [SerializeField, Range(1, 50)] private float _JumpForce = 5;
    [SerializeField] private LayerMask _JumpableGround;
    [SerializeField] private float _FallMultiplier = 2.5f;
    [SerializeField] private float _LowJumpMultiplier = 4f;
    private bool _doJump = false;

    [Header ("Shooting")]
    [SerializeField] private Gun[] _Guns;

    private Rigidbody _rb;
    private SphereCollider _coll;
    private float _horizontalAxis;
    private float _verticalAxis;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (CanMove)
        {
            _horizontalAxis = Input.GetAxis("Horizontal");
            _verticalAxis = Input.GetAxis("Vertical");
        }

        if (Input.GetButtonDown("Jump") && CanMove)
        {
            if (IsGrounded() || IsSloping())
            {
                _doJump = true;
            }
        }

        if (_horizontalAxis == 0 && _verticalAxis == 0) // Decelerate when not giving movement input
        {
            _Decelerate = true;
        }
        else
        {
            _Decelerate = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            foreach (Gun gun in _Guns) // Firing all equipped guns
            {
                gun.Shoot();
            }
        }
    }

    private void FixedUpdate()
    {
        var camera = Camera.main;

        //camera forward and right vectors:
        var forward = camera.transform.forward;
        var right = camera.transform.right;

        //project forward and right vectors on the horizontal plane (y = 0)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        //Get direction in world space:
        var moveDirection = forward * _verticalAxis + right * _horizontalAxis;
        moveDirection.Normalize();

        //Calculate the velocity based on user input and speed:
        var velocity = moveDirection * _Speed;

        //Apply the acceleration:
        _rb.AddForce(velocity, ForceMode.Acceleration);

        //Add drag when not moving for deceleration:
        if (_Decelerate)
        {
            _rb.drag = _Deceleration;
        }
        else
        {
            _rb.drag = 0f;
        }

        //Cap the speed to a certain amount:
        if (_rb.velocity.magnitude > _MaxSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * _MaxSpeed;
        }

        #region Jump
        if (!IsOnPlatform) // Only applying downforce for faster falling when off platform
        {
            if (_rb.velocity.y < -0.1f)
            {
                _rb.velocity += Vector3.up * Physics.gravity.y * (_FallMultiplier - 1) * Time.deltaTime;
            }
            else if (_rb.velocity.y > 0.1f && !Input.GetButton("Jump") && !IsSloping() && !IsGrounded())
            {
                _rb.velocity += Vector3.up * Physics.gravity.y * (_LowJumpMultiplier - 1) * Time.deltaTime;
            }
        }

        if (_doJump)
        {
            Jump();
            _doJump = false;
        }
        #endregion
    }

    /// <summary>
    /// Applying jump physics with force
    /// </summary>
    private void Jump()
    {
        _rb.AddForce(Vector3.up * _JumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Checking for ground at a certain point underneath the player on a set layer
    /// </summary>
    private bool IsGrounded() 
    {
        return Physics.Raycast(transform.position, Vector3.down, _coll.radius + 0.1f, _JumpableGround);
    }

    /// <summary>
    /// Ground check with greater radius, because slopes will not be perfectly underneath the player on a set layer
    /// </summary>
    private bool IsSloping() 
    {
        if (Physics.CheckSphere(transform.position, _coll.radius + 0.1f, _JumpableGround))
        {
            return true;
        }

        return false;
    }
}
