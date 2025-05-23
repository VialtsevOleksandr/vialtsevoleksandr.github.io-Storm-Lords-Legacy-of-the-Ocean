using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab; // префаб нашої клітинки
    [SerializeField] private int gridSize = 10;     // розмір сітки, наприклад 10x10
    [SerializeField] private float cellOffset = 1.15f; // відстань між клітинками (залежить від розміру Mesh-а)

    public void GenerateGrid()
    {
        // Генеруємо gridSize x gridSize клітинок
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                // Створюємо локальну позицію відносно цього Generator’а
                Vector3 localPos = new Vector3(x * cellOffset, 0f, z * cellOffset);
                // Додаємо світову позицію transform
                Vector3 worldPos = transform.position + localPos;

                // Інстанціюємо клітинку безпосередньо з батьком
                GameObject newCell = Instantiate(
                    cellPrefab,
                    worldPos,
                    Quaternion.identity,
                    this.transform      // одразу ставимо в ієрархію
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