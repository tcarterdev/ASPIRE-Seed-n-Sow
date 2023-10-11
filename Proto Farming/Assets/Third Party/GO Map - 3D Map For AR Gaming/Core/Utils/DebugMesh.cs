using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMesh : MonoBehaviour {

    [Range(0.1f, 10)]
    public float scale = 2;

    [InspectorButton("Reload")] public bool reload;


    // Use this for initialization
    void Start () {

        CreateSpheres();
    }

    public void Reload()
    {
        CreateSpheres();
    }

    public void CreateSpheres () {

        while (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;

        foreach (Vector3 vertex in vertices)
        {
            //Spheres
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * scale;
            sphere.transform.position = new Vector3(vertex.x, vertex.y, vertex.z);
            sphere.transform.parent = transform;
        }
    }
}
