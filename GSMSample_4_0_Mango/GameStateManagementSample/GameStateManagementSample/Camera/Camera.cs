using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameStateManagementSample
{
    class Camera
    {
        private SpriteBatch spriteRenderer;
        private Vector2 cameraPosition;

        public Vector2 Position
        {
            get { return cameraPosition; }
            set { cameraPosition = value; }
        }

        public Camera(SpriteBatch renderer)
        {
            spriteRenderer = renderer;
            cameraPosition = new Vector2(0, 0);
        }

        public void DrawNode(Scene2DNode node)
        {
            // get the screen position of the node
            Vector2 drawPosition = ApplyTransformations(node.Position);
            node.Draw(spriteRenderer, drawPosition);
        }

        private Vector2 ApplyTransformations(Vector2 nodePosition)
        {
            // apply translation
            Vector2 finalPosition = nodePosition - cameraPosition;
            // you can apply scaling and rotation here also
            //.....
            //--------------------------------------------
            return finalPosition;
        }

        public void Update(GameTime gameTime, Level level, GraphicsDevice graphics, Player player)
        {
            CapCameraPosition(level, graphics);

            switch (player.state)
            {
                case CharacterState.MOVELEFT:
                    this.Translate(new Vector2(5, 0));
                    break;

                case CharacterState.MOVERIGHT:
                    this.Translate(new Vector2(-5, 0));
                    break;
            }
        }

        public void Translate(Vector2 moveVector)
        {
            cameraPosition += moveVector;
        }

        private void CapCameraPosition(Level level, GraphicsDevice graphics)
        {
            Vector2 cameraPosition = this.Position;
            if (cameraPosition.X > level.WIDTH - graphics.Viewport.Width)
            {
                cameraPosition.X = level.WIDTH - graphics.Viewport.Width;
            }
            if (cameraPosition.X < 0)
            {
                cameraPosition.X = 0;
            }
            
            this.Position = cameraPosition;
        }
    }
}
