using System;
using UnityEngine;

public class MapFunction
{
    public delegate float EvaluateMapFunction(float x, float z);

    public string FunctionText { get; private set; }
    public Vector2 ARange { get; private set; }
    public Vector2 BRange { get; private set; }

    public float ARangeCenter => (ARange.y + ARange.x) / 2f;
    public float BRangeCenter => (BRange.y + BRange.x) / 2f;

    private EvaluateMapFunction evaluateMapFunction;

    public MapFunction(string functionText, Vector2 aRange, Vector2 bRange, EvaluateMapFunction evaluateMapFunction)
    {
        FunctionText = functionText;
        ARange = aRange;
        BRange = bRange;
        this.evaluateMapFunction = evaluateMapFunction;
    }

    public float Evaluate (float x, float z)
    {
        return evaluateMapFunction.Invoke(x, z);
    }
}
