using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class CutoutObject : MonoBehaviour
{
    [SerializeField]
    private List<Transform> players;
    [SerializeField]
    private bool setPlayers;
    private int noPlayers;
    [SerializeField]
    private LayerMask wallMask;

    private List<Material> oldMaterials;

    private Camera mainCamera;

    private void Awake()
    {
        noPlayers = 0;
        players = new List<Transform>();
        mainCamera = GetComponent<Camera>();
        oldMaterials = new List<Material>();
    }

    public void SetPlayers(List<Transform> currentPlayers)
    {
       noPlayers = currentPlayers.Count;
        players = currentPlayers;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 cutoutPos;
        Vector3 offset;
        RaycastHit[] hitObjects;
        float aspectRatioRecip = 1/(Screen.width / Screen.height);
        for (int i = 0; i < noPlayers; ++i)
        {
            cutoutPos = mainCamera.WorldToViewportPoint(players[i].position);
            cutoutPos.y *= aspectRatioRecip;

            foreach (Material oldmat in oldMaterials) {
                oldmat.SetFloat("_CutoutNo", 0);
            }

            oldMaterials = new List<Material>();
            offset = players[i].position - transform.position;
            hitObjects = Physics.RaycastAll(transform.position, offset, offset.magnitude, wallMask);

            foreach (RaycastHit hit in hitObjects)
            {
                Material[] materials = hit.transform.GetComponent<Renderer>().materials;
                oldMaterials.AddRange(materials);

                foreach (Material mat in materials)
                {
                    mat.SetFloat("_CutoutNo", noPlayers);
                    mat.SetVector("_CutoutPos"+(i+1), cutoutPos);
                    mat.SetFloat("_CutoutSize"+(i+1), math.remap(8, 20, 0.15f, 0.02f, offset.magnitude));
                }
            }
        }
    }
}
