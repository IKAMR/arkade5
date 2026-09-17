using System.Collections.Generic;
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
        => DiasTarArchiveUtility.CreateTarArchive(directory, "source.tar", entryNames);

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
