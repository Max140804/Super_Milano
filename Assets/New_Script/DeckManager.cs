using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    private Grid grid;
    private DominoGameManager gManager;

    private int[,] board;
    private List<DominoCardTest> hand;

    public GameObject dominoTilePrefab; // Reference to the Domino Tile Prefab

    private void Awake()
    {
        grid = GetComponentInChildren<Grid>();
        gManager = GetComponent<DominoGameManager>();
    }

    void Start()
    {
        // Initialize a sample board and a hand of dominoes
        board = new int[10, 10]; // A 10x10 board for simplicity
        hand = new List<DominoCardTest>
        {
            new DominoCardTest(3, 5),
            new DominoCardTest(5, 6),
            new DominoCardTest(6, 1),
            new DominoCardTest(1, 1)
        };

        // Example of placing a domino on the board
        if (PlaceDomino(board, new DominoCardTest(3, 5), 5, 5))
            Debug.Log("Domino placed successfully!");
        else
            Debug.Log("Invalid placement.");

        // Print the board to console
        PrintBoard(board);

        // Try to place another domino
        if (PlaceDomino(board, new DominoCardTest(5, 6), 5, 6))
            Debug.Log("Domino placed successfully!");
        else
            Debug.Log("Invalid placement.");

        // Print the board to console
        PrintBoard(board);
    }

    void Update()
    {
        // Handle user inputs and other game logic here
    }

    public void PrintBoard(int[,] board)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            string row = "";
            for (int j = 0; j < board.GetLength(1); j++)
            {
                row += board[i, j] + " ";
            }
            Debug.Log(row);
        }
    }

    public bool PlaceDomino(int[,] board, DominoCardTest domino, int x, int y)
    {
        if (IsValidPlacement(board, domino, x, y))
        {
            board[x, y] = domino.Side1;
            board[x, y + 1] = domino.Side2;

            // Instantiate and place the visual domino tile
            PlaceDominoTile(domino, x, y);

            return true;
        }
        return false;
    }

    public bool IsValidPlacement(int[,] board, DominoCardTest domino, int x, int y)
    {
        if (x < 0 || x >= board.GetLength(0) || y < 0 || y + 1 >= board.GetLength(1))
            return false;

        if (board[x, y] == 0 && board[x, y + 1] == 0)
        {
            if ((y > 0 && board[x, y - 1] == domino.Side1) ||
                (y + 2 < board.GetLength(1) && board[x, y + 2] == domino.Side2) ||
                (x > 0 && board[x - 1, y] == domino.Side1 && board[x - 1, y + 1] == domino.Side2) ||
                (x + 1 < board.GetLength(0) && board[x + 1, y] == domino.Side1 && board[x + 1, y + 1] == domino.Side2))
            {
                return true;
            }
        }

        return false;
    }

    private void PlaceDominoTile(DominoCardTest domino, int x, int y)
    {
        // Instantiate the first half of the domino
        GameObject tile1 = Instantiate(dominoTilePrefab, new Vector3(x, y, 0), Quaternion.identity);
        tile1.GetComponent<DominoTile>().SetValue(domino.Side1);

        // Instantiate the second half of the domino
        GameObject tile2 = Instantiate(dominoTilePrefab, new Vector3(x, y + 1, 0), Quaternion.identity);
        tile2.GetComponent<DominoTile>().SetValue(domino.Side2);
    }
}

public class DominoCardTest
{
    public int Side1 { get; set; }
    public int Side2 { get; set; }

    public DominoCardTest(int side1, int side2)
    {
        Side1 = side1;
        Side2 = side2;
    }

    public void Rotate()
    {
        int temp = Side1;
        Side1 = Side2;
        Side2 = temp;
    }
}
