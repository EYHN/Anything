using Anything.FileSystem;
using Anything.FileSystem.Exception;
using Anything.Utils;
using NUnit.Framework;

namespace Anything.Tests.FileSystem.Exception;

public class ExceptionTests
{
    [Test]
    public void NewExceptionTest()
    {
        Assert.NotNull(new FileSystemException("test"));
        Assert.NotNull(new FileSystemException(Url.Parse("file://foo/bar")));
        Assert.NotNull(new FileSystemException(Url.Parse("file://foo/bar"), "test"));
        Assert.NotNull(new FileSystemException(new FileHandle("test_handle")));
        Assert.NotNull(new FileSystemException(new FileHandle("test_handle"), "test"));

        Assert.NotNull(new FileExistsException("test"));
        Assert.NotNull(new FileExistsException(Url.Parse("file://foo/bar")));
        Assert.NotNull(new FileExistsException(Url.Parse("file://foo/bar"), "test"));
        Assert.NotNull(new FileExistsException(new FileHandle("test_handle")));
        Assert.NotNull(new FileExistsException(new FileHandle("test_handle"), "test"));

        Assert.NotNull(new FileIsADirectoryException("test"));
        Assert.NotNull(new FileIsADirectoryException(Url.Parse("file://foo/bar")));
        Assert.NotNull(new FileIsADirectoryException(Url.Parse("file://foo/bar"), "test"));
        Assert.NotNull(new FileIsADirectoryException(new FileHandle("test_handle")));
        Assert.NotNull(new FileIsADirectoryException(new FileHandle("test_handle"), "test"));

        Assert.NotNull(new FileNotADirectoryException("test"));
        Assert.NotNull(new FileNotADirectoryException(Url.Parse("file://foo/bar")));
        Assert.NotNull(new FileNotADirectoryException(Url.Parse("file://foo/bar"), "test"));
        Assert.NotNull(new FileNotADirectoryException(new FileHandle("test_handle")));
        Assert.NotNull(new FileNotADirectoryException(new FileHandle("test_handle"), "test"));

        Assert.NotNull(new FileNotFoundException("test"));
        Assert.NotNull(new FileNotFoundException(Url.Parse("file://foo/bar")));
        Assert.NotNull(new FileNotFoundException(Url.Parse("file://foo/bar"), "test"));
        Assert.NotNull(new FileNotFoundException(new FileHandle("test_handle")));
        Assert.NotNull(new FileNotFoundException(new FileHandle("test_handle"), "test"));

        Assert.NotNull(new NoPermissionsException("test"));
        Assert.NotNull(new NoPermissionsException(Url.Parse("file://foo/bar")));
        Assert.NotNull(new NoPermissionsException(Url.Parse("file://foo/bar"), "test"));
        Assert.NotNull(new NoPermissionsException(new FileHandle("test_handle")));
        Assert.NotNull(new NoPermissionsException(new FileHandle("test_handle"), "test"));

        Assert.NotNull(new NotSupportedException("test"));
        Assert.NotNull(new NotSupportedException(Url.Parse("file://foo/bar")));
        Assert.NotNull(new NotSupportedException(Url.Parse("file://foo/bar"), "test"));
        Assert.NotNull(new NotSupportedException(new FileHandle("test_handle")));
        Assert.NotNull(new NotSupportedException(new FileHandle("test_handle"), "test"));

        Assert.NotNull(new UnavailableException("test"));
        Assert.NotNull(new UnavailableException(Url.Parse("file://foo/bar")));
        Assert.NotNull(new UnavailableException(Url.Parse("file://foo/bar"), "test"));
        Assert.NotNull(new UnavailableException(new FileHandle("test_handle")));
        Assert.NotNull(new UnavailableException(new FileHandle("test_handle"), "test"));
    }
}
