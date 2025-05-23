using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("Вісь, вздовж якої снаряд націлений в моделі префабу")]
    public Vector3 modelForwardAxis = Vector3.up;
    public void Launch(Vector3 start, Vector3 target, float height, float duration)
    {
        StartCoroutine(ArcRoutine(start, target, height, duration));
    }

    private IEnumerator ArcRoutine(Vector3 start, Vector3 target, float height, float duration)
    {
        float elapsed = 0f;
        // початкова позиція
        transform.position = start;

        // Для створення правильної початкової орієнтації
        Vector3 initialDirection = target - start;
        initialDirection.y = 0; // Забираємо вертикальну складову для початкової орієнтації

        if (initialDirection.magnitude > 0.001f)
        {
            // Створюємо ротацію, яка спрямовує modelForwardAxis у бік цілі
            Quaternion initialRotation = Quaternion.FromToRotation(modelForwardAxis, initialDirection.normalized);
            transform.rotation = initialRotation;
        }

        Vector3 previousPos = start;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // Пряма між двома точками
            Vector3 pos = Vector3.Lerp(start, target, t);
            // Додаємо параболічний підйом
            pos.y += height * 4f * t * (1f - t);

            // Розраховуємо поточний вектор руху
            Vector3 velocity = pos - previousPos;

            if (velocity.magnitude > 0.001f)
            {
                // Створюємо ротацію від модельної осі до напрямку руху
                Quaternion targetRotation = Quaternion.FromToRotation(modelForwardAxis, velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }

            transform.position = pos;
            previousPos = pos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        Destroy(gameObject);
    }
}
