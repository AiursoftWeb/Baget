using System.Text;
using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;
using Xunit;
using Assert = Xunit.Assert;

// ReSharper disable InconsistentNaming

namespace Aiursoft.BaGet.Core.Tests.Services
{
    public class PackageStorageServiceTests
    {
        public class SavePackageContentAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfPackageIsNull()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _target.SavePackageContentAsync(
                        null,
                        packageStream: Stream.Null,
                        nuspecStream: Stream.Null,
                        readmeStream: Stream.Null,
                        iconStream: Stream.Null,
                        cancellationToken: TestContext.Current.CancellationToken));
            }

            [Fact]
            public async Task ThrowsIfPackageStreamIsNull()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _target.SavePackageContentAsync(
                        _package,
                        packageStream: null,
                        nuspecStream: Stream.Null,
                        readmeStream: Stream.Null,
                        iconStream: Stream.Null,
                        cancellationToken: TestContext.Current.CancellationToken));
            }

            [Fact]
            public async Task ThrowsIfNuspecStreamIsNull()
            {
                await Assert.ThrowsAsync<ArgumentNullException>(
                    () => _target.SavePackageContentAsync(
                        _package,
                        packageStream: Stream.Null,
                        nuspecStream: null,
                        readmeStream: Stream.Null,
                        iconStream: Stream.Null,
                        cancellationToken: TestContext.Current.CancellationToken));
            }

            [Fact]
            public async Task SavesContent()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);

                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                using (var readmeStream = StringStream("My readme"))
                using (var iconStream = StringStream("My icon"))
                {
                    // Act
                    await _target.SavePackageContentAsync(
                        _package,
                        packageStream: packageStream,
                        nuspecStream: nuspecStream,
                        readmeStream: readmeStream,
                        iconStream: iconStream,
                        cancellationToken: TestContext.Current.CancellationToken);

                    // Assert
                    Assert.True(_puts.ContainsKey(PackagePath));
                    Assert.Equal("My package", await ToStringAsync(_puts[PackagePath].Content));
                    Assert.Equal("binary/octet-stream", _puts[PackagePath].ContentType);

                    Assert.True(_puts.ContainsKey(NuspecPath));
                    Assert.Equal("My nuspec", await ToStringAsync(_puts[NuspecPath].Content));
                    Assert.Equal("text/plain", _puts[NuspecPath].ContentType);

                    Assert.True(_puts.ContainsKey(ReadmePath));
                    Assert.Equal("My readme", await ToStringAsync(_puts[ReadmePath].Content));
                    Assert.Equal("text/markdown", _puts[ReadmePath].ContentType);

                    Assert.True(_puts.ContainsKey(IconPath));
                    Assert.Equal("My icon", await ToStringAsync(_puts[IconPath].Content));
                    Assert.Equal("image/xyz", _puts[IconPath].ContentType);
                }
            }

            [Fact]
            public async Task DoesNotSaveReadmeIfItIsNull()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);

                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                {
                    // Act
                    await _target.SavePackageContentAsync(
                        _package,
                        packageStream: packageStream,
                        nuspecStream: nuspecStream,
                        readmeStream: null,
                        iconStream: null,
                        cancellationToken: TestContext.Current.CancellationToken);
                }

                // Assert
                Assert.False(_puts.ContainsKey(ReadmePath));
            }

            [Fact]
            public async Task NormalizesVersionWhenContentIsSaved()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Success);

                _package.Version = new NuGetVersion("1.2.3.0");
                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                using (var readmeStream = StringStream("My readme"))
                using (var iconStream = StringStream("My icon"))
                {
                    // Act
                    await _target.SavePackageContentAsync(
                        _package,
                        packageStream: packageStream,
                        nuspecStream: nuspecStream,
                        readmeStream: readmeStream,
                        iconStream: iconStream,
                        cancellationToken: TestContext.Current.CancellationToken);
                }

                // Assert
                Assert.True(_puts.ContainsKey(PackagePath));
                Assert.True(_puts.ContainsKey(NuspecPath));
                Assert.True(_puts.ContainsKey(ReadmePath));
            }

            [Fact]
            public async Task DoesNotThrowIfContentAlreadyExistsAndContentsMatch()
            {
                // Arrange
                SetupPutResult(StoragePutResult.AlreadyExists);

                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                using (var readmeStream = StringStream("My readme"))
                using (var iconStream = StringStream("My icon"))
                {
                    await _target.SavePackageContentAsync(
                        _package,
                        packageStream: packageStream,
                        nuspecStream: nuspecStream,
                        readmeStream: readmeStream,
                        iconStream: iconStream,
                        cancellationToken: TestContext.Current.CancellationToken);

                    // Assert
                    Assert.True(_puts.ContainsKey(PackagePath));
                    Assert.Equal("My package", await ToStringAsync(_puts[PackagePath].Content));
                    Assert.Equal("binary/octet-stream", _puts[PackagePath].ContentType);

                    Assert.True(_puts.ContainsKey(NuspecPath));
                    Assert.Equal("My nuspec", await ToStringAsync(_puts[NuspecPath].Content));
                    Assert.Equal("text/plain", _puts[NuspecPath].ContentType);

                    Assert.True(_puts.ContainsKey(ReadmePath));
                    Assert.Equal("My readme", await ToStringAsync(_puts[ReadmePath].Content));
                    Assert.Equal("text/markdown", _puts[ReadmePath].ContentType);

                    Assert.True(_puts.ContainsKey(IconPath));
                    Assert.Equal("My icon", await ToStringAsync(_puts[IconPath].Content));
                    Assert.Equal("image/xyz", _puts[IconPath].ContentType);
                }
            }

            [Fact]
            public async Task ThrowsIfContentAlreadyExistsButContentsDoNotMatch()
            {
                // Arrange
                SetupPutResult(StoragePutResult.Conflict);

                using (var packageStream = StringStream("My package"))
                using (var nuspecStream = StringStream("My nuspec"))
                using (var readmeStream = StringStream("My readme"))
                using (var iconStream = StringStream("My icon"))
                {
                    // Act
                    await Assert.ThrowsAsync<InvalidOperationException>(() =>
                        _target.SavePackageContentAsync(
                            _package,
                            packageStream: packageStream,
                            nuspecStream: nuspecStream,
                            readmeStream: readmeStream,
                            iconStream: iconStream,
                            cancellationToken: TestContext.Current.CancellationToken));
                }
            }
        }

        public class GetPackageStreamAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfStorageThrows()
            {
                // Arrange
                var cancellationToken = TestContext.Current.CancellationToken;
                _storage.ExpectedGet = (PackagePath, cancellationToken);
                _storage.GetException = new DirectoryNotFoundException();

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    _target.GetPackageStreamAsync(_package.Id, _package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = TestContext.Current.CancellationToken;
                using (var packageStream = StringStream("My package"))
                {
                    _storage.ExpectedGet = (PackagePath, cancellationToken);
                    _storage.GetContent = packageStream;

                    // Act
                    var result = await _target.GetPackageStreamAsync(_package.Id, _package.Version, cancellationToken);

                    // Assert
                    Assert.Equal("My package", await ToStringAsync(result));

                    Assert.Equal(1, _storage.GetCalls.Count(call => call == (PackagePath, cancellationToken)));
                }
            }
        }

        public class GetNuspecStreamAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfDoesntExist()
            {
                // Arrange
                var cancellationToken = TestContext.Current.CancellationToken;
                _storage.ExpectedGet = (NuspecPath, cancellationToken);
                _storage.GetException = new DirectoryNotFoundException();

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    _target.GetNuspecStreamAsync(_package.Id, _package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = TestContext.Current.CancellationToken;
                using (var nuspecStream = StringStream("My nuspec"))
                {
                    _storage.ExpectedGet = (NuspecPath, cancellationToken);
                    _storage.GetContent = nuspecStream;

                    // Act
                    var result = await _target.GetNuspecStreamAsync(_package.Id, _package.Version, cancellationToken);

                    // Assert
                    Assert.Equal("My nuspec", await ToStringAsync(result));

                    Assert.Equal(1, _storage.GetCalls.Count(call => call == (NuspecPath, cancellationToken)));
                }
            }
        }

        public class GetReadmeStreamAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfDoesntExist()
            {
                // Arrange
                var cancellationToken = TestContext.Current.CancellationToken;
                _storage.ExpectedGet = (ReadmePath, cancellationToken);
                _storage.GetException = new DirectoryNotFoundException();

                // Act
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    _target.GetReadmeStreamAsync(_package.Id, _package.Version, cancellationToken));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                var cancellationToken = TestContext.Current.CancellationToken;
                using (var readmeStream = StringStream("My readme"))
                {
                    _storage.ExpectedGet = (ReadmePath, cancellationToken);
                    _storage.GetContent = readmeStream;

                    // Act
                    var result = await _target.GetReadmeStreamAsync(_package.Id, _package.Version, cancellationToken);

                    // Assert
                    Assert.Equal("My readme", await ToStringAsync(result));

                    Assert.Equal(1, _storage.GetCalls.Count(call => call == (ReadmePath, cancellationToken)));
                }
            }
        }

        public class DeleteAsync : FactsBase
        {
            [Fact]
            public async Task Deletes()
            {
                // Act
                var cancellationToken = TestContext.Current.CancellationToken;
                await _target.DeleteAsync(_package.Id, _package.Version, cancellationToken);

                Assert.Equal(1, _storage.DeleteCalls.Count(call => call == (PackagePath, cancellationToken)));
                Assert.Equal(1, _storage.DeleteCalls.Count(call => call == (NuspecPath, cancellationToken)));
                Assert.Equal(1, _storage.DeleteCalls.Count(call => call == (ReadmePath, cancellationToken)));
            }
        }

        public class FactsBase
        {
            protected readonly Package _package = new()
            {
                Id = "My.Package",
                Version = new NuGetVersion("1.2.3")
            };

            protected readonly RecordingStorage _storage;
            protected readonly PackageStorageService _target;

            protected readonly Dictionary<string, (Stream Content, string ContentType)> _puts;

            public FactsBase()
            {
                _storage = new RecordingStorage();
                _target = new PackageStorageService(_storage, NullLogger<PackageStorageService>.Instance);

                _puts = new Dictionary<string, (Stream Content, string ContentType)>();
            }

            protected string PackagePath => Path.Combine("packages", "my.package", "1.2.3", "my.package.1.2.3.nupkg");
            protected string NuspecPath => Path.Combine("packages", "my.package", "1.2.3", "my.package.nuspec");
            protected string ReadmePath => Path.Combine("packages", "my.package", "1.2.3", "readme");
            protected string IconPath => Path.Combine("packages", "my.package", "1.2.3", "icon");

            protected Stream StringStream(string input)
            {
                var bytes = Encoding.ASCII.GetBytes(input);

                return new MemoryStream(bytes);
            }

            protected async Task<string> ToStringAsync(Stream input)
            {
                using (var reader = new StreamReader(input))
                {
                    return await reader.ReadToEndAsync();
                }
            }

            protected void SetupPutResult(StoragePutResult result)
            {
                _storage.PutResult = (path, content, contentType, _) =>
                {
                    _puts[path] = (content, contentType);
                    return Task.FromResult(result);
                };
            }
        }
    }
    public sealed class RecordingStorage : IStorageService
    {
        public (string Path, CancellationToken Token) ExpectedGet { get; set; }
        public Stream GetContent { get; set; }
        public Exception GetException { get; set; }
        public Func<string, Stream, string, CancellationToken, Task<StoragePutResult>> PutResult { get; set; }
        public List<(string Path, CancellationToken Token)> GetCalls { get; } = new();
        public List<(string Path, CancellationToken Token)> DeleteCalls { get; } = new();

        public Task<Stream> GetAsync(string path, CancellationToken cancellationToken = default)
        {
            GetCalls.Add((path, cancellationToken));
            Assert.Equal(ExpectedGet, (path, cancellationToken));
            return GetException == null ? Task.FromResult(GetContent) : Task.FromException<Stream>(GetException);
        }

        public Task<StoragePutResult> PutAsync(string path, Stream content, string contentType, CancellationToken cancellationToken = default)
            => PutResult(path, content, contentType, cancellationToken);

        public Task<Uri> GetDownloadUriAsync(string path, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            DeleteCalls.Add((path, cancellationToken));
            return Task.CompletedTask;
        }
    }
}
