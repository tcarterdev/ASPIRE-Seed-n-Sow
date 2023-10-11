using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    [SerializeField] private float speed;
    private Vector3 startingPos;

    private void Start()
    {
        startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3();
        newPos.x += speed * Time.deltaTime;
        transform.position += newPos;

        if (transform.position.x < -125)
        {
            Debug.Log("Cloud reset");
            transform.position = startingPos;
        }
    }
}
