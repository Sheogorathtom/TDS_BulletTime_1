// using System;
// using UnityEngine;
// using UnityEngine.InputSystem;

// // Обеспечивает, что на объекте есть Rigidbody, иначе Unity добавит его автоматически
// [RequireComponent(typeof(Rigidbody))]
// public class PlayerController : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     [SerializeField] private float _speed;          // Скорость движения игрока
//     [SerializeField] private float _deadZone;       // Мертвая зона для стиков (чтобы маленькие значения не учитывались)
//     [SerializeField] private float _rotationSpeed;  // Скорость поворота игрока

//     private Vector2 _moveInput, _lookInput;         // Вектор ввода для движения и поворота
//     private bool _shoot;
//     private Rigidbody _rb;                          // Ссылка на Rigidbody для физического движения
//     private PlayerAnimationController _animationController; // Ссылка на анимации игрока

//     void Awake()
//     {
//         // Получаем компонент Rigidbody на объекте
//         _rb = GetComponent<Rigidbody>();
//         // Получаем компонент анимации у дочернего объекта (например, модель персонажа)
//         _animationController = GetComponentInChildren<PlayerAnimationController>();
//     }

//     // Метод, вызываемый системой InputSystem при перемещении стика движения
//     public void OnMove(InputAction.CallbackContext context)
//     {
//         // Считываем значение вектора движения из Input System
//         _moveInput = context.ReadValue<Vector2>();
//     }

//     // Метод для ввода направления взгляда или поворота (правый стик)
//     public void OnLookInput(InputAction.CallbackContext context)
//     {
//         _lookInput = context.ReadValue<Vector2>();
//     }

//     public void OnShoot(InputAction.CallbackContext context)
//     {
//     if (context.performed)
//     {
//         _shoot = true;
//         Debug.Log("Стреляю!");
//     }

//     if (context.canceled)
//     {
//         _shoot = false;
//     }
//     }


//     // FixedUpdate используется для работы с физикой
//     void FixedUpdate()
//     {
//         movePlayer();      // Двигаем игрока
//         handleRotation();  // Поворачиваем игрока
//     }

//     // Метод движения игрока
//     private void movePlayer()
//     {
//         // Преобразуем 2D-вектор ввода в 3D-вектор движения
//         Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
        
//         float inputMagnitude = movement.magnitude; // Длина вектора (скорость для анимации)

//         // Двигаем игрока только если сила ввода больше мертвой зоны
//         if(inputMagnitude > _deadZone)
//         {
//             Vector3 newPosition = _rb.position + movement * _speed * Time.fixedDeltaTime;
//             // Перемещаем Rigidbody на новую позицию
//             _rb.MovePosition(newPosition);
//         }

//         // Обновляем анимацию скорости и направления
//         _animationController.UpdateSpeed(inputMagnitude);
//         _animationController.UpdateDirection(movement);
//     }

//     // Метод управления поворотом игрока
//     private void handleRotation()
//     {
//         // Если правый стик (look) активен, поворачиваемся в его сторону
//         if (_lookInput.sqrMagnitude > _deadZone * _deadZone)
//         {
//             Vector3 lookDirection = new Vector3(_lookInput.x, 0f, _lookInput.y);
//             RotateTowards(lookDirection);
//             return; // Выходим, чтобы не использовать движение для поворота
//         }

//         // Если правый стик не активен, поворачиваемся в сторону движения
//         Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
//         if (movement.sqrMagnitude > _deadZone * _deadZone)
//         {
//             RotateTowards(movement);
//         }
//     }

//     // Метод для плавного поворота игрока в заданном направлении
//     private void RotateTowards(Vector3 direction)
//     {
//         // Создаем цельный поворот в направлении вектора
//         Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
//         // Плавно вращаем Rigidbody к цели с заданной скоростью
//         Quaternion newRotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
//         _rb.MoveRotation(newRotation);
//     }
// }


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

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireRate = 0.12f; // скорость стрельбы при удержании

    private bool _shoot;
    private float _nextFireTime;

    private Vector2 _moveInput, _lookInput;

    private Rigidbody _rb;
    private PlayerAnimationController _animationController;
    private Shooting _shooting;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animationController = GetComponentInChildren<PlayerAnimationController>();
        _shooting = GetComponentInChildren<Shooting>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _shoot = true;
            _animationController.SetShooting(true);
            Fire();
        }

        if (context.canceled)
        {
            _shoot = false;
            _animationController.SetShooting(false);
        }
    }

    void FixedUpdate()
    {
        movePlayer();
        handleRotation();
    }

    void Update()
    {
        HandleAutoFire();
        if(_shoot)
        {
             _shooting.ShootingRay();
        }
    }

    private void HandleAutoFire()
    {
        if (_shoot == false) return;

        if (Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + fireRate;
        }
    }

    private void Fire()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * bulletSpeed;   
        }
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

        _animationController.UpdateSpeed(inputMagnitude);
        _animationController.UpdateDirection(movement);
    }

    private void handleRotation()
    {
        if (_lookInput.sqrMagnitude > _deadZone * _deadZone)
        {
            Vector3 lookDirection = new Vector3(_lookInput.x, 0f, _lookInput.y);
            RotateTowards(lookDirection);
            return;
        }

        Vector3 movement = new Vector3(_moveInput.x, 0f, _moveInput.y);
        if (movement.sqrMagnitude > _deadZone * _deadZone)
        {
            RotateTowards(movement);
        }
    }

    private void RotateTowards(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        Quaternion newRotation = Quaternion.RotateTowards(_rb.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        _rb.MoveRotation(newRotation);
    }
}
