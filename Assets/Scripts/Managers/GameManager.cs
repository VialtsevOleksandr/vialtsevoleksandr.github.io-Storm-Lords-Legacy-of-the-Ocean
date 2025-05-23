using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
public enum GameState
{
    Player1Placing,
    Player2Placing,
    Player1Turn,
    Player2Turn,
    GameOver
}

[System.Serializable]
public class ShipTypeInfo
{
    [Tooltip("Префаб корабля з компонентом ShipBase")]
    public ShipBase prefab;
    [Tooltip("Кількість таких кораблів")]
    public int quantity;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State => state;

    [Header("Menus")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI winnerText;

    [SerializeField] private Button autoPlaceButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Grids")]
    [SerializeField] private GridGenerator playerGrid;
    [SerializeField] private GridGenerator opponentGrid;

    [Header("Ships Setup")]
    [SerializeField] private ShipTypeInfo[] shipTypes;

    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileHeight = 2f;
    [SerializeField] private float projectileDuration = 0.8f;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject missEffectPrefab;


    private List<Cell> playerCells = new List<Cell>();
    private List<Cell> opponentCells = new List<Cell>();
    private List<ShipBase> playerShips = new List<ShipBase>();
    private List<ShipBase> opponentShips = new List<ShipBase>();
    private int gridDim;
    private GameState state = GameState.Player1Placing;

    // Перевірка на кількість кораблів
    private int totalShipCount = 0;
    public void OnStartGame()
    {
        // сховати стартовий екран
        startPanel.SetActive(false);
        // перейти у фазу розстановки гравця 1
        state = GameState.Player1Placing;
        UpdateUI();
        ApplyShipsVisibility();
    }

    public void OnRestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnGameOver(string winner)
    {
        state = GameState.GameOver;
        endPanel.SetActive(true);
        winnerText.text = $"{winner} переміг!";
    }
    private void CheckReadyButton()
    {
        readyButton.interactable = !readyButton.interactable;
    }
    // Центри спостереження для пострілів
    private Vector3 playerOrigin, opponentOrigin;
    private void Awake()
    {
        // Singleton
        Instance = this;

        // Генеруємо дві дошки
        playerGrid.GenerateGrid();
        opponentGrid.GenerateGrid();
    }

    private void Start()
    {
        // Збираємо клітинки обох дошок
        playerCells = CollectCells(playerGrid);
        opponentCells = CollectCells(opponentGrid);
        gridDim = (int)Mathf.Sqrt(playerCells.Count);

        // Вираховуємо середину кожної дошки
        playerOrigin = ComputeCenter(playerCells) + Vector3.up * 0.5f;
        opponentOrigin = ComputeCenter(opponentCells) + Vector3.up * 0.5f;

        // Прив’язуємо кнопки
        autoPlaceButton.onClick.AddListener(OnAutoPlace);
        readyButton.onClick.AddListener(OnReady);

        // підписка на Start
        startButton.onClick.AddListener(OnStartGame);
        // підписка на Restart
        restartButton.onClick.AddListener(OnRestartGame);

        // спочатку показуємо лише стартову панель
        startPanel.SetActive(true);
        endPanel.SetActive(false);

        UpdateUI();
    }
    private void Update()
    {
        if (state == GameState.Player1Turn)
            HandleShot(opponentCells, GameState.Player2Turn, "Гравець 1", playerOrigin);
        else if (state == GameState.Player2Turn)
            HandleShot(playerCells, GameState.Player1Turn, "Гравець 2", opponentOrigin);
    }

    // Для запобігання повторних пострілів
    private bool isProcessingShot = false;

    private void HandleShot(List<Cell> targets, GameState nextState, string label, Vector3 origin)
    {
        // якщо зараз обробляється постріл, ігноруємо нові кліки
        if (isProcessingShot) return;

        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;

        var cell = hit.collider.GetComponent<Cell>();
        if (cell == null || !targets.Contains(cell) || cell.isHit) return;

        // Встановлюємо флаг, що обробляємо постріл
        isProcessingShot = true;
        StartCoroutine(ShotSequence(cell, nextState, label, origin));
    }
    private IEnumerator ShotSequence(Cell cell, GameState nextState, string label, Vector3 origin)
    {
        // Створюємо снаряд
        Vector3 targetPosition = cell.transform.position + Vector3.up * 0.2f;
        Vector3 direction = targetPosition - origin;

        // Cтворюємо та орієнтуємо снаряд
        Projectile proj = Instantiate(projectilePrefab, origin, Quaternion.identity);
        proj.Launch(origin, targetPosition, projectileHeight, projectileDuration);

        // Чекаємо, поки полетить
        yield return new WaitForSeconds(projectileDuration);

        // Після прильоту відмічаємо влучання
        cell.TakeHit();
        bool wasHit = cell.shipOnCell != null;

        // Якщо влучили, показуємо ефект
        if (wasHit)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, cell.transform.position + Vector3.up * 0.2f, Quaternion.identity);
            Destroy(hitEffect, 1f);
        }
        else
        {
            GameObject missEffect = Instantiate(missEffectPrefab, cell.transform.position + Vector3.up * 0.2f, Quaternion.identity);
            Destroy(missEffect, 1f);
        }

