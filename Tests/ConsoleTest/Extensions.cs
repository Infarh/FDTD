using System;
using System.Collections.Generic;
using System.IO;

namespace ConsoleTest
{
    internal static class Extensions
    {
        public static IEnumerable<T> Decimate<T>(this IEnumerable<T> items, int count)
        {
            var i = 0;
            foreach (var item in items)
                if (i++ >= count)
                {
                    i = 0;
                    yield return item;
                }
        }

        public static void DisposeAfter<T>(this T value, Action<T> action)
            where T : IDisposable
        {
            using (value) action(value);
        }

        public static void DisposeAfter<T, TParameter>(this T value, TParameter p, Action<T, TParameter> action)
            where T : IDisposable
        {
            using (value) action(value, p);
        }

        public static FileInfo GetFile(this DirectoryInfo dir, string FileName)
        {
            if (FileName is null) throw new ArgumentNullException(nameof(FileName));
            if (dir is null) throw new ArgumentNullException(nameof(dir));
            if (Path.IsPathRooted(FileName))
                throw new InvalidOperationException("Указан абсолютный путь к файлу");

            return new (Path.Combine(dir.FullName, FileName));
        }

        public static FileInfo EnsureDirExist(this FileInfo file)
        {
            if (!file.Directory!.Exists) file.Directory.Create();
            return file;
        }

        public static StreamWriter CreateText(this DirectoryInfo dir, string FileName) => 
            dir
               .GetFile(FileName)
               .EnsureDirExist()
               .CreateText();

        public static FileStream CreateFile(this DirectoryInfo dir, string FileName) => 
            dir
               .GetFile(FileName)
               .EnsureDirExist()
               .Create();
    }
}
