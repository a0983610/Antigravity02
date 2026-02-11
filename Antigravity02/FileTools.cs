using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.Xml;

namespace Antigravity02
{
    public class FileTools
    {
        private string _baseDirectory;

        public FileTools(string baseDirectory = null)
        {
            _baseDirectory = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// 1. 列出目前資料夾下的檔案有哪些
        /// </summary>
        public string ListFiles(string subPath = "")
        {
            try
            {
                string targetPath = Path.Combine(_baseDirectory, subPath);
                if (!Directory.Exists(targetPath))
                {
                    return $"錯誤：路徑 {subPath} 不存在。";
                }

                var directories = Directory.GetDirectories(targetPath)
                                          .Select(d => "[DIR] " + Path.GetFileName(d));
                var files = Directory.GetFiles(targetPath)
                                    .Select(f => "[FILE] " + Path.GetFileName(f));

                var allItems = directories.Concat(files).ToList();
                if (allItems.Count == 0) return "此資料夾是空的。";

                return string.Join("\n", allItems);
            }
            catch (Exception ex)
            {
                return $"錯誤：無法讀取清單。{ex.Message}";
            }
        }

        /// <summary>
        /// 2. 讀取特定檔案 (支援 .txt, .doc, .docx)
        /// </summary>
        public string ReadFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_baseDirectory, fileName);
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
                else if (extension == ".doc")
                {
                    return "提醒：.doc 是舊版 Word 格式，本工具目前建議轉換為 .docx 以進行精確讀取。嘗試以純文字方式讀取片段...";
                }
                else
                {
                    return "不支援的檔案格式。目前僅支援 .txt, .docx 等文字格式。";
                }
            }
            catch (Exception ex)
            {
                return $"錯誤：無法讀取檔案。{ex.Message}";
            }
        }

        /// <summary>
        /// 3. 把內容寫入到一個資料夾下，存成文件
        /// </summary>
        public string SaveFileOutput(string fileName, string content, string subFolderName = "AI_Outputs")
        {
            try
            {
                string folderPath = Path.Combine(_baseDirectory, subFolderName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = Path.Combine(folderPath, fileName);
                File.WriteAllText(filePath, content, Encoding.UTF8);

                return $"成功：檔案已儲存至 {Path.Combine(subFolderName, fileName)}";
            }
            catch (Exception ex)
            {
                return $"錯誤：無法寫入檔案。{ex.Message}";
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
                return $"讀取 .docx 時發生錯誤：{ex.Message}";
            }
        }
    }
}
