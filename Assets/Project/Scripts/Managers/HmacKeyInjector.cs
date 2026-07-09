using System.Reflection;
using UnityEngine;
using unityroom.Api;

namespace CrackShot
{
    public class HmacKeyInjector : MonoBehaviour
    {
        private const string HmacKeyResourceName = "HmacKey";
        private const string HmacKeyFieldName = "HmacKey";

        private void Start()
        {
            var keyAsset = Resources.Load<TextAsset>(HmacKeyResourceName);
            if (keyAsset == null || string.IsNullOrWhiteSpace(keyAsset.text))
            {
                return;
            }

            var client = UnityroomApiClient.Instance;
            if (client == null)
            {
                return;
            }

            var field = typeof(UnityroomApiClient).GetField(
                HmacKeyFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(client, keyAsset.text.Trim());
        }
    }
}
