using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public abstract class ShipBase : MonoBehaviour
{
    public abstract int Size { get; }    // кількість палуб
    public int HitsTaken { get; private set; }

    // Список клітинок, які займає корабель
    [HideInInspector]
    public List<Cell> occupiedCells = new List<Cell>();
    public bool IsSunk => HitsTaken >= Size;

    public void RegisterHit()
    {
        HitsTaken++;
        if (IsSunk) OnSunk();
    }

    protected virtual void OnSunk()
    {
        Debug.Log($"{name} ({Size}-палубний) знищено!");

        // При знищенні – “стріляємо” у всі навколишні клітинки
        foreach (var cell in occupiedCells)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    var coord = cell.coordinates + new Vector2Int(dx, dy);
                    var neighbor = GameManager.Instance.GetCellAt(coord, cell.transform.parent);
                    if (neighbor != null && !neighbor.isHit)
                    {
                        neighbor.TakeHit();
                    }
                }
        }
    }
}
