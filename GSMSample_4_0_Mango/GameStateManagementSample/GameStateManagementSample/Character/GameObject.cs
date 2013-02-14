using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameStateManagementSample
{
    abstract class GameObject : AnimateSprite
    {
        private string id;
        public Vector2 position;
        protected float speed;

        public GameObject()
            : base()
        {
            this.id = "";
            this.position = Vector2.Zero;
            this.speed = Character.main.speed;
        }

        public GameObject(string id, Texture2D texture, Vector2 position, Vector2 size)
            : base(texture,0, (int)size.X, (int)size.Y)
        {
            this.id = id;
            this.position = position;
            this.speed = Character.main.speed;
        }

        public void SetTexture(Texture2D newTexture)
        {
            this.texture = newTexture;
        }

        public Texture2D GetTexture()
        {
            return this.texture;
        }

        public void SetId(string newID)
        {
            this.id = newID;
        }

        public string ID { get { return this.id; } }

        public Vector2 Position { get { return this.position; } }

        public void SetPosition(Vector2 newPosition)
        {
            this.position = newPosition;
        }

        public Vector2 GetPosition()
        {
            return this.position;
        }

        public void SetSpeed(float newSpeed)
        {
            this.speed = newSpeed;
        }

        public float Speed{ get { return this.speed; } }



        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.texture, Camera2D.main.WorldToScreenPoint(this.position), Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            switch (Character.main.currentState)
            {
                case CharacterState.IDLE:
                    break;

                case CharacterState.MOVEUP:
                    break;

                case CharacterState.MOVEDOWN:
                    break;

                case CharacterState.MOVELEFT:
                    break;

                case CharacterState.MOVERIGHT:
                    break;

                case CharacterState.JUMP:
                    break;

                case CharacterState.DUCK:
                    break;

                case CharacterState.SHOOT:
                    break;
            }
        }
    }
}
