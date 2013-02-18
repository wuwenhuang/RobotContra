using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameStateManagementSample
{
    abstract class AnimateSprite
    {
        protected Texture2D texture;
        protected int spriteWidth, spriteHeight;
        protected int frame;

        private float timer = 0;
        public float interval = 0.1f;

        protected Rectangle sourceRect;

        public AnimateSprite()
        {
        }

        public AnimateSprite(Texture2D texture, int frame, int width, int height)
        {
            this.texture = texture;
            this.frame = frame;
            spriteWidth = width;
            spriteHeight = height;
            sourceRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
        }

        public virtual void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            switch (Character.main.currentState)
            {
                case CharacterState.IDLE:
                    if (Character.main.lastState == CharacterState.MOVELEFT)
                        sourceRect = new Rectangle(1 * spriteWidth, 0, spriteWidth, spriteHeight);
                    else
                        sourceRect = new Rectangle(0, 0, spriteWidth, spriteHeight);
                    break;

                case CharacterState.MOVELEFT:
                    if (interval < timer)
                    {
                        if (frame > 3)
                        {
                            frame = 0;
                        }
                        else
                        {
                            frame += 1;
                        }

                        sourceRect = new Rectangle(frame * spriteWidth, 2 * spriteHeight, spriteWidth, spriteHeight);

                        timer = 0f;
                    }
                    break;

                case CharacterState.MOVERIGHT:

                    if (interval < timer)
                    {
                        if (frame > 3)
                        {
                            frame = 0;
                        }
                        else
                        {
                            frame += 1;
                        }

                        sourceRect = new Rectangle(frame * spriteWidth, 1 * spriteHeight, spriteWidth, spriteHeight);

                        timer = 0f;
                    }
                    
                    break;

                case CharacterState.MOVEUP:
                    break;

                case CharacterState.MOVEDOWN:
                    break;

                case CharacterState.SHOOT:
                    if (Character.main.lastState == CharacterState.MOVELEFT)
                        sourceRect = new Rectangle(3 * spriteWidth, 0, spriteWidth, spriteHeight);
                    else
                        sourceRect = new Rectangle(2 * spriteWidth, 0, spriteWidth, spriteHeight);
                    break;

                case CharacterState.DUCK:
                    break;

                case CharacterState.JUMP:
                    break;
            }
        }

        public Texture2D Texture
        {
            get { return this.texture; }
        }

        public Rectangle SourceRect
        {
            get { return this.sourceRect; }
        }
    }
}
