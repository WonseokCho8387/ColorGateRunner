using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ColorGateRunner.Product
{
    public sealed class SystemClockService : IClockService
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    public sealed class GuidProfileIdGenerator : IProfileIdGenerator
    {
        public string CreateProfileId()
        {
            return Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
        }
    }

    public sealed class SystemLocalSaveFileSystem : ILocalSaveFileSystem
    {
        public bool FileExists(string path) => File.Exists(path);

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path, Encoding.UTF8);
        }

        public void WriteAllTextDurable(string path, string contents)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            byte[] bytes = new UTF8Encoding(false).GetBytes(contents);
            using (var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
        }

        public void CopyFile(
            string source,
            string destination,
            bool overwrite)
        {
            File.Copy(source, destination, overwrite);
        }

        public void MoveFile(string source, string destination)
        {
            File.Move(source, destination);
        }

        public void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void ReplaceFileAtomically(
            string source,
            string destination,
            string backup)
        {
            if (File.Exists(backup))
            {
                File.Delete(backup);
            }
            File.Replace(source, destination, backup, true);
        }
    }
}
