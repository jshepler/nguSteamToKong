using System;
using System.Runtime.Serialization;

namespace nguSteamToKong
{
    public class nguSerializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName == "ModPlayerData")
                return typeof(PlayerData);

            if (typeName == "Cooking" || typeName == "Ingredient")
                return typeof(nguSerializationBinder);

            assemblyName = assemblyName.Replace("UnityEngine.CoreModule", "UnityEngine");
            typeName = typeName.Replace("UnityEngine.CoreModule", "UnityEngine");
            typeName = typeName.Replace("Culture=,", "Culture=neutral,");

            try
            {
                //Console.WriteLine($"assemblyName: {assemblyName}, typeName: {typeName}");
                return Type.GetType($"{typeName}, {assemblyName}");
            }

            catch
            {
                Console.WriteLine($"skipping {typeName}, {assemblyName}");
                return typeof(nguSerializationBinder);
            }
        }
    }

    public class NonSerializableSurrogateSelector : ISurrogateSelector
    {
        public void ChainSelector(ISurrogateSelector selector)
        {
            throw new NotImplementedException();
        }

        public ISurrogateSelector GetNextSelector()
        {
            throw new NotImplementedException();
        }

        public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (!type.IsSerializable)
            {
                //type not marked Serializable
                selector = this;
                return new NullSurrogate();
            }

            // use default surrogate
            selector = null;
            return null;
        }
    }

    public class NullSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }
    }
}
