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

        public static Weapon main;
        
        public Weapon(Texture2D texture, Vector2 position, Vector2 size)
            : base("weapon", texture, position, size)
        {
            main = this;
        }
        
        public override void Update(GameTime gameTime)
        {
            switch (Character.main.currentState)
            {
                case CharacterState.MOVELEFT:
                    frame = 1;
                    sourceRect = new Rectangle(spriteWidth, frame * spriteWidth, spriteWidth, spriteHeight);
                    break;
                    
                case CharacterState.MOVERIGHT:
                    frame = 1;
                    sourceRect = new Rectangle(0, frame * spriteWidth, spriteWidth, spriteHeight);
                    break;

                case CharacterState.IDLE:
                    frame = 0;
                    sourceRect = new Rectangle(0,0, spriteHeight, spriteWidth);
                    break;
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.texture, Camera2D.main.WorldToScreenPoint(this.position), this.sourceRect, Color.White);
        }
        
    }
}
