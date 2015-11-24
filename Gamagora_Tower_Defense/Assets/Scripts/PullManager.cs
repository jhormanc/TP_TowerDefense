using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PullManager : ScriptableObject
{
    public int Size { get; private set; }

    private List<GameObject> _objects;
    private int _object_nb;

    public PullManager(GameObject src, int size)
    {
        _object_nb = -1;
    }

    public void Init(GameObject src, int size)
    {
        Size = size;

        _objects = new List<GameObject>();

        if (src != null)
        {
            _objects.Capacity = Size;

            for (int i = 0; i < Size; i++)
            {
                _objects.Add(Instantiate(src));
                _objects[i].SetActive(false);
            }
        }
    }

    private void SetNextObj()
    {
        if (_object_nb < Size - 1)
            _object_nb++;
        else
            _object_nb = 0;
    }

    public void RemoveObj(GameObject obj)
    {
        if (obj != null && _objects.Contains(obj))
        {
            obj.SetActive(false);
        }
    }

    public GameObject GetNextObj()
    {
        SetNextObj();
        GameObject obj = GetCurrentObj();
        UnityEditor.PrefabUtility.ResetToPrefabState(obj);
        obj.SetActive(true);
        return obj;
    }

	public GameObject GetCurrentObj()
    {
        if (_object_nb >= 0 && _object_nb < _objects.Count)
        {
            return _objects[_object_nb];
        }

        return null;
    }
}
