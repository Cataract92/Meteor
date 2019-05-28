using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{ 

    public static Dictionary<Block,Dictionary<(int,int,int),Block>> Chunks = new Dictionary<Block, Dictionary<(int, int, int), Block>>();

    public List<Block> Neighbours = new List<Block>();
    private Rigidbody _rigidbody;
    public bool IsAnchor = false;
    public Texture2D Texture;
    public int TextureId;

    public void applyTexture()
    {
        switch ( TextureId )
        {
            case 0:
            {
                GetComponent<Renderer>().material.color = Color.white;
                break;
            }
            case 1:
            {
                GetComponent<Renderer>().material.color = Color.green;
                break;
            }
            case 2:
            {
                GetComponent<Renderer>().material.color = Color.yellow;
                break;
            }
        }
        
    }

}
