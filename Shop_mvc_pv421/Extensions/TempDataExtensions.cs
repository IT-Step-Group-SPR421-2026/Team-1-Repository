using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace Shop_mvc_pv421.Extensions
{
    public static class TempDataExtensions
    {
        public static void Set<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonSerializer.Serialize(value);
        }

        public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            var item = tempData.ContainsKey(key) ? tempData[key] : null;
            return item is string value ? JsonSerializer.Deserialize<T>(value) : null;


        }
    }
}
