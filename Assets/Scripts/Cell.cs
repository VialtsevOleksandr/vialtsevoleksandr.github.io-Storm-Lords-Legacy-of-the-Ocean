using UnityEngine;
using System;

[Serializable]
public class Cell : MonoBehaviour
{
    [Header("Grid Coordinates")]
    [Tooltip("Координати клітинки у сітці (x,z)")]
    public Vector2Int coordinates;

    [Header("State")]
    [Tooltip("Посилання на корабель, якщо він тут є")]
    public ShipBase shipOnCell;

    // Флаг, що показує — чи по клітинці вже стріляли
    [HideInInspector]
    public bool isHit = false;
    // Флаг, що блокує клітинки навколо корабля
    [HideInInspector]
    public bool isBlocked = false;

    // Локальне посилання на рендерер, щоб змінювати колір матеріалу
    private Renderer _renderer;
    private Color _initialColor;

    private void Awake()
    {
        // Збережемо посилання на Renderer, аби змінювати колір/матеріал при потребі
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

    // Цей метод можна викликати, коли стріляють у цю клітинку
    public void TakeHit()
    {
        // Якщо вже був влучений — нічого не робимо
        if (isHit) return;
        isHit = true;

        // Якщо є посилання на корабель — пофарбувати в червоний та повідомити RegisterHit
        if (shipOnCell != null)
        {
            Debug.Log($"Вистрілили в клітинку з координатами: {coordinates}");
            _renderer.material.color = Color.red;
            shipOnCell.RegisterHit();
        }
        else
        {
            // Пустий постріл — фарбуємо в сірий
            _renderer.material.color = Color.black;
        }
    }
}
