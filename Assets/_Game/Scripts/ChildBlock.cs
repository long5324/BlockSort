using DG.Tweening.Core.Easing;
using UnityEngine;

[System.Serializable]
public class ChildBlock : MonoBehaviour
{
    public BlockColor CurrenColor;
    public MeshRenderer MeshRenderer;
    public void SetDefaultBlockChild()
    {
        CurrenColor = BlockColor.None;
        MeshRenderer.sharedMaterial = null; // ✅ không clone nữa
    }

    private void ResetState()
    {
        CurrenColor = BlockColor.None;
        MeshRenderer.sharedMaterial = null; // ✅
    }

    public void Configure(BlockData Data)
    {
        CurrenColor = Data.Color;
        MeshRenderer.sharedMaterial = Data.BlockMaterial;
     
    }

    private void OnEnable()
    {
        ResetState();
    }


}
