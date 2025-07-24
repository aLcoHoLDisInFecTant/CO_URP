using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{
    public readonly List<Vector3> GridPositions = new List<Vector3>();
    private int gridRowCount;

    public Grid(float gridLength)
    {
        gridRowCount = 1;
        float rowLength = gridLength / gridRowCount;

        foreach (var lane in LaneSystem.Instance.Lanes)
        {
            float lanePosition = lane * LaneSystem.Instance.LaneWidth;

            foreach (var layer in LaneSystem.Instance.Layers)
            {
                float layerPosition = layer * LaneSystem.Instance.LayerHeight;

                for (int i = 0; i < gridRowCount; i++)
                {
                    float z = i * rowLength;
                    Vector3 gridPosition = new Vector3(lanePosition, layerPosition, z);
                    GridPositions.Add(gridPosition);
                }
            }
        }
    }

    public Vector3 GetRandomPosition()
    {
        return GridPositions.GetRandomElement();
    }
}
