using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoxCore.Utilities
{
    public static class DataStructureUtilityMethods
    {
        public static IEnumerable<Transform> GetAllTransformsInScene(Scene scene)
        {
            var queue = new Queue<Transform>();

            foreach (var root in scene.GetRootGameObjects())
            {
                var tf = root.transform;
                yield return tf;
                queue.Enqueue(tf);
            }

            while (queue.Count > 0)
            {
                foreach (Transform child in queue.Dequeue())
                {
                    yield return child;
                    queue.Enqueue(child);
                }
            }
        }
    }
}