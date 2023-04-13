// Authored by: Harley Clark
// Added to by: Finn Davis
using System.Collections.Generic;
using UnityEngine;

public class TrapLayout : MonoBehaviour
{
    // So that designers can prefab multiple layouts and the Game Manager randomizes which layout is used per round.
    public List<Transform> spawnPoints;
    public List<GameObject> trapWaves;

    public List<List<GameObject>> trapDecalPoints = new List<List<GameObject>>();
    public GameObject trapWarningDecal;
    private void Awake()
    {
        foreach (GameObject trapWave in trapWaves)
        {
            List<GameObject> waveTraps = new List<GameObject>();
            Trap[] traps = trapWave.GetComponentsInChildren<Trap>();

            // Attaches the warning Decal to the traps
            foreach (Trap trap in traps)
            {
                GameObject decal = Instantiate(trapWarningDecal, trap.transform.position, Quaternion.identity, transform);
                decal.SetActive(false);
                waveTraps.Add(decal);
            }
            trapDecalPoints.Add(waveTraps);
        }
    }
    public void EnableWaveWarning(int waveIndex)
    {
        for (int decalIndex = 0; decalIndex < trapDecalPoints[waveIndex].Count; decalIndex++)
        {
            trapDecalPoints[waveIndex][decalIndex].SetActive(true);
        }
    }
    public void DisableWaveWarning(int waveIndex)
    {
        for (int decalIndex = 0; decalIndex < trapDecalPoints[waveIndex].Count; decalIndex++)
        {
            trapDecalPoints[waveIndex][decalIndex].SetActive(false);
        }
    }
}

