// Authored by: Harley Clark
// Added to by: Finn Davis
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectModule : MonoBehaviour
{
    int playerIndex;

    private int currentH = 0;
    public bool isReady {  get; private set; }
        
    public Slider headPieceSlider;
    public List<Sprite> backgrounds = new List<Sprite>();
    public Image backgroundImg;
    public Image profileImg;
    public Image left;
    public Image right;
    public Image button;
    public Image label;
    public Image ready;
    // Upon instantiation by the CharacterSelectMenuSetup script asigns the player index so that the correct UI is utilised
    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
        PlayerManager.Instance.joinBGS[index].SetActive(false);
    }
    public void PlayButtonSound()
    {
        AudioManager.Instance.PlaySound("Button Select");
    }
    // Triggered by the slider that is invisible to the user to set the correct head piece
    public void OnSliderValueChange()
    {
        if (headPieceSlider.value == 0) return;
        if (headPieceSlider.value == 1)
        {
            PlayButtonSound();
            right.GetComponent<Animator>().SetTrigger("Selected");
            currentH++;
            if (currentH == PlayerManager.Instance.headpieces.Count) currentH = 0;
        }
        else if (headPieceSlider.value == -1)
        {
            PlayButtonSound();
            left.GetComponent<Animator>().SetTrigger("Selected");
            currentH--;
            if (currentH < 0) currentH = PlayerManager.Instance.headpieces.Count - 1;
        }
        StartCoroutine(ResetToCenter());
        PlayerManager.Instance.SetPlayerAppearance(playerIndex, currentH);
        var color = PlayerManager.Instance.colours[playerIndex].main;
        backgroundImg.color = color;
    }
    IEnumerator ResetToCenter()
    {
        right.GetComponent<Animator>().SetTrigger("Normal");
        left.GetComponent<Animator>().SetTrigger("Normal");
        yield return new WaitForSecondsRealtime(0.2f);
        headPieceSlider.value = 0;        
    }
    // Deactivates the iconography so a Ready image can be displayed and tells the PlayerManager it is ready to start
    public void OnReady()
    {
        if (isReady) isReady = false;
        else isReady = true;
        left.gameObject.SetActive(!isReady);
        right.gameObject.SetActive(!isReady);
        label.gameObject.SetActive(!isReady);
        button.gameObject.SetActive(!isReady);
        ready.gameObject.SetActive(isReady);
        PlayerManager.Instance.ReadyPlayer(playerIndex, isReady);
    }
}
