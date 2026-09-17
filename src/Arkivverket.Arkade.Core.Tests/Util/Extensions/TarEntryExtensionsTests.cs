using Arkivverket.Arkade.Core.Util;
using FluentAssertions;
using ICSharpCode.SharpZipLib.Tar;
using Xunit;

namespace Arkivverket.Arkade.Core.Tests.Util.Extensions;

public class TarEntryExtensionsTests
{
    [Fact]
    public void IsNoark5DocumentsEntryTest()
    {
        IsNoark5DocumentsEntry("uuid/content/dokumenter/5000000.pdf", "uuid").Should().BeTrue();
        IsNoark5DocumentsEntry("uuid/content/dokumenter/2024/5000000.pdf", "uuid").Should().BeTrue();
        IsNoark5DocumentsEntry("uuid/content/dokumenter/", "uuid").Should().BeTrue(); // The directory itself
        IsNoark5DocumentsEntry("uuid/content/DOKUMENT/5000000.pdf", "uuid").Should().BeTrue();
        IsNoark5DocumentsEntry("content/dokumenter/5000000.pdf", null).Should().BeTrue(); // Tar without root directory

        IsNoark5DocumentsEntry("uuid/content/arkivstruktur.xml", "uuid").Should().BeFalse();
        IsNoark5DocumentsEntry("uuid/content/sysdoc/LISTE_DOKUMENT.xlsx", "uuid").Should().BeFalse();

        // The documents directory is placed directly below the content directory, which in turn is
        // placed directly below the archive's root directory:
        IsNoark5DocumentsEntry("uuid/content/sysdoc/dokumenter/5000000.pdf", "uuid").Should().BeFalse();
        IsNoark5DocumentsEntry("uuid/dokumenter/5000000.pdf", "uuid").Should().BeFalse();

        // Names merely beginning with a documents directory name:
        IsNoark5DocumentsEntry("uuid/content/dokumentasjon/systemdokumentasjon.pdf", "uuid").Should().BeFalse();
        IsNoark5DocumentsEntry("uuid/content/dokumentliste.xlsx", "uuid").Should().BeFalse();
    }

    [Fact]
    public void GetRelativePathForNoark5DocumentEntryTest()
    {
        GetRelativePath("uuid/content/dokumenter/5000000.pdf", "uuid").Should().Be("dokumenter/5000000.pdf");
        GetRelativePath("uuid/content/dokumenter/2024/5000000.pdf", "uuid").Should().Be("dokumenter/2024/5000000.pdf");
        GetRelativePath("content/dokumenter/5000000.pdf", null).Should().Be("dokumenter/5000000.pdf");

        // A root directory name of its own containing a documents directory name:
        GetRelativePath("dokumenter/content/dokumenter/5000000.pdf", "dokumenter")
            .Should().Be("dokumenter/5000000.pdf");
    }

    private static bool IsNoark5DocumentsEntry(string entryName, string archiveRootDirectoryName)
        => TarEntry.CreateTarEntry(entryName).IsNoark5DocumentsEntry(archiveRootDirectoryName);

    private static string GetRelativePath(string entryName, string archiveRootDirectoryName)
        => TarEntry.CreateTarEntry(entryName).GetRelativePathForNoark5DocumentEntry(archiveRootDirectoryName);
}
