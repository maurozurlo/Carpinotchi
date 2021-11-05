using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualManager : MonoBehaviour
{
    SkinnedMeshRenderer skinnedMesh;
    public static VisualManager control;
    public Material material;
    public Material glowMaterial;

    public void Start()
    {
        control = this;
        skinnedMesh = GetComponentInChildren<SkinnedMeshRenderer>();
        material = skinnedMesh.material;
    }
    public void Glow()
    {
        skinnedMesh.material = glowMaterial;
    }

    public void Dull()
    {
        skinnedMesh.material = material;
    }
}
