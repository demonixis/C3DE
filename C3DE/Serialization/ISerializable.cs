using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace C3DE.Serialization
{
    public interface ISerializable
    {
        Dictionary<string, object> Serialize();
        void Deserialize(Dictionary<string, object> data);
    }
}
