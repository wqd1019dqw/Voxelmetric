﻿using UnityEngine;
using System.Collections.Generic;

public class BlockIndex {

    public BlockIndex(){
        AddBlockType(new BlockAir());
    }

    public List<BlockController> controllers = new List<BlockController>();
    public Dictionary<string, int> names = new Dictionary<string, int>();

    public int AddBlockType(BlockController controller)
    {
        int index = controllers.Count;

        if (index == ushort.MaxValue)
        {
            Debug.LogError("Too many block types!");
            return -1;
        }

        controllers.Add(controller);
        names.Add(controller.Name().ToLower().Replace(" ", ""), index);
        return index;
    }

    public void GetMissingDefinitions() {
        BlockDefenition[] definitions = World.instance.gameObject.GetComponents<BlockDefenition>();

        foreach (var def in definitions)
        {
            if(def.enabled)
                def.AddToBlocks();
        }

    }

}
