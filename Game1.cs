using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;

namespace Starship
{
    public enum GameState
    {
        FirstScreen,
        Home,
        HowToPlay,
        Playing,
        Scores,
        Pause,
        GameOver
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;
        private Texture2D backgroundTexture;
        private Texture2D flyingSaucer;
        private Texture2D tir;
        private Texture2D vaisseau;
        private Vector2 vaisseauPosition;
        private float saucerSpeed = 5f;

        private List<Tir> tirs;
        private float tirSpeed = 8f;
        private bool isFiring = false;

        private Random random;
        private List<Saucer> saucers;
        
        private float flyingSpeed = 3f;
        private float flyingInterval = 2f; // en secondes
        private float flyingTimer = 0f;
        private int score;
        private SpriteFont scoreFont;
        private Texture2D fond;
        private Texture2D fondHowToPlay;
        private bool previousEnterState = false;
        private bool previousBackState = false;
        private bool previousExitStateReleased = true;
        private bool leftButtonReleased = true;
        private Song soundEffect;
        private Song gameSong;
        KeyboardState previousKeyboardState;

        private GameState gameState = GameState.FirstScreen;
        private string playerName = string.Empty;
        Texture2D buttonTexture;
        Texture2D pauseTexture;

        List<KeyValuePair<string, int>> highScores;


        //create 3 rectangles for buttons
        Rectangle button1;
        Rectangle button2;
        Rectangle button3;
        Rectangle button4;
        Rectangle button5;

        private GameSave gameSave;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
            random = new Random();
            tirs = new List<Tir>();
            saucers = new List<Saucer>();
            
            button5 = new Rectangle((GraphicsDevice.Viewport.Width / 2) - 100, (GraphicsDevice.Viewport.Height / 2) - 200, 200, 50);
            button1 = new Rectangle((GraphicsDevice.Viewport.Width / 2) - 100, (GraphicsDevice.Viewport.Height / 2) - 100, 200, 50);
            button2 = new Rectangle((GraphicsDevice.Viewport.Width / 2) - 100, (GraphicsDevice.Viewport.Height / 2), 200, 50);
            button3 = new Rectangle((GraphicsDevice.Viewport.Width / 2) - 100, (GraphicsDevice.Viewport.Height / 2) + 100, 200, 50);
            button4 = new Rectangle((GraphicsDevice.Viewport.Width / 2) - 100, (GraphicsDevice.Viewport.Height / 2) + 200, 200, 50);
            highScores = RetrieveScores("scores.txt");

            gameSave = new GameSave();
            if (gameSave.IsSaveExists())
            {
                GameSave game1 = gameSave.LoadGame();
                saucers = game1.Saucers;
                tirs = game1.Tirs;
                vaisseauPosition = game1.VaisseauPosition;
                playerName = game1.PlayerName;
                score = game1.Score;
            }
            else
            {
                score = 0;
            }
              

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            backgroundTexture = Content.Load<Texture2D>("ciel-nocturne");
            vaisseau = Content.Load<Texture2D>("vaisseau-ballon-petit");
            tir = Content.Load<Texture2D>("tir");
            flyingSaucer = Content.Load<Texture2D>("flyingSaucer-petit");
            scoreFont = Content.Load<SpriteFont>("galleryFont");
            fond = Content.Load<Texture2D>("fond");
            fondHowToPlay = Content.Load<Texture2D>("howToPlay");
            // Charger le fichier audio
            soundEffect = Content.Load<Song>("tire");
            gameSong = Content.Load<Song>("gameSong");
            MediaPlayer.Play(gameSong);
            MediaPlayer.IsRepeating = true;  

            Color buttonColor = Color.Black; 
            Color pauseColor = Color.White; 

            buttonTexture = new Texture2D(GraphicsDevice, 1, 1);
            buttonTexture.SetData(new[] { buttonColor });

            pauseTexture = new Texture2D(GraphicsDevice, 1, 1);
            pauseTexture.SetData(new[] { pauseColor });


        }

