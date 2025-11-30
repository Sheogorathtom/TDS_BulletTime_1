using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _speed;
    [SerializeField] private float _deadZone;
    [SerializeField] private float _rotationSpeed;

    [Header("Cursor Follower")]
    [SerializeField] private Transform cursorFollower; // Объект, который следует за курсором
    [SerializeField] private float cursorFixedHeight = 1.5f; // Фиксированная высота (постоянная)

    [Header("Dash Settings")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.15f;
    [SerializeField] private float _dashCooldown = 0.5f;

    private Vector2 _moveInput, _mouseLookInput;
    private Vector3 _rotationTarget;
    private Rigidbody _rb;
    private PlayerAnimationController _animationController;
    private Health _health;

    private bool _isDashing = false;
    private bool _canDash = true;
    private float _dashTimer = 0f;
    private float _dashCooldownTimer = 0f;
    private Vector3 _dashDirection;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animationController = GetComponentInChildren<PlayerAnimationController>();
        _health = GetComponent<Health>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnMouseLookInput(InputAction.CallbackContext context)
    {
        _mouseLookInput = context.ReadValue<Vector2>();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (!_canDash) return;

        Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
        if (movement.sqrMagnitude <= _deadZone * _deadZone) return;

        _dashDirection = movement.normalized;
        _isDashing = true;
        _canDash = false;
        _dashTimer = _dashDuration;

        if (_health != null)
            _health.SetInvulnerable(true);
    }

    void FixedUpdate()
    {
        // Обновление таймеров dash
        if (_isDashing)
        {
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                _dashCooldownTimer = _dashCooldown;

                if (_health != null)
                    _health.SetInvulnerable(false);
            }
        }
        else if (!_canDash)
        {
            _dashCooldownTimer -= Time.fixedDeltaTime;
            if (_dashCooldownTimer <= 0f)
                _canDash = true;
        }

        movePlayer();

        Ray ray = Camera.main.ScreenPointToRay(_mouseLookInput);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            _rotationTarget = hit.point;

            if (cursorFollower != null)
            {
                // Берём только X и Z, а Y всегда фиксирован
                Vector3 pos;
                pos.x = hit.point.x;
                pos.z = hit.point.z;
                pos.y = cursorFixedHeight; // постоянная высота

                cursorFollower.position = pos;
            }
        }

        movePlayerWithAim();
    }

    private void movePlayer()
    {
        Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
        float inputMagnitude = movement.magnitude;

        if (_isDashing)
        {
            Vector3 newPosition = _rb.position + _dashDirection * _dashSpeed * Time.fixedDeltaTime;
            _rb.MovePosition(newPosition);
        }
        else if (inputMagnitude > _deadZone)
        {
            Vector3 newPosition = _rb.position + movement * _speed * Time.fixedDeltaTime;
            _rb.MovePosition(newPosition);
        }

        if (_animationController != null)
        {
            _animationController.UpdateSpeed(_isDashing ? 1f : inputMagnitude);
            _animationController.UpdateDirection(movement);
        }
    }

    private void movePlayerWithAim()
    {
        Vector3 lookPos = _rotationTarget - transform.position;
        lookPos.y = 0;

        if (lookPos != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }
    }
}
