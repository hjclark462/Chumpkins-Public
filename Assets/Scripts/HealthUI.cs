// Authored by: Harley Clark
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Slider healthBar;
    private Player player;
    public Image health;
    public List<Image> lives;
    public Image background;
    public Image portrait;
    public int index;
    private ColourSwatch colourSwatch;
    private bool shake = false;

    private void Start()
    {
        player = PlayerManager.Instance.playerConfigs[index].input.GetComponent<Player>();
        colourSwatch = PlayerManager.Instance.colours[index];
        healthBar.maxValue = player.startingHealth;
        health.color = colourSwatch.high;
        portrait.material = PlayerManager.Instance.headUIMaterials[index + 4];
        background.material = PlayerManager.Instance.UIMaterials[index];
        player.shaker += ShakeUI;
        var heart = PlayerManager.Instance.UIMaterials[index + 4];
        foreach (Image life in lives)
        {
            life.material = heart;
        }
    }
    void Update()
    {
        if(shake)
        {
            float shakeOffset = 0;                   
            shakeOffset = Mathf.PerlinNoise(Time.time*2, 0);
            shakeOffset *= 45;
            gameObject.transform.rotation = Quaternion.Euler(0,0,shakeOffset);
        }
        else
        {
            gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        healthBar.value = player.currentHealth;
        if (player.currentLives == 3)
        {
            for (int i = 0; i < lives.Count; i++)
                lives[i].gameObject.SetActive(true);
        }
        if (player.currentLives == 2)
        {
            lives[0].gameObject.SetActive(false);
        }
        if (player.currentLives == 1)
        {
            lives[1].gameObject.SetActive(false);
        }
        if (player.currentLives == 0)
        {
            lives[2].gameObject.SetActive(false);
        }
    }
    public void SetPlayerIndex(int i)
    {
        index = i;
    }
    private void ShakeUI()
    {
        shake = true;
        Invoke(nameof(NoShake), 0.1f);
    }
    private void NoShake()
    {
        shake = false;
    }
}
