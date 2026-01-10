using System;
using System.Collections.Generic;

namespace Eraflo.Catalyst.Core.Save
{
    /// <summary>
    /// Root data structure for a game save.
    /// </summary>
    [Serializable]
    public class GameData
    {
        public SaveMetadata Metadata;
        
        /// <summary>
        /// Map of Entity GUID -> (Component Type Name -> State Object)
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> Entities = new Dictionary<string, Dictionary<string, object>>();

        public GameData()
        {
            Metadata = new SaveMetadata();
        }

        public GameData(string name)
        {
            Metadata = new SaveMetadata { Name = name, Timestamp = DateTime.Now.Ticks };
        }
    }

    /// <summary>
    /// Lightweight metadata for a save file.
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        public string Name;
        public long Timestamp;
        public string Version;

        public DateTime GetDateTime() => new DateTime(Timestamp);
    }
}
