using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lowscope.Saving.Examples
{
    /// <summary>
    /// Data container for warping between scenes
    /// </summary>
    public class ExampleGameWarpPointData : ScriptableObject
    {
        public string Scene;
        public Vector3 Position;
    }
}