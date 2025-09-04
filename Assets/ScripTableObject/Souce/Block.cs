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
[System.Serializable] 

[CreateAssetMenu(fileName = "BlockData", menuName = "Data/BlockData")]
public class Block : ScriptableObject
{
    public GameObject ObjectBoolingControl;
    public List<ChildBlock> DataBases = new List<ChildBlock>();
    public GameObject GetBlockChild(BlockColor Color)
    {
        GameObject ObjectR = null;

        foreach (ChildBlock b in DataBases)
        {
            if (b.CurrenColor == Color)
            {
                ObjectR = LeanPool.Spawn(b.gameObject);
                ObjectR.transform.SetParent(ObjectBoolingControl.transform);
                break;
            }
        }
        return ObjectR;
    }
    public GameObject SpawnBlockNotBool(BlockColor Color)
    {
        GameObject ObjectR = null;

        foreach (ChildBlock b in DataBases)
        {
            if (b.CurrenColor == Color)
            {
                ObjectR = Instantiate(b.gameObject);
            
                break;
            }
        }
        return ObjectR;
    }
    public Material GetMaterial(BlockColor Color)
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


}
