using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoogleARCore.Examples.Common;
using GoogleARCore.Examples.HelloAR;
using UnityEditor;
using UnityEngine;


public class Map: MonoBehaviour
{
    public TextAsset MapPath;
    public GameObject LinePrefab;
    public GameObject PlayerPrefab;

    [HideInInspector]
    public Block Anchor;

    public Dictionary<(int,int,int),Block> BlockMap = new Dictionary<(int, int, int), Block>();

    public List<GameObject> Lines = new List<GameObject>();

    void Start()
    {
        //SaveMap();
        LoadMap();

        
        for (int i = -10; i < 10; i++)
        {
            for (int j = -10; j < 10; j++)
            {
                
                var line =  Instantiate(LinePrefab);
                line.transform.parent = transform;
                line.transform.localRotation = Quaternion.Euler(new Vector3(0, 90, 0));
                line.transform.localPosition = new Vector3(-10, i-0.5f,j - 0.5f);
                Lines.Add(line);
                
                line = Instantiate(LinePrefab);
                line.transform.parent = transform;
                line.transform.localRotation = Quaternion.Euler(new Vector3(270,0,0));
                line.transform.localPosition = new Vector3(i - 0.5f, -10, j - 0.5f);
                Lines.Add(line);
                
                line = Instantiate(LinePrefab);
                line.transform.parent = transform;
                line.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                line.transform.localPosition = new Vector3(i - 0.5f, j - 0.5f, -10);
                Lines.Add(line);
            }
        }
        
        foreach (var line in Lines)
        {
            line.SetActive(false);
        }
        
    }

    public void SaveMap()
    {
        if (Anchor == null)
        {
            Anchor = FindObjectsOfType<Block>().First(block => block.IsAnchor);
            if (Anchor == null)
            {
                Debug.LogError("No Anchor found!");
                return;
            }
        }

        var blockList = new List<string>();
        foreach (var block in FindObjectsOfType<Block>())
        {
            var pos = block.transform.localPosition;

            blockList.Add(pos.x+";"+ pos.y + ";" + pos.z + ";" + block.TextureId);
        }

        //File.WriteAllText(AssetDatabase.GetAssetPath(MapPath), String.Join("|", blockList));

    }

    public void LoadMap()
    {
        var d = "";
        for (int i = -5; i < 5; i++)
        {
            for (int j = -5; j < 5; j++)
            {
                d+=i+";0;"+j+";0|";
            }
        }
        Debug.Log(d);

        FindObjectOfType<HelloARController>().gameObject.SetActive(false);
        FindObjectOfType<PointcloudVisualizer>().gameObject.SetActive(false);
        FindObjectOfType<PlaneDiscoveryGuide>().enabled = false;
        FindObjectOfType<DetectedPlaneGenerator>().gameObject.SetActive(false);
        Resources.FindObjectsOfTypeAll<SingleJoystick>()[0].gameObject.SetActive(true);
        Resources.FindObjectsOfTypeAll<SingleJoystickTouchController>()[0].enabled = true;

        var blocks = MapPath.text.Split('|');
        Block tmpAnchor = null;

        foreach (var block in blocks)
        {
            var properties = block.Split(';');
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.parent = transform;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            var comp = go.AddComponent<Block>();
            var x = Int32.Parse(properties[0]);
            var y = Int32.Parse(properties[1]);
            var z = Int32.Parse(properties[2]);
            go.transform.localPosition = new Vector3(x,y,z);
            comp.TextureId = Int32.Parse(properties[3]);
            comp.applyTexture();
            tmpAnchor = comp;
            BlockMap.Add((x,y,z),comp);
        }
        
        if (BlockMap.ContainsKey((0, 0, 0)))
            Anchor = BlockMap[(0, 0, 0)];
        else
            Anchor = tmpAnchor;

        Anchor.IsAnchor = true;

        foreach (var block in FindObjectsOfType<Block>())
        {
            if (block != Anchor)
                block.transform.parent = Anchor.transform;

            Vector3Int pos = Vector3Int.RoundToInt(block.transform.localPosition);

            if (BlockMap.ContainsKey((pos.x + 1, pos.y, pos.z)))
                block.Neighbours.Add(BlockMap[(pos.x + 1, pos.y, pos.z)]);

            if (BlockMap.ContainsKey((pos.x - 1, pos.y, pos.z)))
                block.Neighbours.Add(BlockMap[(pos.x - 1, pos.y, pos.z)]);

            if (BlockMap.ContainsKey((pos.x, pos.y + 1, pos.z)))
                block.Neighbours.Add(BlockMap[(pos.x, pos.y + 1, pos.z)]);

            if (BlockMap.ContainsKey((pos.x, pos.y - 1, pos.z)))
                block.Neighbours.Add(BlockMap[(pos.x, pos.y - 1, pos.z)]);

            if (BlockMap.ContainsKey((pos.x, pos.y, pos.z + 1)))
                block.Neighbours.Add(BlockMap[(pos.x, pos.y, pos.z + 1)]);

            if (BlockMap.ContainsKey((pos.x, pos.y, pos.z - 1)))
                block.Neighbours.Add(BlockMap[(pos.x, pos.y, pos.z - 1)]);
        }

        int count = 0;
        while (true)
        {
            if (!BlockMap.ContainsKey((0, count, 0)))
                break;

            count++;
        }

        var player = Instantiate(PlayerPrefab,transform);
        player.transform.localPosition = new Vector3(0,count,0);
    }
}