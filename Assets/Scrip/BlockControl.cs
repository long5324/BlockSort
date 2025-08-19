using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BlockControl : MonoBehaviour
{
    public List<ChildBlock> ListChildBlock { get; set; } = new List<ChildBlock>();
    public bool Tagert { get; set; }
    public Vector2 PosionBlock { get; set; } = new Vector2();
}
