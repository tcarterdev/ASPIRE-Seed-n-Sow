using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmGrid : MonoBehaviour
{
    public bool showGizmos = true;
    public int grid_width = 64;
    public int grid_height = 64;
    public float gizmoSize = 0.125f;
    public float cellSize = 1f;

    public bool[,] boolMap;
    public Vector2Int playerPosInGrid;

    private void Awake()
    {
        boolMap = new bool[grid_width, grid_height];
    }

    public void AddObjectToFarmGrid(Vector2Int posInGrid)
    {
        boolMap[posInGrid.x, posInGrid.y] = true;
    }

    private void OnDrawGizmosSelected()
    {
        for (int x = 0; x < grid_width; x++)
        {
            for (int y = 0; y < grid_height; y++)
            {
                Vector3 cellPos = new Vector3((x * cellSize), 0f, y * cellSize);

                if (!Application.isPlaying) { return; }
                if (boolMap[x,y] == true) { Gizmos.color = Color.red; }
                else { Gizmos.color = Color.green; }
                
                Gizmos.DrawSphere(cellPos, gizmoSize);
            }
        }
    }
}
