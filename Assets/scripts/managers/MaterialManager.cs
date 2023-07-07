using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates material for player in runtime for robots
/// </summary>
public class MaterialManager : MonoBehaviour
{
    [SerializeField] private Material[] robotMaterial;

    public Material CreateTeamMaterial(Color baseColor, Color detailColor, Color highlightColor, int logoIndex)
    {
        // Instantiate a copy of the material template
        Material material = new(robotMaterial[logoIndex]);

        // Set the user-selected colors
        material.SetColor("_Base", baseColor);
        material.SetColor("_Detail", detailColor);
        material.SetColor("_Highlight", highlightColor);

        return material;
    }
}
