// Authored by: Finn Davis
// Added to by: Harley Clark
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class MenuUI : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject instructions;
    [SerializeField] private GameObject options;
    [SerializeField] private GameObject characterSelect;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject pause;
    [SerializeField] private GameObject winScreen;

    [Header("SelectedButtons")]
    [SerializeField] private GameObject MainSelectedButton;
    [SerializeField] private GameObject instructionsSelectedButton;
    [SerializeField] private GameObject PauseSelectedButton;
    [SerializeField] private GameObject OptionsSelectedButton;
    [SerializeField] private GameObject WinScreenSelectedButton;

    [Header("Instruction Menu Extras")]
    [SerializeField] private List<GameObject> InstructionPages;
    [SerializeField] private Image InstructionsBack;
    [SerializeField] private Image InstructionsNext;

    [Header("Volume Sliders")]
    [SerializeField] private Slider MasterVolSlider;
    [SerializeField] private Slider MusicVolSlider;
    [SerializeField] private Slider EffectVolSlider;

    [SerializeField] private TMP_Text masterVolText;
    [SerializeField] private TMP_Text musicVolText;
    [SerializeField] private TMP_Text effectVolText;

    [SerializeField] private Material masterSliderMaterial;
    [SerializeField] private Material musicSliderMaterial;
    [SerializeField] private Material effectSliderMaterial;

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;    

    public InputActionAsset inputAsset;
    InputActionMap uiActionMap;

    PlayerManager pm;
    GameManager gm;

    private int lastPage, currentPage;

    private void OnEnable()
    {
        uiActionMap.FindAction("Cancel").started += DoBack;
    }
    private void OnDisable()
    {
        uiActionMap.FindAction("Cancel").started -= DoBack;
    }
    private void Awake()
    {
        uiActionMap = inputAsset.FindActionMap("UI");
        masterSliderMaterial.SetFloat("_Value", MasterVolSlider.value);
        musicSliderMaterial.SetFloat("_Value", MusicVolSlider.value);
        effectSliderMaterial.SetFloat("_Value", EffectVolSlider.value);

        masterVolText.text = ((int)(MasterVolSlider.value * 100)).ToString();
        musicVolText.text = ((int)(MusicVolSlider.value * MasterVolSlider.value * 100)).ToString();
        effectVolText.text = ((int)(EffectVolSlider.value * MasterVolSlider.value * 100)).ToString();

        pm = PlayerManager.Instance;
        gm = GameManager.Instance;

        GameManager.Instance.OnGameStateChanged += GameManagerOnGameStateChanged;
    }
    private void Start()
    {
        AudioManager.Instance.PlayMenuMusic(true);
        UpdateState(MenuState.Main);
    }
    private void GameManagerOnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.TitleScreen:
                {
                    UpdateState(MenuState.Main);
                }
                break;
            case GameManager.GameState.GameState:
                {                    
                    if(GameManager.Instance.lastState == GameManager.GameState.Paused || GameManager.Instance.lastState == GameManager.GameState.EndGame)
                    {
                        Pause_ResumeGame();
                    }
                    else if (GameManager.Instance.lastState == GameManager.GameState.TitleScreen)
                    {
                        CharacterSelect_StartGame();
                    }
                }
                break;
            case GameManager.GameState.Paused:
                {
                    Pause_PauseGame();
                }
                break;
            case GameManager.GameState.EndGame:
                {
                    AudioManager.Instance.PlaySound("End Game Cheer");
                    WinScreen_SetCurrent();
                }
                break;
        }
    }
    MenuState state;
    enum MenuState
    {
        Main,
        Options,
        Instructions,
        CharacterSelection,
        InGame,
        Paused,
        WinScreen
    }
    private void DoBack(InputAction.CallbackContext obj)
    { 
        if (state == MenuState.Options)
        {
            ButtonBackSound();
            Options_Back();
        }
        else if (state == MenuState.Instructions)
        {
            ButtonBackSound();
            Instructions_Menu();
        }
        else if (state == MenuState.CharacterSelection)
        {
            // UpdateState(MenuState.Main);
        }
        else if (state == MenuState.Paused)
        {
            ButtonBackSound();
            Pause_ResumeGame();
        }
    }
    private void UpdateState(MenuState newState)
    {
        menu.SetActive(newState == MenuState.Main);
        options.SetActive(newState == MenuState.Options);
        instructions.SetActive(newState == MenuState.Instructions);
        characterSelect.SetActive(newState == MenuState.CharacterSelection);
        hud.SetActive(newState == MenuState.InGame);
        pause.SetActive(newState == MenuState.Paused);
        winScreen.SetActive(newState == MenuState.WinScreen);

        if (newState == state) return;
        state = newState;
    }

    public void ButtonSound() 
    {
      AudioManager.Instance.PlaySound("Button Select");
    }

    public void ButtonBackSound()
    {
        AudioManager.Instance.PlaySound("Button SelectBack");
    }
    #region Pause
    private void Pause_PauseGame()
    {
        PlayerManager.Instance.EnableInGameInputs(false);
        UpdateState(MenuState.Paused);
        EventSystem.current.SetSelectedGameObject(PauseSelectedButton);
    }
    public void Pause_ResumeGame()
    {
        PlayerManager.Instance.EnableInGameInputs(true);
        UpdateState(MenuState.InGame);
        gm.UpdateGameState(GameManager.GameState.GameState);
    }
    public void Pause_Options()
    {
        UpdateState(MenuState.Options);
        EventSystem.current.SetSelectedGameObject(OptionsSelectedButton);
    }
    public void Pause_Main()
    {
        EventSystem.current.SetSelectedGameObject(MainSelectedButton);
        gm.UpdateGameState(GameManager.GameState.TitleScreen);
    }
    #endregion
    #region Menu

    public void Menu_StartGame_CharacterSelect()
    {
        UpdateState(MenuState.CharacterSelection);
        pm.playerInputManager.EnableJoining();
    }
    public void Menu_Instructions()
    {
        InstructionsNext.color = PlayerManager.Instance.colours[currentPage].main;
        InstructionsBack.color = PlayerManager.Instance.colours[currentPage].main;
        UpdateState(MenuState.Instructions);
        EventSystem.current.SetSelectedGameObject(instructionsSelectedButton);
    }
    public void Menu_Options()
    {
        UpdateState(MenuState.Options);
        EventSystem.current.SetSelectedGameObject(OptionsSelectedButton);
    }
    #endregion
    public void CharacterSelect_StartGame()
    {
        UpdateState(MenuState.InGame);
        AudioManager.Instance.PlayGameMusic(true);
        AudioManager.Instance.PlayMenuMusic(false);
    }
    #region Options
    public void SelectSlider()
    {
        Slider selectedSlider = EventSystem.current.currentSelectedGameObject.GetComponent<Slider>();
        if (selectedSlider == MasterVolSlider) masterSliderMaterial.SetFloat("_Selected", 1);
        else masterSliderMaterial.SetFloat("_Selected", 0);
        if (selectedSlider == MusicVolSlider) musicSliderMaterial.SetFloat("_Selected", 1);
        else musicSliderMaterial.SetFloat("_Selected", 0);
        if (selectedSlider == EffectVolSlider) effectSliderMaterial.SetFloat("_Selected", 1);
        else effectSliderMaterial.SetFloat("_Selected", 0);
    }
    
    public void Options_OnVolumeChange()
    {
        masterSliderMaterial.SetFloat("_Value", MasterVolSlider.value);
        musicSliderMaterial.SetFloat("_Value", MusicVolSlider.value);
        effectSliderMaterial.SetFloat("_Value", EffectVolSlider.value);

        masterVolText.text = ((int)(MasterVolSlider.value * 100)).ToString();
        musicVolText.text = ((int)(MusicVolSlider.value * MasterVolSlider.value * 100)).ToString();
        effectVolText.text = ((int)(EffectVolSlider.value * MasterVolSlider.value * 100)).ToString();

        AudioManager.Instance.menuSource.volume = MusicVolSlider.value * MasterVolSlider.value;
        AudioManager.Instance.gameSource.volume = MusicVolSlider.value * MasterVolSlider.value;        
        AudioManager.Instance.evffectsVolume = EffectVolSlider.value * MasterVolSlider.value;
    }
    public void Options_Back()
    {
        if (gm.State == GameManager.GameState.TitleScreen)
        {
            UpdateState(MenuState.Main);
            EventSystem.current.SetSelectedGameObject(MainSelectedButton);
        }
        else
        {
            UpdateState(MenuState.Paused);
            EventSystem.current.SetSelectedGameObject(PauseSelectedButton);
        }
    }
    #endregion
    #region Instructions
    public void Instructions_Next()
    {
        if (currentPage + 1 == InstructionPages.Count)
        {
            Instructions_Menu();
            return;
        }
        lastPage = currentPage;
        currentPage++;
        LoadPage();
    }
    public void Instructions_Back()
    {
        if (currentPage == 0)
        {
            Instructions_Menu();
            return;
        }
        lastPage = currentPage;
        currentPage--;
        LoadPage();
    }
    private void LoadPage()
    {
        InstructionsNext.color = PlayerManager.Instance.colours[currentPage].main;
        InstructionsBack.color = PlayerManager.Instance.colours[currentPage].main;
        InstructionPages[lastPage].SetActive(false);
        InstructionPages[currentPage].SetActive(true);
    }
    public void Instructions_Menu()
    {
        InstructionPages[currentPage].SetActive(false);
        InstructionPages[0].SetActive(true);
        currentPage = 0;
        UpdateState(MenuState.Main);
        EventSystem.current.SetSelectedGameObject(MainSelectedButton);
    }
    #endregion
    #region Win Screen
    public void WinScreen_SetCurrent()
    {
        UpdateState(MenuState.WinScreen);
        EventSystem.current.SetSelectedGameObject(WinScreenSelectedButton);
    }
    public void WinScreen_Main()
    {
        AudioManager.Instance.PlayGameMusic(false);
        AudioManager.Instance.PlayMenuMusic(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void WinScreen_PlayAgain()
    {
        //UpdateState(MenuState.InGame);
        gm.UpdateGameState(GameManager.GameState.GameState);
        gm.StartGame();
        pm.ResetGame();
    }
    #endregion

    public void QuitAplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
