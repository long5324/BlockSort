using DG.Tweening.Core.Easing;
using UnityEngine;

[System.Serializable]
public class ChildBlock : MonoBehaviour
{
    public BlockColor CurrenColor;
    public MeshRenderer MeshRenderer;

    public void Configure(BlockData Data)
    {
        CurrenColor = Data.Color;
        MeshRenderer.material = Data.BlockMaterial;
    }

}
