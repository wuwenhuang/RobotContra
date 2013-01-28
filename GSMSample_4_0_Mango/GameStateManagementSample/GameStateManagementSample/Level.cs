using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameStateManagementSample
{
    class Level : LevelLoader
    {
        public Level(string levelFile, ContentManager content)
            : base(levelFile, content)
        {
        } 

        public override void  Draw(SpriteBatch theBatch)
        {
 	        base.Draw(theBatch);
        }

        public void Update(GameTime gameTime, Player player)
        {
            switch (player.state)
            {
                case CharacterState.MOVELEFT:
                    foreach (Background background in tilesBackground)
                    { 
                        background.position.X += 5;
                    }
                    break;

                case CharacterState.MOVERIGHT:
                    foreach (Background background in tilesBackground)
                    {
                        background.position.X -= 5;
                    }
                    break;
            }
        }
        
    }
}
