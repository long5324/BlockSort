using System.Collections.Generic;
using UnityEngine;

public class ObjectSet : MonoBehaviour
{
    public List<ChildBlock> ListChildBlock { get; set; } = new List<ChildBlock>();
    public void AddLisst()
    {
        foreach(Transform i in transform)
        {
            ChildBlock bc = i.GetComponent<ChildBlock>();
            if(bc!=null)
            ListChildBlock.Add(bc);
        }
    }
}
