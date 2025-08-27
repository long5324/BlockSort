using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class InitGrid : MonoBehaviour
{
    public float  sizeGrid = 1;
    public int NumberInit = 4;
    public Vector3 DefaultCenter = new Vector3(0, 1, 0);
    public bool DrawGrid=false ;
    List<Vector3> CenterGird = new List<Vector3>();
    List<BlockControl> ListblockGround = new List<BlockControl>();
    [ContextMenu("Init Grid")]
    public void StartInitGrid()
    {
        CenterGird.Clear();
     
        CenterGird.Add(DefaultCenter);

        List<Vector3> frontier = new List<Vector3>();
        frontier.Add(DefaultCenter);

        for (int j = 0; j < NumberInit; j++)
        {
            List<Vector3> newFrontier = new List<Vector3>();

            for (int i = 0; i < frontier.Count; i++)
            {
                Vector3 CenterCheck = frontier[i];
                List<Vector3> ListAdd = GetHexNeighbors(CenterCheck);

                for (int k = ListAdd.Count - 1; k >= 0; k--)
                {
                    if (CheckGrid(ListAdd[k]))
                    {
                        ListAdd.RemoveAt(k);
                    }
                }

                CenterGird.AddRange(ListAdd);
                newFrontier.AddRange(ListAdd);
            }

            frontier = newFrontier;
        }
    }
    [ContextMenu("Setup Block")]
    public void SetupBlock()
    {
        ListblockGround.Clear();
        ChangePositonGround();
    }
    void ChangePositonGround()
    {
        foreach(Transform i in gameObject.transform)
        {
            float Distance = DistanceXZ(i.transform.position, CenterGird[0] );
            Vector3 newPosition = CenterGird[0];
            foreach (var j in CenterGird)
            {
                if(Vector3.Distance(j,i.position) < Distance)
                {
                    Distance = DistanceXZ(j , i.position);
                    newPosition = j;
                }
            }
            i.position = new Vector3(newPosition.x , i.position.y , newPosition.z);
            BlockControl bc = i.GetComponent<BlockControl>();
            ListblockGround.Add(bc);
            bc.ClearLink();
        }
        LinkGroud();
    }
    void LinkGroud()
    {
        foreach(var i in ListblockGround)
        {
            List<Vector3> ListCheck = GetHexNeighbors(i.transform.position);
            foreach(var j in ListCheck)
            {
                foreach (var k in ListblockGround)
                {
                    if(j.z == k.transform.position.z && j.x == k.transform.position.x)
                    i.BlockLink.Add(k.GetComponent<BlockControl>());
                }
            }
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
        float dx = Mathf.Sqrt(3f) / 2f * sizeGrid; // ~0.866 * size
        float dz = 0.5f * sizeGrid;

        List<Vector3> ListReturn = new List<Vector3>();

        ListReturn.Add(RoundVector(center + new Vector3(dx, 0, dz)));   // phải trên
        ListReturn.Add(RoundVector(center + new Vector3(dx, 0, -dz)));  // phải dưới
        ListReturn.Add(RoundVector(center + new Vector3(0, 0, sizeGrid)));  // trên
        ListReturn.Add(RoundVector(center + new Vector3(0, 0, -sizeGrid))); // dưới
        ListReturn.Add(RoundVector(center + new Vector3(-dx, 0, dz)));  // trái trên
        ListReturn.Add(RoundVector(center + new Vector3(-dx, 0, -dz))); // trái dưới

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
            DrawHex(i, sizeGrid);
        }

    }
    void DrawHex(Vector3 center, float width)
    {
        float radius = width / 2f;
        float outerRadius = radius / Mathf.Cos(Mathf.PI / 6f);

        Vector3[] points = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.Deg2Rad * (60 * i);
            points[i] = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * outerRadius;
        }

        // Vẽ nhiều lần để "đậm" hơn
        for (int j = 0; j < 3; j++) // 3 lần cho dày hơn
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 offset = new Vector3(0, 0.001f * j, 0);
                Gizmos.DrawLine(points[i] + offset, points[(i + 1) % 6] + offset);
            }
        }
    }

}
