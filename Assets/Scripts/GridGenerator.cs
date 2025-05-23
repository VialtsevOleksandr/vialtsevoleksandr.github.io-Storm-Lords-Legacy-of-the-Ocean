using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab; // ������ ���� �������
    [SerializeField] private int gridSize = 10;     // ����� ����, ��������� 10x10
    [SerializeField] private float cellOffset = 1.15f; // ������� �� ��������� (�������� �� ������ Mesh-�)

    public void GenerateGrid()
    {
        // �������� gridSize x gridSize �������
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                // ��������� �������� ������� ������� ����� Generator��
                Vector3 localPos = new Vector3(x * cellOffset, 0f, z * cellOffset);
                // ������ ������ ������� transform
                Vector3 worldPos = transform.position + localPos;

                // ������������ ������� ������������� � �������
                GameObject newCell = Instantiate(
                    cellPrefab,
                    worldPos,
                    Quaternion.identity,
                    this.transform      // ������ ������� � ��������
                );

                newCell.name = $"Cell_{x}_{z}";

                if (newCell.TryGetComponent<Cell>(out var cellComponent))
                {
                    cellComponent.coordinates = new Vector2Int(x, z);
                }
            }
        }
    }
}