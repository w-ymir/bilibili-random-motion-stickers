using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BiliStickers
{
    class StickersTable
    {
        const string ReadmeFilePath = "../README.md";
        const string ImageDirPath = "./gif";  // Note: It is different from BilibiliJson.ImageDirPath
        const string MarkdownTitle = "## Stickers";
        const string TableHeader = "| # | Title |\n|---|-------|";

        StringBuilder stTable = new StringBuilder("\n");

        void StickersToTable(ref List<Sticker> stickers)
        {
            foreach (var sticker in stickers)
            {
                string fileName = String.Format("{0}-{1}{2}", sticker.Id, sticker.Title, sticker.Icon.Substring(sticker.Icon.LastIndexOf('.')));
                string curFilePath = String.Format("{0}/{1}", ImageDirPath, fileName.Replace(" ", "%20"));
                stTable.Append(String.Format("| {0} | [{1}]({2}) |\n", sticker.Id, sticker.Title == "" ? "(no title)" : sticker.Title, curFilePath));
            }
        }

        public void AddToReadme(ref List<Sticker> stickers)
        {
            Console.WriteLine("Start to update README.");

            StickersToTable(ref stickers);

            if (!File.Exists(ReadmeFilePath))
            {
                var fs = new FileStream(ReadmeFilePath, FileMode.Create);
                var sw = new StreamWriter(fs);
                sw.Write(String.Format("\n{0}\n", MarkdownTitle));
                sw.Write(TableHeader);
                sw.Write(stTable.ToString());
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            else
            {
                var fs = new FileStream(ReadmeFilePath, FileMode.Open);
                var sr = new StreamReader(fs);
                string oldString = sr.ReadToEnd();
                sr.Close();
                fs.Close();

                StringBuilder str;
                var tablePos = oldString.IndexOf(MarkdownTitle);
                if (tablePos == -1)
                {
                    str = new StringBuilder(oldString);
                    str.Append(String.Format("\n{0}\n", MarkdownTitle));
                    str.Append(TableHeader);
                    str.Append(stTable);
                }
                else
                {
                    var tableEndPos = oldString.IndexOf("\n#", tablePos + MarkdownTitle.Length);
                    str = new StringBuilder(oldString.Substring(0, tablePos));
                    str.Append(String.Format("{0}\n", MarkdownTitle));
                    str.Append(TableHeader);
                    str.Append(stTable);
                    if (tableEndPos != -1)
                        str.Append(oldString.Substring(tableEndPos +1));
                }

                fs = new FileStream(ReadmeFilePath, FileMode.Create);
                var sw = new StreamWriter(fs);
                sw.Write(str);
                sw.Flush();
                sw.Close();
                fs.Close();

                Console.WriteLine("README updated.");
            }
        }
    }
}
