using System.IO;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Utils
{
    public static class IOHelper
    {
        public static FileInfo GetFile(this DirectoryInfo parent, string fileName)
        {
            return new FileInfo(Path.Combine(parent.FullName, fileName));
        }

        public static DirectoryInfo GetFolder(this DirectoryInfo parent, string folderName)
        {
            return new DirectoryInfo(Path.Combine(parent.FullName, folderName));
        }
    }
}
