
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BlockControl : MonoBehaviour
{
    
    public List<ChildBlock> ListChildBlock ;
    public bool Tagert { get; set; }
    public Vector3 PosionBlock ;
    public Renderer Renderer { get; set; }
    [SerializeField] public List< BlockControl >BlockLink = new List<BlockControl> ();

    private void Start()
    {
        Renderer = GetComponent<Renderer>();
       
    }
    public void SetColor(Material material)
    {
        Renderer.material = material;
    }
    public BlockColor CheckColor()
    {
        if (ListChildBlock == null || ListChildBlock.Count == 0)
            return BlockColor.None;

        ChildBlock last = ListChildBlock[ListChildBlock.Count - 1];
        if (last == null)
            return BlockColor.None;

        return last.CurrenColor;
    }
    

    public int GetNumberSameColor()
    {
        // Nếu list rỗng thì trả về 0
        if (ListChildBlock == null || ListChildBlock.Count == 0)
            return 0;

        int count = 0;
        int index = ListChildBlock.Count - 1;

        // Lấy màu cuối cùng (chắc chắn tồn tại vì đã check count > 0)
        BlockColor color = ListChildBlock[index].CurrenColor;

        // Đếm ngược các block có cùng màu
        while (index >= 0 && ListChildBlock[index] != null && ListChildBlock[index].CurrenColor == color)
        {
            count++;
            index--;
        }

        return count;
    }
    public void CopyDataFrom(BlockControl other)
    {
        transform.localPosition = other.transform.localPosition;
        BlockLink = other.BlockLink;
        ListChildBlock = other.ListChildBlock;
    }
    public List<ChildBlock> GetSameBlock ()
    {
        List<ChildBlock> ListBlock = new List<ChildBlock>();    
        if (ListChildBlock == null || ListChildBlock.Count == 0)
            return ListBlock;
        int index = ListChildBlock.Count - 1;
        
        BlockColor color = ListChildBlock[index].CurrenColor;
        while (index >= 0 && ListChildBlock[index] != null && ListChildBlock[index].CurrenColor == color)
        {
             ListBlock.Add(ListChildBlock[index]);
             index--;
        }
        return ListBlock;
    }
    public BlockControl(BlockControl bc)
    {
     ListChildBlock = bc.ListChildBlock;
     Tagert = bc.Tagert;
     PosionBlock = bc.PosionBlock;
     Renderer  = bc.Renderer;
     BlockLink = bc.BlockLink;
}
    public List<BlockControl> CheckArow()
    {
        List<BlockControl> ListBlock = new List<BlockControl>();
       
        foreach (var i in BlockLink)
        {
            if (i != null &&
     CheckColor() == i.CheckColor() &&
     i.CheckColor() != BlockColor.None)
            {
                ListBlock.Add(i);
            }

        }
        return ListBlock;
    }
    public void ClearLink()
    {
        if(BlockLink!=null) 
        BlockLink.Clear();
    }
}
