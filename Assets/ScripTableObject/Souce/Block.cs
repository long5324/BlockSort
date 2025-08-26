using System;
using System.Collections.Generic;
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
    Pink
}
[Serializable]
public struct BlockDataBase
{
    public BlockColor BlockColor;
    public Material BlockMaterial;
}

[CreateAssetMenu(fileName = "BlockData", menuName = "Data/BlockData")]
public class Block : ScriptableObject
{
    public List<BlockDataBase> DataBases;
    public Material GetMaterial(BlockColor Color)
    {
        foreach (var data in DataBases) { 
           if(data.BlockColor == Color)
            {
                return data.BlockMaterial;
            }
        }
        return null;    
    }
}
