using System;

namespace Eraflo.Catalyst.Core.Save
{
    /// <summary>
    /// Interface for object serialization.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes an object to a byte array.
        /// </summary>
        byte[] Serialize<T>(T obj);

        /// <summary>
        /// Deserializes an object from a byte array.
        /// </summary>
        T Deserialize<T>(byte[] data);

        /// <summary>
        /// Deserializes an object from a byte array into an existing instance.
        /// </summary>
        void Populate(byte[] data, object target);

        /// <summary>
        /// Efficiently reads a specific field from the serialized data without full deserialization.
        /// </summary>
        bool TryReadHeader<T>(byte[] data, string fieldName, out T value);
    }
}
