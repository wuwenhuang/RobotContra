using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameStateManagementSample
{
    class Player : Character
    {
        Weapon weapon;
        
        public Player(ContentManager content)
            : base(content.Load<Texture2D>("Character/player"), new Vector2(10,350), new Vector2(60, 130))
        {
            weapon = new Weapon(content.Load<Texture2D>("Weapon/rifle"), new Vector2(this.position.X + 5, this.position.Y + 35), new Vector2(100,30));

            this.childObjects.Add(weapon);
        }

        public void Reset()
        {
            position = new Vector2(10, 350);
        }
        
        public override void Update(GameTime gameTime, Level level, GraphicsDevice graphic)
        {
            
            base.Update(gameTime, level, graphic);  
        }

        public void HandleInput(GamePadState gamePad, KeyboardState keyboard)
        {
            if (gamePad.IsButtonDown(Buttons.DPadLeft)
                || keyboard.IsKeyDown(Keys.Left))
            {
                this.currentState = CharacterState.MOVELEFT;
                this.lastState = currentState;
            }

            else if (gamePad.IsButtonDown(Buttons.DPadRight)
                || keyboard.IsKeyDown(Keys.Right))
            {
                this.currentState = CharacterState.MOVERIGHT;
                this.lastState = currentState;
            }
                
            else if (gamePad.IsButtonDown(Buttons.DPadUp)
            || keyboard.IsKeyDown(Keys.Up))
            {
                this.currentState = CharacterState.MOVEUP;
            }

            else if (gamePad.IsButtonDown(Buttons.DPadDown)
            || keyboard.IsKeyDown(Keys.Down))
            {
                this.currentState = CharacterState.MOVEDOWN;
            }

            else if (gamePad.IsButtonDown(Buttons.DPadDown)
                || keyboard.IsKeyDown(Keys.Down))
            {
                this.currentState = CharacterState.JUMP;
            }

            else
            {
                this.currentState = CharacterState.IDLE;
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

            base.Draw(gameTime, spriteBatch);
        }

        public Texture2D getTexture()
        {
            return this.texture;
        }
    }
}
