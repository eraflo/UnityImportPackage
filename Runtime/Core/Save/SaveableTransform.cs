using UnityEngine;

namespace Eraflo.Catalyst.Core.Save
{
    /// <summary>
    /// Automatically saves and restores Transform data.
    /// </summary>
    public class SaveableTransform : MonoBehaviour, ISaveable
    {
        [System.Serializable]
        private struct TransformData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        public object SaveState()
        {
            return new TransformData
            {
                Position = transform.localPosition,
                Rotation = transform.localRotation,
                Scale = transform.localScale
            };
        }

        public void LoadState(object state)
        {
            if (state is TransformData data)
            {
                transform.localPosition = data.Position;
                transform.localRotation = data.Rotation;
                transform.localScale = data.Scale;
            }
            else if (state is Newtonsoft.Json.Linq.JObject jo)
            {
                // Handle JObject because JsonSerializer might return it when type name handling is not triggered
                var deserialized = jo.ToObject<TransformData>();
                transform.localPosition = deserialized.Position;
                transform.localRotation = deserialized.Rotation;
                transform.localScale = deserialized.Scale;
            }
        }
    }
}
