using UnityEngine;

namespace CrackShot
{
    public static class ComponentExtensions
    {
        public static T GetOrAdd<T>(this GameObject go) where T : Component
            => go.TryGetComponent<T>(out var c) ? c : go.AddComponent<T>();
    }
}
