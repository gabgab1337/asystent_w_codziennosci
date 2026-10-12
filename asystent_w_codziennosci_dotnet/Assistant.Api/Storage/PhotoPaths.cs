using Microsoft.AspNetCore.Hosting;

namespace Assistant.Api.Storage
{
    internal static class PhotoPaths
    {
        private const string UploadsDirectoryName = "uploads";
        private const string StoredPathPrefix = "/uploads/";
        private const string DownloadPathPrefix = "/api/v1/files/";

        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public static string GetUploadsRoot(IWebHostEnvironment environment)
        {
            return Path.GetFullPath(
                Path.Combine(environment.ContentRootPath, "wwwroot", UploadsDirectoryName));
        }

        public static string ToStoredPath(string fileName)
        {
            return StoredPathPrefix + fileName;
        }

        public static bool HasAllowedExtension(string fileName)
        {
            return AllowedExtensions.Contains(Path.GetExtension(fileName));
        }

        public static string? ToDownloadPath(string? storedPath)
        {
            return TryGetStoredFileName(storedPath, out string fileName)
                ? DownloadPathPrefix + Uri.EscapeDataString(fileName)
                : null;
        }

        public static bool TryGetStoredFileName(string? storedPath, out string fileName)
        {
            fileName = string.Empty;
            if (string.IsNullOrWhiteSpace(storedPath))
            {
                return false;
            }

            string normalized = storedPath.Replace('\\', '/');
            if (!normalized.StartsWith(StoredPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return TryValidateFileName(normalized[StoredPathPrefix.Length..], out fileName);
        }

        public static bool TryResolveDownloadPath(
            string uploadsRoot,
            string? requestedPath,
            out string physicalPath)
        {
            physicalPath = string.Empty;
            if (!TryValidateFileName(requestedPath, out string fileName))
            {
                return false;
            }

            string root = Path.GetFullPath(uploadsRoot);
            string candidate = Path.GetFullPath(Path.Combine(root, fileName));
            string rootPrefix = root.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

            if (!candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            physicalPath = candidate;
            return true;
        }

        public static void DeleteStoredPhoto(
            IWebHostEnvironment environment,
            string? storedPath)
        {
            if (!TryGetStoredFileName(storedPath, out string fileName))
            {
                return;
            }

            string uploadsRoot = GetUploadsRoot(environment);
            if (TryResolveDownloadPath(uploadsRoot, fileName, out string physicalPath)
                && File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        private static bool TryValidateFileName(string? value, out string fileName)
        {
            fileName = string.Empty;
            if (string.IsNullOrWhiteSpace(value)
                || value.Contains("..", StringComparison.Ordinal)
                || value.Contains('/')
                || value.Contains('\\')
                || value.Contains(':')
                || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || Path.IsPathRooted(value)
                || !string.Equals(Path.GetFileName(value), value, StringComparison.Ordinal))
            {
                return false;
            }

            fileName = value;
            return true;
        }
    }
}
