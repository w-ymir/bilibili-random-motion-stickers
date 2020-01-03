using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace BiliStickers
{
    public class Sticker
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }    // Url to the icon
    }

    class BilibiliJson
    {
        const string JsonUrl = "https://www.bilibili.com/index/index-icon.json";
        const string JsonFileName = "index-icon.json";
        public const string ImageDirPath = "../gif";
        public string RawString;

        public BilibiliJson(string source)
        {
            switch (source)
            {
                case "--url":
                    GetJsonFromBilibili();
                    break;
                case "--file":
                case "--table":
                    if (File.Exists(JsonFileName))
                        GetJsonFromFile();
                    else
                    {
                        Console.WriteLine("\"{0}\" not found in the current directory.", JsonFileName);
                        GetJsonFromBilibili();
                    }
                    break;
                default:
                    Console.Error.WriteLine("Argument(s) not recognised.");
                    break;
            }
        }

        ref string GetJsonFromBilibili()
        {
            Console.WriteLine("Requesting from \"{0}\".", JsonUrl);

            // Get stream.
            WebRequest request = WebRequest.Create(JsonUrl);
            request.Timeout = 5000;
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();

            // Convert to string.
            StreamReader reader = new StreamReader(stream);
            RawString = reader.ReadToEnd();
            reader.Close();
            stream.Close();
            response.Close();

            Console.WriteLine("Finished reading \"{0}\".", JsonFileName);
            return ref RawString;
        }

        ref string GetJsonFromFile()
        {
            Console.WriteLine("Reading from the local file.");

            var reader = new StreamReader(JsonFileName);
            RawString = reader.ReadToEnd();
            reader.Close();

            Console.WriteLine("Finished reading \"{0}\".", JsonFileName);
            return ref RawString;
        }

        // count - number of stickers added
        public List<Sticker> GetStickers(int lowerBound, out int count)
        {
            var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(RawString);
            IList<Newtonsoft.Json.Linq.JToken> fix = jsonObject["fix"].Children().ToList();
            var stickers = new List<Sticker>();

            count = 0;
            foreach (Newtonsoft.Json.Linq.JToken jsonFragment in fix)
            {
                Sticker sticker = jsonFragment.ToObject<Sticker>();
                if (sticker.Id <= lowerBound)
                    break;
                stickers.Add(sticker);
                count++;
            }

            return stickers;
        }

        public void SaveStickerImages(ref List<Sticker> stickers)
        {
            Console.WriteLine("Start to get and save stickers.");

            if (!Directory.Exists(ImageDirPath))
                Directory.CreateDirectory(ImageDirPath);

            WebClient client = new WebClient();
            Random rnd = new Random();
            foreach (var sticker in stickers)
            {
                string curImageName = String.Format("{0}-{1}{2}", sticker.Id, sticker.Title, sticker.Icon.Substring(sticker.Icon.LastIndexOf('.')));
                string curImagePath = String.Format("{0}/{1}", ImageDirPath, curImageName);
                if (!File.Exists(curImagePath) || new FileInfo(curImagePath).Length == 0)
                {
                    string curImageUrl = sticker.Icon;
                    if (curImageUrl.StartsWith("//"))
                        curImageUrl = "http:" + curImageUrl;
                    else if (!curImageUrl.StartsWith("http"))
                        curImageUrl = "http://" + curImageUrl;

                    System.Threading.Thread.Sleep(rnd.Next(1000, 10000));

                    bool downloadSucceed = true;
                    try
                    {
                        client.DownloadFile(curImageUrl, curImagePath);
                    }
                    catch (System.Net.WebException ex)
                    {
                        Console.Error.WriteLine(ex.ToString());
                        downloadSucceed = false;
                    }
                    if (downloadSucceed)
                        Console.WriteLine("\"{0}\" saved.", curImageName);
                }
                else
                {
                    //Console.WriteLine("\"{0}\" already existed.", curImageName);
                }
            }
        }

        public bool SaveJsonFile()
        {
            try
            {
                var fs = new FileStream(JsonFileName, FileMode.Create);
                var sw = new StreamWriter(fs);
                sw.Write(RawString);
                sw.Flush();
                sw.Close();
                fs.Close();
                Console.WriteLine("Updated JSON file saved.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }
    }
}
