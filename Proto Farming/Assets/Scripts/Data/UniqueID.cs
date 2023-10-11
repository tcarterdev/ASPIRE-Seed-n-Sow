using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
public class UniqueID : MonoBehaviour
{
    [ReadOnly, SerializeField] private string uid = Guid.NewGuid().ToString();
    [SerializeField] private static SerializableDictionary<string, GameObject> idDatabase = new SerializableDictionary<string, GameObject>();

    public string GetUID() => uid;

    private void Start()
    {
        if (idDatabase.ContainsKey(uid)) { Generate(); }
        else { idDatabase.Add(uid, this.gameObject); }
    }

    private void OnDestroy()
    {
        if (idDatabase.ContainsKey(uid)) { idDatabase.Remove(uid); }
    }

    private void Generate()
    {
        uid = Guid.NewGuid().ToString();
        idDatabase.Add(uid, this.gameObject);
    }
}
