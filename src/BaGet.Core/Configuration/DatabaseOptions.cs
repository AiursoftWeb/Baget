using System.ComponentModel.DataAnnotations;

namespace Aiursoft.BaGet.Core
{
    public class DatabaseOptions
    {
        public string Type { get; set; }

        [Required]
        public string ConnectionString { get; set; }
    }
}
