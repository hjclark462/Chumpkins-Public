// Authored by: Harley Clark
// Added to by: Finn Davis
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using UnityEngine.VFX;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _instance;
    public List<PlayerConfiguration> playerConfigs = new List<PlayerConfiguration>();
    public CameraControl m_camera;
    [HideInInspector]
    public PlayerInputManager playerInputManager;
    private int minPlayers = 2;
    private int gameOverLimit;
    public List<DummyTarget> dummyTargets = new List<DummyTarget>();
    public List<GameObject> headpieces = new List<GameObject>();
    public List<Material> bodyMaterials = new List<Material>();
    public List<ColourSwatch> colours = new List<ColourSwatch>();
    [HideInInspector]
    public List<Player> deathOrder = new List<Player>();
    public List<Transform> winPodium = new List<Transform>();
    [SerializeField]
    private Winner winner;
    public List<Transform> profilePoints = new List<Transform>();
    public List<Material> headUIMaterials = new List<Material>();
    public List<Material> UIMaterials = new List<Material>();
    public List<GameObject> joinBGS = new List<GameObject>();
    public List<LeaderBoard> leaderBoards = new List<LeaderBoard>();
    public List<TMP_FontAsset> fonts = new List<TMP_FontAsset>();
    public List<Material> decalMaterials = new List<Material>();
    public GridLayoutGroup HUD;
    public Image finish;
    public VisualEffect confetti;
    // Singleton
    public static PlayerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerManager>();
                if (_instance == null)
                {
                    Debug.LogError("Player Manager is Null!!!");
                }
            }
            return _instance;
        }
    }
    private void Start()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        playerInputManager.DisableJoining();
    }
    // Spawns the character model to get a profile pic and disables the input while generating a
    // PlayerConfiguration to store appearance and choices
    public void HandlePlayerJoin(PlayerInput pi)
    {
        AudioManager.Instance.PlaySound("Button Select");    
        if (playerConfigs.FindIndex(p => p.playerIndex == pi.playerIndex) == -1)
        {
            pi.transform.position = profilePoints[pi.playerIndex].position;
            pi.transform.rotation = profilePoints[pi.playerIndex].rotation;
            playerConfigs.Add(new PlayerConfiguration(pi));
            SetPlayerAppearance(pi.playerIndex, pi.playerIndex);
            pi.actions.FindActionMap("Player").Disable();
        }
    }
    // Changes the colour of the characters based on device order and applies the user chosen head.
    public void SetPlayerAppearance(int pi, int headIndex)
    {
        playerConfigs[pi].head = headpieces[headIndex];
        playerConfigs[pi].playerMaterial = bodyMaterials[pi];
        playerConfigs[pi].input.GetComponent<Player>().bodyMesh.material = playerConfigs[pi].playerMaterial;
        if (playerConfigs[pi].input.GetComponent<Player>().head.transform.childCount > 0)
        {
            for (int i = 0; i < playerConfigs[pi].input.GetComponent<Player>().head.transform.childCount; i++)
                Destroy(playerConfigs[pi].input.GetComponent<Player>().head.transform.GetChild(i).gameObject);
        }
        Instantiate(playerConfigs[pi].head, playerConfigs[pi].input.GetComponent<Player>().head.transform);
    }
    // Starts the game if all players are ready.
    public void ReadyPlayer(int pi, bool check)
    {
        playerConfigs[pi].isReady = check;
        if (playerConfigs.Count >= minPlayers && playerConfigs.TrueForAll(p => p.isReady == true)) SetupGame();
    }
    private void SetupGame()
    {
        GameManager.Instance.StartGame();        
        playerInputManager.DisableJoining();
        List<Transform> playerOrientationTfs = new List<Transform>();
        // Moves each players to the correct spawn points to start the game
        // Assigns the dummy target that will follow it for the camera and the cutout so that the fences 
        // won't block the view of the players. Sets the HealthUI HUD into the correct layout for player count
        foreach (PlayerConfiguration pc in playerConfigs)
        {
            int playerIndex = pc.playerIndex;

            Player player = pc.input.GetComponent<Player>();
            playerOrientationTfs.Add(player.orientation);
            player.SetDecalColor(decalMaterials[playerIndex]);

            Transform spawnPos = GameManager.Instance.traps.GetComponentInParent<TrapLayout>().spawnPoints[playerIndex];
            player.hipRB.transform.position = spawnPos.position;
            player.orientation.rotation = spawnPos.rotation;
            player.hipJoint.targetRotation = Quaternion.LookRotation(Vector3.Reflect(player.orientation.forward, Vector3.right));
            player.targetable = true;

            DummyTarget dummyTarget = dummyTargets[playerIndex];
            dummyTarget.gameObject.SetActive(true);
            dummyTarget.target = player;
            dummyTarget.gameObject.transform.position = spawnPos.position;
            m_camera.AddTarget(dummyTarget.transform);
        }

        m_camera.GetComponent<CutoutObject>().SetPlayers(playerOrientationTfs);
        GameManager.Instance.UpdateGameState(GameManager.GameState.GameState);
        if (playerConfigs.Count == 2)
        {
            HUD.cellSize = new Vector2(50, 200);
            HUD.childAlignment = TextAnchor.UpperCenter;
        }
    }

    public void EnableInGameInputs(bool _bool)
    {
        foreach (PlayerConfiguration player in playerConfigs)
        {
            if (_bool) player.input.actions.FindActionMap("Player").Enable();
            else player.input.actions.FindActionMap("Player").Disable();
        }
    }

    public void GameOverCheck(Player player, int index)
    {
        deathOrder.Insert(0, player);        
        gameOverLimit++;
        if (gameOverLimit == playerConfigs.Count - 1)
        {
            gameOverLimit = 0;
            StartCoroutine(GameOver(index));
        }
        else
        {
            m_camera.RemoveTarget(dummyTargets[index].transform);
        }
    }

    // Starts the transitions to the wins screen by stopping damage to players, slowing down time to show the winner and 
    // Setting the correct apperance to the animated winner on the podium. Correctly lines up the leaderboard for the round
    IEnumerator GameOver(int secondLast)
    {
        foreach (PlayerConfiguration pi in playerConfigs)
        {
            Player player = pi.input.GetComponent<Player>();            
            if (player.currentLives > 0)
            {
                player.invincibool = true;
                FindObjectOfType<InputSystemUIInputModule>().actionsAsset = player.inputActionAsset;
                dummyTargets[secondLast].target = player; 
            }
        }
        Time.timeScale = 0.1f;
        finish.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        finish.gameObject.SetActive(false);
        GameManager.Instance.GameOver();
        int ind = 0;
        foreach (PlayerConfiguration pi in playerConfigs)
        {
            Player player = pi.input.GetComponent<Player>();
            pi.input.actions.FindActionMap("Player").Disable();
            player.damagedEffect.Stop();
            leaderBoards[ind].gameObject.SetActive(true);
            ind++;
            if (player.currentLives > 0)
            {
                leaderBoards[0].SetWinner(pi.playerIndex);
                winner.gameObject.SetActive(true);
                winner.SetWinnerAppearance(pi.head, pi.playerMaterial, player.currentHealth);              
                confetti.gameObject.SetActive(true);
            }
        }

        ind = 1;
        for (int i = 0; i < deathOrder.Count; i++)
        {
            deathOrder[i].onFire = false;
            leaderBoards[i+1].SetWinner(deathOrder[i].playerInput.playerIndex);

            ind++;
        }
    }
    // For resetting the game with the current configuration of players
    public void ResetGame()
    {
        winner.gameObject.SetActive(false);
        confetti.gameObject.SetActive(false);
        deathOrder.Clear();
        m_camera.targetList.Clear();
        EnableInGameInputs(false);        
        for (int i = 0; i < playerConfigs.Count; i++)
        {
            Player player = playerConfigs[i].input.GetComponent<Player>();
            player.targetable = true;
            dummyTargets[i].target = player;
            m_camera.AddTarget(dummyTargets[i].transform);
            player.currentHealth = player.startingHealth;
            player.currentLives = player.startingLives;
            player.invincibool = false;            
            dummyTargets[i].transform.position = GameManager.Instance.traps.GetComponentInParent<TrapLayout>().spawnPoints[i].position;
            player.decaObj.SetActive(true);
            player.SetAnimated();
            player.SetPosAndRot(GameManager.Instance.traps.GetComponentInParent<TrapLayout>().spawnPoints[i]);
        }
    }
}
// Embedded class that is used to handle choices and players.
public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        playerIndex = pi.playerIndex;
        input = pi;
    }
    public PlayerInput input { get; set; }
    public int playerIndex { get; set; }
    public bool isReady { get; set; }
    public Material playerMaterial { get; set; }
    public GameObject head { get; set; }    
}