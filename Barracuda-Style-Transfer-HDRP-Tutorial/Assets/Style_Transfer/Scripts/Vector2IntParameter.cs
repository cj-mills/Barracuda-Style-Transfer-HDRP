using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class Vector2IntParameter : VolumeParameter<Vector2Int>
{
    public Vector2IntParameter(Vector2Int value, bool overrideState = false)
        : base(value, overrideState)
    {

    }
}