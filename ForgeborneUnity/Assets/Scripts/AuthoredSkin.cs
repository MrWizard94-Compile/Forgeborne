using System;
using UnityEngine;

/// <summary>Game-space wrapper preserves the authored model's import-axis/rest transform.</summary>
public static class AuthoredSkin
{
    public static GameObject Create(GameObject prefab,Transform parent,string label)
    {
        if(prefab==null) throw new ArgumentNullException(nameof(prefab));
        var root=new GameObject(label);
        root.transform.SetParent(parent,false);
        UnityEngine.Object.Instantiate(prefab,root.transform,false);
        if(root.GetComponentsInChildren<Collider>(true).Length!=0)
        {
            UnityEngine.Object.Destroy(root);
            throw new InvalidOperationException("Authored view art must not create parallel collision ownership.");
        }
        return root;
    }
}
