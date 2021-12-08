using UnityEngine;
using UnityEngine.Rendering;
using Unity.Barracuda;

[System.Serializable]
public class NNModelParameter : VolumeParameter<NNModel>
{
    public NNModelParameter(NNModel value, bool overrideState = false)
        : base(value, overrideState)
    {

    }
}