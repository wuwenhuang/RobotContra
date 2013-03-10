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
        MYPOSITION,
        UPDATEPLAYERS
    };

    class MultiplayerPlayers
    {
        public long id;
        public int x,y;

        public MultiplayerPlayers(long id)
        {
            this.id = id;
        }
    };

    class Program
    {
        static List<MultiplayerPlayers> multiplayerPlayers = new List<MultiplayerPlayers>();

        static void Main(string[] args)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("robotcontra");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.Port = 16868;

            // create and start server
            NetServer server = new NetServer(config);
            server.Start();

            // schedule initial sending of position updates
            double nextSendUpdates = NetTime.Now;

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

                            break;
                        case NetIncomingMessageType.Data:
                            switch (msg.ReadByte())
                            {
                                case (byte)PacketTypes.MYPOSITION:
                                    int xPosition = msg.ReadInt32();
                                    int yPosition = msg.ReadInt32();

                                    foreach (MultiplayerPlayers players in multiplayerPlayers)
                                    {
                                        if (players.id == msg.SenderConnection.RemoteUniqueIdentifier)
                                        {
                                            players.x = xPosition;
                                            players.y = yPosition;
                                            break;
                                        }
                                    }

                                    foreach (NetConnection player in server.Connections)
                                    {
                                        NetOutgoingMessage om = server.CreateMessage();

                                        for (int i = 0; i < multiplayerPlayers.Count; i++)
                                        {
                                            // write who this position is for
                                            om.Write((byte)PacketTypes.UPDATEPLAYERS);
                                            om.Write(multiplayerPlayers[i].id);
                                            om.Write(multiplayerPlayers[i].x);
                                            om.Write(multiplayerPlayers[i].y);


                                            // send message
                                            server.SendMessage(om, player, NetDeliveryMethod.ReliableOrdered);
                                        }

                                    }
                                    break;
                            }
                            
                            break;
                    }

                    ////
                    //// send position updates 30 times per second
                    ////
                    //double now = NetTime.Now;
                    //if (now > nextSendUpdates)
                    //{
                    //    // Yes, it's time to send position updates

                    //    // for each player...
                    //    for (int i = 0; i < server.Connections.Count; i++)
                    //    {
                    //        NetConnection player = server.Connections[i] as NetConnection;
                    //        // ... send information about every other player (actually including self)
                    //        for (int j = 0; j < server.Connections.Count; j++)
                    //        {
                    //            // send position update about 'otherPlayer' to 'player'
                    //            NetOutgoingMessage om = server.CreateMessage();

                    //            // write who this position is for
                    //            om.Write((byte)PacketTypes.UPDATEPLAYERS);
                    //            om.Write(multiplayerPlayers[i].id);
                    //            om.Write(multiplayerPlayers[j].x);
                    //            om.Write(multiplayerPlayers[j].y);

                    //            // send message
                    //            server.SendMessage(om, player, NetDeliveryMethod.Unreliable);
                    //        }
                            
                    //    }

                    //    // schedule next update
                    //    nextSendUpdates += (1.0 / 30.0);
                    //}
                }

                // sleep to allow other processes to run smoothly
                Thread.Sleep(1);
            }

            server.Shutdown("app exiting");
        }
    }
}
