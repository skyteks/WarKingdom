﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "new ListHolderObject", menuName = "List Holder Object")]
public class ListHolderObject : ScriptableObject
{
    public System.Type typeOfListObjects = null;
    private List<MonoBehaviour> list = new List<MonoBehaviour>();

    private void OnEnable()
    {
        list.Clear();
    }

    public bool AddObject(MonoBehaviour obj)
    {
        if (obj == null)
        {
            throw new System.NullReferenceException();
        }
        if (list.Contains(obj))
        {
            return false;
        }

        if (typeOfListObjects == null)
        {
            typeOfListObjects = obj.GetType();
        }
        else
        {
            System.Type objType = obj.GetType();
            if (typeOfListObjects.BaseType == objType.BaseType && objType.BaseType != typeof(MonoBehaviour) && objType.BaseType != typeof(Object))
            {
                typeOfListObjects = typeOfListObjects.BaseType;
            }
            if (!(typeOfListObjects.IsAssignableFrom(objType) || objType.IsAssignableFrom(typeOfListObjects)))
            {
                throw new System.ArrayTypeMismatchException(string.Concat("List Type: ", typeOfListObjects.ToString(), "  Object Type: ", objType.ToString()));
            }
        }

        list.Add(obj);
        return true;
    }

    public bool RemoveObject(MonoBehaviour obj)
    {
        if (obj == null)
        {
            throw new System.NullReferenceException();
        }

        return list.Remove(obj);
    }

    public bool ContainsObject(MonoBehaviour obj)
    {
        if (obj == null)
        {
            throw new System.NullReferenceException();
        }
        return list.Contains(obj);
    }

    public MonoBehaviour GetByIndex(int index)
    {
        return list[index];
    }

    public IEnumerable<MonoBehaviour> GetEnumerable()
    {
        return list.AsEnumerable();
    }

#if UNITY_EDITOR
    public List<MonoBehaviour> listForEditor { get { return list; } }
#endif
}
