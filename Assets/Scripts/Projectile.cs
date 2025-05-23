using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Tooltip("³��, ������ ��� ������ ��������� � ����� �������")]
    public Vector3 modelForwardAxis = Vector3.up;
    public void Launch(Vector3 start, Vector3 target, float height, float duration)
    {
        StartCoroutine(ArcRoutine(start, target, height, duration));
    }

    private IEnumerator ArcRoutine(Vector3 start, Vector3 target, float height, float duration)
    {
        float elapsed = 0f;
        // ��������� �������
        transform.position = start;

        // ��� ��������� ��������� ��������� ��������
        Vector3 initialDirection = target - start;
        initialDirection.y = 0; // �������� ����������� �������� ��� ��������� ��������

        if (initialDirection.magnitude > 0.001f)
        {
            // ��������� �������, ��� �������� modelForwardAxis � �� ���
            Quaternion initialRotation = Quaternion.FromToRotation(modelForwardAxis, initialDirection.normalized);
            transform.rotation = initialRotation;
        }

        Vector3 previousPos = start;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            // ����� �� ����� �������
            Vector3 pos = Vector3.Lerp(start, target, t);
            // ������ ����������� �����
            pos.y += height * 4f * t * (1f - t);

            // ����������� �������� ������ ����
            Vector3 velocity = pos - previousPos;

            if (velocity.magnitude > 0.001f)
            {
                // ��������� ������� �� �������� �� �� �������� ����
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
