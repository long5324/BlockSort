using DG.Tweening.Core.Easing;
using UnityEngine;

[System.Serializable]
public class ChildBlock : MonoBehaviour
{
    GameManager gameManager;
    public BlockColor CurrenColor;
    public void InitBlock(BlockColor Color)
    {
        CurrenColor = Color;
        gameManager = GameManager.Ins;
        GetComponent<Renderer>().material = gameManager.BlockData.GetMaterial(Color);
    }
}
