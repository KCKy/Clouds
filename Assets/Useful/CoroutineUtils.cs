using System;
using System.Collections;
using UnityEngine;

namespace Useful
{
    public static class CoroutineUtils
    {
        public static IEnumerator CallDelayed(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action();
        }
    }
}
