using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameStateManagementSample
{
    abstract class Character
    {
        protected Texture2D texture;
        protected Vector2 position;

        public Character()
        {
        }

        public Character(Texture2D texture, Vector2 pos)
        {
            this.texture = texture;
            this.position = pos;
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.texture, this.position, Color.White);
        }

        public virtual void Update(GameTime gameTime)
        {
            
        }

    }
}
