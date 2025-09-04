
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UIElements;
[System.Serializable]
public class BlockLevelDefaut
{
    public BlockColor Color;
    public int NumberSpawm;                            
}
public class BlockControl : MonoBehaviour
{
    [Header("SupportSaveLevel")]
    public List<BlockLevelDefaut> DataSpawn = new List<BlockLevelDefaut>();

    public List<ChildBlock> ListChildBlock ;
    public bool Tagert { get; set; }
    public Vector3 PosionBlock ;
    public Renderer Renderer { get; set; }
    [SerializeField] public List< BlockControl >BlockLink = new List<BlockControl> ();
    Material MaterialDF { get; set; }
    private void Start()
    {
        Renderer = GetComponent<Renderer>();
        MaterialDF = Renderer.material;
    }
    public void SetColor(Material material)
    {
        Renderer.material = material;
    }
    public void BacktoDFColor()
    {
        Renderer.material = MaterialDF;
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
    public void UpdateList()
    {
        if (transform.childCount == 0) { ListChildBlock.Clear(); return; }
        ListChildBlock.Clear();
         foreach (Transform i in transform)
        {
            ListChildBlock.Add(i.GetComponent<ChildBlock>());
        }
    }
    [Button(ButtonSizes.Large)]
    public void SpawnBlockChild()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {

            if (!Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
                Destroy(transform.GetChild(i).gameObject);

            ListChildBlock.Clear();
        }
        foreach(var i in DataSpawn)
        {
            for(int j = 0;j < i.NumberSpawm; j++)
            {
                GamePlayManager gamePlaymanager = GamePlayManager.Ins;
                ChildBlock Obj = GameManager.Ins.SpawnBlockNotBool(i.Color);
                Obj.transform.SetParent(transform);
                Obj.transform.localScale = gamePlaymanager.baseScale;
                Obj.transform.localPosition = new Vector3(0, gamePlaymanager.sizeYBlock * transform.childCount, 0);
                ListChildBlock.Add(Obj);

            }
        }
    }
    public void SpawnBlockChildWithBool()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {

            if (!Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
                Destroy(transform.GetChild(i).gameObject);

            ListChildBlock.Clear();
        }
        foreach (var i in DataSpawn)
        {
            for (int j = 0; j < i.NumberSpawm; j++)
            {
                GamePlayManager gamePlaymanager = GamePlayManager.Ins;
                ChildBlock Obj = GameManager.Ins.SpawnBlockChild(i.Color);
                Obj.transform.SetParent(transform);
                Obj.transform.localScale = gamePlaymanager.baseScale;
                Obj.transform.localPosition = new Vector3(0, gamePlaymanager.sizeYBlock * (j + 1), 0);
                ListChildBlock.Add(Obj);

            }
        }
    }
    public void UpdateDataSpawn()
    {
        if (ListChildBlock == null || ListChildBlock.Count == 0)
            return;

        DataSpawn.Clear(); // tránh cộng dồn dữ liệu cũ

        int CurrenCheck = 0;
        BlockColor CurrenColor = ListChildBlock[0].CurrenColor;
        int count = 0;

        while (CurrenCheck < ListChildBlock.Count)
        {
            if (CurrenColor == ListChildBlock[CurrenCheck].CurrenColor)
            {
                count++;
            }
            else
            {
                // Lưu nhóm cũ
                BlockLevelDefaut Df = new BlockLevelDefaut();
                Df.Color = CurrenColor;
                Df.NumberSpawm = count;
                DataSpawn.Add(Df);

                // Reset cho nhóm mới
                CurrenColor = ListChildBlock[CurrenCheck].CurrenColor;
                count = 1;
            }

            CurrenCheck++;
        }

        // 🔥 Đừng quên add nhóm cuối cùng
        BlockLevelDefaut last = new BlockLevelDefaut();
        last.Color = CurrenColor;
        last.NumberSpawm = count;
        DataSpawn.Add(last);
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
            if (i != null && CheckColor() == i.CheckColor() && i.CheckColor() != BlockColor.None )
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
