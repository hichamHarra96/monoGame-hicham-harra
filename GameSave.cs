using System;
using Microsoft.Xna.Framework;

using System.Collections.Generic;


using Newtonsoft.Json;
using System.IO;

namespace Starship
{
    public  class GameSave : Save 
    {
        public List<Saucer> Saucers;
        public List<Tir> Tirs ;
        public Vector2 VaisseauPosition;
        public string PlayerName;
        public int Score;

        public GameSave(List<Saucer> saucers, List<Tir> tirs, Vector2 vaisseauPosition, string playerName, int score)
        {
            this.Saucers = saucers;
            this.Tirs = tirs;
            this.VaisseauPosition = vaisseauPosition;
            this.PlayerName = playerName;
            this.Score = score;
        }

        public GameSave()
        {
                this.Saucers = new List<Saucer>();
                this.Tirs = new List<Tir>();
                this.VaisseauPosition = new Vector2(0, 0);
                this.PlayerName = "";
                this.Score = 0;
            
            
        }


        public void SaveGame()
        {
            string filename = "save.json";
            //check if the file exists
            if (!System.IO.File.Exists(filename))
            {
                //create the file
                System.IO.File.Create(filename);
            }
            //write the data
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(filename, json);

        }

        public GameSave LoadGame()
        {
            string filename = "save.json";
            //check if the file exists
            if (!System.IO.File.Exists(filename))
            {
                //create the file
                System.IO.File.Create(filename);
            }
            //write the data

                string jsonFromFile = System.IO.File.ReadAllText(filename);
                GameSave gameState = (GameSave)JsonConvert.DeserializeObject<GameSave>(jsonFromFile);

                Console.WriteLine("game loaded successfully.");
                return gameState;

        }

        //check if there is a save file
        public bool IsSaveExists()
        {
            string filename = "save.json";
            //check if the file exists
            if (!System.IO.File.Exists(filename))
            {
                return false;
            }
            else
            {
               //check if the file is empty
               if (new FileInfo(filename).Length == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }


        }

        //clear save file
        public void ClearSave()
        {
            string filename = "save.json";
            //check if the file exists
            if (!System.IO.File.Exists(filename))
            {
                //create the file
                System.IO.File.Create(filename);
            }
            //write the data
            try
            {
                //delete any data from the file
                File.WriteAllText(filename, string.Empty);
                Console.WriteLine("game cleared successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occurred while clearing game: " + e.Message);
            }

        }
        

    }
}
