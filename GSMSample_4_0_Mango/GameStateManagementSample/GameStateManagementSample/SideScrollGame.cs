﻿using System;
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
using System.Net.Sockets;

namespace GameStateManagement.SideScrollGame
{
    public enum PacketTypes
    {
        CREATEPLAYER,
        GETNUMBEROFPLAYERS,
        DELETEPLAYER,
        CHANGEHOST,

        WRITELEVEL,
        GETLEVEL,
        GETSERVERLEVEL,
        GETNEWLEVEL,

        MYPOSITION,
        UPDATEPLAYERS,
        UPDATEVELOCITY,
        SENDUPDATEVELOCITY,
        SENDPLAYERDEAD,

        UPDATEENEMYPOSITION,
        SENDENEMYPOSITIONS,
        GETSERVERENEMYPOSITIONS,
        DELETEENEMY,

        SENDENEMYTARGETPLAYER,
        GETENEMYTARGETPLAYER
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

        public bool isFinishWriteLevel;

        GameplayScreen gameplay;

        private bool _isNetwork;

        private bool _goToNextLevel;

        public bool allPlayersDead;
        public bool allEnemiesDead;

    #endregion

    #region PublicFunctions

        public SideScrollGame(bool isNetworkGame, PlayerColor color, GameplayScreen gameplay)
        {
            main = this;

            gameOver = false;
            allPlayersDead = false;
            allEnemiesDead = false;

            this.gameplay = gameplay;

            currentLevel = 1;

            _goToNextLevel = false;

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
                string localIp = LocalIPAddress();
                config.LocalAddress = IPAddress.Parse(localIp); // disable if there is firewall
                config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);

                client = new NetClient(config);
                client.Start();
                client.DiscoverLocalPeers(16868);

                Thread updateClientsWorld = new Thread(new ThreadStart(getPlayerUpdate));
                updateClientsWorld.Start();

                isFinishWriteLevel = false;

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
            if (gameOver == false)
            {
                if (_isNetwork == false)
                {
                    if (player != null)
                    {
                        if (player.Dead == true)
                        {
                            gameOver = true;
                            allPlayersDead = true;
                            allEnemiesDead = false;
                        }
                        else
                        {
                            player.Update(gameTime, _level);
                            _level.Update(gameTime, player);
                        }

                    }
                    if (currentLevel <= level.Count)
                    {
                        for (int i = 0; i < _level.enemiesLevel.Count; i++)
                        {
                            if (i == _level.enemiesLevel.Count - 1)
                            {
                                if (_level.enemiesLevel[i].Dead == true && _goToNextLevel == false)
                                {
                                    _goToNextLevel = true;
                                    break;
                                }
                                else
                                    break;
                            }

                            if (_level.enemiesLevel[i].Dead == true)
                                continue;
                            else
                            {
                                _goToNextLevel = false;
                                break;
                            }
                        }
                    }

                    if (_goToNextLevel == true && currentLevel <= level.Count)
                    {
                        _goToNextLevel = false;
                        currentLevel += 1;

                        if (currentLevel > level.Count)
                        {
                            gameOver = true;
                            allEnemiesDead = true;
                            allPlayersDead = false;
                            _goToNextLevel = false;
                        }

                        //single player reset level
                        if (gameOver == false)
                            this.Reset(currentLevel, level[currentLevel]);

                    }

                }
                // Networks is true  
                else
                {
                    if (player != null)
                    {
                        if (player.Dead == false)
                            player.Update(gameTime, _level);
                        else
                            CheckPlayerOtherPlayersDead();
                        
                        foreach (var otherplayers in otherPlayers)
                        {
                            if (otherplayers.Value.Dead == false)
                                otherplayers.Value.CharacterUpdate(gameTime, _level);
                            else
                                CheckPlayerOtherPlayersDead();
                        }

                        
                        _level.Update(gameTime, player);

                        if (gameOver)
                        {
                            allPlayersDead = true;
                            allEnemiesDead = false;
                            _goToNextLevel = false;
                        }

                    }
                }
            }
            
        }

