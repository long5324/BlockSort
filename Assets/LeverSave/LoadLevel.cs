using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadLevel : MonoBehaviour
{

    public LevelSave OpenLevel;
    public GameObject BottomBlock;
    public List<BlockControl> ListblockGround;

    [ContextMenu("Read Level")]
    public void ReadLevel()
    {
        int targetCount = OpenLevel.Database.ListblockGround.Count;
        int currentCount = transform.childCount;

        // Xóa thừa block
        if (currentCount > targetCount)
        {
            int toDelete = currentCount - targetCount;
            for (int i = 0; i < toDelete; i++)
            {
                DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
            }
        }
        // Thêm block nếu thiếu
        else if (currentCount < targetCount)
        {
            int toCreate = targetCount - currentCount;
            for (int i = 0; i < toCreate; i++)
            {
                GameObject newObj = Instantiate(BottomBlock, transform);
                newObj.name = "Child_" + (currentCount + i);
            }
        }

        SetPosition();
    }

    public void SetPosition()
    {
        int targetCount = OpenLevel.Database.ListblockGround.Count;

        for (int i = 0; i < targetCount; i++)
        {
            if (i >= transform.childCount) continue;

            Transform child = transform.GetChild(i);
            BlockControl block = child.GetComponent<BlockControl>();
            BlockData data = OpenLevel.Database.ListblockGround[i]; // dữ liệu thuần

            if (block != null && data != null)
            {
                // Set vị trí và thông tin
                child.localPosition = data.PosionBlock;
                block.PosionBlock = data.PosionBlock;
                block.ListChildBlock = data.ListChildBlock;
                block.BlockLink = data.BlockLink;
            }
        }


    }
}
