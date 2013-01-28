using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameStateManagementSample
{
    enum CharacterState
    {
        IDLE,
        JUMP,
        MOVERIGHT,
        MOVELEFT,
        MOVEUP,
        MOVEDOWN,
        SHOOT,
        DUCK
    };

    abstract class Character
    {
        protected Texture2D texture;
        public Vector2 position;
        protected Vector2 acceleration;
        public CharacterState state;

        public Character()
        {
            this.state = CharacterState.IDLE;
        }

        public Character(Texture2D texture, Vector2 pos)
        {
            this.texture = texture;
            this.position = pos;
            this.state = CharacterState.IDLE;
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.texture, this.position, Color.White);
        }

        public virtual void Update(GameTime gameTime, Level level)
        {
            switch(state)
            {
                case CharacterState.IDLE:
                    break;

                case CharacterState.JUMP:
                    break;

                case CharacterState.SHOOT:
                    break;

                case CharacterState.MOVELEFT:
                    this.position.X -= 5;
                    break;

                case CharacterState.MOVERIGHT:
                    this.position.X += 5;
                    break;

                case CharacterState.MOVEUP:
                    foreach (Background background in level.tilesBackground)
                    {
                        if (background.walkable)
                        {
                            if (this.position.Y > background.position.Y - background.texture.Height)
                            {
                                this.position.Y -= 5;
                            }
                        }
                    }
                    break;

                case CharacterState.MOVEDOWN:
                    if (this.position.Y + this.texture.Height < 480)
                        this.position.Y += 5;
                    break;
            }
        }
    }
}
