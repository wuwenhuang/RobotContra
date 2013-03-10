using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using GameStateManagementSample;

namespace GameStateManagement.SideScrollGame
{

    class Level : LevelLoader
    {
        public static Level main;
        public Color color;
        public Character targetPlayer;

        public Level(Character targetPlayer)
            : base()
        {
            
            main = this;
            gameWidth = 0;
            color = Color.CornflowerBlue;
            if (targetPlayer != null)
                this.targetPlayer = targetPlayer;
        }

        public void setTargetPlayer(Character targetPlayer)
        {
            this.targetPlayer = targetPlayer;
        }

        public void Draw(ScreenManager screenManager)
        {
            // This game has a blue background. Why? Because!
            screenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               this.color, 0, 0);

            SpriteBatch spriteBatch = screenManager.SpriteBatch;

 	        base.Draw(spriteBatch);

            if (enemiesLevel != null)
            {
                foreach (Enemy enemy in enemiesLevel)
                {
                    if (enemy.Alive && enemy.Texture != null)
                        enemy.Draw(spriteBatch);
                }
            }

        }

        public void ChangeLevel(int level)
        {

            base.level = level;
            
            base.LoadLevel();

            foreach (Enemy enemy in enemiesLevel)
            {
                enemy.SetTargetPlayer(targetPlayer);
            }
        }

        public void ChangeLevel(int level, Color color)
        {
            this.color = color;
            base.level = level;
            base.LoadLevel();

            foreach (Enemy enemy in enemiesLevel)
            {
                enemy.SetTargetPlayer(targetPlayer);
            }
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
                background.position = Vector2.Zero;
            }

            foreach (Enemy enemy in enemiesLevel)
            {
                enemy.Texture = null;
                enemy.position = Vector2.Zero;
            }
            enemiesLevel.Clear();
            tilesBackground.Clear();
            gameWidth = 0;
            color = Color.White;
        }

        public void Update(GameTime gameTime, Player player)
        {
            if (player.position.X + player.SourceRect.Width > GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Width / 2 && player.position.X + player.SourceRect.Width < this.width - GameplayScreen.main.ScreenManager.GraphicsDevice.Viewport.Width / 2)
            {
                Camera2D.main.setPosition(new Vector2(player.position.X + player.SourceRect.Width - Camera2D.main.rect.Width / 2, 0));
            }

            if (enemiesLevel != null)
            {
                for (int i=0; i < enemiesLevel.Count; i++)
                {
                    if (!enemiesLevel[i].Alive && enemiesLevel[i].position.X < Camera2D.main.getPosition().X + Camera2D.main.rect.Width)
                        enemiesLevel[i].setAlive(true);

                    if (enemiesLevel[i].Alive)
                    {
                        enemiesLevel[i].Update(gameTime, this);
                    }

                    if (enemiesLevel[i].Texture == null)
                    {
                        enemiesLevel.RemoveAt(i);
                        break;
                    }
                }
            }

        }
        
    }
}
