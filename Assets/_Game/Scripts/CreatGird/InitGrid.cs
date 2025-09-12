using DG.Tweening.Core.Easing;
using Lean.Pool;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net.WebSockets;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

public class InitGrid : MonoBehaviour 
{
    [SerializeField] float  sizeGrid = 1;
    [SerializeField] int numberRandom = 6;
    [SerializeField] bool DrawGrid=false ;
    [SerializeField] Material MaterialLock;
    [SerializeField] ParticleSystem EffectLockCount;
    [SerializeField] GameObject ObjectPrefabLock;
    [SerializeField] GameObject ObjectPrefabLockCount;
    [SerializeField] GameObject ObjectPrefabUpport;
    public List<BlockControl> ListblockGround = new List<BlockControl>();
    public DragRotate rotate;
    private List<Vector3> CenterGird = new List<Vector3>();
    int NumberInit = 10;
    private void Start()
    {
        rotate = GetComponent<DragRotate>();
        foreach (Transform i in transform)
        {
            BlockControl bc = i.GetComponent<BlockControl>();
            if (bc != null)
            {
                if (bc.State == StateBlock.Lock) continue;
                bc.SpawnBlockChildWithBool();
            }
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

            frontier = newFrontier; 
        }
      
        foreach(var i in ListblockGround)
        {
         
           if(i!=null)
            i.SpawnBlockChild();
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
                bc.State = StateBlock.Nomal;
                bc.BacktoDFColor();
            }
            child.gameObject.layer = 3;
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
        List<Transform> pickedChildren = GetRandomChildren(transform, numberRandom);

        for (int i = 0; i < pickedChildren.Count; i++)
        {
            BlockControl block = pickedChildren[i].GetComponent<BlockControl>();
            if (block == null) continue;
            int colorIndex = Random.Range(0, 7);
            BlockColor color = GameManager.Ins.BlockData.BlockDataBase[colorIndex].Color;

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
                    ObjectGame[j].transform.localPosition = new Vector3(0, GamePlayManager.Ins.sizeYBlock * (j+1), 0);
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
        ListblockGround.Clear();
        List<Vector3> usedPositions = new List<Vector3>(); // danh sách vị trí đã dùng
        
        foreach (Transform i in transform)
        {
            BlockControl bcComponent = i.GetComponent<BlockControl>();
            if (bcComponent == null)
            {
                continue;
            }
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
            if (bcComponent != null)
            {
                if (bcComponent.GameObjectMod != null)
                {
                    DestroyImmediate(bcComponent.GameObjectMod.gameObject);
                    bcComponent.GameObjectMod = null;
                }
                bcComponent.PosionBlock = i.localPosition;
                bcComponent.ClearLink();
                ClearAllState(bcComponent);
                SetUpStateBlock(bcComponent, bcComponent.State);
                ListblockGround.Add(bcComponent);
            }

            usedPositions.Add(newPosition);
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
            DrawRect(i, sizeGrid, sizeGrid);
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
    public void ClearAllState(BlockControl bc)
    {
            if (bc.GameObjectMod != null) {

                DestroyImmediate(bc.gameObject);
                bc.GameObjectMod = null;
            }
            if (bc.Effect != null) {
                DestroyImmediate(bc.Effect.gameObject); 
                bc.Effect = null;
            }
       
    }
    public void SpawnEffect(BlockControl bcComponent)
    {
        ParticleSystem Effect = Instantiate(EffectLockCount, Vector3.zero, Quaternion.identity);
        Effect.transform.SetParent(bcComponent.transform, true);
        Effect.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        Effect.transform.localPosition = new Vector3(0, 0.02f, 0);
        bcComponent.Effect = Effect;
    }
    public void SetUpStateBlock(BlockControl bcComponent,StateBlock State)
    {
        if (bcComponent.State == StateBlock.none)
        {
            bcComponent.State = StateBlock.Nomal;
        }
        if (bcComponent.State == StateBlock.Lock)
        {
            bcComponent.GetComponent<Renderer>().sharedMaterial = MaterialLock;
            bcComponent.gameObject.layer = LayerMask.NameToLayer("Lock");
            GameObject ObjectLock = Instantiate(ObjectPrefabLock, Vector3.zero, Quaternion.identity);
            ObjectLock.transform.SetParent(bcComponent.transform, true);
            ObjectLock.transform.localScale = new Vector3(0.01f, 0.01f, bcComponent.transform.localScale.z);
            ObjectLock.transform.rotation = Quaternion.Euler(90, 0, 0);
            ObjectLock.transform.localPosition = Vector3.zero + new Vector3(0, 0.003f, 0);
            bcComponent.GameObjectMod = ObjectLock;
        }
        else if (bcComponent.State == StateBlock.Nomal)
        {
            bcComponent.gameObject.layer = 3;
            bcComponent.BacktoDFColor();
        }
        else if (bcComponent.State == StateBlock.LockCount)
        {
            bcComponent.GetComponent<Renderer>().sharedMaterial = MaterialLock;
            bcComponent.gameObject.layer = LayerMask.NameToLayer("Lock");
            GameObject ObjectLock = Instantiate(ObjectPrefabLockCount, Vector3.zero, Quaternion.identity);
            ObjectLock.transform.SetParent(bcComponent.transform, true);
            ObjectLock.transform.localScale = new Vector3(0.4f, 1f, 0.4f);
            ObjectLock.transform.localPosition = Vector3.zero + new Vector3(-0.004f,0.011f, 0.004f);
            bcComponent.GameObjectMod = ObjectLock;
        }
        else if (bcComponent.State == StateBlock.Support)
        {
            bcComponent.GetComponent<Renderer>().sharedMaterial = MaterialLock;
            bcComponent.gameObject.layer = LayerMask.NameToLayer("Lock");
            GameObject ObjectLock = Instantiate(ObjectPrefabUpport, Vector3.zero, Quaternion.identity);
            ObjectLock.transform.SetParent(bcComponent.transform, true);
            ObjectLock.transform.localScale = new Vector3(0.008f, 0.008f, 0.008f);
            ObjectLock.transform.localPosition = Vector3.zero + new Vector3(0,0.011f,0);
            bcComponent.GameObjectMod = ObjectLock;
        }
    }
}
