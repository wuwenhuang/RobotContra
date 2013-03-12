using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using Microsoft.Xna.Framework.Input;
using Lidgren.Network;
using GameStateManagementSample;

namespace GameStateManagement.SideScrollGame
{
    public enum PacketTypes
    {
        CREATEPLAYER,
        DELETEPLAYER,
        MYPOSITION,
        UPDATEPLAYERS
    };

    class SideScrollGame
    {

    #region Fields
        
        private Camera2D _camera;

        public Player player;
        public Dictionary<long, Player> otherPlayers = new Dictionary<long, Player>();

        public bool gameOver = false;

        public Dictionary<int, Color> level = new Dictionary<int, Color>
        {
            {1, Color.Blue},
            {2, Color.Red},
        };

        public int currentLevel;

        public NetClient client;

        private Level _level;

        public static SideScrollGame main;

        GameplayScreen gameplay;

        private bool _isNetwork;

    #endregion

    #region PublicFunctions

        public SideScrollGame(bool isNetworkGame, PlayerColor color, GameplayScreen gameplay)
        {
            main = this;

            this.gameplay = gameplay;

            currentLevel = 1;

            _isNetwork = isNetworkGame;

            if (_isNetwork == false)
            {
                if (color == PlayerColor.BLUE)
                {
                    player = new Player(gameplay.content.Load<Texture2D>("Character/player"), new Vector2(10, 350));
                }
            }
            else
            {

                NetPeerConfiguration config = new NetPeerConfiguration("robotcontra");
                config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);

                client = new NetClient(config);
                client.Start();
                client.DiscoverLocalPeers(16868);

                Thread updateClientsWorld = new Thread(new ThreadStart(getPlayerUpdate));
                updateClientsWorld.Start();

                while (player == null)
                {
                    //getting creating new player
                }

            }
            Awake();
        }

        public static Vector2 GRAVITY { get { return new Vector2(0, -9.8f); } }
        public bool IsNetwork { get { return this._isNetwork; } }
        public bool GameOver { get { return this.gameOver; } }

        public void HandleInput(GamePadState gamePadState)
        {
            if (player != null)
                player.HandleInput(gamePadState);
        }

        public void HandleInput(KeyboardState keyboardState, MouseState mouseState)
        {
            if (player != null)
                player.HandleInput(keyboardState, mouseState);
        }

        public void Update(GameTime gameTime)
        {
            if (_isNetwork == false)
            {
                if (player != null)
                {
                    if (player.currentState == CharacterState.DEAD)
                    {
                        gameOver = true;
                    }
                    else
                    {
                        _level.Update(gameTime, player);
                    }

                    player.Update(gameTime, _level);
                }

                if (_level.enemiesLevel.Count == 0 && currentLevel < level.Count)
                {
                    currentLevel += 1;
                    this.Reset(currentLevel, level[currentLevel]);
                }
            }
            else
            {
                player.Update(gameTime, _level);

                foreach (var otherplayers in otherPlayers)
                {
                    otherplayers.Value.CharacterUpdate(gameTime, _level);
                }
            }
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _level.Draw(gameplay.ScreenManager);

            if (player != null)
                player.Draw(spriteBatch);

            if (_isNetwork)
            {
                foreach (var players in otherPlayers)
                {
                    players.Value.Draw(spriteBatch);
                }
            }
        }

        public void Reset(int levelnum, Color color)
        {
            if (player != null)
                player.Reset();

            _level.Reset();
            _camera.setPosition(Vector2.Zero);
            _level.ChangeLevel(levelnum, color);
            gameOver = false;
        }

     #endregion


    #region PrivateFunctions

        void Awake()
        {
            if (player != null)
                _level = new Level(player);

            _camera = new Camera2D();
            _camera.setPosition(Vector2.Zero);
            _camera.setSize(gameplay.ScreenManager.GraphicsDevice.Viewport.Width,
                gameplay.ScreenManager.GraphicsDevice.Viewport.Height);

            gameOver = false;

            this.Reset(currentLevel, level[currentLevel]);
        }

        void getPlayerUpdate()
        {
            while (true)
            {
                NetIncomingMessage msg;
                while ((msg = client.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryResponse:
                            // just connect to first server discovered
                            client.Connect(msg.SenderEndPoint);
                            
                            break;

                        case NetIncomingMessageType.Data:

                            switch(msg.ReadByte())
                            {
                                case (byte)PacketTypes.CREATEPLAYER:
                                    long id = msg.ReadInt64();
                                    int xPos = msg.ReadInt32();
                                    int yPos = msg.ReadInt32();
                            
                                    if (player == null)
                                    {
                                        player = new Player(id, gameplay.content.Load<Texture2D>("Character/player"), new Vector2(xPos, yPos));
                                    }
                                    break;

                                case (byte)PacketTypes.DELETEPLAYER:
                                    long deleteID = msg.ReadInt64();

                                    if (player.id == deleteID)
                                    {
                                        player = null;
                                    }
                                    else
                                    {
                                        if (otherPlayers.Count > 0)
                                        {
                                            if (otherPlayers[deleteID].id.Equals(deleteID))
                                            {
                                                otherPlayers.Remove(deleteID);
                                            }
                                        }
                                    }

                                    break;

                                case (byte)PacketTypes.UPDATEPLAYERS:
                                    long who = msg.ReadInt64();
                                    CharacterState state = (CharacterState)msg.ReadByte();
                                    CharacterState laststate = (CharacterState)msg.ReadByte();
                                    int health = msg.ReadInt32();
                                    int x = msg.ReadInt32();
                                    int y = msg.ReadInt32();

                                    if (player.id != who)
                                    {
                                        if (otherPlayers.Count > 0)
                                        {
                                            
                                            if (otherPlayers[who].id.Equals(who))
                                            {
                                                otherPlayers[who].position = new Vector2(x, y);
                                                otherPlayers[who].currentState = state;
                                                otherPlayers[who].lastState = laststate;
                                                otherPlayers[who].health = health;
                                            }
                                            else
                                            {
                                                otherPlayers[who] = new Player(who, gameplay.content.Load<Texture2D>("Character/player"), new Vector2(x, y));
                                            }
                                        }
                                        else
                                        {
                                            otherPlayers[who] = new Player(who, gameplay.content.Load<Texture2D>("Character/player"), new Vector2(x, y));
                                        }
                                    }
                                    break;
                            }
                            break;
                    }
                }

                
            }
        }

    #endregion
    }
}