        protected override void Update(GameTime gameTime)
        { 
            switch (gameState)
            {
                case GameState.Home:
                    UpdateHome(gameTime);
                    break;
                case GameState.Playing:
                    
                    UpdateGame(gameTime);
                    break;
                case GameState.FirstScreen:
                    UpdateFirstScreen(gameTime);
                    break;
                case GameState.HowToPlay:
                    UpdateHowToPlay(gameTime);
                    break;
                case GameState.GameOver:
                    UpdateGameOver(gameTime);
                    break;
                case GameState.Scores:
                    UpdateScores(gameTime);
                    break;
                case GameState.Pause:
                    UpdatePause(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        



        private void UpdateHome(GameTime gameTime)
        {

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || (Keyboard.GetState().IsKeyDown(Keys.Escape)))
            {
                gameState = GameState.FirstScreen;
                previousExitStateReleased = false;
            }
            KeyboardState keyboardState = Keyboard.GetState();
            bool currentEnterState = keyboardState.IsKeyDown(Keys.Enter);
            bool currentBackState = keyboardState.IsKeyDown(Keys.Back);
            KeyboardState currentKeyboardState = Keyboard.GetState();

            if (currentEnterState && !previousEnterState && !string.IsNullOrWhiteSpace(playerName))
            {
                gameState = GameState.Playing;
                InitializeGame();
            }
            else if (currentBackState && !previousBackState && playerName.Length > 0)
            {
                playerName = playerName.Remove(playerName.Length - 1);
            }
            else
            {
                Keys[] pressedKeys = keyboardState.GetPressedKeys();

                if (pressedKeys.Length > 0)
                {
                    Keys key = pressedKeys[0];
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        char character = (char)key;
                        if (!previousKeyboardState.IsKeyDown(key))
                        {
                            playerName += character;
                        }
                    }
                }
            }

            previousEnterState = currentEnterState;
            previousBackState = currentBackState;
            previousKeyboardState = currentKeyboardState;
        }

        private void UpdateScores(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousExitStateReleased == true)
            {
                gameState = GameState.FirstScreen;
                previousExitStateReleased = false;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Escape))
                previousExitStateReleased = true;

