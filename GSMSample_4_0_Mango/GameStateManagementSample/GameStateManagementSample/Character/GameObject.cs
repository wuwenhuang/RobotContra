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
        public Vector2 parentPosition;
        private Vector2 offsetPosition;

        public GameObject()
            : base()
        {
            this.id = "";
            this.position = Vector2.Zero;
            this.speed = Character.main.speed;
            this.parentPosition = Character.main.position;
            this.offsetPosition = Vector2.Zero;
        }

        public GameObject(string id, Texture2D texture, Vector2 position, Vector2 size)
            : base(texture,0, (int)size.X, (int)size.Y)
        {
            this.id = id;
            this.speed = Character.main.speed;
            this.parentPosition = Character.main.position;
            this.offsetPosition = position;
            this.position = offsetPosition + parentPosition;
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

        public void Destroy()
        {
            this.texture = null;
            this.position = Vector2.Zero;
            this.speed = 0;
            this.sourceRect = new Rectangle();
            this.id = "";
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.texture, Camera2D.main.WorldToScreenPoint(this.position), this.sourceRect, Color.White);
        }

        public override void Update(GameTime gameTime)
        {
            this.parentPosition = Character.main.position;

            switch (Character.main.currentState)
            {
                case CharacterState.IDLE:
                case CharacterState.MOVEUP:
                case CharacterState.MOVEDOWN:
                case CharacterState.MOVELEFT:
                case CharacterState.MOVERIGHT:
                case CharacterState.SHOOT:
                    this.position = parentPosition + offsetPosition;
                    break;

                case CharacterState.JUMP:
                    break;

                case CharacterState.DUCK:
                    break;

                
            }
        }
    }
}
