using UnityEngine;

public class Grid : MonoBehaviour
{
    public int rows = 10;
    public int columns = 20;
    public float cellWidth = 50f;  // Width of each cell
    public float cellHeight = 50f; // Height of each cell
    public GameObject cellPrefab;
    private GameObject[,] gridArray;
    private GameObject[,] occupiedObjects; // Track the object in each cell

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

            if (IsValidCellIndex(cellIndex))
            {
                Debug.Log($"Clicked on cell {cellIndex}");

                // Example: Locking a cell
                LockCell(cellIndex);

                // Example: Updating values in a cell
                UpdateCellTopValue(cellIndex, 10);
                UpdateCellBottomValue(cellIndex, 5);
            }
            else
            {
                Debug.Log($"Clicked outside the grid.");
            }
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
                cellRectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);  // Set cell size here
                cellRectTransform.anchoredPosition = new Vector2((col * cellWidth) - xOffset, -(row * cellHeight) + yOffset);
                cell.name = $"Cell_{row}_{col}";
                gridArray[row, col] = cell;
                occupiedObjects[row, col] = null; // Initialize as not occupied
            }
        }
    }

    private void InitializeOccupiedObjectsArray()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                occupiedObjects[row, col] = null; // Initialize all cells as not occupied
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
            if (!IsCellLocked(cellIndex)) // Check if the cell is not already occupied
            {
                GameObject cell = gridArray[cellIndex.y, cellIndex.x];

                // Determine neighboring cells to snap to
                Vector2Int[] neighborIndices = new Vector2Int[1];
                neighborIndices[0] = cellIndex;

                if (snapToHorizontalNeighbors)
                {
                    // Snap to both left and right neighbors
                    neighborIndices = new Vector2Int[2];
                    neighborIndices[0] = cellIndex + new Vector2Int(-1, 0); // Left neighbor
                    neighborIndices[1] = cellIndex + new Vector2Int(1, 0);  // Right neighbor
                }

                // Find the nearest neighbor cell
                Vector2Int nearestCellIndex = FindNearestCellIndex(neighborIndices, localMousePosition);

                if (IsValidCellIndex(nearestCellIndex))
                {
                    RectTransform nearestCellRectTransform = GetCellRectTransform(nearestCellIndex);

                    // Snap the object to the center of the nearest cell
                    objRectTransform.SetParent(nearestCellRectTransform, false);
                    objRectTransform.anchoredPosition = Vector3.zero;

                    // Calculate the size of the object in grid cells
                    Vector2Int objCellSize = CalculateCardCellSize(obj);

                    // Mark the cells as occupied by this object
                    MarkCellsOccupied(nearestCellIndex, objCellSize, obj);

                    Debug.Log($"Snapped object to cell {nearestCellIndex}");

                    return true; // Successfully attached object to grid
                }
                else
                {
                    Debug.LogError("No valid neighboring cell found.");
                }
            }
            else
            {
                Debug.LogError($"Cell {cellIndex} is already occupied.");
            }
        }
        else
        {
            Debug.LogError("Invalid cell index");
        }

        return false; // Failed to attach object to grid
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
        return false; // Invalid cell index
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

    public void UpdateCellTopValue(Vector2Int cellIndex, int topValue)
    {
        if (IsValidCellIndex(cellIndex))
        {
            GameObject cellObject = gridArray[cellIndex.y, cellIndex.x];
            if (cellObject != null)
            {
                CardData cardData = cellObject.GetComponentInChildren<CardData>();
                if (cardData != null)
                {
                    cardData.topValue = topValue;
                    Debug.Log($"Updated top value of cell {cellIndex} to {topValue}");
                }
                else
                {
                    Debug.LogError($"No CardData component found in cell {cellIndex}");
                }
            }
            else
            {
                Debug.LogError($"No cell object found at {cellIndex}");
            }
        }
        else
        {
            Debug.LogError($"Invalid cell index: {cellIndex}");
        }
    }

    public void UpdateCellBottomValue(Vector2Int cellIndex, int bottomValue)
    {
        if (IsValidCellIndex(cellIndex))
        {
            GameObject cellObject = gridArray[cellIndex.y, cellIndex.x];
            if (cellObject != null)
            {
                CardData cardData = cellObject.GetComponentInChildren<CardData>();
                if (cardData != null)
                {
                    cardData.bottomValue = bottomValue;
                    Debug.Log($"Updated bottom value of cell {cellIndex} to {bottomValue}");
                }
                else
                {
                    Debug.LogError($"No CardData component found in cell {cellIndex}");
                }
            }
            else
            {
                Debug.LogError($"No cell object found at {cellIndex}");
            }
        }
        else
        {
            Debug.LogError($"Invalid cell index: {cellIndex}");
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
        cellCenterWorld += (Vector2)transform.position; // Adjust for grid's position if necessary

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

    public RectTransform GetCellRectTransform(Vector2Int cellIndex)
    {
        if (IsValidCellIndex(cellIndex))
        {
            GameObject cellObject = gridArray[cellIndex.y, cellIndex.x];
            return cellObject.GetComponent<RectTransform>();
        }
        return null;
    }

    public bool IsValidCellIndex(Vector2Int cellIndex)
    {
        return cellIndex.x >= 0 && cellIndex.x < columns && cellIndex.y >= 0 && cellIndex.y < rows;
    }

    public Vector2Int CalculateCardCellSize(GameObject cardObject)
    {
        RectTransform cardRectTransform = cardObject.GetComponent<RectTransform>();

        // Calculate the size of the card object in local units
        float cardWidth = cardRectTransform.rect.width;
        float cardHeight = cardRectTransform.rect.height;

        // Calculate number of cells needed based on the card size
        int numCellsWide = Mathf.CeilToInt(cardWidth / cellWidth);
        int numCellsHigh = Mathf.CeilToInt(cardHeight / cellHeight);

        return new Vector2Int(numCellsWide, numCellsHigh);
    }

    public int GetTopValueInCell(Vector2Int cellIndex)
    {
        GameObject cellObject = gridArray[cellIndex.y, cellIndex.x];
        if (cellObject != null)
        {
            CardData dominoPiece = cellObject.GetComponentInChildren<CardData>();
            if (dominoPiece != null)
            {
                return dominoPiece.topValue;
            }
            else
            {
                Debug.LogError($"No DominoPiece component found in cell {cellIndex}");
            }
        }
        else
        {
            Debug.LogError($"No cell object found at {cellIndex}");
        }
        return -1; // Return an invalid value or handle as needed
    }

    public int GetBottomValueInCell(Vector2Int cellIndex)
    {
        GameObject cellObject = gridArray[cellIndex.y, cellIndex.x];
        if (cellObject != null)
        {
            CardData dominoPiece = cellObject.GetComponentInChildren<CardData>();
            if (dominoPiece != null)
            {
                return dominoPiece.bottomValue;
            }
            else
            {
                Debug.LogError($"No DominoPiece component found in cell {cellIndex}");
            }
        }
        else
        {
            Debug.LogError($"No cell object found at {cellIndex}");
        }
        return -1; // Return an invalid value or handle as needed
    }

    public bool CanSnapHorizontally(Vector2Int cellIndex, int topValue, int bottomValue)
    {
        if (!IsValidCellIndex(cellIndex))
            return false;

        if (IsCellLocked(cellIndex))
            return false;

        // Check left neighbor
        if (cellIndex.x > 0)
        {
            Vector2Int leftIndex = new Vector2Int(cellIndex.x - 1, cellIndex.y);
            int leftTopValue = GetTopValueInCell(leftIndex);
            int leftBottomValue = GetBottomValueInCell(leftIndex);

            // Check if left neighbor matches the card's values or is uninitialized
            if (leftTopValue == topValue || leftTopValue == -1 || leftBottomValue == bottomValue || leftBottomValue == -1)
            {
                return true;
            }
        }

        // Check right neighbor
        if (cellIndex.x < columns - 1)
        {
            Vector2Int rightIndex = new Vector2Int(cellIndex.x + 1, cellIndex.y);
            int rightTopValue = GetTopValueInCell(rightIndex);
            int rightBottomValue = GetBottomValueInCell(rightIndex);

            // Check if right neighbor matches the card's values or is uninitialized
            if (rightTopValue == topValue || rightTopValue == -1 || rightBottomValue == bottomValue || rightBottomValue == -1)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanSnapVertically(Vector2Int cellIndex, int topValue, int bottomValue)
    {
        if (!IsValidCellIndex(cellIndex))
            return false;

        if (IsCellLocked(cellIndex))
            return false;

        // Check bottom neighbor
        if (cellIndex.y > 0)
        {
            Vector2Int bottomIndex = new Vector2Int(cellIndex.x, cellIndex.y - 1);
            int bottomTopValue = GetTopValueInCell(bottomIndex);
            int bottomBottomValue = GetBottomValueInCell(bottomIndex);

            // Check if bottom neighbor matches the card's values or is uninitialized
            if (bottomTopValue == topValue || bottomTopValue == -1 || bottomBottomValue == bottomValue || bottomBottomValue == -1)
            {
                return true;
            }
        }

        // Check top neighbor
        if (cellIndex.y < rows - 1)
        {
            Vector2Int topIndex = new Vector2Int(cellIndex.x, cellIndex.y + 1);
            int topTopValue = GetTopValueInCell(topIndex);
            int topBottomValue = GetBottomValueInCell(topIndex);

            // Check if top neighbor matches the card's values or is uninitialized
            if (topTopValue == topValue || topTopValue == -1 || topBottomValue == bottomValue || topBottomValue == -1)
            {
                return true;
            }
        }

        return false;
    }




    private bool leftRightValuesMatch(int leftTop, int leftBottom, int rightTop, int rightBottom)
    {
        return (leftTop == rightTop || leftBottom == rightBottom);
    }

    private bool topBottomValuesMatch(int topTop, int topBottom, int bottomTop, int bottomBottom)
    {
        return (topTop == bottomTop || topBottom == bottomBottom);
    }
}
