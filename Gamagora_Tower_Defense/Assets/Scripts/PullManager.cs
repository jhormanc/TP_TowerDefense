using UnityEngine;
using System.Collections;

public class PullManager : MonoBehaviour
{
    public float Size { get; private set; }

    private GameObject[] _objects;
    private int _object_nb;

    public PullManager(GameObject src, int size)
    {
        Size = size;
        if (src != null)
        {
            for (int i = 0; i < Size; i++)
            {
                _objects[i] = (GameObject)Instantiate(src);
                _objects[i].SetActive(false);
            }
        }

        _object_nb = 0;
    }

    public void SetNextObj()
    {
        if (_object_nb < Size - 1)
            _object_nb++;
        else
            _object_nb = 0;
    }

    public void RemoveObj(GameObject obj)
    {
        for (int i = 0; i < Size; i++)
        {
            if (_objects[i] == obj)
            {
                _objects[i].SetActive(false);
                break;
            }
        }
    }

	public GameObject GetObj()
    {
        if (_object_nb < Size)
        {
            GameObject obj = _objects[_object_nb];
            UnityEditor.PrefabUtility.ResetToPrefabState(obj);
            obj.SetActive(true);
            return obj;
        }

        return null;
    }
}
