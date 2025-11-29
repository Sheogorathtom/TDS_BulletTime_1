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

    private Vector2 _moveInput, _mouseLookInput;
    private Vector3 _rotationTarget;
    private Rigidbody _rb;
    private PlayerAnimationController _animationController;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animationController = GetComponentInChildren<PlayerAnimationController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnMouseLookInput(InputAction.CallbackContext context)
    {
        _mouseLookInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
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

        if (inputMagnitude > _deadZone)
        {
            Vector3 newPosition = _rb.position + movement * _speed * Time.fixedDeltaTime;
            _rb.MovePosition(newPosition);
        }

        if (_animationController != null)
        {
            _animationController.UpdateSpeed(inputMagnitude);
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


