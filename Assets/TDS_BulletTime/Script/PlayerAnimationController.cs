using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    Animator animator; // Ссылка на Animator, управляющий анимациями персонажа

    void Start()
    {
        // Получаем компонент Animator, который должен быть на этом объекте
        animator = GetComponent<Animator>();
    }

    // Обновляет параметр скорости для Blend Tree или других анимаций движения
    public void UpdateSpeed(float speed)
    {
        animator.SetFloat("Velocity", speed);
    }

    // Обновляет направление движения персонажа относительно его локальной системы координат
    public void UpdateDirection(Vector3 direction)
    {
        // Переводим мировое направление движения в локальное пространство персонажа
        // Это важно, чтобы анимация работала корректно независимо от поворота модели
        Vector3 localDir = transform.InverseTransformDirection(direction);

        // Устанавливаем параметры X и Y в Animator для 8-direction Blend Tree
        animator.SetFloat("X", localDir.x);
        animator.SetFloat("Y", localDir.z);
    }

    public void SetShooting(bool state)
    {
        animator.SetBool("isShooting", state);
    }
}
