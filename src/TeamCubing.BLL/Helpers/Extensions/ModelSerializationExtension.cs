using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeamCubing.BLL.Helpers.Extensions;

public static class ModelSerializationExtension
{
    public static string ToJsonString<T>(this T model)
    {
        return JsonSerializer.Serialize(
            model,
            new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            });
    }
}
