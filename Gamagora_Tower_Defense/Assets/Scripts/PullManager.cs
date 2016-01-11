using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PullManager : ScriptableObject
{
    public int Size { get; private set; }

    private List<GameObject> _objects;
    private GameObject _template;

    public PullManager()
    {

    }

    public void Init(GameObject src, int size, int max_size, Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion))
    {
        Size = size;
        _template = src;
        _objects = new List<GameObject>();

        if (src != null)
        {
            _objects.Capacity = max_size;

            for (int i = 0; i < Size; i++)
            {
                _objects.Add((GameObject)Instantiate(_template, pos, rot));
                _objects[i].SetActive(false);
            }
        }
    }

    public void Init(GameObject src, int size)
    {
        Init(src, size, size);
    }

    public void RemoveObj(GameObject obj)
    {
        if (obj != null && _objects.Contains(obj))
        {
            obj.SetActive(false);
        }
    }

    public void RemoveAll()
    {
        _objects.ForEach(x => x.SetActive(false));
    }

    public List<GameObject> GetAllActive()
    {
        return _objects.FindAll(x => x.activeSelf);
    }

    public GameObject GetNextObj(bool expand = false)
    {
        GameObject next = _objects.Find(x => x.activeSelf == false);

        if (next == null && expand && Size < _objects.Capacity)
        {
            _objects.Add(Instantiate(_template));
            _objects[Size].SetActive(false);
            next = _objects[Size];
            Size++;
        }

        if(next == null)
        {
            next = _objects.Find(x => x.activeSelf);
            next.SetActive(false);
        }
            
        
        //PrefabUtility.ResetToPrefabState(next);
        return next;
    }
}
