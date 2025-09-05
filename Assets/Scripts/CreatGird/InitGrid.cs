using DG.Tweening.Core.Easing;
using Lean.Pool;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class InitGrid : MonoBehaviour 
{
    public float  sizeGrid = 1;
    public int NumberInit = 4;
    public int numberRandom = 6;
    public Vector3 DefaultCenter ;
    public bool DrawGrid=false ;
    [SerializeField] List<Vector3> CenterGird = new List<Vector3>();
    public  List<BlockControl> ListblockGround  = new List<BlockControl>();
    GamePlayManager gamePlayManager;
    GameManager gameManager;

   

    private void Start()
    {
        foreach(Transform i in transform)
        {
            i.GetComponent<BlockControl>().SpawnBlockChildWithBool();
        }
    }
    [Button(ButtonSizes.Large)]
    public void StartInitGrid()
    {
        CenterGird.Clear();
        CenterGird.Add(Vector3.zero);
        List<Vector3> frontier = new List<Vector3>();
        frontier.Add(Vector3.zero);

        float halfSize = sizeGrid / 2f;

        for (int step = 0; step < NumberInit; step++)
        {
            List<Vector3> newFrontier = new List<Vector3>();

            foreach (var center in frontier)
            {
                List<Vector3> neighbors = new List<Vector3>()
            {
                RoundVector(center + new Vector3(sizeGrid, 0, 0)),  // phải
                RoundVector(center + new Vector3(-sizeGrid, 0, 0)), // trái
                RoundVector(center + new Vector3(0, 0, sizeGrid)),  // trên
                RoundVector(center + new Vector3(0, 0, -sizeGrid))  // dưới
            };

                // Chỉ thêm neighbor chưa có trong grid
                for (int k = neighbors.Count - 1; k >= 0; k--)
                {
                    if (CheckGrid(neighbors[k]))
                    {
                        neighbors.RemoveAt(k);
                    }
                }

                CenterGird.AddRange(neighbors);
                newFrontier.AddRange(neighbors);
            }

            frontier = newFrontier; // cập nhật frontier cho bước tiếp theo
        }
        SetupBlock();
    }
    public void SetupBlock()
    {
        ListblockGround.Clear();
        ChangePositonGround();
    }
    [Button(ButtonSizes.Large)]
    public void ClearData()
    {
        ClearChildren(transform);
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            BlockControl bc = child.GetComponent<BlockControl>();
            if (bc != null)
            {
                bc.DataSpawn.Clear();
                bc.ListChildBlock.Clear();
            }
            for (int j = child.childCount - 1; j >= 0; j--)
            {
                Transform grandChild = child.GetChild(j);

               
                if (Application.isPlaying)
                    Destroy(grandChild.gameObject);
                else
                    DestroyImmediate(grandChild.gameObject);
            }
        }
    }

    [Button(ButtonSizes.Large)]
    public void RandomSpawn()
    {
        ClearData();
        gamePlayManager = GamePlayManager.Ins;
        gameManager = GameManager.Ins;
        List<Transform> pickedChildren = GetRandomChildren(transform, numberRandom);

        for (int i = 0; i < pickedChildren.Count; i++)
        {
            BlockControl block = pickedChildren[i].GetComponent<BlockControl>();
            if (block == null) continue;
            int colorIndex = Random.Range(0, 7);
            BlockColor color = gameManager.BlockData.BlockDataBase[colorIndex].Color;

            int countBlock = Random.Range(2, 7);
            List<Transform> ObjectGame = new List<Transform>();
            for (int j = 0; j < countBlock; j++)
            {
                ObjectGame.Add(GameManager.Ins.SpawnBlockChild(color).transform);

            }

            for (int j = 0; j < countBlock; j++)
            {
                if (j < ObjectGame.Count)
                {
                    ObjectGame[j].gameObject.SetActive(true);
                    ObjectGame[j].transform.SetParent(block.transform);
                    ObjectGame[j].transform.localRotation = Quaternion.identity;
                    ObjectGame[j].transform.localPosition = new Vector3(0, gamePlayManager.sizeYBlock * (j+1), 0);
                    ObjectGame[j].transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

                    // lưu vào danh sách con của block
                    block.ListChildBlock.Add(ObjectGame[j].GetComponent<ChildBlock>());
                }
            }

            // Nếu có ObjectSet thì cập nhật
            ObjectSet set = block.GetComponent<ObjectSet>();
            if (set != null)
                set.AddLisst();
        }
        foreach(var i in ListblockGround)
        {
            if(i.ListChildBlock.Count > 0)
            {
                i.UpdateDataSpawn();
            }
        }
    }


    List<Transform> GetRandomChildren(Transform parent, int amount)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in parent)
            children.Add(child);

        List<Transform> picked = new List<Transform>();

        for (int i = 0; i < amount && children.Count > 0; i++)
        {
            int index = Random.Range(0, children.Count);
            picked.Add(children[index]);   // 🟢 Tham chiếu gốc
            children.RemoveAt(index);      // tránh trùng
        }

        return picked;
    }

    void ChangePositonGround()
    {
        ListblockGround.Clear();  // reset list trước
        List<Vector3> usedPositions = new List<Vector3>(); // danh sách vị trí đã dùng

        foreach (Transform i in transform)
        {
            Vector3 local = i.localPosition;
            float Distance = float.MaxValue;
            Vector3 newPosition = local;

            foreach (var j in CenterGird)
            {
                if (usedPositions.Contains(j)) continue; // nếu đã có block ở vị trí này, bỏ qua

                float d = DistanceXZ(j, local);
                if (d < Distance)
                {
                    Distance = d;
                    newPosition = j;
                }
            }

            i.localPosition = new Vector3(newPosition.x, 0, newPosition.z);

            BlockControl bcComponent = i.GetComponent<BlockControl>();
            if (bcComponent != null)
            {
                bcComponent.PosionBlock = i.localPosition;
                bcComponent.ClearLink();

                ListblockGround.Add(bcComponent);
            }

            usedPositions.Add(newPosition); // đánh dấu vị trí đã được sử dụng
        }


        LinkGroud();
    }

  
    void LinkGroud()
    {
        foreach(Transform i in gameObject.transform)
        {
            BlockControl Center =  i.GetComponent<BlockControl>();
            if (Center == null) continue;
            Vector3 local = i.localPosition;
            Center.BlockLink = GetBlockLink(local);
        }
    }
    bool CheckGrid(Vector3 Check)
    {
        foreach (var i in CenterGird) {
        if(Check == i)
            {
                return true;
            }
        }
        return false;
    }
    float DistanceXZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
    public List<Vector3> GetHexNeighbors(Vector3 center)
    {

        List<Vector3> ListReturn = new List<Vector3>();
        ListReturn.Add(RoundVector(center + new Vector3(sizeGrid, 0, 0)));  // phải
        ListReturn.Add(RoundVector(center + new Vector3(-sizeGrid, 0, 0)));// trái
        ListReturn.Add(RoundVector(center + new Vector3(0, 0, sizeGrid)));  // trên
        ListReturn.Add(RoundVector(center + new Vector3(0, 0, -sizeGrid)));  // dưới
        ListReturn.Add(RoundVector(center + new Vector3(sizeGrid, 0, sizeGrid)));  // trên
        ListReturn.Add(RoundVector(center + new Vector3(-sizeGrid, 0, -sizeGrid)));  // dưới
        ListReturn.Add(RoundVector(center + new Vector3(-sizeGrid, 0, sizeGrid)));  // trên
        ListReturn.Add(RoundVector(center + new Vector3(sizeGrid, 0, -sizeGrid)));  // dưới


        return ListReturn;
    }
    public List<BlockControl> GetBlockLink(Vector3 Center)
    {
        List<BlockControl> ListReturn = new List<BlockControl>();
        List<Vector3> ListAround = GetHexNeighbors(Center);
       
        foreach (var j in ListAround)
        {
            foreach (Transform i in transform)
            {
                if (Vector3.zero == Center) Debug.Log(i.localPosition  + "  " + j);
                Vector3 local = i.localPosition;
                if (j == local)
                {
                    BlockControl Block = i.GetComponent<BlockControl>();
                    if (Block != null)
                    {
                        ListReturn.Add(Block);
                    }
                }
            }
        }
        return ListReturn;
    }
    private Vector3 RoundVector(Vector3 v)
    {
        return new Vector3(
            Mathf.Round(v.x * 100f) / 100f,
            Mathf.Round(v.y * 100f) / 100f,
            Mathf.Round(v.z * 100f) / 100f
        );
    }
    void OnDrawGizmos()
    {
        if(!DrawGrid) return ;
        Gizmos.color = UnityEngine.Color.red;

        foreach (var i in CenterGird) {
            DrawRect(i+DefaultCenter, sizeGrid, sizeGrid);
        }
    }
    void DrawRect(Vector3 center, float width, float height)
    {
        // Tính 4 góc của hình chữ nhật
        Vector3 halfSize = new Vector3(width / 2f, 0, height / 2f);
        Vector3[] points = new Vector3[4];
        points[0] = center + new Vector3(-halfSize.x, 0, -halfSize.z); // góc dưới trái
        points[1] = center + new Vector3(-halfSize.x, 0, halfSize.z);  // góc trên trái
        points[2] = center + new Vector3(halfSize.x, 0, halfSize.z);   // góc trên phải
        points[3] = center + new Vector3(halfSize.x, 0, -halfSize.z);  // góc dưới phải

        // Vẽ nhiều lần để "đậm" hơn
        for (int j = 0; j < 3; j++) // 3 lần cho dày hơn
        {
            Vector3 offset = new Vector3(0, 0.001f * j, 0);
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(points[i] + offset, points[(i + 1) % 4] + offset);
            }
        }
    }
}
