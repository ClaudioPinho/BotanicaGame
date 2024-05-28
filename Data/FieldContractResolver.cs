using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using BotanicaGame.Debug;

namespace BotanicaGame.Data;

public class FieldContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (member.MemberType == MemberTypes.Property)
        {
            property.ShouldSerialize = _ => false; // Ignore properties
        }
        
        if (member.MemberType == MemberTypes.Field)
        {
            property.ShouldSerialize = _ => true; // Ignore properties
        }

        return property;
    }
}