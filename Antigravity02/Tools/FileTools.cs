using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using System.Xml;

namespace Antigravity02.Tools
{
    public class FileTools
    {
        private readonly string _aiOutputFolder = "AI_Workspace";
        private string _baseDirectory;

        public FileTools(string baseDirectory = null)
        {
            // 規範化路徑並確保以分隔符結尾，防止 "C:\Path" 比對到 "C:\PathSecret"
            _baseDirectory = Path.GetFullPath(baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory);
            if (!_baseDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                _baseDirectory += Path.DirectorySeparatorChar;
            }

            // 確保 AI 輸出資料夾存在
            string path = Path.Combine(_baseDirectory, _aiOutputFolder);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        public string ListFiles(string subPath = "")
        {
            try
            {
                // 安全檢查：不允許向上層目錄存取
                if (subPath.Contains("..")) return "錯誤：禁止存取上層目錄。";

                string targetPath = Path.GetFullPath(Path.Combine(_baseDirectory, subPath));
                
                // 確保目標路徑仍在根目錄內
                if (!targetPath.StartsWith(_baseDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    return "錯誤：超出授權存取範圍。";
                }

                if (!Directory.Exists(targetPath))
                {
                    return $"錯誤：路徑 {subPath} 不存在。";
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"[Folder Tree: {subPath}]");
                BuildTree(targetPath, 0, 3, sb);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                UsageLogger.LogError($"FileTools(ListFiles) Error: {ex.Message}");
                return $"錯誤：無法讀取清單。{ex.Message}";
            }
        }

        private void BuildTree(string currentPath, int currentDepth, int maxDepth, StringBuilder sb)
        {
            if (currentDepth >= maxDepth) return;

            try
            {
                var entries = Directory.GetFileSystemEntries(currentPath);
                string indent = new string(' ', currentDepth * 4);

                foreach (var entry in entries)
                {
                    bool isDir = Directory.Exists(entry);
                    string name = Path.GetFileName(entry);
                    
                    if (isDir)
                    {
                        DirectoryInfo di = new DirectoryInfo(entry);
                        sb.AppendLine($"{indent}[DIR]  {name} (Created: {di.CreationTime:yyyy-MM-dd})");
                        BuildTree(entry, currentDepth + 1, maxDepth, sb);
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(entry);
                        string sizeStr = FormatSize(fi.Length);
                        // 對齊優化：檔名靠左30字元，大小靠右8字元
                        sb.AppendLine($"{indent}[FILE] {name, -30} | {sizeStr, 8} | Mod: {fi.LastWriteTime:yyyy-MM-dd HH:mm}");
                    }
                }
                
                if (entries.Length == 0 && currentDepth == 0)
                {
                    sb.AppendLine("(此資料夾是空的)");
                }
            }
            catch (UnauthorizedAccessException)
            {
                sb.AppendLine(new string(' ', currentDepth * 4) + "[存取被拒絕]");
            }
            catch (Exception ex)
            {
                sb.AppendLine(new string(' ', currentDepth * 4) + $"[錯誤: {ex.Message}]");
            }
        }

        /// <summary>
        /// 2. 讀取特定檔案 (保持唯讀，且限制範圍)
        /// </summary>
        public string ReadFile(string fileName)
        {
            try
            {
                if (fileName.Contains("..")) return "錯誤：格式不合法。";

                string filePath = Path.GetFullPath(Path.Combine(_baseDirectory, fileName));

                // 安全檢查
                if (!filePath.StartsWith(_baseDirectory, StringComparison.OrdinalIgnoreCase))
                    return "錯誤：超出授權範圍。";

                if (!File.Exists(filePath))
                {
                    return $"錯誤：找不到檔案 {fileName}。";
                }

                string extension = Path.GetExtension(filePath).ToLower();

                if (extension == ".txt" || extension == ".md" || extension == ".json" || extension == ".cs")
                {
                    return File.ReadAllText(filePath, Encoding.UTF8);
                }
                else if (extension == ".docx")
                {
                    return ReadDocxText(filePath);
                }
                else
                {
                    return "不支援的檔案格式或禁止存取。";
                }
            }
            catch (Exception ex)
            {
                UsageLogger.LogError($"FileTools(ReadFile) Error: {ex.Message}");
                return $"錯誤：無法讀取檔案。{ex.Message}";
            }
        }

        /// <summary>
        /// 3. 儲存筆記/輸出至 AI_Workspace 資料夾 (限 .txt)
        /// </summary>
        public string SaveNote(string fileName, string content, bool append = true)
        {
            try
            {
                // 強制加上 .txt 副檔名
                if (!fileName.ToLower().EndsWith(".txt")) fileName += ".txt";

                // 過濾檔名，防禦 Path Traversal
                string safeFileName = Path.GetFileName(fileName);

                string folderPath = Path.Combine(_baseDirectory, _aiOutputFolder);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, safeFileName);
                
                if (append)
                {
                    File.AppendAllText(filePath, content + Environment.NewLine, Encoding.UTF8);
                }
                else
                {
                    File.WriteAllText(filePath, content, Encoding.UTF8);
                }

                string actionType = append ? "附加筆記" : "儲存筆記(覆蓋)";
                return $"成功：{actionType}已完成至 {_aiOutputFolder}/{safeFileName}";
            }
            catch (Exception ex)
            {
                UsageLogger.LogError($"FileTools(SaveNote) Error: {ex.Message}");
                return $"錯誤：無法儲存筆記。{ex.Message}";
            }
        }


        /// <summary>
        /// 簡易的 .docx 文字提取 (透過讀取 zip 內的 word/document.xml)
        /// </summary>
        private string ReadDocxText(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    var entry = archive.GetEntry("word/document.xml");
                    if (entry == null) return "無效的 .docx 檔案。";

                    using (Stream stream = entry.Open())
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(stream);

                        // 使用命名空間管理器處理 Word 的 XML 命名空間
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                        nsmgr.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

                        XmlNodeList nodes = xmlDoc.SelectNodes("//w:t", nsmgr);
                        foreach (XmlNode node in nodes)
                        {
                            sb.Append(node.InnerText);
                        }
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                UsageLogger.LogError($"FileTools(ReadDocxText) Error: {ex.Message}");
                return $"讀取 .docx 時發生錯誤：{ex.Message}";
            }
        }
        private string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = (decimal)bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
    }
}