        if (wasHit && cell.shipOnCell.IsSunk)
        {
            ShipBase sunkShip = cell.shipOnCell;

            // Знаходимо всі рендерери в кораблі
            Renderer[] renderers = sunkShip.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material.color = Color.red;
            }

            // Перевіряємо, чи всі кораблі супротивника знищені
            if (opponentShips.Count(ship => ship.IsSunk) == 10 || playerShips.Count(ship => ship.IsSunk) == 10)
            {
                yield return new WaitForSeconds(1f);
                OnGameOver(label);
                yield break;
            }
        }

        // Оновлюємо статус
        statusText.text = wasHit
            ? $"{label} влучив!"
            : $"{label} промахнувся!";

        // Якщо промах — передаємо хід
        if (!wasHit)
        {
            statusText.text = $"Перехід хода, зачекайте 2 секунди";
            yield return new WaitForSeconds(2f);
            state = nextState;
            UpdateUI();
            ApplyShipsVisibility();
        }
        // Після завершення всіх дій, знімаємо флаг обробки пострілу
        isProcessingShot = false;
    }

    private void OnAutoPlace()
    {
        if (state == GameState.Player1Placing)
            PlaceShipsOnBoard(playerCells, playerShips);
        else if (state == GameState.Player2Placing)
            PlaceShipsOnBoard(opponentCells, opponentShips);
    }

    private void OnReady()
    {
        totalShipCount = 0;

        if (state == GameState.Player1Placing)
        {
            CheckReadyButton();
            state = GameState.Player2Placing;
        }
        else if (state == GameState.Player2Placing)
        {
            StartCoroutine(StartGameWithDelay());
            isCustomStatusActive = true;
            autoPlaceButton.gameObject.SetActive(false);
            readyButton.gameObject.SetActive(false);
        }

        UpdateUI();
        ApplyShipsVisibility();
    }
    private bool isCustomStatusActive = false;
    private IEnumerator StartGameWithDelay()
    {
        statusText.text = "Запускаємо гру, передайте керування першому гравцю";
        yield return new WaitForSeconds(2f);

        state = GameState.Player1Turn;
        UpdateUI();
        ApplyShipsVisibility();
    }

    private void UpdateUI()
    {
        if (isCustomStatusActive)
        {
            isCustomStatusActive = false; // Скидаємо прапорець
            return; // Пропускаємо оновлення тексту, якщо активний кастомний статус
        }

        bool placing = state == GameState.Player1Placing || state == GameState.Player2Placing;
        autoPlaceButton.gameObject.SetActive(placing);
        readyButton.gameObject.SetActive(placing);

        switch (state)
        {
            case GameState.Player1Placing:
                statusText.text = "Гравець 1: розставте кораблі";
                AnchorButtonsLeft();
                break;
            case GameState.Player2Placing:
                statusText.text = "Гравець 2: розставте кораблі";
                AnchorButtonsRight();
                break;
            case GameState.Player1Turn:
                statusText.text = "Хід Гравця 1";
                break;
            case GameState.Player2Turn:
                statusText.text = "Хід Гравця 2";
                break;
        }
    }

    private void AnchorButtonsLeft()
    {
        var apRT = autoPlaceButton.GetComponent<RectTransform>();
        var rdRT = readyButton.GetComponent<RectTransform>();
        // Лівий верхній кут
        apRT.anchorMin = apRT.anchorMax = new Vector2(0, 1);
        apRT.pivot = new Vector2(0, 1);
        rdRT.anchorMin = rdRT.anchorMax = new Vector2(0, 1);
        rdRT.pivot = new Vector2(0, 1);
        // Відступ від краю
        apRT.anchoredPosition = new Vector2(10, -10);
        rdRT.anchoredPosition = new Vector2(10, -45);
    }

    private void AnchorButtonsRight()
    {
        var apRT = autoPlaceButton.GetComponent<RectTransform>();
        var rdRT = readyButton.GetComponent<RectTransform>();
        // Правий верхній кут
        apRT.anchorMin = apRT.anchorMax = new Vector2(1, 1);
        apRT.pivot = new Vector2(1, 1);
        rdRT.anchorMin = rdRT.anchorMax = new Vector2(1, 1);
        rdRT.pivot = new Vector2(1, 1);
        // Відступ від краю
        apRT.anchoredPosition = new Vector2(-10, -10);
        rdRT.anchoredPosition = new Vector2(-10, -45);
    }
    private void ApplyShipsVisibility()
    {
        // Ваші кораблі видно лише під час вашого ходу або фази розстановки
        bool showP1 = state == GameState.Player1Placing || state == GameState.Player1Turn;
        bool showP2 = state == GameState.Player2Placing || state == GameState.Player2Turn;
        SetShipsVisibility(playerShips, showP1);
        SetShipsVisibility(opponentShips, showP2);
    }
    private void SetShipsVisibility(List<ShipBase> ships, bool visible)
    {
        foreach (var s in ships)
            if (s) s.gameObject.SetActive(visible);
    }

    private void PlaceShipsOnBoard(List<Cell> cells, List<ShipBase> shipsList)
    {
        foreach (var c in cells)
            c.ResetCell();

        foreach (var s in shipsList)
            if (s) Destroy(s.gameObject);
        shipsList.Clear();

        foreach (var info in shipTypes)
            for (int i = 0; i < info.quantity; i++)
            {
                TryPlaceOne(cells, shipsList, info.prefab);
                totalShipCount += 1;
            }

        if (totalShipCount == 10)
            CheckReadyButton();
    }


    private void TryPlaceOne(List<Cell> cells, List<ShipBase> shipsList, ShipBase prefab)
    {
        int size = prefab.Size;
        bool placed = false;
        int tries = 0;

        while (!placed && tries++ < 1000)
        {
            Cell start = cells[Random.Range(0, cells.Count)];
            bool horiz = Random.value > .5f;
            var seg = GetSegment(cells, start, horiz, size);
            if (seg == null) continue;

            var go = Instantiate(prefab.gameObject);
            var ship = go.GetComponent<ShipBase>();
            shipsList.Add(ship);

            // Прив’язуємо клітинки до корабля
            foreach (var c in seg)
            {
                c.shipOnCell = ship;
                ship.occupiedCells.Add(c);
            }

            // Блокуємо навколо
            foreach (var c in seg)
            {
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var coord = c.coordinates + new Vector2Int(dx, dy);
                        var neigh = GetCellByCoord(coord, cells);
                        if (neigh != null && neigh.shipOnCell == null)
                            neigh.isBlocked = true;
                    }
            }

            go.transform.position = CenterOf(seg);
            if (!horiz) go.transform.rotation = Quaternion.Euler(0, 90, 0);

            placed = true;
        }

        if (!placed)
            Debug.LogWarning($"Не вдалося розставити {prefab.name}");
    }

    private List<Cell> GetSegment(List<Cell> cells, Cell start, bool horiz, int size)
    {
        var seg = new List<Cell>();
        var s = start.coordinates;

        // Перебір i=0..size-1
        for (int i = 0; i < size; i++)
        {
            Vector2Int coord = horiz
                ? new Vector2Int(s.x + i, s.y)
                : new Vector2Int(s.x, s.y + i);

            // Перевірка меж
            if (coord.x < 0 || coord.y < 0 || coord.x >= gridDim || coord.y >= gridDim)
                return null;

            var c = GetCellByCoord(coord, cells);
            if (c == null || c.shipOnCell != null || c.isBlocked)
                return null;
            seg.Add(c);
        }

        return seg;
    }

    private Vector3 CenterOf(List<Cell> seg)
    {
        Vector3 sum = Vector3.zero;
        foreach (var c in seg) sum += c.transform.position;
        var ctr = sum / seg.Count; ctr.y += 0.25f;
        return ctr;
    }

    private List<Cell> CollectCells(GridGenerator gen)
    {
        var list = new List<Cell>();
        foreach (Transform t in gen.transform)
            if (t.TryGetComponent<Cell>(out var c))
                list.Add(c);
        return list;
    }

    public Cell GetCellAt(Vector2Int coord, Transform gridRoot)
    {
        // gridRoot — це transform того GridGenerator, на якому сидять клітинки
        foreach (Transform t in gridRoot)
            if (t.TryGetComponent<Cell>(out var c) && c.coordinates == coord)
                return c;
        return null;
    }
    private Cell GetCellByCoord(Vector2Int coord, List<Cell> cells)
    {
        return cells.Find(c => c.coordinates == coord);
    }

    private Vector3 ComputeCenter(List<Cell> cells)
    {
        Vector3 sum = Vector3.zero;
        foreach (var c in cells) sum += c.transform.position;
        return sum / cells.Count;
    }
}
