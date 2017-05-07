using StardewModdingAPI;
using StardewModdingAPI.Events;
using CustomTV;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;


namespace DailyNews
{

  public class ModEntry : Mod
    {
        private int dailyNews;
        private ModConfig config;
        private Texture2D newsScreen;
        private List<string> contentFolderFiles;
        private string contentFolderExtension;
        private List<string> combinedNewsItems = new List<string>();
		public string customContentFolder;

        public override void Entry(IModHelper helper)
        {
			this.config = helper.ReadConfig<ModConfig>();

			customContentFolder = Path.Combine(helper.DirectoryPath, this.config.contentFolder);
			if (!Directory.Exists(customContentFolder))
			{
				Directory.CreateDirectory(customContentFolder);
                showMessage("The '" + this.config.contentFolder + "' folder is empty. Mod broken.",1);
			}

            this.newsScreen = helper.Content.Load<Texture2D>(@"assets\" + config.texture);
            this.contentFolderExtension = config.extension;


			SaveEvents.AfterLoad += (x, y) => Load();

            TimeEvents.DayOfMonthChanged += (x,y) => checkIfNews();
        }

        private void Load()
        {
			contentFolderFiles = ParseDir(customContentFolder,contentFolderExtension);
            foreach (string file in contentFolderFiles)
            {
                var contentFiles = this.Helper.ReadJsonFile<ModData>(file) ?? new ModData();
                combinedNewsItems.AddRange(contentFiles.newsItems);
            }
        }
     
        private void checkIfNews()
        {
            CustomTVMod.removeChannel("News");

            string str = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
            string season = Game1.currentSeason;
            if (str.Equals("Tue") || str.Equals("Fri") || str.Equals("Sat"))
            {
                CustomTVMod.addChannel("News", "News Report", deliverNews);
                Random randomNews = new Random();
                dailyNews = randomNews.Next(0, combinedNewsItems.Count);

                if (config.showMessages)
                    showMessage("Breaking News for " + char.ToUpper(season[0]) + season.Substring(1) + " " + Game1.dayOfMonth,2);
            }
        }

        private void deliverNews(TV tv, TemporaryAnimatedSprite sprite, StardewValley.Farmer who, string answer)
        {
            TemporaryAnimatedSprite newsSprite = new TemporaryAnimatedSprite(newsScreen, new Rectangle(0, 0, 42, 28), 150f, 2, 999999, tv.getScreenPosition(), false, false, (float)((double)(tv.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06), 0.0f, Color.White, tv.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false);
            string text = combinedNewsItems[dailyNews];
            CustomTVMod.showProgram(newsSprite, text, CustomTVMod.endProgram);
        }

        private static void showMessage(string msg, int type)
        {
            var hudmsg = new HUDMessage(msg, Color.SeaGreen, 5250f, true);
            hudmsg.whatType = type;
            Game1.addHUDMessage(hudmsg);
        }

		private List<string> ParseDir(string path, string extension)
		{
            List<string> customFiles = new List<string>();
            foreach (string dir in Directory.EnumerateDirectories(path))
			{
				ParseDir(Path.Combine(path, dir),extension);
			}

			foreach (string file in Directory.EnumerateFiles(path))
			{
				if (Path.GetExtension(file) == extension)
				{
					string filePath = Path.Combine(path, Path.GetDirectoryName(file), Path.GetFileName(file));
					customFiles.Add(filePath);
					Monitor.Log(filePath);
				}
			}
            return customFiles;
		}


    }

}
