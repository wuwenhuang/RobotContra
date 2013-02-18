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
        private Weapon weapon;
        
        public Player()
            : base(GameplayScreen.main.content.Load<Texture2D>("Character/player"), new Vector2(10,350), new Vector2(70, 130))
        {
            weapon = new Weapon(GameplayScreen.main.content.Load<Texture2D>("Weapon/rifle"), new Vector2(5,35), new Vector2(100,30));

            this.AddChild(weapon);
        }

        public void Reset()
        {
            position = new Vector2(10, 350);
        }
        
        public override void Update(GameTime gameTime, Level level, GraphicsDevice graphic)
        {
            
            base.Update(gameTime, level, graphic);  
        }

        public void HandleInput(GamePadState gamePad)
        {
            switch (currentState)
            {
                case CharacterState.IDLE:

                    if (gamePad.IsButtonDown(Buttons.DPadLeft))
                    {
                        this.currentState = CharacterState.MOVELEFT;

                    }

                    if (gamePad.IsButtonDown(Buttons.DPadRight))
                    {
                        this.currentState = CharacterState.MOVERIGHT;
                    }

                    if (gamePad.IsButtonDown(Buttons.DPadUp))
                    {
                        this.currentState = CharacterState.MOVEUP;
                    }

                    if (gamePad.IsButtonDown(Buttons.DPadDown))
                    {
                        this.currentState = CharacterState.MOVEDOWN;
                    }

                    if (gamePad.IsButtonDown(Buttons.X))
                    {
                        this.currentState = CharacterState.JUMP;
                    }

                    if (gamePad.IsButtonDown(Buttons.Y))
                    {
                        this.currentState = CharacterState.SHOOT;
                    }
                    break;

                case CharacterState.MOVERIGHT:
                    if (gamePad.IsButtonUp(Buttons.DPadRight))
                    {
                        this.lastState = currentState;
                        this.currentState = CharacterState.IDLE;
                    }
                    else
                    {
                        if (gamePad.IsButtonDown(Buttons.Y))
                        {
                            this.currentState = CharacterState.SHOOT;
                        }
                    }
                    break;

                case CharacterState.MOVELEFT:
                    if (gamePad.IsButtonUp(Buttons.DPadLeft))
                    {
                        this.lastState = currentState;
                        this.currentState = CharacterState.IDLE;
                    }
                    else
                    {
                        if (gamePad.IsButtonDown(Buttons.Y))
                        {
                            this.currentState = CharacterState.SHOOT;
                        }
                    }
                    break;

                case CharacterState.MOVEUP:
                    if (gamePad.IsButtonUp(Buttons.DPadUp))
                    {
                        this.currentState = CharacterState.IDLE;
                    }
                    break;

                case CharacterState.MOVEDOWN:
                    if (gamePad.IsButtonUp(Buttons.DPadDown))
                    {
                        this.currentState = CharacterState.IDLE;
                    }

                    break;

                case CharacterState.SHOOT:
                    if (gamePad.IsButtonUp(Buttons.X))
                    {
                        this.currentState = CharacterState.IDLE;
                    }
                    break;
            }

        }

        public void HandleInput(KeyboardState keyPad, MouseState mousePad)
        {
            switch (currentState)
            {
                case CharacterState.IDLE:

                    if (keyPad.IsKeyDown(Keys.A))
                    {
                        this.currentState = CharacterState.MOVELEFT;
                    }

                    if (keyPad.IsKeyDown(Keys.D))
                    {
                        this.currentState = CharacterState.MOVERIGHT;
                    }

                    if (keyPad.IsKeyDown(Keys.W))
                    {
                        this.currentState = CharacterState.MOVEUP;
                    }

                    if (keyPad.IsKeyDown(Keys.S))
                    {
                        this.currentState = CharacterState.MOVEDOWN;
                    }

                    if (keyPad.IsKeyDown(Keys.Space))
                    {
                        this.currentState = CharacterState.JUMP;
                    }
                    if (mousePad.LeftButton == ButtonState.Pressed)
                    {
                        this.currentState = CharacterState.SHOOT;
                    }
                    
                    break;

                case CharacterState.MOVERIGHT:
                    if (keyPad.IsKeyUp(Keys.D))
                    {
                        this.lastState = currentState;
                        this.currentState = CharacterState.IDLE;
                    }
                    else
                    {
                        if (mousePad.LeftButton == ButtonState.Pressed)
                        {
                            this.currentState = CharacterState.SHOOT;
                        }
                    }
                    
                    break;

                case CharacterState.MOVELEFT:
                    if (keyPad.IsKeyUp(Keys.A))
                    {
                        this.lastState = currentState;
                        this.currentState = CharacterState.IDLE;
                    }
                    else
                    {
                        if (mousePad.LeftButton == ButtonState.Pressed)
                        {
                            this.currentState = CharacterState.SHOOT;
                        }
                    }
                    break;

                case CharacterState.MOVEUP:
                    if (keyPad.IsKeyUp(Keys.W))
                    {
                        this.currentState = CharacterState.IDLE;
                    }
                    break;

                case CharacterState.MOVEDOWN:
                    if (keyPad.IsKeyUp(Keys.S))
                    {
                        this.currentState = CharacterState.IDLE;
                    }
                    break;

                case CharacterState.SHOOT:
                    if (mousePad.LeftButton == ButtonState.Released)
                    {
                        this.currentState = CharacterState.IDLE;
                    }
                    break;
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
