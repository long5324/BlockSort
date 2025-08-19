using NUnit;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class GameManager : MonoBehaviour
{
    [SerializeField] List<GameObject> ListBlockGamePlay;
    [SerializeField] GameObject BottomBlockGameObject;
    [SerializeField] GameObject PrefabBlockChild;
    List<BlockControl> BottomBlock = new List<BlockControl>();
    Camera cam;
    GameObject selectedBlock;
    BlockMaterialControl blockMaterialControl;
    List<Vector3> ListDefaulPossitionBlockGamePlay = new List<Vector3>();
    public GameObject TagertBlock { get; set; }
    private void Start()
    {
        blockMaterialControl = BlockMaterialControl.Instance;
        cam = Camera.main;
        foreach(var i in ListBlockGamePlay)
        {
            ListDefaulPossitionBlockGamePlay.Add(i.transform.position);
        }
        if (BottomBlockGameObject != null)
        for(int i = 0; i < BottomBlockGameObject.transform.childCount - 1; i++)
        {
            for (int j = 0; j < BottomBlockGameObject.transform.GetChild(i).childCount; j++) {
                BottomBlockGameObject.transform.GetChild(i).GetChild(j).GetComponent<BlockControl>().PosionBlock = new Vector2(i, j);
                BottomBlock.Add(BottomBlockGameObject.transform.GetChild(i).GetChild(j).gameObject.GetComponent<BlockControl>());
            }

        }
        RandomSpawnBlockChild();
    }
    

    private void LateUpdate()
    {
     
        if (Input.GetMouseButtonDown(0))
        {
            TagertBlockPlay();
        }
        if (selectedBlock != null && Input.GetMouseButton(0))
        {
            CheckBottomBlock();
        }
        if (Input.GetMouseButtonUp(0))
        {
            EndClicK();
           
        }
    }
    void CheckFirt(BlockControl P)
    {
        List<BlockControl> ListCheck = new List<BlockControl>();
        int countCanChange=0;
        
        foreach (var j in BottomBlock)
        {
            if (j.PosionBlock == P.PosionBlock + new Vector2(1, 1)
                || j.PosionBlock == P.PosionBlock + new Vector2(-1, -1)
                || j.PosionBlock == P.PosionBlock + new Vector2(1, 0)
                || j.PosionBlock == P.PosionBlock + new Vector2(0, 1)
                || j.PosionBlock == P.PosionBlock + new Vector2(-1, 0)
                || j.PosionBlock == P.PosionBlock + new Vector2(0, -1)
                || j.PosionBlock == P.PosionBlock + new Vector2(1, -1)
                || j.PosionBlock == P.PosionBlock + new Vector2(-1, 1))
            {
                ListCheck.Add(j);
            }   
        }
        foreach (var k in ListCheck)
        {
            if (k.ListChildBlock.Count == 0
                || k.ListChildBlock[0].Material.name != P.ListChildBlock[0].Material.name)
                continue;
            countCanChange++;
        }
            foreach (var k in ListCheck)
        {
            if (k.ListChildBlock.Count == 0
                || k.ListChildBlock[0].Material.name != P.ListChildBlock[0].Material.name)
                continue;

            int CountChange = 0;
            while (CountChange < k.ListChildBlock.Count &&
                   k.ListChildBlock[CountChange].Material.name == P.ListChildBlock[0].Material.name)
            {
                CountChange++;
            }

            List<Transform> childrenToMove = new List<Transform>();
            for (int z = 0; z < CountChange; z++)
            {
                childrenToMove.Add(k.transform.GetChild(z));
            }


            foreach (var child in childrenToMove)
            {
                child.SetParent(P.transform);
                child.localPosition = new Vector3(0, 0.003f * P.transform.childCount, 0);
                child.localScale = Vector3.one;
            }


            for (int z = 0; z < CountChange; z++)
            {
                P.ListChildBlock.Insert(0, k.ListChildBlock[0]);
                k.ListChildBlock.RemoveAt(0);
            }
        }
    }
    void Sort()
    {
        bool check = false;
        for (int i = 0; i < BottomBlock.Count; i++)
        {
            if (BottomBlock[i].ListChildBlock.Count == 0) continue;

            List<BlockControl> ListCheck = new List<BlockControl>();

            // Tìm các block lân cận
            foreach (var j in BottomBlock)
            {
                if (j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(1, 1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(-1, -1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(1, 0)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(0, 1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(-1, 0)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(0, -1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(1, -1)
                    || j.PosionBlock == BottomBlock[i].PosionBlock + new Vector2(-1, 1))
                {
                    ListCheck.Add(j);
                }
            }
            foreach (var k in ListCheck)
            {
                if (k.ListChildBlock.Count == 0
                    || k.ListChildBlock[0].Material.name != BottomBlock[i].ListChildBlock[0].Material.name)
                    continue;
                check = true;
                int CountChange = 0;
                while (CountChange < k.ListChildBlock.Count &&
                       k.ListChildBlock[CountChange].Material.name == BottomBlock[i].ListChildBlock[0].Material.name)
                {
                    CountChange++;
                }

                List<Transform> childrenToMove = new List<Transform>();
                for (int z = 0; z < CountChange; z++)
                {
                    childrenToMove.Add(k.transform.GetChild(z));
                }

               
                foreach (var child in childrenToMove)
                {
                    child.SetParent(BottomBlock[i].transform);
                     child.localPosition = new Vector3(0, 0.003f * BottomBlock[i].transform.childCount, 0);
                     child.localScale = Vector3.one;
                }

               
                for (int z = 0; z < CountChange; z++)
                {
                    BottomBlock[i].ListChildBlock.Insert(0, k.ListChildBlock[0]);
                    k.ListChildBlock.RemoveAt(0);
                }
            }
        }
        if (check)
        {
            Sort();
        }
    }

    void TagertBlockPlay() {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Block"))
            {
                selectedBlock = hit.collider.gameObject;
            }
        }
    }
    void CheckBottomBlock()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("GridBlock")))
        {
            Vector3 targetPos = hit.point;
            Ray downRay = new Ray(targetPos + Vector3.up * 50f, Vector3.down);
            RaycastHit downHit;
            if (Physics.Raycast(downRay, out downHit, 100f, LayerMask.GetMask("GridBlock")))
            {
                foreach (var i in BottomBlock)
                {
                    if (blockMaterialControl != null)
                        i.GetComponent<Renderer>().material = blockMaterialControl.MaterialList[0];
                    TagertBlock = null;
                }
                GameObject BottomBlockTagert = downHit.collider.gameObject;
                if (BottomBlockTagert.CompareTag("BottomBlock"))
                {
                    if (blockMaterialControl != null)
                    {
                        BottomBlockTagert.GetComponent<Renderer>().material = blockMaterialControl.MaterialList[1];
                        TagertBlock = BottomBlockTagert;
                        if(BottomBlockTagert.transform.childCount > 0)
                        {
                            TagertBlock = null;
                        }
                    }
                }
                targetPos.y = downHit.point.y + 1f;
            }
            else
            {
                targetPos.y = hit.point.y + 1f;
            }
            selectedBlock.transform.position = targetPos;
        }
    }
    void EndClicK()
    {
        foreach (var i in BottomBlock)
        {
            if (blockMaterialControl != null)
                i.GetComponent<Renderer>().material = blockMaterialControl.MaterialList[0];
        }
       
        if(TagertBlock==null)
        for (int i = 0; i < ListBlockGamePlay.Count; i++)
        {
            ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
        }
        else {
            SetBlock();
        }
        if(TagertBlock!=null)
        CheckFirt(TagertBlock.GetComponent<BlockControl>());
        selectedBlock = null;
    }
    void ChangeChildBlock(BlockControl Start, BlockControl End)
    {
        int countChange = Start.transform.childCount;
        for (int i = countChange - 1; i >= 0; i--)
        {
            Transform child = Start.transform.GetChild(i);
            child.SetParent(End.transform);
            child.localPosition = new Vector3(0, 0.003f * (countChange+1- End.transform.childCount), 0);
            child.localScale = Vector3.one;
            End.ListChildBlock.Add(Start.ListChildBlock[i]);
        }
        Start.ListChildBlock.Clear();
    }
    void SetBlock()
    {
        if (selectedBlock == null || TagertBlock == null) return;

        ChangeChildBlock(
            selectedBlock.GetComponent<BlockControl>(),
            TagertBlock.GetComponent<BlockControl>()
        );
        for (int i = 0; i < ListBlockGamePlay.Count; i++)
        {
            ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
        }
        bool hasChild = ListBlockGamePlay.Exists(b => b.transform.childCount > 0);

        if (!hasChild)
        {
            foreach (var block in ListBlockGamePlay)
            {
                block.SetActive(true);
            }
            RandomSpawnBlockChild();
        }
    }

    void RandomSpawnBlockChild()
    {

        foreach (var i in ListBlockGamePlay)
        {
            int countColor = Random.Range(1, 4);
            int countBlock = Random.Range(2, 7);
            List<int> ColorBlock = new List<int>();
            for (int j = 0; j < countColor; j++)
            {
                ColorBlock.Add(UnityEngine.Random.Range(2, 8));
            }
            int BlockE = countBlock;
            for (int j = 1; j <= countColor; j++)
            {
                if (BlockE <= 0) break ;
                int cureenBlock = Random.Range(1, BlockE + 1);
                BlockE = BlockE - cureenBlock;
                for (int k = 1; k <= cureenBlock; k++)
                {
                    GameObject ChildGameObject = Instantiate(PrefabBlockChild, i.transform.position + new Vector3(0, 0.003f * (j + 1), 0), Quaternion.identity);
                    ChildGameObject.transform.SetParent(i.transform);
                    ChildGameObject.transform.localPosition = new Vector3(0, 0.003f * i.transform.childCount, 0);
                    ChildGameObject.transform.localScale = new Vector3(1, 1, 1);
                    ChildGameObject.GetComponent<Renderer>().material = blockMaterialControl.MaterialList[ColorBlock[j - 1]];
                    ChildBlock Child = new ChildBlock();
                    Child.Material = ChildGameObject.gameObject.GetComponent<Renderer>().material;
                    i.GetComponent<BlockControl>().ListChildBlock.Add(Child);
                }

            }
        }
    }


}
