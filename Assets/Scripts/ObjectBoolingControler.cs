using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectBoolingControler : Singleton<ObjectBoolingControler> { 
    [System.Serializable]
    public class BoolingIf
    {
        
        public BlockColor color;
        public int NumberObject;
        public int NumberNone;
        public List<Transform> ObjectChild = new List<Transform>();
    }
    GameManager gameManager;
    GamePlayManager gamePlayManager;
    [SerializeField] public int NumberBlockInit = 20;
    [SerializeField] GameObject PrefabBlockChild;
    public List<BoolingIf> BoolingData { get; set; } = new List<BoolingIf>();
    void Awake()
    {
        gameManager = GameManager.Ins;
        gamePlayManager = GamePlayManager.Ins;
        InitObjectBooling();
        SpawnBlockChid(NumberBlockInit);
    }
    public void InitObjectBooling()
    {
        foreach (var i in gameManager.BlockData.DataBases)
        {
            GameObject newGameObject = new GameObject(i.BlockColor.ToString());
            newGameObject.transform.SetParent(gameObject.transform);
            BoolingIf BIf = new BoolingIf();
            BIf.color = i.BlockColor;
            BoolingData.Add(BIf);
        }
    }
    public List<Transform> getObjectChile(BlockColor color, int Number)
    {
        foreach(var i in BoolingData)
        {
            if(i.color == color)
            {
                i.NumberNone -= Number; break;
            }
        }
        List<Transform> objects = new List<Transform>();
        foreach (var i in BoolingData)
        {
            if (i.color == color)
            {
                for (int j = 0; j < i.ObjectChild.Count; j++)
                {
                    if (i.ObjectChild[j] != null && !i.ObjectChild[j].gameObject.activeSelf)
                    {
                        
                        if (j + Number > i.NumberNone)
                        {
                            int blocksToSpawn = (j + Number) - i.NumberNone;
                            SpawnBlockChidColor(blocksToSpawn, color); 
                        }

                        // Thêm đối tượng vào danh sách
                        for (int k = j; k < Mathf.Min(j + Number, i.ObjectChild.Count); k++)
                        {
                            objects.Add(i.ObjectChild[k]);
                        }
                        break;  
                    }
                }
                break;  
            }
        }
        return objects;
    }

    public void SpawnBlockChid(int NumberInit)
    {
       
        List<Transform> ObjectBooling = new List<Transform>();
        foreach (Transform i in gameObject.transform)
        {
            
            ObjectBooling.Add(i);
        }

        
        foreach (var i in BoolingData)
        {
            if (i.color == BlockColor.None) continue;
            i.NumberObject += NumberInit;
            i.NumberNone += NumberInit;
        }

       
        for (int i = 0; i < NumberInit; i++)
        {
            for (int j = 0; j < gameManager.BlockData.DataBases.Count; j++)
            {
                if (gameManager.BlockData.DataBases[j].BlockColor == BlockColor.None) continue;
                GameObject ChildGameObject = Instantiate(PrefabBlockChild, Vector3.zero, Quaternion.identity);
                ChildGameObject.SetActive(false);


                ChildGameObject.transform.SetParent(ObjectBooling[j]);

                BoolingData[j].ObjectChild.Add(ChildGameObject.transform);
                foreach (var blockData in gameManager.BlockData.DataBases)
                {
                    if (blockData.BlockColor == BoolingData[j].color) 
                    {
                        ChildBlock Child = ChildGameObject.GetComponent<ChildBlock>();
                        Child.CurrenColor = BoolingData[j].color;
                        Child.InitBlock(BoolingData[j].color);
                        break;
                    }
                }
            }
        }
    }
    public void ObjectBack(List<Transform> List)
    {
        if (List == null || List.Count == 0) return;

        // Bỏ hết phần tử null (đã bị Destroy)
        List.RemoveAll(item => item == null);

        if (List.Count == 0) return;

        // Tìm container đúng màu
        foreach (Transform container in transform)
        {
            if (container.gameObject.name == List[0].GetComponent<ChildBlock>().CurrenColor.ToString())
            {
                foreach (var obj in List)
                {
                    if (obj == null) continue; // an toàn hơn nữa

                    obj.gameObject.SetActive(false);
                    obj.SetParent(container, true);
                }
                break;
            }
        }

        // Cập nhật dữ liệu BoolingData
        BlockColor color = List[0].GetComponent<ChildBlock>().CurrenColor;
        foreach (var data in BoolingData)
        {
            if (data.color == color)
            {
                data.NumberNone += List.Count;
                data.ObjectChild.InsertRange(0, List);
                break;
            }
        }
    }

    public void SpawnBlockChidColor(int NumberInit, BlockColor color)
    {
        Transform P = GameObject.Find(color.ToString())?.transform;
        if (P == null)
        {
          
            return;
        }

       
        BoolingIf Pf = null;
        foreach (var i in BoolingData)
        {
            if (i.color == color)
            {
                Pf = i;
                Pf.NumberNone += NumberInit;
                Pf.NumberObject += NumberInit;
                break;
            }
        }

       
        if (Pf == null)
        {
           
            return;
        }

       
        Material material = null;
        foreach (var i in gameManager.BlockData.DataBases)
        {
            if (i.BlockColor == color)
            {
                material = i.BlockMaterial;
                break;
            }
        }

       
        if (material == null)
        {
           
            return;
        }

      
        for (int i = 0; i < NumberInit; i++)
        {
            GameObject ChildGameObject = Instantiate(PrefabBlockChild, Vector3.zero, Quaternion.identity);
            ChildGameObject.SetActive(false); 
            ChildGameObject.transform.SetParent(P);
            Pf.ObjectChild.Add(ChildGameObject.transform);
            ChildBlock Child = ChildGameObject.GetComponent<ChildBlock>();
            Child.CurrenColor = color;
            Child.InitBlock(color);


        }
    }

}
