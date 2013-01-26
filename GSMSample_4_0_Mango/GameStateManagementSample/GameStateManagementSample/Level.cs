using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace GameStateManagementSample
{
    class Level : LevelLoader
    {
        public Level(string levelFile, ContentManager content)
            : base(levelFile, content)
        {
        }

        public void Update(GameTime gameTime)
        {
            
        }
        
    }
}
