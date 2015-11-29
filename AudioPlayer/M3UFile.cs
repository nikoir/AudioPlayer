using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AudioPlayer
{
    class M3UFile
    {
        public static string[] Parse(string FilePath)
        {
            using (StreamReader sr = new StreamReader(FilePath, Encoding.Default))
            {
                string FileName;
                string DirectoryName = Path.GetDirectoryName(FilePath);
                List<string> FileNames = new List<string>();
                while (!sr.EndOfStream)
                {
                    FileName = sr.ReadLine();
                    if (!Path.IsPathRooted(FileName))
                        FileName = DirectoryName + "\\" + FileName;
                    if (File.Exists(FileName))
                        FileNames.Add(FileName);
                    else
                        throw new FileNotFoundException("File from playlist is not found!", FileName);
                }
                return FileNames.ToArray();
            }
        }
        public static void Save(string[] FileNames, string FilePath)
        {
            if (Path.GetExtension(FilePath).ToLower() != ".m3u")
                throw new FileFormatException("Playlist has incorrect extension!");
            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                throw new DirectoryNotFoundException("Directory with this name is not exists!");
            using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.Default))
            {
                foreach (string FileName in FileNames)
                    sw.WriteLine(FileName);
            }
        }
    }
}
