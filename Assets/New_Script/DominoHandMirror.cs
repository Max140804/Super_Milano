using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class DominoHandMirror : MonoBehaviourPun
{
    private Dictionary<int, List<GameObject>> playerHands = new Dictionary<int, List<GameObject>>();
    private static DominoHandMirror instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static DominoHandMirror Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("DominoHandMirror");
                instance = obj.AddComponent<DominoHandMirror>();
            }
            return instance;
        }
    }

    public void AddToHand(int playerID, GameObject domino)
    {
        if (!playerHands.ContainsKey(playerID))
        {
            playerHands[playerID] = new List<GameObject>();
        }
        playerHands[playerID].Add(domino);
        int dominoViewID = domino.GetComponent<PhotonView>().ViewID;
        Debug.Log($"Adding domino with ViewID {dominoViewID} to player {playerID}'s hand.");
        domino.GetComponent<PhotonView>().RPC("RPC_AddToHand", RpcTarget.Others, playerID, dominoViewID);
    }

    [PunRPC]
    public void RPC_AddToHand(int playerID, int dominoViewID)
    {
        GameObject domino = PhotonView.Find(dominoViewID).gameObject;
        if (!playerHands.ContainsKey(playerID))
        {
            playerHands[playerID] = new List<GameObject>();
        }

        playerHands[playerID].Add(domino);
        domino.transform.SetParent(transform, false);
        domino.transform.localPosition = Vector3.zero;
        domino.transform.localRotation = Quaternion.identity;
        domino.transform.localScale = Vector3.one;

        Debug.Log($"RPC_AddToHand: Domino with ViewID {dominoViewID} added to player {playerID}'s hand.");
    }

    public bool RemoveFromHand(int playerID, GameObject domino)
    {
        if (playerHands.ContainsKey(playerID) && playerHands[playerID].Contains(domino))
        {
            playerHands[playerID].Remove(domino);
            domino.transform.SetParent(null);

            if (photonView.IsMine)
            {
                photonView.RPC("RPC_RemoveFromHand", RpcTarget.Others, playerID, domino.GetComponent<PhotonView>().ViewID);
            }

            Debug.Log($"Removed domino with ViewID {domino.GetComponent<PhotonView>().ViewID} from player {playerID}'s hand.");
            return true;
        }
        return false;
    }

    [PunRPC]
    public void RPC_RemoveFromHand(int playerID, int dominoViewID)
    {
        GameObject domino = PhotonView.Find(dominoViewID).gameObject;
        if (playerHands.ContainsKey(playerID) && playerHands[playerID].Contains(domino))
        {
            playerHands[playerID].Remove(domino);
            domino.transform.SetParent(null);

            Debug.Log($"RPC_RemoveFromHand: Domino with ViewID {dominoViewID} removed from player {playerID}'s hand.");
        }
    }

    public void UpdateCardPosition(int dominoViewID, Vector3 position, Quaternion rotation, Vector2Int cellIndex)
    {
        Debug.Log($"UpdateCardPosition: Updating position for domino with ViewID {dominoViewID} to {position} at cell {cellIndex}.");
        photonView.RPC("RPC_UpdateCardPosition", RpcTarget.All, dominoViewID, position, rotation, cellIndex);
    }

    [PunRPC]
    public void RPC_UpdateCardPosition(int dominoViewID, Vector3 position, Quaternion rotation, Vector2Int cellIndex)
    {
        GameObject domino = PhotonView.Find(dominoViewID).gameObject;
        domino.transform.position = position;
        domino.transform.rotation = rotation;

        Debug.Log($"RPC_UpdateCardPosition: Domino with ViewID {dominoViewID} moved to position {position} at cell {cellIndex}.");

        // Assuming you have a Grid instance to handle the cell snapping
        Grid grid = FindObjectOfType<Grid>();
        if (grid != null)
        {
            RectTransform cellTransform = grid.GetCellRectTransform(cellIndex);
            if (cellTransform != null)
            {
                domino.transform.SetParent(cellTransform, false);
                domino.transform.localPosition = Vector3.zero;
                Debug.Log($"RPC_UpdateCardPosition: Domino with ViewID {dominoViewID} snapped to cell {cellIndex}.");
            }
            else
            {
                Debug.LogError($"RPC_UpdateCardPosition: No cell found for index {cellIndex}.");
            }
        }
        else
        {
            Debug.LogError("RPC_UpdateCardPosition: Grid instance not found.");
        }
    }

    public List<GameObject> GetHand(int playerID)
    {
        if (playerHands.ContainsKey(playerID))
        {
            return new List<GameObject>(playerHands[playerID]);
        }
        return new List<GameObject>();
    }
}
