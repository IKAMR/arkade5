using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.Text;
using Arkivverket.Arkade.Core.Base;
using Arkivverket.Arkade.Core.Tests.UnitTestUtilities;
using FluentAssertions;
using ICSharpCode.SharpZipLib.Tar;
using Xunit;

namespace Arkivverket.Arkade.Core.Tests.Base;

public class DocumentFilesTest(TestSessionLifeTimeFilesFixture testSessionLifeTimeFilesFixture)
{
    [Fact]
    [Trait("Category", "Integration")]
    public void TransferFromTarToInformationPackageTest()
    {
        DirectoryInfo directory = testSessionLifeTimeFilesFixture.CreateIsolatedDirectory<DocumentFilesTest>();

        // A root directory name which occurs again further down the entry paths:
        string tarFilePath = CreateTarFile(directory, [
            "arkivuttrekk/content/arkivstruktur.xml",
            "arkivuttrekk/content/dokumenter/",
            "arkivuttrekk/content/dokumenter/arkivuttrekk.pdf"
        ]);

        var documentFiles = new DocumentFiles(tarFilePath, "arkivuttrekk");

        List<string> packageEntryNames = TransferToPackage(documentFiles, directory, "cd0b1f9c/");

        packageEntryNames.Should().BeEquivalentTo([
            "cd0b1f9c/content/dokumenter/",
            "cd0b1f9c/content/dokumenter/arkivuttrekk.pdf"
        ]);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void TransferFromRootlessTarToInformationPackageTest()
    {
        DirectoryInfo directory = testSessionLifeTimeFilesFixture.CreateIsolatedDirectory<DocumentFilesTest>();

        string tarFilePath = CreateTarFile(directory, [
            "content/arkivstruktur.xml",
            "content/dokumenter/5000000.pdf"
        ]);

        var documentFiles = new DocumentFiles(tarFilePath, null);

        List<string> packageEntryNames = TransferToPackage(documentFiles, directory, "cd0b1f9c/");

        packageEntryNames.Should().BeEquivalentTo(["cd0b1f9c/content/dokumenter/5000000.pdf"]);
    }

    private static string CreateTarFile(DirectoryInfo directory, IEnumerable<string> entryNames)
    {
        string tarFilePath = Path.Combine(directory.FullName, "source.tar");

        using Stream tarFileStream = File.Create(tarFilePath);
        using var tarWriter = new TarWriter(tarFileStream);

        foreach (string entryName in entryNames)
        {
            bool isDirectory = entryName.EndsWith('/');

            var entry = new PaxTarEntry(
                isDirectory ? TarEntryType.Directory : TarEntryType.RegularFile, entryName
            );

            if (!isDirectory)
                entry.DataStream = new MemoryStream("test"u8.ToArray());

            tarWriter.WriteEntry(entry);
        }

        return tarFilePath;
    }

    private static List<string> TransferToPackage(DocumentFiles documentFiles, DirectoryInfo directory,
        string packageRootDirectory)
    {
        string packageFilePath = Path.Combine(directory.FullName, "package.tar");

        using (Stream packageFileStream = File.Create(packageFilePath))
        using (var tarOutputStream = new TarOutputStream(packageFileStream, Encoding.UTF8))
            documentFiles.TransferFromTarToInformationPackage(tarOutputStream, packageRootDirectory);

        return DiasTarArchiveUtility.GetFileList(packageFilePath);
    }
}
