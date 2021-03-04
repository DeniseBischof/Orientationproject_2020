using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lowscope.Saving.Examples
{
    [AddComponentMenu("")]
    public class ExampleGameBreakablePlatform : MonoBehaviour, ExampleGameIDamageable
    {
        bool isBroken = false;

        public void Damage(int amount)
        {
            if (!isBroken)
            {
                this.gameObject.SetActive(false);
            }
        }
    }
}