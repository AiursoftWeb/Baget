using System.Text;
using Aiursoft.BaGet.Core.Configuration;
using Aiursoft.BaGet.Core.Storage;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Aiursoft.BaGet.Core.Tests.Services
{
    public class FileStorageServiceTests
    {
        public class GetAsync : FactsBase
        {
            [Fact]
            public async Task ThrowsIfStorePathDoesNotExist()
            {
                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    _target.GetAsync("hello.txt"));
            }

            [Fact]
            public async Task ThrowsIfFileDoesNotExist()
            {
                // Ensure the store path exists.
                Directory.CreateDirectory(StorePath);

                await Assert.ThrowsAsync<FileNotFoundException>(() =>
                    _target.GetAsync("hello.txt"));

                await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
                    _target.GetAsync("hello/world.txt"));
            }

            [Fact]
            public async Task GetsStream()
            {
                // Arrange
                await using (var content = StringStream("Hello world"))
                {
                    await _target.PutAsync("hello.txt", content, "text/plain");
                }

                // Act
                var result = await _target.GetAsync("hello.txt");

                // Assert
                Assert.Equal("Hello world", await ToStringAsync(result));
            }

            [Fact]
            public async Task NoAccessOutsideStorePath()
            {
                foreach (var path in OutsideStorePathData)
                {
                    await Assert.ThrowsAsync<ArgumentException>(() =>
                        _target.GetAsync(path));
                }
            }
        }

        public class GetDownloadUriAsync : FactsBase
        {
            [Fact]
            public async Task CreatesUriEvenIfDoesntExist()
            {
                var result = await _target.GetDownloadUriAsync("test.txt");
                var expected = new Uri(Path.Combine(StorePath, "test.txt"));

                Assert.Equal(expected, result);
            }

            [Fact]
            public async Task NoAccessOutsideStorePath()
            {
                foreach (var path in OutsideStorePathData)
                {
                    await Assert.ThrowsAsync<ArgumentException>(() =>
                        _target.GetDownloadUriAsync(path));
                }
            }
        }

        public class PutAsync : FactsBase
        {
            [Fact]
            public async Task SavesContent()
            {
                StoragePutResult result;
                await using (var content = StringStream("Hello world"))
                {
                    result = await _target.PutAsync("test.txt", content, "text/plain");
                }

                var path = Path.Combine(StorePath, "test.txt");

                Assert.True(File.Exists(path));
                Assert.Equal(StoragePutResult.Success, result);
                Assert.Equal("Hello world", await File.ReadAllTextAsync(path));
            }

            [Fact]
            public async Task ReturnsAlreadyExistsIfContentAlreadyExists()
            {
                // Arrange
                var path = Path.Combine(StorePath, "test.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new ArgumentNullException(nameof(path)));
                await File.WriteAllTextAsync(path, "Hello world");

                StoragePutResult result;
                await using (var content = StringStream("Hello world"))
                {
                    // Act
                    result = await _target.PutAsync("test.txt", content, "text/plain");
                }

                // Assert
                Assert.Equal(StoragePutResult.AlreadyExists, result);
            }

            [Fact]
            public async Task ReturnsConflictIfContentAlreadyExistsButContentsDoNotMatch()
            {
                // Arrange
                var path = Path.Combine(StorePath, "test.txt");

                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new ArgumentNullException(nameof(path)));
                await File.WriteAllTextAsync(path, "Hello world");

                StoragePutResult result;
                await using (var content = StringStream("foo bar"))
                {
                    // Act
                    result = await _target.PutAsync("test.txt", content, "text/plain");
                }

                // Assert
                Assert.Equal(StoragePutResult.Conflict, result);
            }

            [Fact]
            public async Task NoAccessOutsideStorePath()
            {
                foreach (var path in OutsideStorePathData)
                {
                    await using var content = StringStream("Hello world");
                    await Assert.ThrowsAsync<ArgumentException>(() =>
                        _target.PutAsync(path, content, "text/plain"));
                }
            }
        }

        public class DeleteAsync : FactsBase
        {
            [Fact]
            public async Task DoesNotThrowIfPathDoesNotExist()
            {
                await _target.DeleteAsync("test.txt");
            }

            [Fact]
            public async Task Deletes()
            {
                // Arrange
                var path = Path.Combine(StorePath, "test.txt");

                Directory.CreateDirectory(StorePath);
                await File.WriteAllTextAsync(path, "Hello world");

                // Act & Assert
                await _target.DeleteAsync("test.txt");

                Assert.False(File.Exists(path));
            }

            [Fact]
            public async Task NoAccessOutsideStorePath()
            {
                foreach (var path in OutsideStorePathData)
                {
                    await Assert.ThrowsAsync<ArgumentException>(() =>
                        _target.DeleteAsync(path));
                }
            }
        }

        public class FactsBase : IDisposable
        {
            protected readonly string StorePath;
            protected readonly FileStorageService _target;

            protected FactsBase()
            {
                StorePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
                var options = new Mock<IOptionsSnapshot<FileSystemStorageOptions>>();

                options
                    .Setup(o => o.Value)
                    .Returns(() => new FileSystemStorageOptions { Path = StorePath });

                _target = new FileStorageService(options.Object);
            }

            public void Dispose()
            {
                try
                {
                    Directory.Delete(StorePath, recursive: true);
                }
                catch (DirectoryNotFoundException)
                {
                }
            }

            protected Stream StringStream(string input)
            {
                var bytes = Encoding.ASCII.GetBytes(input);

                return new MemoryStream(bytes);
            }

            protected async Task<string> ToStringAsync(Stream input)
            {
                using var reader = new StreamReader(input);
                return await reader.ReadToEndAsync();
            }

            protected IEnumerable<string> OutsideStorePathData
            {
                get
                {
                    var fullPath = Path.GetFullPath(StorePath);
                    yield return "../file";
                    yield return ".";
                    yield return $"../{Path.GetFileName(StorePath)}";
                    yield return $"../{Path.GetFileName(StorePath)}suffix";
                    yield return $"../{Path.GetFileName(StorePath)}suffix/file";
                    yield return fullPath;
                    yield return fullPath + Path.DirectorySeparatorChar;
                    yield return fullPath + Path.DirectorySeparatorChar + "..";
                    yield return fullPath + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "file";
                    yield return Path.GetPathRoot(StorePath);
                    yield return Path.Combine(Path.GetPathRoot(StorePath) ?? throw new ArgumentNullException(nameof(StorePath)), "file");
                }
            }
        }
    }
}
