using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct InfoLevel{
    public float sizeGrid ;
    public int NumberInit ;
    public Vector3 DefaultCenter;
    public List<Vector3> CenterGird ;
    public List<BlockData> ListblockGround ;
}
[CreateAssetMenu(fileName = "Lever", menuName = "CreatLevel/NewLevel")]
public class LevelSave : ScriptableObject
{
    public InfoLevel Database = new InfoLevel();
}
