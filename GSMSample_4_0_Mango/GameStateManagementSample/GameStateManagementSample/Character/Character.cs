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

    abstract class Character : AnimateSprite
    {
        public Vector2 position;
        public float speed = 250.0f;
        public CharacterState currentState;
        public CharacterState lastState;

        protected List<GameObject> childObjects;

        public static Character main;
        
        public Character()
        {
            main = this;
            this.currentState = CharacterState.IDLE;
            this.lastState = currentState;
            this.childObjects = new List<GameObject>();
        }

        public Character(Texture2D texture, Vector2 pos, Vector2 size)
            : base(texture,0, (int)size.X,(int)size.Y)
        {
            this.position = pos;
            main = this;
            this.currentState = CharacterState.IDLE;
            this.lastState = currentState;
            this.childObjects = new List<GameObject>();
        }

        public void AddChild(GameObject newGameObject)
        {
            childObjects.Add(newGameObject);
        }

        public void RemoveChild(GameObject deleteObject)
        {
            foreach (GameObject child in childObjects)
            {
                if (child.Equals(deleteObject))
                {
                    childObjects.Remove(child);
                    break;
                }
            }
        }

        public void RemoveChild(string id)
        {
            foreach (GameObject child in childObjects)
            {
                if (child.ID.Equals(id))
                {
                    childObjects.Remove(child);
                    break;
                }
            }
        }

        public GameObject[] GetAllChildren()
        {
            return this.childObjects.ToArray();
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.texture, Camera2D.main.WorldToScreenPoint(this.position),this.sourceRect,Color.White);

            if (childObjects != null)
            {
                foreach (GameObject child in childObjects)
                {
                    child.Draw(gameTime, spriteBatch);
                }
            }
        }

        public virtual void Update(GameTime gameTime, Level level, GraphicsDevice graphics)
        {
            switch(currentState)
            {
                case CharacterState.IDLE:
                    break;

                case CharacterState.JUMP:
                    break;

                case CharacterState.SHOOT:
                    break;

                case CharacterState.MOVELEFT:
                    if (this.position.X > 0)
                    {
                        this.position.X -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (childObjects != null)
                        {
                            foreach (GameObject child in childObjects)
                            {
                                child.position.X -= child.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }
                            break;
                        }
                    }
                    break;

                case CharacterState.MOVERIGHT:
                    if (this.position.X + this.sourceRect.Width < level.width)
                    {
                        this.position.X += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (childObjects != null) 
                        {
                            foreach (GameObject child in childObjects)
                            {
                                child.position.X += child.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }
                            break;
                        }
                    }
                    break;

                case CharacterState.MOVEUP:
                    foreach (Background background in level.tilesBackground)
                    {
                        if (background.walkable)
                        {
                            if (this.position.Y > background.position.Y - background.texture.Height)
                            {
                                this.position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                                if (childObjects != null)
                                {
                                    foreach (GameObject child in childObjects)
                                    {
                                        child.position.Y -= child.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                    }
                                    break;
                                }
                            }
                            break;
                        }
                    }
                    break;

                case CharacterState.MOVEDOWN:
                    if (this.position.Y + this.sourceRect.Height < graphics.Viewport.Height)
                    {
                        this.position.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (childObjects != null)
                        {
                            foreach (GameObject child in childObjects)
                            {
                                child.position.Y += child.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                            }
                            break;
                        }
                    }
                    break;
            }
            foreach (GameObject child in childObjects)
            {
                child.Update(gameTime);
            }
            base.Update(gameTime);
        }
    }
}
