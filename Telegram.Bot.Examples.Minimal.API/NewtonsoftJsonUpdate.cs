using Newtonsoft.Json;
using System.Reflection;
using Telegram.Bot.Types;

namespace Telegram.Bot.Examples.Minimal.API;

// The Telegram.Bot library heavily depends on Newtonsoft.Json library to deserialize
// incoming webhook updates and send serialized responses back.
// So, as Minimal API uses System.Text.Json instead of Newtonsoft, we can use custom model binding
// to handle incoming model with Newtonsoft.Json JsonConvert manually.
// Please check the following link for the unified approach:
// https://stackoverflow.com/questions/69850917/how-to-configure-newtonsoftjson-with-minimalapi-in-net-6-0
public class NewtonsoftJsonUpdate : Update
{
    public static async ValueTask<NewtonsoftJsonUpdate?> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        using var streamReader = new StreamReader(context.Request.Body);
        var updateJsonString = await streamReader.ReadToEndAsync();

        return JsonConvert.DeserializeObject<NewtonsoftJsonUpdate>(updateJsonString);
    }
}
