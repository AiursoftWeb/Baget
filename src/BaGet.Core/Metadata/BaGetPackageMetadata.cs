// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Text.Json.Serialization;
using Aiursoft.BaGet.Protocol.Models;

namespace Aiursoft.BaGet.Core.Metadata
{
    /// <summary>
    /// BaGet's extensions to the package metadata model. These additions
    /// are not part of the official protocol.
    /// </summary>
    public class BaGetPackageMetadata : PackageMetadata
    {
        [JsonPropertyName("hasReadme")]
        public bool HasReadme { get; set; }

        [JsonPropertyName("packageTypes")]
        public IReadOnlyList<string> PackageTypes { get; set; }

        /// <summary>
        /// The package's release notes.
        /// </summary>
        [JsonPropertyName("releaseNotes")]
        public string ReleaseNotes { get; set; }

        [JsonPropertyName("repositoryUrl")]
        public string RepositoryUrl { get; set; }

        [JsonPropertyName("repositoryType")]
        public string RepositoryType { get; set; }
    }
}
