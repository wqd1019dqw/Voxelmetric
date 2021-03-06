﻿using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public static class Voxelmetric
{

    public static BlockPos GetBlockPos(RaycastHit hit, bool adjacent = false)
    {
        Vector3 pos = new Vector3(
            MoveWithinBlock(hit.point.x, hit.normal.x, adjacent),
            MoveWithinBlock(hit.point.y, hit.normal.y, adjacent),
            MoveWithinBlock(hit.point.z, hit.normal.z, adjacent)
            );

        return pos;
    }

    static float MoveWithinBlock(float pos, float norm, bool adjacent = false)
    {
        //Because of float imprecision we can't guarantee a hit on the side of a
        //block will be exactly 0.5 so we add a bit of padding
        float offset = pos - (int)pos;
        if ((offset > 0.49f && offset < 0.51) || (offset > -0.51f && offset < -0.49))
        {
            if (adjacent)
            {
                pos += (norm / 2);
            }
            else
            {
                pos -= (norm / 2);
            }
        }

        return pos;
    }

    public static bool SetBlock(RaycastHit hit, Block block, bool adjacent = false)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();

        if (chunk == null)
            return false;

        BlockPos pos = GetBlockPos(hit, adjacent);
        chunk.world.SetBlock(pos, block, !Config.Toggle.BlockLighting);

        if (Config.Toggle.BlockLighting)
        {
            BlockLight.LightArea(chunk.world, pos);
        }

        return true;
    }

    public static bool SetBlock(BlockPos pos, Block block, World world = null)
    {
        if (!world)
            world = World.instance;

        Chunk chunk = world.GetChunk(pos);
        if (chunk == null)
            return false;

        chunk.world.SetBlock(pos, block, !Config.Toggle.BlockLighting);

        if (Config.Toggle.BlockLighting)
        {
            BlockLight.LightArea(world, pos);
        }

        return true;
    }

    public static Block GetBlock(RaycastHit hit)
    {
        Chunk chunk = hit.collider.GetComponent<Chunk>();
        if (chunk == null)
            return Block.Air;

        BlockPos pos = GetBlockPos(hit, false);

        return GetBlock(pos, chunk.world);
    }

    public static Block GetBlock(BlockPos pos, World world = null)
    {
        if (!world)
            world = World.instance;

        Block block = world.GetBlock(pos);

        return block;
    }

    public static SaveProgress SaveAll(World world = null)
    {
        if (!world)
            world = World.instance;

        SaveProgress saveProgress = new SaveProgress(world.chunks.Keys);
        List<Chunk> chunksToSave = new List<Chunk>();
        chunksToSave.AddRange(world.chunks.Values);

        if (Config.Toggle.UseMultiThreading)
        {
            Thread thread = new Thread(() =>
           {

               foreach (var chunk in chunksToSave)
               {

                   while (!chunk.terrainGenerated || chunk.busy)
                   {
                       Thread.Sleep(0);
                   }

                   Serialization.SaveChunk(chunk);

                   saveProgress.SaveCompleteForChunk(chunk.pos);
               }
           });
            thread.Start();
        }
        else
        {
            foreach (var chunk in chunksToSave)
            {
                Serialization.SaveChunk(chunk);
                saveProgress.SaveCompleteForChunk(chunk.pos);
            }
        }

        return saveProgress;
    }
}