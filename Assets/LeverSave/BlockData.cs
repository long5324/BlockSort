using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class BlockData
{
    public List<ChildBlock> ListChildBlock;
    public Vector3 PosionBlock;
    public List<BlockControl> BlockLink = new List<BlockControl>();

    public BlockData(BlockControl bc)
    {
        PosionBlock = bc.PosionBlock;
        ListChildBlock = bc.ListChildBlock;
        BlockLink = bc.BlockLink;
    }
}
