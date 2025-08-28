using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class InitGrid : Singleton<InitGrid>
{
    public float  sizeGrid = 1;
    public int NumberInit = 4;
    public Vector3 DefaultCenter = new Vector3(5.5f, -5, 1);
    public bool DrawGrid=false ;
    [SerializeField] List<Vector3> CenterGird = new List<Vector3>();
    public  List<BlockData> ListblockGround  = new List<BlockData>();
    public LevelSave NewLevel;
    [ContextMenu("Init Grid")]
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
    void ChangePositonGround()
    {
        ListblockGround.Clear();  // reset list trước

        foreach (Transform i in transform)
        {
            Vector3 local = i.localPosition;
            float Distance = DistanceXZ(local, CenterGird[0]);
            Vector3 newPosition = CenterGird[0];

            foreach (var j in CenterGird)
            {
                if (Vector3.Distance(j, local) < Distance)
                {
                    Distance = DistanceXZ(j, local);
                    newPosition = j;
                }
            }

            i.localPosition = new Vector3(newPosition.x, 0, newPosition.z);

            BlockControl bcComponent = i.GetComponent<BlockControl>();
            if (bcComponent != null)
            {
                bcComponent.PosionBlock = i.localPosition;
                bcComponent.ClearLink();

                // Thêm bản sao dữ liệu vào list
                BlockData bcData = new BlockData(bcComponent);
                ListblockGround.Add(bcData);
            }
        }

        // nếu cần remove cái cuối thì dùng ListBlockData
        if (ListblockGround.Count > 0)
            ListblockGround.RemoveAt(ListblockGround.Count - 1);

        LinkGroud();
    }
    [ContextMenu("SaveLevel")]
    public void SaveLevel()
    {
        NewLevel.Database.sizeGrid = sizeGrid;
        NewLevel.Database.NumberInit = NumberInit;
        NewLevel.Database.DefaultCenter = DefaultCenter;

        // Tạo bản sao list Vector3
        NewLevel.Database.CenterGird = new List<Vector3>(CenterGird);

        // Tạo bản sao BlockData (không lưu BlockControl trực tiếp)
        List<BlockData> blockDataList = new List<BlockData>();
        foreach (var bc in ListblockGround)
        {
            blockDataList.Add(bc); 
        }
        NewLevel.Database.ListblockGround = blockDataList;
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
