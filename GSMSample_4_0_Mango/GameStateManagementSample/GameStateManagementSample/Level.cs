using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;

namespace GameStateManagementSample
{

    class Level : LevelLoader
    {
        public static Level main;
        public Color color;

        public Level(string file, ContentManager content)
            : base(file, content)
        {
            main = this;
            gameWidth = 0;
            color = Color.CornflowerBlue;
        } 

        public void Draw(ScreenManager screenManager)
        {
            // This game has a blue background. Why? Because!
            screenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               this.color, 0, 0);

            SpriteBatch spriteBatch = screenManager.SpriteBatch;

 	        base.Draw(spriteBatch);
        }

        public void ChangeLevel(int level)
        {
            base.level = level;
            
            base.LoadLevel();
        }

        public void ChangeLevel(int level, Color color)
        {
            this.color = color;
            base.level = level;
            base.LoadLevel();
        }
        
        public void ChangeColor(Color color)
        {
            this.color = color;
        }

        public void Reset()
        {
            foreach (Background background in tilesBackground)
            {
                background.texture = null;
                background.position = new Vector2();
            }
            tilesBackground.Clear();
            gameWidth = 0;
            color = Color.White;
        }

        public void Update(GameTime gameTime, Player player, GraphicsDevice device)
        {
            if (player.position.X + player.SourceRect.Width > device.Viewport.Width / 2 && player.position.X + player.SourceRect.Width < this.width - device.Viewport.Width / 2)
            {
                Camera2D.main.setPosition(new Vector2(player.position.X + player.SourceRect.Width - Camera2D.main.rect.Width / 2, 0));
            }
            
        }
        
    }
}