        public void CheckPlayerOtherPlayersDead()
        {
            if (player.Dead == true)
            {
                if (otherPlayers.Count <= 0)
                {
                    gameOver = true;
                }
                else
                {
                    foreach (var otherplayers in otherPlayers)
                    {
                        if (otherplayers.Value.Dead == true)
                        {
                            gameOver = true;
                            continue;
                        }
                        else
                        {
                            gameOver = false;
                            break;
                        }
                    }
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

        private string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

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
                _level = new Level();
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

        void ResetAllPlayersPositions()
        {
            player.position = player.initialPosition;

            if (otherPlayers.Count > 0)
            {
                foreach (var otherPlayer in otherPlayers)
                {
                    otherPlayer.Value.position = otherPlayer.Value.initialPosition;
                }
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
                                        player.initialPosition = player.position;
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

                                case (byte)PacketTypes.CHANGEHOST:
                                    this.isHost = msg.ReadBoolean();

                                    break;

                                case (byte)PacketTypes.SENDUPDATEVELOCITY:
                                    long updateVelocityWho = msg.ReadInt64();
                                    CharacterState currState = (CharacterState)msg.ReadByte();
                                    CharacterState lastState = (CharacterState)msg.ReadByte();
                                    float lastPosX = msg.ReadFloat();
                                    float lastPosY = msg.ReadFloat();
                                    float velocityX = msg.ReadFloat();
                                    float velocityY = msg.ReadFloat();

                                    if (player != null && player.id != updateVelocityWho)
                                    {
                                        if (otherPlayers.Count > 0)
                                        {
                                            if (otherPlayers[updateVelocityWho].id.Equals(updateVelocityWho))
                                            {
                                                if (otherPlayers[updateVelocityWho].currentState != CharacterState.JUMP)
                                                {
                                                    otherPlayers[updateVelocityWho].currentState = currState;
                                                    otherPlayers[updateVelocityWho].lastState = lastState;
                                                    otherPlayers[updateVelocityWho].lastPosition.X = lastPosX;
                                                    otherPlayers[updateVelocityWho].lastPosition.Y = lastPosY;
                                                    otherPlayers[updateVelocityWho].velocity.X = velocityX;
                                                    otherPlayers[updateVelocityWho].velocity.Y = velocityY;
                                                }
                                            }
                                        }
                                    }
                                    break;

                                case (byte)PacketTypes.UPDATEPLAYERS:
                                    long who = msg.ReadInt64();
                                    CharacterState state = (CharacterState)msg.ReadByte();
                                    CharacterState laststate = (CharacterState)msg.ReadByte();
                                    int health = msg.ReadInt32();
                                    bool dead = msg.ReadBoolean();
                                    float x = msg.ReadFloat();
                                    float y = msg.ReadFloat();

                                    if (player != null && player.id != who)
                                    {
                                        if (otherPlayers.Count > 0)
                                        {
                                            if (otherPlayers[who].id.Equals(who))
                                            {
                                                if (state != CharacterState.JUMP && otherPlayers[who].currentState != CharacterState.JUMP)
                                                {
                                                    otherPlayers[who].currentState = state;
                                                    otherPlayers[who].lastState = laststate;
                                                }
                                                otherPlayers[who].health = health;
                                                otherPlayers[who].setDead(dead);
                                            }
                                            else
                                            {
                                                otherPlayers[who] = new Player(who, gameplay.content.Load<Texture2D>("Character/player"), new Vector2(x, y));
                                                otherPlayers[who].initialPosition = otherPlayers[who].position;
                                            }
                                        }
                                        else
                                        {
                                            otherPlayers[who] = new Player(who, gameplay.content.Load<Texture2D>("Character/player"), new Vector2(x, y));
                                            otherPlayers[who].initialPosition = otherPlayers[who].position;
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
                                        
                                        NetOutgoingMessage outMsg = SideScrollGame.main.client.CreateMessage();

                                        outMsg.Write((byte)PacketTypes.WRITELEVEL);

                                        outMsg.Write((short)_level.enemiesLevel.Count);
                                        outMsg.Write((short)SideScrollGame.main.currentLevel);

                                        foreach (Enemy enemy in _level.enemiesLevel)
                                        {
                                            outMsg.Write((short)enemy.health);
                                            outMsg.Write((byte)enemy.currentState);
                                            outMsg.Write((byte)enemy.lastState);

                                            outMsg.Write((float)enemy.position.X);
                                            outMsg.Write((float)enemy.position.Y);

                                        }
                                        SideScrollGame.main.client.SendMessage(outMsg, NetDeliveryMethod.Unreliable);

                                        isFinishWriteLevel = true;
                                    }

                                    else
                                    {
                                        _camera.setPosition(new Vector2(player.position.X + player.SourceRect.Width - _camera.rect.Width / 2, 0));

                                        isHost = false;

                                        NetOutgoingMessage msgOut = client.CreateMessage();
                                        msgOut.Write((byte)PacketTypes.GETSERVERLEVEL);
                                        SideScrollGame.main.client.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
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
                                        _level.enemiesLevel[i].setDead(msg.ReadBoolean());
                                        _level.enemiesLevel[i].position.X = msg.ReadFloat();
                                        _level.enemiesLevel[i].position.Y = msg.ReadFloat();
                                    }

                                    NetOutgoingMessage levelOut = client.CreateMessage();

                                    levelOut.Write((byte)PacketTypes.GETENEMYTARGETPLAYER);

                                    client.SendMessage(levelOut, NetDeliveryMethod.ReliableOrdered);
                                    break;

                                case (byte)PacketTypes.SENDENEMYPOSITIONS:


                                    if (currentLevel > 0)
                                    {
                                        for (int i = 0; i < _level.enemiesLevel.Count; i++)
                                        {
                                            _level.enemiesLevel[i].currentState = (CharacterState)msg.ReadByte();
                                            _level.enemiesLevel[i].lastState = (CharacterState)msg.ReadByte();
                                            _level.enemiesLevel[i].health = msg.ReadInt32();
                                            _level.enemiesLevel[i].setDead(msg.ReadBoolean());
                                            _level.enemiesLevel[i].position.X = msg.ReadFloat();
                                            _level.enemiesLevel[i].position.Y = msg.ReadFloat();

                                        }
                                    }

                                    break;

                                case (byte)PacketTypes.SENDENEMYTARGETPLAYER:
                                    for (int i = 0; i < _level.enemiesLevel.Count; i++)
                                    {
                                        long tempTargetPlayer = msg.ReadInt64();

                                        if (player.id == tempTargetPlayer)
                                        {
                                            _level.enemiesLevel[i].SetTargetPlayer(player);
                                        }
                                        else
                                        {
                                            if (otherPlayers.ContainsKey(tempTargetPlayer) == true)
                                                _level.enemiesLevel[i].SetTargetPlayer(otherPlayers[tempTargetPlayer]);
                                        }
                                    }
                                    break;

                                case (byte)PacketTypes.GETNEWLEVEL:

                                    isFinishWriteLevel = false;

                                    int newLevel = msg.ReadInt32();
                                    currentLevel = newLevel;

                                    if (currentLevel > this.level.Count)
                                    {
                                        gameOver = true;
                                        allEnemiesDead = true;
                                        allPlayersDead = false;
                                        _goToNextLevel = false;
                                    }
                                    else
                                    {

                                        this.Reset(currentLevel, this.level[currentLevel]);

                                        if (IsNetwork == true)
                                        {
                                            ResetAllPlayersPositions();
                                            if (isHost == true)
                                            {
                                                NetOutgoingMessage outMsg = client.CreateMessage();

                                                outMsg.Write((byte)PacketTypes.WRITELEVEL);

                                                outMsg.Write((short)_level.enemiesLevel.Count);
                                                outMsg.Write((short)currentLevel);

                                                foreach (Enemy enemy in _level.enemiesLevel)
                                                {
                                                    outMsg.Write((short)enemy.health);
                                                    outMsg.Write((byte)enemy.currentState);
                                                    outMsg.Write((byte)enemy.lastState);

                                                    outMsg.Write((float)enemy.position.X);
                                                    outMsg.Write((float)enemy.position.Y);

                                                }
                                                client.SendMessage(outMsg, NetDeliveryMethod.Unreliable);

                                                isFinishWriteLevel = true;
                                            }

                                            else
                                            {
                                                NetOutgoingMessage msgOut = client.CreateMessage();
                                                msgOut.Write((byte)PacketTypes.GETSERVERLEVEL);
                                                SideScrollGame.main.client.SendMessage(msgOut, NetDeliveryMethod.ReliableOrdered);
                                            }
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
