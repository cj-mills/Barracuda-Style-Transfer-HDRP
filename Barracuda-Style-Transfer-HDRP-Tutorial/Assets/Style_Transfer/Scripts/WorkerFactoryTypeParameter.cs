using UnityEngine;
using UnityEngine.Rendering;
using Unity.Barracuda;

[System.Serializable]
public class WorkerFactoryTypeParameter : VolumeParameter<WorkerFactory.Type>
{
    public WorkerFactoryTypeParameter(WorkerFactory.Type value, bool overrideState = false)
        : base(value, overrideState)
    {

    }
}