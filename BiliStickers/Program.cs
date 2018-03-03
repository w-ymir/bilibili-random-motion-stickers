using System;
using System.IO;

namespace BiliStickers
{
    class CurrentId
    {
        const string StoredIdFileName = "currentId.txt";    // Stores the maximum id of the currently stored stickers.
        bool fileExists;

        public CurrentId()
        {
            fileExists = File.Exists(StoredIdFileName) ? true : false;
        }

        public bool Exists()
        {
            return fileExists;
        }

        public int GetCurrentId()
        {
            if (!fileExists)
                return -1;

            var reader = new StreamReader(StoredIdFileName);
            var raw = reader.ReadLine();
            int id;
            return int.TryParse(raw, out id) ? id : int.MaxValue;
        }

        public bool SaveNewId(int id)
        {
            try
            {
                var fs = new FileStream(StoredIdFileName, FileMode.Create);
                var sw = new StreamWriter(fs);
                sw.Write(id);
                sw.Flush();
                sw.Close();
                fs.Close();
                Console.WriteLine("New id saved.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string task = "--file";
            if (args.Length != 0)
                task = args[0];

            switch (task)
            {
                case "--file":
                case "--url":
                    // Find the bound of sticker id.
                    var currentId = new CurrentId();
                    int bound = currentId.GetCurrentId();

                    // Get stickers.
                    var bilibiliJson = new BilibiliJson(task);
                    int count;
                    var stickers = bilibiliJson.GetStickers(bound, out count);
                    stickers.Reverse(); // make id in ascending order

                    if (count > 0)
                    {
                        // Save updated JSON file if necessary.
                        if (task == "--url")
                            bilibiliJson.SaveJsonFile();

                        // Save images.
                        Console.WriteLine();
                        bilibiliJson.SaveStickerImages(ref stickers);
                        Console.WriteLine();

                        // Save new bound of sticker id.
                        currentId.SaveNewId(stickers[stickers.Count - 1].Id);    // the max sticker id is in the front
                    }
                    break;
                case "--table":
                    // Get all stickers.
                    bilibiliJson = new BilibiliJson(task);
                    stickers = bilibiliJson.GetStickers(-1, out count);

                    // Update Readme.
                    StickersTable stickersTable = new StickersTable();
                    stickersTable.AddToReadme(ref stickers);
                    break;
                default:
                    Console.Error.WriteLine("Argument(s) not recognised.");
                    break;
            }
        }
    }
}
