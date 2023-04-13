// Authored by: Harley Clark
using UnityEngine;

[CreateAssetMenu(fileName = "ColourSwatch", menuName = "ColourSwatch", order = 1)]
public class ColourSwatch : ScriptableObject
{
    public Color main;
    public Color high;
    public Color darker;
    public Color darkest;
}