            highScores = RetrieveScores("scores.txt");

        }

        private void UpdateGameOver(GameTime gameTime)
        {

            
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousExitStateReleased == true)
            {
                gameState = GameState.FirstScreen;
                previousExitStateReleased = false;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Escape))
                previousExitStateReleased = true;


            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Released)
                leftButtonReleased = true;
            if (mouseState.LeftButton == ButtonState.Pressed && leftButtonReleased)
            {
                Rectangle mouseRectangle = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

                if (mouseRectangle.Intersects(button1) )
                {
                    gameState = GameState.Playing;
                    InitializeGame();
                    leftButtonReleased = false;
                }
                else if (mouseRectangle.Intersects(button2) )
                {
                    gameState = GameState.FirstScreen;
                    leftButtonReleased = false;

                }
                else if (mouseRectangle.Intersects(button3) )
                {
                    gameState = GameState.Scores;
                    leftButtonReleased = false;

                }
            }

        }

        private void UpdatePause(GameTime gameTime)
        {


            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && previousExitStateReleased == true)
            {
                gameState = GameState.Playing;
                previousExitStateReleased = false;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Escape))
                previousExitStateReleased = true;

            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Released)
                leftButtonReleased = true;
            if (mouseState.LeftButton == ButtonState.Pressed && leftButtonReleased)
            {
                Rectangle mouseRectangle = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

                if (mouseRectangle.Intersects(button1))
                {
                    gameState = GameState.Playing;
                    leftButtonReleased = false;
                }
                else if (mouseRectangle.Intersects(button2))
                {
                    gameSave.ClearSave();
                    gameState = GameState.GameOver;
                    AddScore(playerName, score, "scores.txt");
                    leftButtonReleased = false;

                }
                else if (mouseRectangle.Intersects(button3))
                {
                    gameSave.Saucers= saucers;
                    gameSave.Tirs = tirs;
                    gameSave.Score = score;
                    gameSave.PlayerName = playerName;
                    gameSave.VaisseauPosition = vaisseauPosition;
                    gameSave.ClearSave();
                    gameSave.SaveGame();
                    gameState = GameState.FirstScreen;
                    leftButtonReleased = false;

                }

            }


        }

        private void UpdateHowToPlay(GameTime gameTime)
        {

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||  (Keyboard.GetState().IsKeyDown(Keys.Escape)))
            {
                gameState = GameState.FirstScreen;
                previousExitStateReleased = false;
            }
                
        }

        private void UpdateFirstScreen(GameTime gameTime)
        {

            if (Keyboard.GetState().IsKeyUp(Keys.Escape))
                previousExitStateReleased = true;
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) && previousExitStateReleased)
                Exit();

            //add the logic for clicking on buttons in the menu
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Released)
                leftButtonReleased = true;
            if (mouseState.LeftButton == ButtonState.Pressed && leftButtonReleased)
            {
                Rectangle mouseRectangle = new Rectangle(mouseState.X, mouseState.Y, 1, 1);

                if (mouseRectangle.Intersects(button1))
                {
                    gameSave.ClearSave();
                    gameState = GameState.Home;
                }
                else if (mouseRectangle.Intersects(button2))
                {
                    gameState = GameState.HowToPlay;
                }
                else if (mouseRectangle.Intersects(button3))
                {
                    gameState = GameState.Scores;
                }
                else if (mouseRectangle.Intersects(button4))
                {
                    Exit();
                }
                else if (mouseRectangle.Intersects(button5) && gameSave.IsSaveExists())
                {
                    
                    gameState = GameState.Playing;
                    
                }
            }
        }


        private void UpdateGame(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyUp(Keys.Escape))
                previousExitStateReleased = true;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape) && previousExitStateReleased)
            {
                gameState = GameState.Pause;
                previousExitStateReleased = false;
            }
                

            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                // Vérifier si le vaisseau dépasse le bord supérieur
                if (vaisseauPosition.Y > 0)
                    vaisseauPosition.Y -= saucerSpeed;
            }
            else if (keyboardState.IsKeyDown(Keys.Down))
            {
                // Vérifier si le vaisseau dépasse le bord inférieur
                if (vaisseauPosition.Y < GraphicsDevice.Viewport.Height - vaisseau.Height)
                    vaisseauPosition.Y += saucerSpeed;
            }

            bool isSpacePressed = keyboardState.IsKeyDown(Keys.Space);

            if (isSpacePressed && !isFiring)
            {
                // son 
                MediaPlayer.Play(soundEffect);
                MediaPlayer.IsRepeating = false;

                Vector2 newTirPosition = new Vector2(vaisseauPosition.X + vaisseau.Width, vaisseauPosition.Y + vaisseau.Height / 2 - tir.Height / 2);

                tirs.Add(new Tir(newTirPosition));

                isFiring = true;
            }
            else if (!isSpacePressed)
            {
                isFiring = false;
            }

            for (int i = tirs.Count - 1; i >= 0; i--)
            {

                tirs[i].Move("right", tirSpeed);
                

                if (tirs[i].GetPosition().X > GraphicsDevice.Viewport.Width)
                {
                    tirs.RemoveAt(i);
                }
                else
                {
                    // Vérifier les collisions avec les secoupes volantes
                    Rectangle tirRect = new Rectangle((int)tirs[i].GetPosition().X, (int)tirs[i].GetPosition().Y, tir.Width, tir.Height);
                    for (int j = saucers.Count - 1; j >= 0; j--)
                    {
                        Rectangle flyingRect = new Rectangle((int)saucers[j].GetPosition().X, (int)saucers[j].GetPosition().Y, flyingSaucer.Width, flyingSaucer.Height);    
                        if (tirRect.Intersects(flyingRect) && saucers[j].IsActive)
                        {
                            score++;
                            saucers[j].IsActive = false;
                            tirs.RemoveAt(i);
                            break; // Sortir de la boucle, car un tir ne peut toucher qu'un seul vaisseau volant
                        }


                    }
                }
            }

            flyingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (flyingTimer >= flyingInterval)
            {
                SpawnFlying();
                flyingTimer = 0f;
            }

            for (int i = saucers.Count - 1; i >= 0; i--)
            {
                if (saucers[i].IsActive)
                {
                    saucers[i].Move("left", flyingSpeed);
                    

                    if (saucers[i].GetPosition().X + flyingSaucer.Width < 0)
                    {

                        saucers[i].IsInScreen = false;
                        gameSave.ClearSave();
                        gameState = GameState.GameOver;
                        AddScore(playerName, score, "scores.txt");
                        previousExitStateReleased = false;
                    }
                }
                else
                {
                    // Faire tomber le vaisseau volant verticalement
                    saucers[i].Move("down", saucerSpeed);

                    // Supprimer le vaisseau volant s'il sort de l'écran
                    if (saucers[i].GetPosition().Y > GraphicsDevice.Viewport.Height)
                    {
                        saucers[i].IsInScreen = false;
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            switch (gameState)
            {
                case GameState.Home:
                    DrawHome();
                    break;
                case GameState.Playing:
                    DrawGame();
                    break;
                case GameState.FirstScreen:
                    DrawFirstScreen();
                    break;
                case GameState.HowToPlay:
                    DrawHowToPlay();
                    break;
                case GameState.GameOver:
                    DrawGameOver();
                    break;
                case GameState.Scores:
                    DrawScores();
                    break;
                case GameState.Pause:
                    DrawPause();
                    break;
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawHome()
        {
            Rectangle backgroundRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(fond, backgroundRectangle, Color.White);

            spriteBatch.DrawString(scoreFont, "Enter your name:", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(scoreFont, playerName, new Vector2(10, 40), Color.White);
        }

        private void DrawScores()
        {
            Rectangle backgroundRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(fond, backgroundRectangle, Color.White);

            //Draw the scores in the center of the screen
            spriteBatch.DrawString(scoreFont, "High Scores", new Vector2(GraphicsDevice.Viewport.Width / 2 - 200, GraphicsDevice.Viewport.Height / 2 - 200), Color.White);


            int i = 0;
            var modifiedScores = highScores.Select(score =>
            {
                string name = score.Key.Length > 10 ? score.Key.Substring(0, 10) + "..." : score.Key;
                return new { Name = name, Value = score.Value };
            });

            foreach (var score in modifiedScores)
            {
                spriteBatch.DrawString(scoreFont, score.Name + " : " + score.Value, new Vector2(GraphicsDevice.Viewport.Width / 2 - 200, GraphicsDevice.Viewport.Height / 2 - 200 + 60 + 30 * i), Color.White);
                i++;
            }
        }

        private void DrawGameOver()
        {
            Rectangle backgroundRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(fond, backgroundRectangle, Color.White);
            //Draw game over text in center of the screen
            spriteBatch.DrawString(scoreFont, "Game Over", new Vector2(GraphicsDevice.Viewport.Width / 2 - 200, GraphicsDevice.Viewport.Height / 2 - 200), Color.White);
            //Draw current score
            spriteBatch.DrawString(scoreFont, "Your score is: " + score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 200, GraphicsDevice.Viewport.Height / 2 - 200 + 30), Color.White);
            //draw three buttons as rectangles (play again, main menu, high scores)
            spriteBatch.Draw(buttonTexture,button1, Color.White);
            spriteBatch.Draw(buttonTexture,button2, Color.White);
            spriteBatch.Draw(buttonTexture,button3, Color.White);
            //draw text on top of the buttons
            spriteBatch.DrawString(scoreFont, "Play again", new Vector2(button1.X + 10, button1.Y + 10), Color.White);
            spriteBatch.DrawString(scoreFont, "Main menu", new Vector2(button2.X + 10, button2.Y + 10), Color.White);
            spriteBatch.DrawString(scoreFont, "High scores", new Vector2(button3.X + 10, button3.Y + 10), Color.White);

        }

        private void DrawPause()
        {
            //Draw the screen of the game
            Rectangle backgroundRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(backgroundTexture, backgroundRectangle, Color.White);

            spriteBatch.Draw(vaisseau, vaisseauPosition, Color.White);

            for (int i = 0; i < tirs.Count; i++)
            {   
                spriteBatch.Draw(tir, tirs[i].GetPosition(), Color.White);
            }

            for (int i = 0; i < saucers.Count; i++)
            {
                if (saucers[i].IsInScreen)
                {
                    spriteBatch.Draw(flyingSaucer, saucers[i].Position, Color.White);

                }
            }

            spriteBatch.DrawString(scoreFont, "Score: " + score, new Vector2(10, 10), Color.White);

            //Draw a blue rectangle in the center of the screen for the pause menu
            spriteBatch.Draw(fond, new Rectangle(GraphicsDevice.Viewport.Width / 2 - 200, GraphicsDevice.Viewport.Height / 2 - 200, 400, 400), Color.White);
            //Draw the text on top of the rectangle
            spriteBatch.DrawString(scoreFont, "Game paused", new Vector2(GraphicsDevice.Viewport.Width / 2 - 200, GraphicsDevice.Viewport.Height / 2 - 200), Color.White);
            spriteBatch.DrawString(scoreFont, "Press escape to resume", new Vector2(GraphicsDevice.Viewport.Width / 2 - 200, GraphicsDevice.Viewport.Height / 2 - 200 + 30), Color.Black);
            //Draw three buttons as rectangles (resume, main menu)
            spriteBatch.Draw(buttonTexture, button1, Color.White);
            spriteBatch.Draw(buttonTexture, button2, Color.White);
            spriteBatch.Draw(buttonTexture, button3, Color.White);
            //draw text on top of the buttons
            spriteBatch.DrawString(scoreFont, "Resume", new Vector2(button1.X + 10, button1.Y + 10), Color.White);
            spriteBatch.DrawString(scoreFont, "End game", new Vector2(button2.X + 10, button2.Y + 10), Color.White);
            spriteBatch.DrawString(scoreFont, "Save Game", new Vector2(button3.X + 10, button3.Y + 10), Color.White);


        }

        private void DrawHowToPlay()
        { //draw text in center of the screen
            Rectangle backgroundRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(fondHowToPlay, backgroundRectangle, Color.White);

        }

        private void DrawFirstScreen()
        {
            Rectangle backgroundRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(fond, backgroundRectangle, Color.White);

            //draw 4 buttons in the center of the screen
            spriteBatch.Draw(buttonTexture, button1, Color.White);
            spriteBatch.Draw(buttonTexture, button2, Color.White);
            spriteBatch.Draw(buttonTexture, button3, Color.White);
            spriteBatch.Draw(buttonTexture, button4, Color.White);
            if(gameSave.IsSaveExists()) 
            {
                spriteBatch.Draw(buttonTexture, button5, Color.White);
                //draw text on top of the buttons
                spriteBatch.DrawString(scoreFont, "Resume", new Vector2(button5.X + 10, button5.Y + 10), Color.White);
                
            }

            //draw text on buttons
            spriteBatch.DrawString(scoreFont, "Play", new Vector2(button1.X + 10, button1.Y + 10), Color.White);
            spriteBatch.DrawString(scoreFont, "How to play", new Vector2(button2.X + 10, button2.Y + 10), Color.White);
            spriteBatch.DrawString(scoreFont, "High scores", new Vector2(button3.X + 10, button3.Y + 10), Color.White);
            spriteBatch.DrawString(scoreFont, "Exit", new Vector2(button4.X + 10, button4.Y + 10), Color.White);
        }


        private void DrawGame()
        {
            Rectangle backgroundRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            spriteBatch.Draw(backgroundTexture, backgroundRectangle, Color.White);

            spriteBatch.Draw(vaisseau, vaisseauPosition, Color.White);

            for (int i = 0; i < tirs.Count; i++)
            {
                spriteBatch.Draw(tir, tirs[i].GetPosition(), Color.White);
            }

            for (int i = 0; i < saucers.Count; i++)
            {
                if (saucers[i].IsInScreen)
                {
                    spriteBatch.Draw(flyingSaucer, saucers[i].GetPosition(), Color.White);

                }
            }

            spriteBatch.DrawString(scoreFont, "Score: " + score, new Vector2(10, 10), Color.White);
        }

        private void InitializeGame()
        {
            // Réinitialiser les paramètres du jeu
            vaisseauPosition = new Vector2(10, GraphicsDevice.Viewport.Height / 2 - vaisseau.Height / 2);
            tirs.Clear();
            saucers.Clear();
            score = 0;

            // Initialiser le timer des vaisseaux volants
            flyingTimer = 0f;
        }

        private void SpawnFlying()
        {
            float newFlyingY = random.Next(0, GraphicsDevice.Viewport.Height - flyingSaucer.Height);
            Vector2 newFlyingPosition = new Vector2(GraphicsDevice.Viewport.Width, newFlyingY);
            Saucer saucer = new Saucer(newFlyingPosition, true);

            saucers.Add(saucer);

        }

        public List<KeyValuePair<string, int>> RetrieveScores(string fileName)
        {
            //check if the file exists
            if (!File.Exists(fileName))
            {
                //if not, create it
                File.Create(fileName).Close();
            }
            List<KeyValuePair<string, int>> scores = new List<KeyValuePair<string, int>>();

            string[] lines = File.ReadAllLines(fileName);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');

                if (parts.Length == 2)
                {
                    string name = parts[0];
                    int score;

                    if (int.TryParse(parts[1], out score))
                    {
                        scores.Add(new KeyValuePair<string, int>(name, score));
                    }
                }
            }

            return scores;
        }
        public void AddScore(string username, int score, string fileName)
        {
            List<KeyValuePair<string, int>> existingScores = RetrieveScores(fileName);

            existingScores.Add(new KeyValuePair<string, int>(username, score));

            existingScores = existingScores.OrderByDescending(s => s.Value).ToList();

            List<KeyValuePair<string, int>> topScores = existingScores.Take(10).ToList();

            SaveScores(topScores, fileName);
        }

        public void SaveScores(List<KeyValuePair<string, int>> scores, string fileName)
        {
            if (!File.Exists(fileName))
            {
                //if not, create it
                File.Create(fileName).Close();
            }
            // Create a StringBuilder to build the content of the file
            StringBuilder sb = new StringBuilder();

            // Iterate through each score in the list
            foreach (var score in scores)
            {
                // Append the score information to the StringBuilder
                sb.AppendLine($"{score.Key},{score.Value}");
            }

            // Write the contents of the StringBuilder to the file
            File.WriteAllText(fileName, sb.ToString());
        }
    }
}
