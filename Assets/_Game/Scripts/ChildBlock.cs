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
        MeshRenderer.material = null;
    }
    public void Configure(BlockData Data)
    {
        CurrenColor = Data.Color;
        MeshRenderer.material = Data.BlockMaterial;
    }
    private void OnEnable()
    {
        ResetState();
    }

    private void ResetState()
    {
        CurrenColor = BlockColor.None;
        MeshRenderer.material = null; 
    }
}
