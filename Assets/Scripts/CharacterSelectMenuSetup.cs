// Authored by: Harley Clark
// Added to by: Finn Davis
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

public class CharacterSelectMenuSetup : MonoBehaviour
{
    [SerializeField] GameObject characterSelect;
    [SerializeField] PlayerInput input;
    [SerializeField] GameObject hud;

    CharacterSelectModule characterSelectModule;
    public void OnCancel()
    {
        if (GameManager.Instance.State == GameManager.GameState.TitleScreen)
        {
            AudioManager.Instance.PlaySound("Button SelectBack");
            if (characterSelectModule.isReady) characterSelectModule.OnReady();
        }
    }
    // Generates character select prefabs and HealthUI as needed set to the colour of the current device's index
    private void Start()
    {
        var rootMenu = GameObject.Find("SelectionPanel");

        if (rootMenu == null) return;

        var pm = PlayerManager.Instance;
        int playerIndex = input.playerIndex;

        var gameObj = Instantiate(characterSelect, rootMenu.transform);
        characterSelectModule = gameObj.GetComponent<CharacterSelectModule>();

        characterSelectModule.SetPlayerIndex(playerIndex);
        StartCoroutine(HUDSpawn());

        input.uiInputModule = characterSelectModule.GetComponentInChildren<InputSystemUIInputModule>();
        pm.HandlePlayerJoin(input);

        var color = pm.colours[playerIndex];
        characterSelectModule.backgroundImg.color = color.main;
        characterSelectModule.left.color = color.high;
        characterSelectModule.right.color = color.high;
        characterSelectModule.label.color = color.high;
        characterSelectModule.button.color = color.high;
        characterSelectModule.ready.color = color.high;

        var head = pm.headUIMaterials[playerIndex];
        characterSelectModule.profileImg.material = head;

        var font = pm.fonts[playerIndex];
        characterSelectModule.button.GetComponentInChildren<TextMeshProUGUI>().font = font;
        characterSelectModule.label.GetComponentInChildren<TextMeshProUGUI>().font = font;

        characterSelectModule.backgroundImg.sprite = characterSelectModule.backgrounds[playerIndex];
    }
    // Health on a delay so that the positions can be set correctly based on the amount of players.
    IEnumerator HUDSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        var hudCanvas = GameObject.Find("HealthHUD");
        var hudUI = Instantiate(hud, hudCanvas.transform);
        hudUI.GetComponent<HealthUI>().SetPlayerIndex(input.playerIndex); 
    }
}
