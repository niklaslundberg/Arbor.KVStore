using System;
using System.IO;
using JetBrains.Annotations;

namespace Arbor.KVStore
{
    public static class DirectoryExtensions
    {
        public static DirectoryInfo EnsureExists([NotNull] this DirectoryInfo directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            directory.Refresh();

            if (!directory.Exists)
            {
                directory.Create();
            }

            directory.Refresh();

            return directory;
        }
    }
}