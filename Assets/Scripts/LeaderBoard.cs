// Authored by: Harley Clark
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderBoard : MonoBehaviour
{
    public Image banner;
    public Image profile;
    public TextMeshProUGUI text;

    public void SetWinner(int index)
    {
        banner.color = PlayerManager.Instance.colours[index].main;
        profile.material = PlayerManager.Instance.headUIMaterials[index];    
    }
}
