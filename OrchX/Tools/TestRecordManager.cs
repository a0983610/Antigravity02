using System;
using System.IO;
using System.Text;

namespace OrchX.Tools
{
    public class TestRecordManager
    {
        public static bool IsRecordingTest { get; set; } = false;

        public static void RecordRequest(string json)
        {
            if (!IsRecordingTest || string.IsNullOrWhiteSpace(json)) return;
            RecordData("Request", "request", json);
        }

        public static void RecordResponse(string json)
        {
            if (!IsRecordingTest || string.IsNullOrWhiteSpace(json)) return;
            RecordData("Response", "response", json);
        }

        private static void RecordData(string folderName, string fileSuffix, string rawJson)
        {
            try
            {
                string basePath = Environment.CurrentDirectory;
                string targetDir = Path.Combine(basePath, "Test", folderName);

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                int nextSequenceNumber = 1;
                string searchPattern = $"*_{fileSuffix}.json";
                string[] existingFiles = Directory.GetFiles(targetDir, searchPattern);
                foreach (string file in existingFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string suffixStr = $"_{fileSuffix}";
                    if (fileName.EndsWith(suffixStr))
                    {
                        string prefixStr = fileName.Substring(0, fileName.Length - suffixStr.Length);
                        if (int.TryParse(prefixStr, out int num))
                        {
                            if (num >= nextSequenceNumber)
                            {
                                nextSequenceNumber = num + 1;
                            }
                        }
                    }
                }

                string targetFileName = $"{nextSequenceNumber:D4}_{fileSuffix}.json";
                string targetPath = Path.Combine(targetDir, targetFileName);

                File.WriteAllText(targetPath, rawJson, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[Error] 寫入 Test 紀錄失敗 ({folderName}): {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
