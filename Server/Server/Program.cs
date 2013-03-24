using System;
using System.Threading;
using GameStateManagement;
using Lidgren.Network;
using System.Collections.Generic;

namespace XnaGameServer
{

    enum PacketTypes
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

    enum CharacterState
    {
        IDLE,
        JUMP,
        MOVERIGHT,
        MOVELEFT,
        MOVEUP,
        MOVEDOWN,
        SHOOT,
        BOOST,
        DEAD,

        // SOME ENEMY ATTACK
        ATTACK
    };

    class MultiplayerPlayers
    {
        public long id;
        public int x,y;
        public CharacterState state;
        public CharacterState lastState;
        public int health;

        public MultiplayerPlayers(long id)
        {
            this.id = id;
        }
    };

    class Enemy
    {
        public int x,y;
        public CharacterState state;
        public CharacterState lastState;
        public int health;

    };

    class Program
    {
        static List<MultiplayerPlayers> multiplayerPlayers = new List<MultiplayerPlayers>();
        static List<Enemy> enemies = new List<Enemy>();

        static double nextSendUpdates;
        static NetServer server;
        static bool updateEnemy = true;
        static Semaphore sem;
        static bool deleteEnemy = false;

        static int level;

        static void Main(string[] args)
        {
            sem = new Semaphore(1, 1);
            NetPeerConfiguration config = new NetPeerConfiguration("robotcontra");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 16868;

            Thread _writeClientUpdate = new Thread(new ThreadStart(writeClientsUpdate));
            
            // create and start server
            server = new NetServer(config);
            server.Start();
            
            // schedule initial sending of position updates
            double nextSendUpdates = NetTime.Now;

            _writeClientUpdate.Start();

            // run until escape is pressed
            while (!Console.KeyAvailable || Console.ReadKey().Key != ConsoleKey.Escape)
            {
                NetIncomingMessage msg;
                while ((msg = server.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryRequest:
                            //
                            // Server received a discovery request from a client; send a discovery response (with no extra data attached)
                            //
                            
                            server.SendDiscoveryResponse(null, msg.SenderEndPoint);
                            break;
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            //
                            // Just print diagnostic messages to console
                            //
                            Console.WriteLine(msg.ReadString());
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                            if (status == NetConnectionStatus.Connected)
                            {
                                //
                                // A new player just connected!
                                //
                                Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " connected!");
                                
                                multiplayerPlayers.Add(new MultiplayerPlayers(msg.SenderConnection.RemoteUniqueIdentifier));

                                // randomize his position and store in connection tag
                                if (multiplayerPlayers.Count <= 1)
                                {
                                    multiplayerPlayers[0].x = 10;
                                    multiplayerPlayers[0].y = 350;
                                }
                                else
                                {
                                    multiplayerPlayers[multiplayerPlayers.Count-1].x = multiplayerPlayers[multiplayerPlayers.Count - 2].x + 70;
                                    multiplayerPlayers[multiplayerPlayers.Count-1].y = multiplayerPlayers[multiplayerPlayers.Count - 2].y;
                                }

                                for (int i = 0; i < server.Connections.Count; i++)
                                {
                                    if (server.Connections[i].RemoteUniqueIdentifier == msg.SenderConnection.RemoteUniqueIdentifier)
                                    {
                                        NetConnection player = server.Connections[i] as NetConnection;
                                        NetOutgoingMessage outMessage = server.CreateMessage();
                                        outMessage.Write((byte)PacketTypes.CREATEPLAYER);
                                        outMessage.Write((long)multiplayerPlayers[i].id);
                                        outMessage.Write((int)multiplayerPlayers[i].x);
                                        outMessage.Write((int)multiplayerPlayers[i].y);
                                        server.SendMessage(outMessage, player, NetDeliveryMethod.ReliableOrdered);
                                        break;
                                    }
                                }
                            }
                            else if (status == NetConnectionStatus.Disconnected || status == NetConnectionStatus.Disconnecting)
                            {
                                Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " DISCONNECTED FROM SERVER!");

                                for (int i = 0; i < multiplayerPlayers.Count; i++)
                                {
                                    if (multiplayerPlayers[i].id == msg.SenderConnection.RemoteUniqueIdentifier)
                                    {

                                        if (deletePlayerFromServer(msg.SenderConnection.RemoteUniqueIdentifier))
                                        {
                                            Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " DELETED!");
                                        }
                                        else
                                        {
                                            Console.WriteLine(NetUtility.ToHexString(msg.SenderConnection.RemoteUniqueIdentifier) + " IS NOT EXIST!");
                                        }

                                        foreach (NetConnection player in server.Connections)
                                        {
                                            NetOutgoingMessage outMessage = server.CreateMessage();
                                            outMessage.Write((byte)PacketTypes.DELETEPLAYER);
                                            outMessage.Write((long)msg.SenderConnection.RemoteUniqueIdentifier);

                                            server.SendMessage(outMessage, player, NetDeliveryMethod.ReliableOrdered);
                                        }
                                        break;
                                    }
                                }
                            }

                            break;
                        case NetIncomingMessageType.Data:
                            
                            switch (msg.ReadByte())
                            {
                                case (byte)PacketTypes.MYPOSITION:
                                    CharacterState state = (CharacterState)msg.ReadByte();
                                    CharacterState laststate = (CharacterState)msg.ReadByte();
                                    int health = msg.ReadInt32();
                                    int xPosition = msg.ReadInt32();
                                    int yPosition = msg.ReadInt32();

                                    foreach (MultiplayerPlayers players in multiplayerPlayers)
                                    {
                                        if (players.id == msg.SenderConnection.RemoteUniqueIdentifier)
                                        {
                                            players.state = state;
                                            players.lastState = laststate;
                                            players.health = health;
                                            players.x = xPosition;
                                            players.y = yPosition;
                                            break;
                                        }
                                    }
                                    break;

                                case (byte)PacketTypes.GETNUMBEROFPLAYERS:
                                    NetOutgoingMessage msgOut = server.CreateMessage();

                                    msgOut.Write((byte)PacketTypes.GETNUMBEROFPLAYERS);
                                    msgOut.Write((short)multiplayerPlayers.Count);
                                    server.SendMessage(msgOut, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

                                    break;

                                case (byte)PacketTypes.WRITELEVEL:
                                    level = msg.ReadInt16();
                                    int enemiesInLevel = msg.ReadInt16();

                                    for (int i = 0; i < enemiesInLevel; i++)
                                    {
                                        Enemy tempEnemy = new Enemy();
                                        tempEnemy.state = (CharacterState)msg.ReadByte();
                                        tempEnemy.lastState = (CharacterState)msg.ReadByte();
                                        tempEnemy.health = msg.ReadInt32();
                                        tempEnemy.x = msg.ReadInt32();
                                        tempEnemy.y = msg.ReadInt32();
                                        enemies.Add(tempEnemy);

                                    }


                                    break;

                                case (byte)PacketTypes.UPDATEENEMYPOSITION:
                                    int enemyInLevel = msg.ReadInt16();
                                    for (int i = 0; i < enemyInLevel; i++)
                                    {
                                        int readHealth = msg.ReadInt16();
                                        if (readHealth > 0)
                                        {
                                            enemies[i].state = (CharacterState)msg.ReadByte();
                                            enemies[i].lastState = (CharacterState)msg.ReadByte();
                                            enemies[i].health = readHealth;
                                            enemies[i].x = msg.ReadInt32();
                                            enemies[i].y = msg.ReadInt32();
                                        }
                                        else
                                        {
                                            enemies.RemoveAt(i);
                                            break;
                                        }

                                    }
                                    break;

                                case (byte)PacketTypes.DELETEENEMY:
                                    sem.WaitOne();
                                    enemies.RemoveAt(msg.ReadInt16());
                                    sem.Release();
                                    break;

                                case (byte)PacketTypes.GETSERVERENEMYPOSITIONS:
                                    msgOut = server.CreateMessage();

                                    msgOut.Write((byte)PacketTypes.SENDENEMYPOSITIONS);
                                    msgOut.Write((short)enemies.Count);

                                    for (int i = 0; i < enemies.Count; i++)
                                    {
                                        if (enemies[i].health > 0)
                                        {
                                            msgOut.Write((byte)enemies[i].state);
                                            msgOut.Write((byte)enemies[i].lastState);
                                            msgOut.Write((short)enemies[i].health);
                                            msgOut.Write((int)enemies[i].x);
                                            msgOut.Write((int)enemies[i].y);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    server.SendMessage(msgOut, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);

                                    break;

                                case (byte)PacketTypes.GETSERVERLEVEL:
                                    msgOut = server.CreateMessage();

                                    msgOut.Write((byte)PacketTypes.GETLEVEL);
                                    msgOut.Write((short)level);
                                    msgOut.Write((short)enemies.Count);

                                    for (int i = 0; i < enemies.Count; i++)
                                    {
                                        msgOut.Write((byte)enemies[i].state);
                                        msgOut.Write((byte)enemies[i].lastState);
                                        msgOut.Write((short)enemies[i].health);
                                        msgOut.Write((int)enemies[i].x);
                                        msgOut.Write((int)enemies[i].y);

                                    }
                                    server.SendMessage(msgOut, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered);
                                    break;
                            }
                            break;
                    }

                }

                double now = NetTime.Now;
                if (now > nextSendUpdates)
                {
                    // Yes, it's time to send position updates

                    // for each player...
                    for (int i = 0; i < server.Connections.Count; i++)
                    {
                        NetConnection player = server.Connections[i] as NetConnection;
                        // ... send information about every other player (actually including self)
                        for (int j = 0; j < server.Connections.Count; j++)
                        {
                            // send position update about 'otherPlayer' to 'player'
                            NetOutgoingMessage om = server.CreateMessage();

                            // write who this position is for
                            om.Write((byte)PacketTypes.UPDATEPLAYERS);
                            om.Write((long)multiplayerPlayers[j].id);
                            om.Write((byte)multiplayerPlayers[j].state);
                            om.Write((byte)multiplayerPlayers[j].lastState);
                            om.Write((int)multiplayerPlayers[j].health);
                            om.Write((int)multiplayerPlayers[j].x);
                            om.Write((int)multiplayerPlayers[j].y);

                            // send message
                            server.SendMessage(om, player, NetDeliveryMethod.Unreliable);
                        }

                    }

                    // schedule next update
                    nextSendUpdates += (1.0 / 60.0);
                }
                // sleep to allow other processes to run smoothly
                Thread.Sleep(1);
            }

            server.Shutdown("app exiting");
        }

        static bool deletePlayerFromServer(long id)
        {
            for (int i = 0; i < multiplayerPlayers.Count; i++)
            {
                if (multiplayerPlayers[i].id.Equals(id))
                {
                    multiplayerPlayers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        static void writeClientsUpdate()
        {
            ////
            //// send position updates 60 times per second
            ////
            while (true)
            {
                if (enemies.Count > 0)
                {
                    for (int i = 0; i < enemies.Count; i++)
                    {
                        if (enemies[i].health <= 0)
                        {
                            enemies.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
