using UnityEngine;
using System;

[Serializable]
public class Cell : MonoBehaviour
{
    [Header("Grid Coordinates")]
    [Tooltip("���������� ������� � ���� (x,z)")]
    public Vector2Int coordinates;

    [Header("State")]
    [Tooltip("��������� �� ��������, ���� �� ��� �")]
    public ShipBase shipOnCell;

    // ����, �� ������ � �� �� ������� ��� �������
    [HideInInspector]
    public bool isHit = false;
    // ����, �� ����� ������� ������� �������
    [HideInInspector]
    public bool isBlocked = false;

    // �������� ��������� �� ��������, ��� �������� ���� ��������
    private Renderer _renderer;
    private Color _initialColor;

    private void Awake()
    {
        // ��������� ��������� �� Renderer, ��� �������� ����/������� ��� ������
        _renderer = GetComponent<Renderer>();
        _initialColor = _renderer.material.color;
    }

    public void ResetCell()
    {
        shipOnCell = null;
        isHit = false;
        isBlocked = false;
        _renderer.material.color = _initialColor;
    }

    // ��� ����� ����� ���������, ���� �������� � �� �������
    public void TakeHit()
    {
        // ���� ��� ��� �������� � ����� �� ������
        if (isHit) return;
        isHit = true;

        // ���� � ��������� �� �������� � ����������� � �������� �� ��������� RegisterHit
        if (shipOnCell != null)
        {
            Debug.Log($"��������� � ������� � ������������: {coordinates}");
            _renderer.material.color = Color.red;
            shipOnCell.RegisterHit();
        }
        else
        {
            // ������ ������ � ������� � ����
            _renderer.material.color = Color.black;
        }
    }
}
