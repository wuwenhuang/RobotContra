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
using System.Net;

namespace GameStateManagement.SideScrollGame
{
    public enum PacketTypes
    {
        CREATEPLAYER,
        GETNUMBEROFPLAYERS,
        DELETEPLAYER,

        WRITELEVEL,
        GETLEVEL,
        GETSERVERLEVEL,

        MYPOSITION,
        UPDATEPLAYERS,

        UPDATEENEMYPOSITION,
        SENDENEMYPOSITIONS,
        GETSERVERENEMYPOSITIONS,
        DELETEENEMY
    };

    class SideScrollGame
    {

    #region Fields
        
        private Camera2D _camera;

        public Player player;
        public float tolerance = 0.5f;
        public bool isHost;
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
                isHost = true;
                if (color == PlayerColor.BLUE)
                {
                    player = new Player(gameplay.content.Load<Texture2D>("Character/player"), new Vector2(10, 350));
                }

                Awake();
            }
            else
            {

                NetPeerConfiguration config = new NetPeerConfiguration("robotcontra");
                //config.LocalAddress = IPAddress.Parse("10.60.233.59"); // disable if there is firewall
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
                NetworkAwake();

            }
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

            }
            else
            {
                if (player != null)
                {
                    player.Update(gameTime, _level);

                    foreach (var otherplayers in otherPlayers)
                    {
                        otherplayers.Value.CharacterUpdate(gameTime, _level);
                    }

                    if (player.currentState == CharacterState.DEAD)
                    {
                        gameOver = true;
                    }
                    else
                    {
                        _level.Update(gameTime, player);
                    }
                }
            }

            if (_level.enemiesLevel.Count == 0 && currentLevel < level.Count)
            {
                currentLevel += 1;

                if (_isNetwork && isHost)
                {
                    this.Reset(currentLevel, level[currentLevel]);

                    NetOutgoingMessage msgOut = client.CreateMessage();

                    msgOut.Write((byte)PacketTypes.WRITELEVEL);
                    msgOut.Write((short)currentLevel);
                    msgOut.Write((short)_level.enemiesLevel.Count);

                    foreach (Enemy enemy in _level.enemiesLevel)
                    {
                        msgOut.Write((byte)enemy.currentState);
                        msgOut.Write((byte)enemy.lastState);
                        msgOut.Write((short)enemy.health);
                        msgOut.Write((int)enemy.position.X);
                        msgOut.Write((int)enemy.position.Y);
                    }

                    client.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
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

        void NetworkAwake()
        {
            currentLevel = -1;

            if (player != null)
                _level = new Level(player);
            _camera = new Camera2D();

            _camera.setSize(gameplay.ScreenManager.GraphicsDevice.Viewport.Width,
                gameplay.ScreenManager.GraphicsDevice.Viewport.Height);

            NetOutgoingMessage msg = client.CreateMessage();

            msg.Write((byte)PacketTypes.GETNUMBEROFPLAYERS);

            client.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);

            while (currentLevel < 0)
            {
            }

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
                                    float xPos = msg.ReadFloat();
                                    float yPos = msg.ReadFloat();
                            
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
                                    float x = msg.ReadFloat();
                                    float y = msg.ReadFloat();
                                    float velX = msg.ReadFloat();
                                    float velY = msg.ReadFloat();

                                    if (player != null && player.id != who)
                                    {
                                        if (otherPlayers.Count > 0)
                                        {
                                            
                                            if (otherPlayers[who].id.Equals(who))
                                            {
                                                otherPlayers[who].velocity.X = velX;
                                                otherPlayers[who].velocity.Y = velY;
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

                                case (byte)PacketTypes.GETNUMBEROFPLAYERS:

                                    int numberOfPlayers = msg.ReadInt16();

                                    if (numberOfPlayers <= 1)
                                    {
                                        currentLevel = 1;
                                        _camera.setPosition(Vector2.Zero);

                                        isHost = true;

                                        this.Reset(currentLevel, this.level[currentLevel]);

                                        NetOutgoingMessage msgOut = client.CreateMessage();

                                        msgOut.Write((byte)PacketTypes.WRITELEVEL);
                                        msgOut.Write((short)currentLevel);
                                        msgOut.Write((short)_level.enemiesLevel.Count);
                                        foreach (Enemy enemy in _level.enemiesLevel)
                                        {
                                            msgOut.Write((byte)enemy.currentState);
                                            msgOut.Write((byte)enemy.lastState);
                                            msgOut.Write((int)enemy.health);
                                            msgOut.Write((float)enemy.position.X);
                                            msgOut.Write((float)enemy.position.Y);
                                        }
                                        client.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
                                    }

                                    else
                                    {
                                        _camera.setPosition(new Vector2(player.position.X + player.SourceRect.Width - _camera.rect.Width / 2, 0));

                                        isHost = false;

                                        NetOutgoingMessage msgOut = client.CreateMessage();

                                        msgOut.Write((byte)PacketTypes.GETSERVERLEVEL);
                                        client.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
                                    }
                                    break;

                                case (byte)PacketTypes.GETLEVEL:
                                    int level = msg.ReadInt16();
                                    currentLevel = level;

                                    this.Reset(currentLevel, this.level[currentLevel]);

                                    int enemiesInLevel = msg.ReadInt16();

                                    for (int i = 0; i < enemiesInLevel; i++)
                                    {
                                        _level.enemiesLevel[i].currentState = (CharacterState)msg.ReadByte();
                                        _level.enemiesLevel[i].lastState = (CharacterState)msg.ReadByte();
                                        _level.enemiesLevel[i].health = msg.ReadInt16();
                                        _level.enemiesLevel[i].position.X = msg.ReadFloat();
                                        _level.enemiesLevel[i].position.Y = msg.ReadFloat();
                                    }

                                    if (enemiesInLevel < _level.enemiesLevel.Count)
                                    {
                                        for (int deleteEnemies = enemiesInLevel; deleteEnemies < _level.enemiesLevel.Count; deleteEnemies++)
                                        {
                                            _level.enemiesLevel.RemoveAt(deleteEnemies);
                                        }
                                    }
                                    break;

                                case (byte)PacketTypes.SENDENEMYPOSITIONS:
                                    int enemiesLevel = msg.ReadInt16();

                                    for (int i = 0; i < enemiesLevel; i++)
                                    {
                                        if (_level.enemiesLevel[i].health > 0)
                                        {
                                            _level.enemiesLevel[i].currentState = (CharacterState)msg.ReadByte();
                                            _level.enemiesLevel[i].lastState = (CharacterState)msg.ReadByte();
                                            _level.enemiesLevel[i].health = msg.ReadInt16();
                                            _level.enemiesLevel[i].position.X = msg.ReadFloat();
                                            _level.enemiesLevel[i].position.Y = msg.ReadFloat();
                                        }
                                        else
                                        {
                                            _level.enemiesLevel.RemoveAt(i);
                                            break;
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
