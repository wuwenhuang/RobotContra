using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace GameStateManagementSample
{
    abstract class LevelLoader : Background
    {
        //The types of textures (symbols) being defined by and being used in the level
        private Dictionary<char, Background> mTextures = new Dictionary<char, Background>();

        public List<Background> tilesBackground = new List<Background>();

        private XmlReader reader;

        private string levelFile;
        
        //All of the tiles that make up the level
        public List<Background> mTexture = new List<Background>();

        //The starting position for the first tile. 
        int mStartX = 0;
        int mStartY = 0;

        public Vector2 pos;

        //The default height and width for the tiles that make up the level
        private int gameWidth = 0;

        public LevelLoader(string theFile, ContentManager theContent)
            : base()
        {
            pos = new Vector2(mStartX, mStartY);
            LoadLevelFile(theFile, theContent);
        }

        public int WIDTH
        {
            get { return this.gameWidth; }
            set { this.gameWidth = value; }
        }

        public void LoadLevelFile(string theLevelFile, ContentManager theContent)
        {
            //Cycle through the elements in the Level XML file. For each "tile" element encountered
            //load the tile information. For each "level" element encountered, load the level information
            levelFile = theLevelFile;
            reader = XmlReader.Create(levelFile);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "texture":
                            {
                                LoadTile(reader, theContent);
                                break;
                            }

                        case "level":
                            {
                                LoadLevel(reader, theContent);
                            }
                            break;
                    }
                }
            }
        }
        
        //Load information about the tile defined in the Level XML file
        private void LoadTile(XmlReader reader, ContentManager theContent)
        {
            string currentElement = string.Empty;
            Background texture = new Background();

            while (reader.Read())
            {
                //Exit the While loop when the end node is encountered and add the Tile
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name.Equals("texture", StringComparison.OrdinalIgnoreCase))
                {
                    mTextures.Add(texture.id, texture);
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    currentElement = reader.Name;

                    switch (currentElement)
                    {
                        case "id":
                            {
                                texture.id = reader.ReadElementContentAsString().ToCharArray()[0];
                                break;
                            }
                        case "picture":
                            {
                                LoadTexture(reader, theContent, texture);
                                break;
                            }
                        case "properties":
                            {
                                LoadProperties(reader, texture);
                                break;
                            }
                    }
                }
            }
        }

        private void LoadTexture(XmlReader reader, ContentManager theContent, Background theTile)
        {
            string currentElement = string.Empty;

            while (reader.Read())
            {
                //Exit the While loop when the end node is encountered and add the Tile
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name.Equals("picture", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    currentElement = reader.Name;

                    switch (currentElement)
                    {
                        case "name":
                            {
                                string aAssetName = reader.ReadElementContentAsString();
                                theTile.texture = theContent.Load<Texture2D>("Background/"+aAssetName);
                                theTile.position = new Vector2(theTile.texture.Width, theTile.texture.Height);
                                break;
                            }
                    }
                }
            }



        }

        private void LoadProperties(XmlReader theReader, Background theTile)
        {
            string aCurrentElement = string.Empty;

            while (theReader.Read())
            {
                if (theReader.NodeType == XmlNodeType.EndElement &&
                    theReader.Name.Equals("properties", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (theReader.NodeType == XmlNodeType.Element)
                {
                    aCurrentElement = theReader.Name;
                    switch (aCurrentElement)
                    {
                        case "walkable":
                            {
                                theTile.walkable = theReader.ReadElementContentAsBoolean();
                                break;
                            }
                    }
                }
            }
        }

        public void LoadLevel(XmlReader reader, ContentManager theContent)
        {
            int aPositionY = 0;
            int aPositionX = 0;
            Random rand = new Random();

            bool addNewLineBool = false;

            string aCurrentElement = string.Empty;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name.Equals("level", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {
                    aCurrentElement = reader.Name;
                    switch (aCurrentElement)
                    {
                        case "startX":
                            {
                                mStartX = reader.ReadElementContentAsInt();
                                break;
                            }

                        case "startY":
                            {
                                mStartY = reader.ReadElementContentAsInt();
                                break;
                            }
                    }
                    pos = new Vector2(mStartX, mStartY);
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (aCurrentElement == "row")
                    {
                        if (addNewLineBool)
                            aPositionY += 120;

                        aPositionX = 0;
                    }
                }
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    if (aCurrentElement == "row")
                    {
                        string aRow = reader.Value;

                        for (int aCounter = 0; aCounter < aRow.Length; ++aCounter)
                        {
                            if (mTextures.ContainsKey(aRow[aCounter]) == true)
                            {
                                if (mTextures[aRow[aCounter]].id == 'R')
                                {
                                    aPositionY = 480 - mTextures[aRow[aCounter]].texture.Height;
                                }
                                else if (mTextures[aRow[aCounter]].id == 'S')
                                {
                                    addNewLineBool = false;
                                }
                                else
                                {
                                    addNewLineBool = true;
                                }
                                tilesBackground.Add(new Background(mTextures[aRow[aCounter]].texture, new Vector2(aPositionX, aPositionY), mTextures[aRow[aCounter]].walkable));
                                aPositionX += mTextures[aRow[aCounter]].texture.Width;

                            }
                            else
                            {
                                aPositionX += rand.Next(100, 200);
                            }

                            if (aCounter == 1)
                                gameWidth = aPositionX;
                        }
                    }
                }
            }
        }

        //Draw the currently loaded level
        public virtual void Draw(SpriteBatch theBatch)
        {
            foreach (Background background in tilesBackground)
            {
                theBatch.Draw(background.texture, background.position, Color.White);
            }
        }
    }
}
