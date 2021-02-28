using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using WinHttp;

namespace TelegramEmoticon
{
    class Program
    {
        static void Main(string[] args)
        {
            //args[0] = Link
            string url = args[0];
            string emo_name = url.Split('/')[url.Split('/').Length-1];
            string api_url = "https://e.kakao.com/api/v1/items/t/" + emo_name;
            string Telegram_API = "bot"+"HTTP API CODE";
            string Telegram_UserID = "User ID";
            string emoticon_title = "Korean";
            string sendLinqName = string.Concat(emo_name.Where(char.IsLetter))+ "_by_emomagic_bot"; //Registration ID
            bool createEmoticon = false;

            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            WinHttpRequest wt = new WinHttpRequest();
            wt.Open("GET", api_url);
            wt.SetRequestHeader("User-Agent", "Mozilla/5.0 (Linux; Android 4.4.2; Nexus 4 Build/KOT49H) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/34.0.1847.114 Mobile Safari/537.36");
            wt.Send();

            string rawReq = Encoding.UTF8.GetString((byte[])wt.ResponseBody);

            JObject parseOBJ = JObject.Parse(rawReq);
            emoticon_title = parseOBJ["result"]["title"].ToString();
            
            string img_raw = string.Join(",", JsonConvert.DeserializeObject<List<string>>(parseOBJ["result"]["thumbnailUrls"].ToString()).ToArray());
            string[] img_array = img_raw.Split(',');

            Directory.CreateDirectory(emo_name);
            int i = 0;

            foreach (string img_link in img_array)
            {
                WebClient wc = new WebClient();
                wc.DownloadFile(img_link, emo_name+"\\"+(++i).ToString()+".png");

                using (FileStream stream = new FileStream(emo_name + "\\" + i.ToString() + ".png", FileMode.Open, FileAccess.Read))
                {
                    Image img = Image.FromStream(stream);
                    Bitmap bmp = new Bitmap(img, new Size(512, 512));
                    img = (Image)bmp;
                    img.Save(emo_name + "\\" + i.ToString() + "_R.png");
                }
                File.Delete(emo_name + "\\" + i.ToString() + ".png");

                //ZipFile.CreateFromDirectory(emo_name, emo_name+".zip");
                //Directory.Delete(emo_name, true);
                try
                {
                    if (!createEmoticon)
                    {
                        string result = string.Empty; // 전송 후 결과값
                        result = RequestHelper.PostMultipart(
                            "https://api.telegram.org/" + Telegram_API + "/createNewStickerSet", new Dictionary<string, object>()
                            {
                        {
                            "png_sticker", new FormFile()
                            {
                                Name = string.Format("{0}.png",i.ToString()),
                                ContentType = "Image/png",
                                FilePath = emo_name + "\\" + i.ToString() + "_R.png"
                            }
                        },
                        {
                            "user_id", Telegram_UserID
                        },
                        {
                            "name", sendLinqName
                        },
                        {
                            "title", emoticon_title
                        },
                        {
                            "emojis", "👍"
                        }
                            }
                            );
                        //Console.WriteLine("First Create Code = " + result);
                        createEmoticon = true;
                    }
                    else
                    {
                        string result = string.Empty; // 전송 후 결과값
                        result = RequestHelper.PostMultipart(
                            "https://api.telegram.org/" + Telegram_API + "/addStickerToSet", new Dictionary<string, object>()
                            {
                        {
                            "png_sticker", new FormFile()
                            {
                                Name = string.Format("{0}.png",i.ToString()),
                                ContentType = "Image/png",
                                FilePath = emo_name + "\\" + i.ToString() + "_R.png"
                            }
                        },
                        {
                            "user_id", Telegram_UserID
                        },
                        {
                            "name", sendLinqName
                        },
                        {
                            "emojis", "👍"
                        }
                            }
                            );
                       //Console.WriteLine("Adding Code = " + result);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ERROR " + ex.Message);
                    return;
                }

            }
            Console.WriteLine("http://t.me/addstickers/" + sendLinqName + "|" + emoticon_title + "|" + "OK");
        }
    }
}
