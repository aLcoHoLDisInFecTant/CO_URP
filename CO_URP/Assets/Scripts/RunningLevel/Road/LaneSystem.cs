using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaneSystem : MonoBehaviour, IResettable
{
    static public LaneSystem Instance { get; private set; }
    [field: SerializeField] public float LaneWidth { get; private set; }

    [SerializeField] private int laneCount;
    
    [SerializeField] private int layerCount;

    [field: SerializeField] public float LayerHeight { get; private set; }
    public List<int> Lanes { get; private set; }
    public float CurrentPosition { get; private set; }
    public float TargetPosition { get; set; }
    public float CurrentOffset { get; private set; }
    public int TargetLane { get; private set; }
    public int CenterLane { get; private set; }

    public List<int> Layers { get; private set; }

    public int TargetLayer { get; private set; }

    public float TargetLayerPosition { get; private set; }

    public int CenterLayer { get; private set; }

    public readonly Dictionary<int, float> LanesDict = new Dictionary<int, float>();

    private void Awake()
    {
        Instance = this;
        Lanes = new List<int>(laneCount);
        bool isLanesEven = laneCount % 2 == 0;
        if (isLanesEven)
        {
            for (int i = -laneCount / 2; i < laneCount / 2; i++)
            {
                Lanes.Add(i);
            }
        }
        else
        {
            for (int i = -laneCount / 2; i <= laneCount / 2; i++)
            {
                Lanes.Add(i);
            }
        }

        if (isLanesEven)
        {
            for (int i = -laneCount / 2; i < laneCount / 2; i++)
            {
                LanesDict.Add(i, i * LaneWidth);
            }
        }
        else
        {
            for (int i = -laneCount / 2; i <= laneCount / 2; i++)
            {
                LanesDict.Add(i, i * LaneWidth);
            }
        }

        Layers = new List<int>(layerCount);
        for (int i = -layerCount / 2; i <= layerCount / 2; i++)
        {
            Layers.Add(i);
        }
        TargetLayer = Layers[layerCount / 2];
        TargetLayerPosition = TargetLayer * LayerHeight;

        ResetToDefault();
    }
    public void IncreaseTargetLane(int amount)
    {
        TargetLane += amount;
        if (TargetLane > Lanes[Lanes.Count - 1])
        {
            TargetLane -= amount;
            return;
        }
        TargetPosition += LaneWidth;
        CurrentOffset += LaneWidth;
    }

    public void DecreaseTargetLane(int amount)
    {
        TargetLane -= amount;
        if (TargetLane < Lanes[0])
        {
            TargetLane += amount;
            return;
        }
        TargetPosition -= LaneWidth;
        CurrentOffset -= LaneWidth;
    }

    public void IncreaseTargetLayer(int amount)
    {
        TargetLayer += amount;
        if (TargetLayer > Layers[Layers.Count - 1])
        {
            TargetLayer -= amount;
            return;
        }
        TargetLayerPosition += LayerHeight;
    }

    public void DecreaseTargetLayer(int amount)
    {
        TargetLayer -= amount;
        if (TargetLayer < Layers[0])
        {
            TargetLayer += amount;
            return;
        }
        TargetLayerPosition -= LayerHeight;
    }

    public bool IsOnTargetLane(float position)
    {
        return Mathf.Abs(TargetPosition - position) < 0.01f;
    }

    public bool IsOnTargetLayer(float position) 
    {
        return Mathf.Abs(TargetLayerPosition - position) < 0.01f;
    }
    public float CalculateDistanceToTargetLane(float position)
    {
        return TargetPosition - position;
    }

    public float CalculateDistanceToTargetLayer(float position) 
    {
        return TargetLayerPosition - position;
    }

    public void ResetToDefault()
    {
        TargetLane = Lanes[laneCount / 2];
        CenterLane = Lanes[laneCount / 2];
        TargetLayer = Layers[laneCount / 2];
        CenterLayer = Layers[laneCount / 2];
        CurrentOffset = 0;
        CurrentPosition = 0;
        TargetPosition = 0;
    }
}
