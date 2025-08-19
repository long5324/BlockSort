using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;
    [SerializeField] List<GameObject> ListBlockGamePlay;
    [SerializeField] GameObject BottomBlockGameObject;
    List<GameObject> BottomBlock = new List<GameObject>();
    Camera cam;
    GameObject selectedBlock;
    BlockMaterialControl blockMaterialControl;
    List<Vector3> ListDefaulPossitionBlockGamePlay = new List<Vector3>();
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
                BottomBlock.Add(BottomBlockGameObject.transform.GetChild(i).GetChild(j).gameObject);
            }

        }
        else { Debug.Log(1); }
    }
    

    private void Update()
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

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            Vector3 targetPos = hit.point;
            Ray downRay = new Ray(targetPos + Vector3.up * 50f, Vector3.down);
            RaycastHit downHit;
            if (Physics.Raycast(downRay, out downHit, 100f, groundLayer))
            {
                foreach (var i in BottomBlock)
                {
                    if (blockMaterialControl != null)
                        i.GetComponent<Renderer>().material = blockMaterialControl.MaterialGray;
                }
                if (downHit.collider.gameObject.CompareTag("BottomBlock"))
                {
                    if (blockMaterialControl != null)
                    {
                        downHit.collider.gameObject.GetComponent<Renderer>().material = blockMaterialControl.MaterialOR;
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
                i.GetComponent<Renderer>().material = blockMaterialControl.MaterialGray;
        }
        selectedBlock = null;
        for (int i = 0; i < ListBlockGamePlay.Count; i++)
        {
            ListBlockGamePlay[i].transform.position = ListDefaulPossitionBlockGamePlay[i];
        }
    }
}
