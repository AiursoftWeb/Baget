namespace Aiursoft.BaGet.Core
{
    /// <summary>
    /// The result of attempting to add the package to the database.
    /// </summary>
    public enum PackageAddResult
    {
        /// <summary>
        /// Failed to add the package as it already exists.
        /// </summary>
        PackageAlreadyExists,

        /// <summary>
        /// The package was added successfully.
        /// </summary>
        Success
    }
}
