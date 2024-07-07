using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class GridManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winConditionText;

    public GameObject playerPrefab;
    public GameObject holePrefab;
    public GameObject gridPrefab;

    public int playerRows;
    public int playerCols;

    public int holeRows;
    public int holeCols;

    private GameObject[,] playerGrid /*= new GameObject[3,3]*/;
    private GameObject[,] holeGrid /*= new GameObject[3,2]*/;
    private GameObject[,] gridGrid /*= new GameObject[3, 2]*/;
    private List<GameObject> holdingGrid = new List<GameObject>();

    private Vector3[,] playergridPosition/* = new Vector3[3,3]*/;
    private Vector3[,] holegridPosition /*= new Vector3[3, 2]*/;
    private Vector3[] holdinggridPosition = new Vector3[6];

    private Dictionary<Color, int> playerCountByColor = new Dictionary<Color, int>();
    private Dictionary<Color, List<GameObject>> holesByColor = new Dictionary<Color, List<GameObject>>();

    private Dictionary<Color, List<GameObject>> playersInHole = new Dictionary<Color, List<GameObject>>();
    private Dictionary<GameObject, int> holePlayerCount = new Dictionary<GameObject, int>(); // New dictionary to track players in holes
    List<Color> usedColors = new List<Color>();
    List<GameObject> reachedPlayers = new List<GameObject>();
    [SerializeField] Color[] color = new Color[9];

    private int totalHoles;


    private void Start()
    {
        playerGrid = new GameObject[playerRows, playerCols];
        holeGrid = new GameObject[holeRows, holeCols];
        gridGrid = new GameObject[holeRows, holeCols];

        playergridPosition = new Vector3[playerRows, playerCols];
        holegridPosition = new Vector3[holeRows, holeCols];

        InitializeGrid();
        SpawnHole();
        SpawnPlayer();
        StartCoroutine(CheckHoldingGridForMatches());
    }

    private void Update()
    {
        if(totalHoles == 0)
        {
            UI_Manager.instance.SetPanel(true);
            winConditionText.text = "You Win!";
            Invoke("MoveToNextScene", 5f);
            
        }
        CheckLoseCondition();
    }

    private void MoveToNextScene()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene + 1 >= SceneManager.sceneCountInBuildSettings)
        {
            return;
        }
        else
        {
            SceneManager.LoadScene(currentScene + 1);
        }
    }


    private void InitializeGrid()
    {
        //For player prefabs
        for (int row = 0 ; row < playerRows; row++)
        {
            for(int col = 0 ; col < playerCols; col++)
            {
                playergridPosition[col,row] = new Vector3(col * 2 , 0, row * 2);
                GameObject grid = Instantiate(gridPrefab, playergridPosition[col,row], Quaternion.identity);
                grid.name = playergridPosition[col, row].ToString();
            }
        }


        //For Hole prefabs
        for (int row = 0; row < holeCols; row++)
        {
            for (int col = 0; col < holeRows; col++)
            {
                holegridPosition[col, row] = new Vector3(col * 2 + 12, 0, row * 2);
                GameObject grid = Instantiate(gridPrefab, holegridPosition[col,row], Quaternion.identity);
                grid.name = holegridPosition[col,row].ToString();

            }
        }

        //For holding player 
        for (int i = 0; i < 6; i++)
        {
            holdinggridPosition[i] = new Vector3(9, 0, i * 2);
            GameObject grid = Instantiate(gridPrefab, holdinggridPosition[i], Quaternion.identity);

            grid.name = holdinggridPosition[i].ToString();
        }
    }


    private void SpawnPlayer()
    {
        List<Color> availableColors = new List<Color>(usedColors);
        for (int row = 0; row < playerRows; row++)
        {
            for(int col = 0 ; col < playerCols; col++)
            {
                GameObject player = Instantiate(playerPrefab, playergridPosition[row, col], Quaternion.identity);
                player.transform.Rotate(0, 90, 0);
                playerGrid[row, col] = player;
                Color playerColor;
                if (availableColors.Count > 0)
                {
                    playerColor = availableColors[0];
                    player.GetComponent<Renderer>().material.color = playerColor;
                    player.GetComponentInChildren<SkinnedMeshRenderer>().material.color = playerColor;
                    availableColors.RemoveAt(0);

                   
                    if(!playerCountByColor.ContainsKey(playerColor))
                    {
                        playerCountByColor[playerColor] = 0;
                    }

                    playerCountByColor[playerColor]++;
                }

                else
                {
                    playerColor = GetRandomColor();
                    player.GetComponent<Renderer>().material.color = playerColor;
                    player.GetComponentInChildren<SkinnedMeshRenderer>().material.color = playerColor;
                    if (!playerCountByColor.ContainsKey(playerColor))
                    {
                        playerCountByColor[playerColor] = 0;
                    }
                    playerCountByColor[playerColor]++;
                }
            }
        }
    }

    private void SpawnHole()
    {
        int colorIndex = 0;
        List<Color> availableColors = new List<Color>(usedColors);
        for (int row = 0; row < holeRows; row++)
        {
            for (int col = 0; col < holeCols; col++)
            {
                if (colorIndex < color.Length)
                {
                    Color holeColor = color[colorIndex++];
                    usedColors.Add(holeColor);

                    GameObject hole = Instantiate(holePrefab, holegridPosition[row, col], Quaternion.identity);
                    holeGrid[row, col] = hole;
                    gridGrid[row, col] = hole;
                    hole.GetComponent<Renderer>().material.color = holeColor;

                    if (!playersInHole.ContainsKey(holeColor))
                    {
                        playersInHole[holeColor] = new List<GameObject>();
                    }
                    totalHoles++;
                }
            }
        }

    }

    private Color GetRandomColor()
    {
       
        return usedColors[(Random.RandomRange(0, usedColors.Count))];
    }

    public void OnPlayerClicked(GameObject player)
    {
        Vector2Int holePosition = FindMatchingHole(player);
        Color playerColor = player.GetComponent<Renderer>().material.color;
        if(player.GetComponent<Player>().playerC <= 3)
        {
            if (holePosition != Vector2Int.one * -1)  //found one matching hole.......
            {
                //Move player to matching hole...

                MovePlayerToHole(player, holePosition);

                //Check if the player has reached the same color hole......

                if (playerCountByColor[playerColor] == 0)
                {

                    //DestroyHole(holePosition.y, holePosition);
                    StartCoroutine(CheckAndDestroyHoles(playerColor));
                }
            }
            else
            {
                //Move player to holding grid.......
                MovePlayerToHoldingGrid(player);
            }
        }
    }

    private Vector2Int FindPlayerPosition(GameObject player)
    {
        for(int row = 0; row < playerRows; row++)
        {
            for(int col = 0; col < playerCols; col++)
            {
                if (playerGrid[row, col] == player)
                {
                    return new Vector2Int(row, col);
                }
            }
        }
        return Vector2Int.one * -1;
    }

    private Vector2Int FindMatchingHole(GameObject player)
    {
        Color playerColor = player.GetComponent<Renderer>().material.color;
        for (int col = 0; col < holeCols; col++)
        {
            if (holeGrid[0, col] != null)
            {
                Color holeColor = holeGrid[0, col].GetComponent<Renderer>().material.color;

                if (playerColor == holeColor)
                {
                    return new Vector2Int(0, col);
                }
            }
        }
        return Vector2Int.one * -1; 
    }

    private void MovePlayerToHole(GameObject player, Vector2Int holePosition)
    {
        Vector2Int playerPosition = FindPlayerPosition(player);
        player.GetComponent<Player>().DoRun(true);
        
        holeGrid[holePosition.x, holePosition.y] = player;

        //Move Player to matching hole position
       
        
        //Decrease playercount...........
        reachedPlayers.Add(player);
        
        Color playerColor = player.GetComponent<Renderer>().material.color;
        playerCountByColor[playerColor]--;

        playersInHole[playerColor].Add(player);
        StartCoroutine(MovePlayerSmoothly(player, holegridPosition[holePosition.x, holePosition.y], 3f));

        
    }
    private IEnumerator MovePlayerSmoothly(GameObject player, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = player.transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            player.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPosition;
        player.GetComponent<Player>().DoRun(false);
    }

    private void MovePlayerToHoldingGrid(GameObject player)
    {
        Vector2Int playerPosition = FindPlayerPosition(player);
        player.GetComponent<Player>().DoRun(true);
        playerGrid[playerPosition.x, playerPosition.y] = null;    

        // Check if there's a matching hole first


        holdingGrid.Add(player);

        // Move player to holding grid...
        StartCoroutine(MovePlayerSmoothly(player, holdinggridPosition[holdingGrid.Count - 1], 3f));
    }

    private void DestroyHole(int col, Vector2Int holePosition)
    {
        Color holeColor = holeGrid[holePosition.x, holePosition.y].GetComponent<Renderer>().material.color;
        Destroy(gridGrid[holePosition.x, holePosition.y]);
        gridGrid[holePosition.x, holePosition.y] = null;
        Destroy(holeGrid[holePosition.x, holePosition.y]);
        holeGrid[holePosition.x, holePosition.y] = null;

        foreach (GameObject go in playersInHole[holeColor])
        {
            Destroy(go);
        }
        Debug.Log(totalHoles);
        totalHoles--;
        playersInHole[holeColor].Clear();



        for (int row = holePosition.x; row < holeRows; row++)
        {
            if (holeGrid[row, col] == null)
            {

                holeGrid[row, col] = holeGrid[row + 1, col];
                gridGrid[row, col] = gridGrid[row + 1, col];
                holeGrid[row + 1, col] = null;
                gridGrid[row + 1, col] = null;

                if (holeGrid[row, col] != null)
                {
                    holeGrid[row, col].transform.position = holegridPosition[row, col];
                    gridGrid[row, col].transform.position = holegridPosition[row, col];
                }
            }
        }

        MoveHoldingGridPlayersToHole();
    }

    private IEnumerator CheckAndDestroyHoles(Color playerColor)
    {
        // Wait for all players to reach their positions
        yield return new WaitForSeconds(3f);

        bool allPlayersReached = true;
        foreach (GameObject player in playersInHole[playerColor])
        {
            if (player.transform.position != holegridPosition[0, 0] &&
                player.transform.position != holegridPosition[0, 1] && 
                player.transform.position != holegridPosition[0, 2] &&
                player.transform.position != holegridPosition[1, 0] &&
                player.transform.position != holegridPosition[1, 1] &&
                player.transform.position != holegridPosition[1, 2])
            {
                allPlayersReached = false;
                break;
            }
        }

        if (allPlayersReached && playerCountByColor[playerColor] == 0)
        {
            Vector2Int holePosition = FindMatchingHole(playersInHole[playerColor][0]);
            DestroyHole(holePosition.y, holePosition);
        }
    }

    private IEnumerator CheckHoldingGridForMatches()
    {
        while (true)
        {

            MoveHoldingGridPlayersToHole();
            yield return new WaitForSeconds(3f); // Check every 0.5 seconds, adjust as needed
            
        }
    }
    private void MoveHoldingGridPlayersToHole()
    {
        for(int i = holdingGrid.Count - 1; i >= 0; i--)
        {
            GameObject player = holdingGrid[i];
            Vector2Int playerPosition = FindPlayerPosition(player);
            Vector2Int holePosition = FindMatchingHole(player);

            if(holePosition != Vector2Int.one * -1)
            {
                MovePlayerToHole(player, holePosition);
                holdingGrid.RemoveAt(i);
               

                Color playerColor = player.GetComponent<Renderer>().material.color;

                if (playerCountByColor[playerColor] == 0)
                {
                    
                    StartCoroutine(CheckAndDestroyHoles(playerColor));
                }

                //shift remaining players in holding grid

                for (int j = i; j < holdingGrid.Count; j++)
                {
                    holdingGrid[j].transform.position = holdinggridPosition[j];
                }
            }
        }
    }

    private void CheckLoseCondition()
    {
        if (holdingGrid.Count == holdinggridPosition.Length && !CanAnyPlayerBeMovedToHole())
        {
            UI_Manager.instance.SetPanel(true);
            winConditionText.text = "You Lose!";
            
        }
    }

    private bool CanAnyPlayerBeMovedToHole()
    {
        foreach (GameObject player in holdingGrid)
        {
            if (FindMatchingHole(player) != Vector2Int.one * -1)
            {
                return true;
            }
        }
        return false;
    }

}
