using JetBrains.Annotations;
using Lean.Pool;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public enum BlockColor
{
    None,
    Red,
    Green,
    Blue,
    Yellow,
    Black,
    White,
    Pink,
    CheckBottomColor,
    DefautColor
}
[Serializable]
public struct BlockData
{
    public BlockColor Color;
    public Material BlockMaterial;
}
[CreateAssetMenu(fileName = "BlockData", menuName = "Data/BlockData")]
public class Block : ScriptableObject
{
    public List<BlockData> BlockDataBase = new List<BlockData>();
    public ChildBlock BlockPrefab;
  
/*    public Material GetMaterial(BlockColor Color)
    {
        foreach (var i in DataBases)
        {
            if (i.CurrenColor == Color)
            {
                return i.gameObject.GetComponent<Renderer>().sharedMaterial;
            }
        }
        return null;
    }

*/
}
