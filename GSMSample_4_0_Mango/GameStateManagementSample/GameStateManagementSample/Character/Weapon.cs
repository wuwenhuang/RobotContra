using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace GameStateManagementSample
{
    class Weapon : GameObject
    {
        protected float rotation = 0;

        public int LEFTOFFSET = 35;

        private float _shootInterval;
        protected float shootInterval;

        public List<Bullet> bullets = new List<Bullet>();
  
        public Weapon(Texture2D texture, Vector2 position, Vector2 size)
            : base("weapon", texture, position, size)
        {
            shootInterval = 0.20f;
        }

        public void SetShootInterval(float newInterval = 0.5f)
        {
            this.shootInterval = newInterval;
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (bullets != null)
            {
                for (int i = 0; i < bullets.Count; i++)
                {
                    if (bullets[i].Texture == null)
                    {
                        bullets.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        bullets[i].Update(gameTime);
                    }
                }
            }

            switch (Character.main.currentState)
            {
                case CharacterState.MOVELEFT:
                    frame = 1;
                    sourceRect = new Rectangle(spriteWidth, frame * spriteWidth, spriteWidth, spriteHeight);
                    this.position.X -= LEFTOFFSET;
                    break;
                    
                case CharacterState.MOVERIGHT:
                    frame = 1;
                    sourceRect = new Rectangle(0, frame * spriteWidth, spriteWidth, spriteHeight);              
                    break;

                case CharacterState.SHOOT:
                    frame = 1;
                    if (_shootInterval < gameTime.TotalGameTime.TotalSeconds)
                    {
                        if (Character.main.lastState == CharacterState.MOVERIGHT)
                        {
                            bullets.Add(new BulletNormal(true));
                        }
                        else if (Character.main.lastState == CharacterState.MOVELEFT)
                        {
                            bullets.Add(new BulletNormal(false));
                        }

                        _shootInterval = (float)gameTime.TotalGameTime.TotalSeconds + shootInterval;
                    }

                    if (Character.main.lastState == CharacterState.MOVELEFT)
                    {
                        sourceRect = new Rectangle(spriteWidth, frame * spriteWidth, spriteWidth, spriteHeight);
                        this.position.X -= LEFTOFFSET;
                    }
                    else
                    {
                        sourceRect = new Rectangle(0, frame * spriteWidth, spriteWidth, spriteHeight);
                    }
                    break;
                     
                case CharacterState.IDLE:
                    if (Character.main.lastState == CharacterState.MOVELEFT)
                    {
                        sourceRect = new Rectangle(1 * spriteHeight, 0, spriteHeight, spriteWidth);
                    }
                    else
                    {
                        sourceRect = new Rectangle(0, 0, spriteHeight, spriteWidth);
                    }
                    _shootInterval = 0;
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (bullets != null)
            {
                foreach (Bullet bullet in bullets)
                {
                    if (bullet.Texture !=null)
                        bullet.Draw(spriteBatch);
                }
            }
        }
        
    }
}
