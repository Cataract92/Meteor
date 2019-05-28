using System;
using System.Collections.Generic;
using UnityEngine;


public class OldMap : MonoBehaviour
{
    public TextAsset MapPath;

    private List<Block> Anchors = new List<Block>();

    private void Start()
    {
        foreach (var block in FindObjectsOfType<Block>())
        {
            if (block.IsAnchor)
            {
                Anchors.Add(block);
            }
        }
    }

    private bool bla = true;

    private void Update()
    {
        if (bla)
        {
            //SaveMap();
            LoadMap();
            bla = false;
        }
    }

    /*
     * anchor.pos.x_anchor.pos.y_anchor.pos.z_anchor.rot.x_anchor.rot.y_anchor.rot.z_anchor.rot.w_freeze;box.pos.x_box.pos.y_box.pos.z_type;...
     */

    public void LoadMap()
    {
        string data = "-4_0_0_0_0_0_1_1;1_0_0_0;0_1_0_0;1_1_0_0;2_1_0_0;3_1_0_0;4_1_0_0;5_1_0_0;6_1_0_0;7_1_0_0;8_1_0_0;9_1_0_0;10_1_0_0\n"+
            "5,401671E-08_-0,003924072_0_0_0_0,3693193_0,9293026_0;1_0_0_0;0_1_0_0;1_1_0_0";

        Block.Chunks.Clear();

        foreach (var bulk in data.Split('\n'))
        {
            var parts = bulk.Split(';');

            var anchorParts = parts[0].Split('_');
            var anchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            anchor.transform.position = new Vector3(float.Parse(anchorParts[0]), float.Parse(anchorParts[1]),
                float.Parse(anchorParts[2]));
            anchor.transform.rotation = new Quaternion(float.Parse(anchorParts[3]), float.Parse(anchorParts[4]),
                float.Parse(anchorParts[5]), float.Parse(anchorParts[6]));
            var anchorBlock = anchor.AddComponent<Block>();
            if (anchor.GetComponent<Rigidbody>() == null)
                anchor.gameObject.AddComponent<Rigidbody>();
            if (anchorParts[7].Equals("1"))
            {
                anchor.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                anchor.GetComponent<Rigidbody>().useGravity = false;
            }

            anchorBlock.IsAnchor = true;
           // anchorBlock.Type = Block.BlockType.NORMAL;

            Block.Chunks.Add(anchorBlock, new Dictionary<(int, int, int), Block>());

            for (int i = 1; i < parts.Length; i++)
            {
                var blockParts = parts[i].Split('_');
                var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.transform.parent = anchor.transform;
                int x = int.Parse(blockParts[0]);
                int y = int.Parse(blockParts[1]);
                int z = int.Parse(blockParts[2]);
                block.transform.localPosition = new Vector3(x, y, z);
                block.transform.localRotation = new Quaternion();
                block.AddComponent<Block>();
                //block.GetComponent<Block>().Type = (Block.BlockType) Int32.Parse(blockParts[3]);
                Block.Chunks[anchorBlock].Add((x, y, z), block.GetComponent<Block>());
            }

            foreach (var block in anchor.GetComponentsInChildren<Block>())
            {
                var posInt = Vector3Int.RoundToInt(block.transform.localPosition);
                if (Block.Chunks[anchorBlock].ContainsKey((posInt.x-1, posInt.y, posInt.z)))
                    block.Neighbours.Add(Block.Chunks[anchorBlock][(posInt.x - 1, posInt.y, posInt.z)]);
                if (Block.Chunks[anchorBlock].ContainsKey((posInt.x + 1, posInt.y, posInt.z)))
                    block.Neighbours.Add(Block.Chunks[anchorBlock][(posInt.x + 1, posInt.y, posInt.z)]);
                if (Block.Chunks[anchorBlock].ContainsKey((posInt.x, posInt.y + 1, posInt.z)))
                    block.Neighbours.Add(Block.Chunks[anchorBlock][(posInt.x, posInt.y + 1, posInt.z)]);
                if (Block.Chunks[anchorBlock].ContainsKey((posInt.x, posInt.y - 1, posInt.z)))
                    block.Neighbours.Add(Block.Chunks[anchorBlock][(posInt.x, posInt.y - 1, posInt.z)]);
                if (Block.Chunks[anchorBlock].ContainsKey((posInt.x, posInt.y, posInt.z + 1)))
                    block.Neighbours.Add(Block.Chunks[anchorBlock][(posInt.x, posInt.y, posInt.z + 1)]);
                if (Block.Chunks[anchorBlock].ContainsKey((posInt.x, posInt.y, posInt.z - 1)))
                    block.Neighbours.Add(Block.Chunks[anchorBlock][(posInt.x, posInt.y, posInt.z - 1)]);
            }
        }
    }

    public void SaveMap()
    {
        var mapStrings = new List<string>();

        foreach (var anchor in Anchors)
        {
            var blockString = new List<string>();

            foreach (var block in anchor.GetComponentsInChildren<Block>())
            {
                if (block == anchor)
                    continue;
                /*
                blockString.Add(Math.Round(block.transform.localPosition.x) + "_" +
                                Math.Round(block.transform.localPosition.y) + "_" +
                                Math.Round(block.transform.localPosition.z) + "_" + (int) block.Type);
                                */
            }

            int isFrozen = anchor.GetComponent<Rigidbody>().constraints == RigidbodyConstraints.FreezeAll ? 1 : 0;
            mapStrings.Add(anchor.transform.position.x + "_" + anchor.transform.position.y + "_" +
                           anchor.transform.position.z + "_" + anchor.transform.rotation.x + "_" +
                           anchor.transform.rotation.y + "_" + anchor.transform.rotation.z + "_" +
                           anchor.transform.rotation.w + "_" + isFrozen + ";" + String.Join(";", blockString));
        }

        Debug.Log(String.Join("\n", mapStrings));
    }
}