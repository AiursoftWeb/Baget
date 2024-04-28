// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Text.Json.Serialization;

namespace Aiursoft.BaGet.Protocol.Models
{
    public class AutocompleteContext
    {
        public static readonly AutocompleteContext Default = new()
        {
            Vocab = "http://schema.nuget.org/schema#"
        };

        [JsonPropertyName("@vocab")]
        public string Vocab { get; set; }
    }
}
