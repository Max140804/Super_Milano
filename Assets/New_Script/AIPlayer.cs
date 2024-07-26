using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    private DominoGameManager gameManager;
    private DominoHand dominoHand;

    void Start()
    {
        gameManager = FindObjectOfType<DominoGameManager>();
        dominoHand = GetComponent<DominoHand>();
    }

    public void TakeTurn()
    {
        // Implement AI logic for taking a turn
        // For example, AI decides which domino to play
        Debug.Log($"{gameObject.name} is taking its turn.");

        // Example: Play a random domino from its hand
        if (dominoHand.GetHandCount() > 0)
        {
            //GameObject dominoToPlay = dominoHand.GetRandomDomino();
            //if (dominoToPlay != null)
            //{
                //dominoHand.PlayDomino(dominoToPlay);
            //}
        }

        // Notify the game manager that the turn is over
        //gameManager.HandleAITurn(); // Or use another mechanism to notify the game manager
    }
}
