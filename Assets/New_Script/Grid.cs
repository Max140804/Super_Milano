using UnityEngine;

public class Grid : MonoBehaviour
{
    public int rows = 10;
    public int columns = 20;
    public float cellWidth = 50f;
    public float cellHeight = 50f;
    public GameObject cellPrefab;
    private GameObject[,] gridArray;
    private GameObject[,] occupiedObjects;

    private void Start()
    {
        GenerateGrid();
        InitializeOccupiedObjectsArray();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            Vector2 localMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), mousePosition, Camera.main, out localMousePosition);

            Vector2Int cellIndex = GetCellIndexFromLocalPosition(localMousePosition);

        }
    }

    private void GenerateGrid()
    {
        gridArray = new GameObject[rows, columns];
        occupiedObjects = new GameObject[rows, columns]; // Initialize the occupied objects array

        RectTransform canvasRectTransform = GetComponent<RectTransform>();

        float xOffset = (columns - 1) / 2f * cellWidth;
        float yOffset = (rows - 1) / 2f * cellHeight;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                RectTransform cellRectTransform = cell.GetComponent<RectTransform>();
                cellRectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);
                cellRectTransform.anchoredPosition = new Vector2((col * cellWidth) - xOffset, -(row * cellHeight) + yOffset);
                cell.name = $"Cell_{row}_{col}";
                gridArray[row, col] = cell;
                occupiedObjects[row, col] = null;
            }
        }
    }

    private void InitializeOccupiedObjectsArray()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                occupiedObjects[row, col] = null;
            }
        }
    }

    public GameObject GetCellObject(Vector2Int cellIndex)
    {
        if (IsValidCellIndex(cellIndex))
        {
            return occupiedObjects[cellIndex.y, cellIndex.x];
        }
        return null;
    }

    public bool AttachObjectToGrid(GameObject obj, bool snapToHorizontalNeighbors)
    {
        RectTransform objRectTransform = obj.GetComponent<RectTransform>();
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out localMousePosition);

        Vector2Int cellIndex = GetCellIndexFromLocalPosition(localMousePosition);

        if (IsValidCellIndex(cellIndex))
        {
            if (!IsCellLocked(cellIndex) && CanSnapToCell(cellIndex, obj))
            {
                GameObject cell = gridArray[cellIndex.y, cellIndex.x];
                Vector2Int[] neighborIndices = new Vector2Int[1];
                neighborIndices[0] = cellIndex;

                if (snapToHorizontalNeighbors)
                {
                    neighborIndices = new Vector2Int[2];
                    neighborIndices[0] = cellIndex + new Vector2Int(-1, 0);
                    neighborIndices[1] = cellIndex + new Vector2Int(1, 0);
                }

                // Find the nearest neighbor cell
                Vector2Int nearestCellIndex = FindNearestCellIndex(neighborIndices, localMousePosition);

                if (IsValidCellIndex(nearestCellIndex))
                {
                    RectTransform nearestCellRectTransform = GetCellRectTransform(nearestCellIndex);
                    objRectTransform.SetParent(nearestCellRectTransform, false);
                    objRectTransform.anchoredPosition = Vector3.zero;
                    Vector2Int objCellSize = CalculateCardCellSize(obj);
                    MarkCellsOccupied(nearestCellIndex, objCellSize, obj);

                    Debug.Log($"Snapped object to cell {nearestCellIndex}");

                    return true;
                }
                else
                {
                    Debug.LogError("No valid neighboring cell found.");
                }
            }
            else
            {
                Debug.LogError($"Cell {cellIndex} is already occupied or cannot be snapped to.");
            }
        }
        else
        {
            Debug.LogError("Invalid cell index");
        }

        return false;
    }

    private bool CanSnapToCell(Vector2Int cellIndex, GameObject card)
    {
        if (IsValidCellIndex(cellIndex))
        {
            GameObject cellObject = gridArray[cellIndex.y, cellIndex.x];
            Cell cellData = cellObject.GetComponent<Cell>();

            if (cellData != null)
            {
                GetValue cardData = card.GetComponentInChildren<GetValue>();

                if (cardData != null)
                {
                    return cellData.cellValue == cardData.topValue || cellData.cellValue == cardData.bottomValue || cellData.cellValue == -1;
                }
            }
        }
        return false;
    }

    private void MarkCellsOccupied(Vector2Int startCellIndex, Vector2Int objCellSize, GameObject obj)
    {
        for (int row = startCellIndex.y; row < startCellIndex.y + objCellSize.y; row++)
        {
            for (int col = startCellIndex.x; col < startCellIndex.x + objCellSize.x; col++)
            {
                occupiedObjects[row, col] = obj;
            }
        }
    }

    public bool IsCellLocked(Vector2Int cellIndex)
    {
        if (IsValidCellIndex(cellIndex))
        {
            return occupiedObjects[cellIndex.y, cellIndex.x] != null;
        }
        return false;
    }

    public void LockCell(Vector2Int cellIndex)
    {
        if (IsValidCellIndex(cellIndex))
        {
            occupiedObjects[cellIndex.y, cellIndex.x] = new GameObject("LockedCell");
        }
    }

    public void UnlockCell(Vector2Int cellIndex)
    {
        if (IsValidCellIndex(cellIndex))
        {
            occupiedObjects[cellIndex.y, cellIndex.x] = null;
        }
    }

    private Vector2Int FindNearestCellIndex(Vector2Int[] indices, Vector2 dragPosition)
    {
        Vector2Int nearestIndex = indices[0];
        float minDistance = Vector2.Distance(dragPosition, GetCellCenterWorld(nearestIndex));

        for (int i = 1; i < indices.Length; i++)
        {
            Vector2Int currentIndex = indices[i];
            float distance = Vector2.Distance(dragPosition, GetCellCenterWorld(currentIndex));

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestIndex = currentIndex;
            }
        }

        return nearestIndex;
    }

    public Vector2 GetCellCenterWorld(Vector2Int cellIndex)
    {
        float cellX = cellIndex.x * cellWidth + cellWidth / 2f;
        float cellY = cellIndex.y * cellHeight + cellHeight / 2f;

        Vector2 cellCenterWorld = new Vector2(cellX, cellY);
        cellCenterWorld += (Vector2)transform.position;

        return cellCenterWorld;
    }

    public Vector2Int GetCellIndexFromLocalPosition(Vector2 localPosition)
    {
        int col = Mathf.FloorToInt((localPosition.x + (columns / 2f) * cellWidth) / cellWidth);
        int row = Mathf.FloorToInt((-localPosition.y + (rows / 2f) * cellHeight) / cellHeight);

        return new Vector2Int(col, row);
    }

    public Vector2Int GetCellIndexFromPosition(Vector3 position)
    {
        Vector2 localPosition = transform.InverseTransformPoint(position);
        return GetCellIndexFromLocalPosition(localPosition);
    }

    public Vector2Int GetCellIndexFromCellObject(GameObject cellObject)
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (gridArray[row, col] == cellObject)
                {
                    return new Vector2Int(col, row);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    public bool IsValidCellIndex(Vector2Int cellIndex)
    {
        return cellIndex.x >= 0 && cellIndex.x < columns && cellIndex.y >= 0 && cellIndex.y < rows;
    }

    private Vector2Int CalculateCardCellSize(GameObject card)
    {
        RectTransform cardRectTransform = card.GetComponent<RectTransform>();
        float cardWidth = cardRectTransform.rect.width;
        float cardHeight = cardRectTransform.rect.height;

        int cellWidthCount = Mathf.CeilToInt(cardWidth / cellWidth);
        int cellHeightCount = Mathf.CeilToInt(cardHeight / cellHeight);

        return new Vector2Int(cellWidthCount, cellHeightCount);
    }

    public RectTransform GetCellRectTransform(Vector2Int cellIndex)
    {
        if (IsValidCellIndex(cellIndex))
        {
            return gridArray[cellIndex.y, cellIndex.x].GetComponent<RectTransform>();
        }
        return null;
    }

    public int GetCellValue(Vector2Int cellIndex)
    {
        if (IsValidCellIndex(cellIndex))
        {
            GameObject cellObject = gridArray[cellIndex.y, cellIndex.x];
            Cell cellData = cellObject.GetComponent<Cell>();

            if (cellData != null)
            {
                return cellData.cellValue;
            }
        }
        return -1;
    }
}
