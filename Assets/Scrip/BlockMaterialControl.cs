using System.Collections.Generic;
using UnityEngine;

public class BlockMaterialControl : MonoBehaviour
{
    public static BlockMaterialControl Instance { get; private set; }
    public List<Material> MaterialList;
    private void Awake()
    {
        Instance = this;
    }
}
