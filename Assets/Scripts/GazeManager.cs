using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.UI;

public class GazeManager : MonoBehaviour
{
    public enum GazeMode
    {
        None,
        Add,
        Delete
    }

    public GazeMode Mode /*{ get; private set; }*/ = GazeMode.None;

    private GameObject _cube,_sphere;
    // Start is called before the first frame update
    void Start()
    {
        _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube.GetComponent<Collider>().enabled = false;
        _cube.layer = 2;
        _cube.GetComponent<Renderer>().material.color = Color.green;
        _cube.SetActive(false);

        /*
        _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _sphere.GetComponent<Collider>().enabled = false;
        _sphere.layer = 2;
        _sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        _sphere.GetComponent<Renderer>().material.color = Color.red;
        */

        var dropdown = FindObjectOfType < TMP_Dropdown >();
        dropdown.onValueChanged.AddListener( i =>
        {
            SetTexture( dropdown.value );
        } );
    }

    private Transform _previousBox = null;
    // Update is called once per frame
    void Update()
    {
        bool cubeActive = false;
        switch (Mode)
        {
            case GazeMode.Add:
            {
                if (_previousBox != null)
                    _previousBox.GetComponent<Block>().applyTexture();


                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)),
                    out hit, float.MaxValue, ~(1 << 2)))
                {
                    if (!hit.transform.GetComponent<Block>())
                        return;

                    _cube.transform.position = hit.transform.position + hit.normal;
                    _cube.transform.rotation = hit.transform.rotation;
                    cubeActive = true;
                }
                break;
            }
            case GazeMode.Delete:
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)),
                    out hit, float.MaxValue, ~(1 << 2)))
                {
                    if (!hit.transform.GetComponent<Block>())
                        return;

                    if (_previousBox != hit.transform)
                    {
                        if (_previousBox != null)
                        {
                            _previousBox.GetComponent<Block>().applyTexture();
                        }

                        _previousBox = hit.transform;
                        _previousBox.GetComponent<Renderer>().material.color = Color.red;
                       
                    }
                }
                break;
            }
        }
        _cube.SetActive(cubeActive);
    }

    public void Spawn(bool active)
    {
        Mode = GazeMode.Add;
        //GameObject.Find("ToggleDelete").GetComponent<Toggle>().isOn = false;
    }

    public void Delete(bool active)
    {
        Mode = GazeMode.Delete;
        //GameObject.Find("ToggleSpawn").GetComponent<Toggle>().isOn = false;
    }

    public int TextureID = 0;
    public void SetTexture( int index )
    {
        Debug.Log( index );
        TextureID = index;
    }

    public void Add()
    {
        if (Mode != GazeMode.Add)
            return;

        var map = FindObjectOfType<Map>();
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = _cube.transform.position;
        go.transform.rotation = _cube.transform.rotation;
        go.transform.parent = map.transform;
        var comp = go.AddComponent<Block>();
        comp.TextureId = TextureID;
        comp.applyTexture();
        var pos = Vector3Int.RoundToInt(comp.transform.localPosition);
        map.BlockMap.Add((pos.x,pos.y,pos.y), comp);

        if (map.BlockMap.ContainsKey((pos.x + 1, pos.y, pos.z)))
        {
            comp.Neighbours.Add(map.BlockMap[(pos.x + 1, pos.y, pos.z)]);
            map.BlockMap[(pos.x + 1, pos.y, pos.z)].Neighbours.Add(comp);
        }
        if (map.BlockMap.ContainsKey((pos.x - 1, pos.y, pos.z)))
        {
            comp.Neighbours.Add(map.BlockMap[(pos.x - 1, pos.y, pos.z)]);
            map.BlockMap[(pos.x - 1, pos.y, pos.z)].Neighbours.Add(comp);
        }
        if (map.BlockMap.ContainsKey((pos.x, pos.y+1, pos.z)))
        {
            comp.Neighbours.Add(map.BlockMap[(pos.x, pos.y+1, pos.z)]);
            map.BlockMap[(pos.x, pos.y+1, pos.z)].Neighbours.Add(comp);
        }
        if (map.BlockMap.ContainsKey((pos.x, pos.y - 1, pos.z)))
        {
            comp.Neighbours.Add(map.BlockMap[(pos.x, pos.y - 1, pos.z)]);
            map.BlockMap[(pos.x, pos.y - 1, pos.z)].Neighbours.Add(comp);
        }
        if (map.BlockMap.ContainsKey((pos.x, pos.y, pos.z+1)))
        {
            comp.Neighbours.Add(map.BlockMap[(pos.x, pos.y, pos.z+1)]);
            map.BlockMap[(pos.x, pos.y, pos.z+1)].Neighbours.Add(comp);
        }
        if (map.BlockMap.ContainsKey((pos.x, pos.y, pos.z - 1)))
        {
            comp.Neighbours.Add(map.BlockMap[(pos.x, pos.y, pos.z - 1)]);
            map.BlockMap[(pos.x, pos.y, pos.z - 1)].Neighbours.Add(comp);
        }
    }

    public void Remove()
    {
        if (Mode != GazeMode.Delete)
            return;

        var map = FindObjectOfType<Map>();
        var comp = _previousBox.GetComponent<Block>();

        foreach (var neighbour in comp.Neighbours)
        {
            neighbour.Neighbours.Remove(comp);
        }

        var pos = Vector3Int.RoundToInt(comp.transform.localPosition);
        map.BlockMap.Remove((pos.x, pos.y, pos.z));

        Destroy(comp.gameObject);

    }

    public void Grid(bool activate)
    {
        var lines = FindObjectOfType<Map>().Lines;
        foreach (var line in lines)
        {
            line.SetActive(!line.activeSelf);
        }
    }
}
