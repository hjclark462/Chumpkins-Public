// Authored by: Harley Clark
// Added to by: Finn Davis
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public GameState lastState;
    public GameState State;
    public event Action<GameState> OnGameStateChanged;
    public Camera camTitle;
    public Camera camGame;
    public Camera camEnd;
    public List<GameObject> trapLayouts;
    [HideInInspector]
    public GameObject traps;
    public List<GameObject> trapWaves;
    public float gameDelay;
    public float waveTime;
    public float warningTime;
    private int currentWave;
    public List<Animator> fences;
    public Image countdown;    
    public List<Camera> cams;
    TrapLayout trapLayout;
    public Material DefaultUI;
    public Light winLight;
    // Singleton
    public static GameManager Instance
    {
        get
        {
            _instance = FindObjectOfType<GameManager>();
            if (_instance == null)
            {
                Debug.LogError("Player Manager is Null!!!");
            }
            return _instance;
        }
    }
    private void Awake()
    {
        OnGameStateChanged += GameStateChanged;
    }
    private void Start()
    {
        UpdateGameState(GameState.TitleScreen);
        Time.timeScale = 0;
        camTitle.enabled = true;
        camGame.enabled = false;
        camEnd.enabled = false;
    }
    private void GameStateChanged(GameState state)
    {
        if (state == GameState.TitleScreen)
        {
            AudioManager.Instance.PlayMenuMusic(true);
            camTitle.enabled = true;
            camGame.enabled = false;
            camEnd.enabled = false;
            countdown.gameObject.SetActive(false);
        }
        else if (state == GameState.GameState || state == GameState.Paused)
        {
            camTitle.enabled = false;
            camGame.enabled = true;
            camEnd.enabled = false;
        }
        else
        {
            camTitle.enabled = false;
            camGame.enabled = false;
            camEnd.enabled = true;
            countdown.gameObject.SetActive(false);
        }
        if (state == GameState.GameState || state == GameState.EndGame)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0;
        }
        if (state == GameState.Paused)
        {
            countdown.enabled = false;
        }
        else
        {
            countdown.enabled = true;
        }
    }
    public void UpdateGameState(GameState newState)
    {
        if (newState == State)
            return;
        lastState = State;
        State = newState;
        switch (newState)
        {
            case GameState.TitleScreen:
                break;
            case GameState.GameState:
                break;
            case GameState.Paused:
                break;
            case GameState.EndGame:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        OnGameStateChanged?.Invoke(newState);       
    }
    public void GameOver()
    {        
        StopAllCoroutines();
        trapWaves.Clear();
        currentWave = 0;
        foreach (Animator fence in fences)
        {
            fence.SetTrigger("fenceDown");
        }
        Destroy(traps);
        UpdateGameState(GameState.EndGame);
        winLight.gameObject.SetActive(true); 
    }

    public void StartGame()
    {
        winLight.gameObject.SetActive(false);
        // Turns of the cameras that handled the render images of teh profiles of the characters
        foreach (Camera cam in cams)
        {
            cam.enabled = false;
        }
        int layout = Random.Range(0, trapLayouts.Count);
        traps = Instantiate(trapLayouts[layout], new Vector3(0.5f,0), Quaternion.identity);
        trapLayout = traps.GetComponentInParent<TrapLayout>();
        trapWaves = trapLayout.trapWaves;
        StartCoroutine(ConstrictArea());
        foreach (Animator fence in fences)
        {
            fence.SetTrigger("fenceUp");
        }       
        StartCoroutine(ResetMaterial());
    }

    IEnumerator ResetMaterial()
    {
        yield return new WaitForSeconds(0.2f);
        DefaultUI.color = Color.white;
    }

  

    IEnumerator ConstrictArea()
    {
        countdown.gameObject.SetActive(true);        
        StartCoroutine(StopCountImage());

        float seconds = 0;
        while (currentWave < trapWaves.Count && State != GameState.EndGame)
        {
            if (currentWave == 0) seconds = gameDelay;
            else seconds = waveTime;

            yield return new WaitForSeconds(seconds - warningTime);
            if(State == GameState.EndGame) break;
            trapLayout.EnableWaveWarning(currentWave);
            countdown.gameObject.SetActive(true);

            yield return new WaitForSeconds(warningTime);
            if (State == GameState.EndGame) break;
            if (currentWave != 0) countdown.gameObject.SetActive(false);
            trapLayout.DisableWaveWarning(currentWave);
            trapWaves[currentWave].SetActive(true);
            currentWave++;
        }
    }
    IEnumerator StopCountImage()
    {
        yield return new WaitForSeconds(gameDelay+0.85f);
        AudioManager.Instance.PlaySound("Starting Bell");
        countdown.gameObject.SetActive(false);
        PlayerManager.Instance.EnableInGameInputs(true);
    }
    public enum GameState
    {
        TitleScreen,
        GameState,
        Paused,
        EndGame
    }
}