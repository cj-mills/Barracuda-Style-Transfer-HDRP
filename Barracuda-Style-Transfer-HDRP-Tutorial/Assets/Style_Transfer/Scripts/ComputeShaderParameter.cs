using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class ComputeShaderParameter : VolumeParameter<ComputeShader>
{
    public ComputeShaderParameter(ComputeShader value, bool overrideState = false)
        : base(value, overrideState)
    {

    }
}