
using System.Collections.Generic;
using UnityEngine;

public class BlockControl : MonoBehaviour
{
    
    public List<ChildBlock> ListChildBlock { get; set; } = new List<ChildBlock>();
    public bool Tagert { get; set; }
    public Vector2 PosionBlock { get; set; } = new Vector2();
    public Renderer Renderer { get; set; }
    public List< BlockControl >BlockLink { get; set; } = new List<BlockControl> ();
    private void Start()
    {
        Renderer = GetComponent<Renderer>();
    }
    public void SetColor(Material material)
    {
        Renderer.material = material;
    }
    public void ClearLink()
    {
        BlockLink.Clear();
    }
}
