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
        

        public Player(ContentManager content)
            : base(content.Load<Texture2D>("Character/player"), new Vector2(10,350))
        { 
        }
        
        public override void Update(GameTime gameTime, Level level)
        {
            
            base.Update(gameTime, level);  
        }

        public void HandleInput(GamePadState gamePad, KeyboardState keyboard)
        {
            if (gamePad.IsButtonDown(Buttons.DPadLeft)
                || keyboard.IsKeyDown(Keys.Left))
            {
                this.state = CharacterState.MOVELEFT;
            }

            else if (gamePad.IsButtonDown(Buttons.DPadRight)
                || keyboard.IsKeyDown(Keys.Right))
            {
                this.state = CharacterState.MOVERIGHT;
            }

            else if (gamePad.IsButtonDown(Buttons.DPadUp)
            || keyboard.IsKeyDown(Keys.Up))
            {
                this.state = CharacterState.MOVEUP;
            }

            else if (gamePad.IsButtonDown(Buttons.DPadDown)
            || keyboard.IsKeyDown(Keys.Down))
            {
                this.state = CharacterState.MOVEDOWN;
            }

            else if (gamePad.IsButtonDown(Buttons.DPadDown)
                || keyboard.IsKeyDown(Keys.Down))
            {
                this.state = CharacterState.JUMP;
            }

            else
            {
                this.state = CharacterState.IDLE;
            }
        }
    }
}
