using System;
using ICSharpCode.SharpZipLib.Tar;
using System.Linq;

namespace Arkivverket.Arkade.Core.Util
{
    public static class TarEntryExtensions
    {
        public static bool IsNoark5DocumentsEntry(this TarEntry tarEntry, string archiveRootDirectoryName)
        {
            string entryName = tarEntry.Name.Replace('\\', '/').TrimEnd('/');

            return ArkadeConstants.DocumentDirectoryNames.Any(documentDirectoryName =>
            {
                string documentsDirectoryPath = ContentDirectoryPath(archiveRootDirectoryName) + documentDirectoryName;

                return entryName.Equals(documentsDirectoryPath, StringComparison.Ordinal) ||
                       entryName.StartsWith(documentsDirectoryPath + '/', StringComparison.Ordinal);
            });
        }

        /// <summary>
        /// Gives the entry's path relative to the archive's content directory, which is how document
        /// files are named in the archive description and in METS.
        /// </summary>
        public static string GetRelativePathForNoark5DocumentEntry(this TarEntry tarEntry, string archiveRootDirectoryName)
        {
            string entryName = tarEntry.Name.Replace('\\', '/');

            string contentDirectoryPath = ContentDirectoryPath(archiveRootDirectoryName);

            return entryName.StartsWith(contentDirectoryPath, StringComparison.Ordinal)
                ? entryName[contentDirectoryPath.Length..]
                : entryName;
        }

        // archiveRootDirectoryName is the tar's actual internal root directory; null means the
        // tar has no single root and its entry paths start at the package level
        private static string ContentDirectoryPath(string archiveRootDirectoryName)
        {
            return archiveRootDirectoryName == null
                ? $"{ArkadeConstants.DirectoryNameContent}/"
                : $"{archiveRootDirectoryName}/{ArkadeConstants.DirectoryNameContent}/";
        }
    }
}
