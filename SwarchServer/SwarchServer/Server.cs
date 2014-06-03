using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections;


namespace SwarchServer
{
    class Server
    {
        //private TcpListener tcpListener;
        //private Thread listenThread;
        protected static Stopwatch physTimer = new Stopwatch();

        protected static double[] ballLocation;
        protected static char[] delimiter = { '\\' };

        // protected static TcpClient[] clientsArray;
        protected static ArrayList clientsArray = new ArrayList();
        // bool allClientsReady = false;
        protected static bool client1Start = false;
        protected static bool client2Start = false;
        protected static bool nextRoundP1 = false;
        protected static bool nextRoundP2 = false;

        protected static Queue client1Queue;
        protected static Queue client2Queue;
        protected static double[] pad1;
        protected static double[] pad2;
        protected static bool collisionHit;
        protected static double[] currentSpeed;
        protected static double initBallPosX;
        protected static double initBallPosY;
        protected static int growSize;
        protected static double moveSpeed;

        protected static gameData p1CollisionCompare;
        protected static gameData p2CollisionCompare;

        protected static bool sendHit;
        protected static bool sendScore;

        protected static int waitTimeLoopCount;
        protected static int[] Score;
        protected static double hitTime;
        protected static bool addAfterStart;
        protected static wallBound walls = new wallBound();

        protected class wallBound
        {
            // 24.6:   sub .95
            public double rightWall = 23.65; //.65: 23.95; // too far 23.12;
            
            // -24.6: add .95 
            public double leftWall = -23.65; //.65: -23.95; // too far -23.12;

            // -13.71: add .95
            public double bottomWall = -12.75; // .65: -13.06; //too far -12.22;
            
            // 15.7 sub .95
            public double topWall = 14.76; //.65: 15.06; //too far 14.23;
        }

        // wallBounds[0] -- Top 
        // wallBounds[1] -- Bottom
        protected static double[] wallBounds;

        // goalWall[0] -- left(p1)
        // goalWall[1] -- right(p2)
        protected static double[] goalWall;

        protected static bool startedProcess;

        protected static int numClientsPass;
        protected static string newClientsInfo;
        protected static List<Client> clientsEntered = new List<Client>();
        protected static DatabaseManager db = new DatabaseManager();
        protected static bool passwordAcepted;
        //protected static List<pellet>  = new List<pellet>();
        protected static string addedNewPellets;
        protected static string clientsHit;
        protected static string wallHitClients;
        protected static string currentClientsMove;
        protected static int currentNumPlayers;
        protected static string clientsConnectedInfo;
        protected static List<playInfo> compareGamePlay = new List<playInfo>();
        protected static List<int> newClientaddedNum = new List<int>();

        protected static Random randomGen;
        protected static List<int> generatedX;
        protected static List<int> generatedY;
        protected static NextRanY ranYPos = new NextRanY();
        protected static NextRanX ranXPos = new NextRanX();


        protected static Stopwatch uniClock = new Stopwatch();
        protected static DateTime dt = new DateTime();

        ///*  
        protected struct playInfo
        {
            public string hitType;
            public int objectNum;
            public int size;

            public int clientsNum;
            public int clientsNum2;
            public double compareX;
            public double compareY;
            public DateTime timeStamp;

        }
        //*/
        protected class pellet
        {
            public int pellNum;
            public int px;
            public int py;
            public int clientHitPell;
            
            // ?? add timestamp or list of timestamps ?

        }

        protected static List<pellet> gamePellets = new List<pellet>();

        // used to stores inportant info about the game
        protected struct gameData
        {
            public string userName;
            public string password;

            public int clientNum;
            public int score;
            public string action;
            public double posX;
            public double posY;
            public double movement1;
            public double movement2;
            public DateTime timeStamp;

            public int size;

        }

        // used to store client communication for thread 
        // and reading and writing 
        protected class Client
        {
            // clientTCP - client connection
            public TcpClient TCPclient;

            // client streamWriter - write out data
            public StreamWriter sw;

            //thread - client thread
            public Thread clientThread;

            //queue - store info
            public Queue clientQueue;

            // clientNumber
            public int clientNumber;

            public double posX;

            public double posY;

            public int playerSize;
            public double playerSpeed;
        }

        //protected static List<gameData> clientData = new List<gameData>(); // array of sturct for each client info

        public Server()
        {
            //this.tcpListener = new TcpListener(IPAddress.Any, 4040);
            Socket sock = new Socket();
            sock.startSock();

            dt = NTPTime.getNTPTime(ref uniClock);
        }

        // THREAD ESTABLISH CONNECTIONS =======================================================================================

        private class Socket
        {
            public TcpListener tcpListener;
            public int clientsConnected;
            //TcpClient[] clientsArray;
            Thread clientThread;
            //Thread clientThread2;
            Thread stateThread;
            Thread collisionsThread;
            Thread processGameThread;
            Thread broadCastThread; 

            bool gameStarted;
            bool startedGameState;

            Object thisLock = new Object();
            public DateTime p1T2;
            public DateTime p2T2;

            public Socket()
            {
                this.tcpListener = new TcpListener(IPAddress.Any, 4040);
                this.tcpListener.Start();
                //clientsArray = new TcpClient[2] { null, null };
                clientsConnected = 0;
                gameStarted = false;

                pad1 = new double[] { 0, 0 };
                pad2 = new double[] { 0, 0 };
                wallBounds = new double[] { 0, 0 };
                goalWall = new double[] { 0, 0 };

                sendHit = false;
                sendScore = false;
                p1T2 = new DateTime();
                p2T2 = new DateTime();
                startedProcess = false;
                startedGameState = false;


            }
            public void startSock()
            {
                Console.WriteLine("-- SWARCH: Server Started --- ");

                while (true)
                {

                    //blocks until a client has connected to the server
                    TcpClient client = this.tcpListener.AcceptTcpClient();

                    //display client that just connected
                    TcpClient tcpClientTemp = client;
                    Console.WriteLine("client localEndPt: " +
                                        tcpClientTemp.Client.LocalEndPoint);
                    Console.WriteLine("client RemoteEndPt: " +
                                        tcpClientTemp.Client.RemoteEndPoint);

                    clientsConnected++;

                    //store the clients in array

                    clientsArray.Add(tcpClientTemp);


                    Console.WriteLine("clients Connected " + clientsConnected);

                    // client sream for reading and writing
                    NetworkStream nws1 = (((TcpClient)clientsArray[clientsConnected - 1]).GetStream());

                    StreamWriter sw1 = new StreamWriter(nws1);
                    sw1.AutoFlush = true;

                    sw1.WriteLine("connected");

                    ThreadSock tSock1 = new ThreadSock(nws1, this);

                    // client sturt that holds thread, stream, client Number
                    Client tempClient = new Client();

                    tempClient.clientThread = new Thread(
                                new ParameterizedThreadStart(tSock1.HandleClientComm));

                    tempClient.TCPclient = tcpClientTemp;

                    tempClient.sw = sw1;
                    tempClient.sw.AutoFlush = true;

                    tempClient.playerSize = 2;
                    tempClient.playerSpeed = 10;//moveSpeed;

                    // array that holds clients to be used globally 
                    clientsEntered.Add(tempClient);

                    tempClient.clientThread.Start(tempClient.TCPclient);


                    if (!startedGameState)
                    {
                        ClientGameState gs = new ClientGameState();

                        stateThread = new Thread(
                                new ThreadStart(gs.gameState));

                        stateThread.Start();
               
         ///////////////////^^^^^^^^^^^^^^/////////////////////////////////////////

                        //handleCollisions hc = new handleCollisions();

                       // collisionsThread = new Thread(
                        //    new ThreadStart(hc.collisions));
                       // collisionsThread.Start();

                        //BroadCast broadcast = new BroadCast();

                        //broadCastThread = new Thread(
                        //    new ThreadStart(broadcast.sendToAll));
                        //broadCastThread.Start();

                        startedGameState = true;

                    }
                    

                    if (!startedProcess)
                    {
                        processGame PG = new processGame();
                        processGameThread = new Thread(new ThreadStart(PG.startProcessGame));
                        processGameThread.Start();
                        startedProcess = true;
                    }

                }
            }

        }

        // PROCESS CLASS (THREADED) ========================================================================================

        //main loop for sending out info to each client also access database

        private class processGame
        {

            Object thisLock = new Object();
            
            
            public string scoreUpdateInfo;

           

            public processGame()
            {
                addAfterStart = false;
                currentNumPlayers = 0;
                clientsConnectedInfo = ""; //new List<int>();
                newClientsInfo = "";
                numClientsPass = 0;
                currentClientsMove = "";
                scoreUpdateInfo = "";

            }

    public void startProcessGame()
    {
        while (clientsEntered.Count != 0)
        {
            gameData gd1;
            
            for (int i = 0; i < clientsEntered.Count; ++i)
            {
               // Client tempClient = clientsEntered[i];

                if (clientsEntered[i].clientQueue !=null && clientsEntered[i].clientQueue.Count!=0) //(tempClient.clientQueue != null && tempClient.clientQueue.Count != 0)
                {
                    lock (thisLock)
                    {

                        gd1 = (gameData)clientsEntered[i].clientQueue.Dequeue();//(gameData)tempClient.clientQueue.Dequeue();
                    }

                    switch (gd1.action)
                    {
                        case "move":
                            {
                                 
                                

                              if ((clientsEntered[i].posX + 0.4) <= (gd1.posX) || (clientsEntered[i].posX - 0.4) >= (gd1.posX)
                                  || (clientsEntered[i].posY + 0.4) <= (gd1.posY) || (clientsEntered[i].posY - 0.4) >= (gd1.posY))
                              {

                                 clientsEntered[i].posX = gd1.posX;
                                 clientsEntered[i].posY = gd1.posY;
                              }
                              else
                              {
                                  Console.WriteLine("\nCHEATING: TRYING TO JUMP TOO FAR");
                              }

                                 if ((clientsEntered[i].posX + (clientsEntered[i].playerSize * .55)) >= walls.rightWall || (clientsEntered[i].posX - (clientsEntered[i].playerSize * .55)) <= walls.leftWall
                                    || (clientsEntered[i].posY + (clientsEntered[i].playerSize * .55)) >= walls.topWall || (clientsEntered[i].posY - (clientsEntered[i].playerSize * .55)) <= walls.bottomWall)
                                 {

                                     switch (clientsEntered[i].clientNumber)
                                     {
                                         case 1:
                                             {
                                                 clientsEntered[i].posX = -17;
                                                 clientsEntered[i].posY = 7;
                                                 break;
                                             }
                                         case 2:
                                             {
                                                 clientsEntered[i].posX = 20;
                                                 clientsEntered[i].posY = 7;
                                                 break;
                                             }

                                         case 3:
                                             {
                                                 clientsEntered[i].posX = 20;
                                                 clientsEntered[i].posY = -10;
                                                 break;
                                             }
                                         default:
                                             break;
                                     }


                                     clientsEntered[i].playerSize = 2;
                                     clientsEntered[i].playerSpeed = 10;

                                     for (int b = 0; b < clientsEntered.Count; ++b)
                                     {
                                         clientsEntered[b].sw.WriteLine("hitwall\\{0}\\{1}\\{2}\\{3}\\{4}",
                                                  clientsEntered[i].clientNumber, clientsEntered[i].playerSize,
                                                  clientsEntered[i].playerSpeed, clientsEntered[i].posX, clientsEntered[i].posY);
                                     }

                                    /*
                                     clientsEntered[1].sw.WriteLine("hitwall\\{0}\\{1}\\{2}\\{3}\\{4}",
                                               clientsEntered[i].clientNumber, clientsEntered[i].playerSize,
                                               clientsEntered[i].playerSpeed, clientsEntered[i].posX, clientsEntered[i].posY);

                                     if (clientsEntered.Count == 3)
                                     {
                                         clientsEntered[2].sw.WriteLine("hitwall\\{0}\\{1}\\{2}\\{3}\\{4}",
                                               clientsEntered[i].clientNumber, clientsEntered[i].playerSize,
                                               clientsEntered[i].playerSpeed, clientsEntered[i].posX, clientsEntered[i].posY);
                                     }
                                   */
                                 }
                                 else
                                 {

                                     //currentClientsMove = string.Concat(currentClientsMove,
                                      //                      string.Format("\\{0}\\{1}\\{2}", gd1.posX, gd1.posY, clientsEntered[i].clientNumber));


                                     for (int b = 0; b < clientsEntered.Count; ++b)
                                     {
                                         clientsEntered[b].sw.WriteLine("move\\{0}\\{1}\\{2}", clientsEntered[i].posX, clientsEntered[i].posY,
                                                                          clientsEntered[i].clientNumber);
                                     }

                                    /*
                                     clientsEntered[1].sw.WriteLine("move\\{0}\\{1}\\{2}", clientsEntered[i].posX, clientsEntered[i].posY,
                                                                      clientsEntered[i].clientNumber);

                                     if (clientsEntered.Count == 3)
                                     {
                                         clientsEntered[2].sw.WriteLine("move\\{0}\\{1}\\{2}", clientsEntered[i].posX, clientsEntered[i].posY,
                                                                      clientsEntered[i].clientNumber);
                                     }
                                   */

                                 }

  // CHECK COLLISION WITH PELLETS /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////              
                                

                                    //WORKING ON MOVEMENT CHECKS FOR PELLET COLLISION



                               // pellet hitPell = new pellet();
                               //  hitPell = gamePellets.Find(x => x.pellNum == gd1.clientNum);

                                // int indexPell = gamePellets.FindIndex(x => x.pellNum == hitPell.pellNum);

                                 List<pellet> tempC1 = new List<pellet>();
                                 tempC1 = gamePellets.FindAll(x =>
                                     // compares the pellet corner: top left 
                                     ((clientsEntered[i].posX + (clientsEntered[i].playerSize * .55)) >= (x.px - (.6)) && (clientsEntered[i].posX - (clientsEntered[i].playerSize * .55)) <= (x.px - (.6))
                                     && (clientsEntered[i].posY + (clientsEntered[i].playerSize * .55)) >= (x.py + (.6)) && (clientsEntered[i].posY - (clientsEntered[i].playerSize * .55)) <= (x.py + (.6)))
                                         // bottom left
                                     || (clientsEntered[i].posX + (clientsEntered[i].playerSize * .55)) >= (x.px - (.6)) && (clientsEntered[i].posX - (clientsEntered[i].playerSize * .55)) <= (x.px - (.6))
                                     && (clientsEntered[i].posY + (clientsEntered[i].playerSize * .55)) >= (x.py - (.6)) && (clientsEntered[i].posY - (clientsEntered[i].playerSize * .55)) <= (x.py - (.6))
                                         // top right
                                     || ((clientsEntered[i].posX + (clientsEntered[i].playerSize * .55)) >= (x.px + (.6)) && (clientsEntered[i].posX - (clientsEntered[i].playerSize * .55)) <= (x.px + (.6))
                                     && (clientsEntered[i].posY + (clientsEntered[i].playerSize * .55)) >= (x.py + (.6)) && (clientsEntered[i].posY - (clientsEntered[i].playerSize * .55)) <= (x.py + (.6)))
                                         //bottom right
                                     || ((clientsEntered[i].posX + (clientsEntered[i].playerSize * .55)) >= (x.px + (.6)) && (clientsEntered[i].posX - (clientsEntered[i].playerSize * .55)) <= (x.px + (.6))
                                     && (clientsEntered[i].posY + (clientsEntered[i].playerSize * .55)) >= (x.py - (.6)) && (clientsEntered[i].posY - (clientsEntered[i].playerSize * .55)) <= (x.py - (.6))));



                                 if (tempC1.Count != 0)
                                 {
                                     // List<playInfo> orderTempList = (List<playInfo>)tempC1.OrderBy(x => x.timeStamp.Millisecond);

                                     int indexP = gamePellets.FindIndex(x => x.pellNum == tempC1[0].pellNum);


                                         double speedChange = (clientsEntered[i].playerSpeed - 2);

                                         if (speedChange < 1)
                                         {
                                             clientsEntered[i].playerSpeed = clientsEntered[i].playerSpeed * .85;

                                         }
                                         else
                                         {
                                             clientsEntered[i].playerSpeed = speedChange;
                                         }


                                         clientsEntered[i].playerSize = clientsEntered[i].playerSize + growSize;

                                         int removePellX = generatedX.FindIndex(x => x == gamePellets[indexP].px);
                                         int removePellY = generatedY.FindIndex(y => y == gamePellets[indexP].py);

                                         generatedX.RemoveAt(removePellX);
                                         generatedY.RemoveAt(removePellY);

                                         gamePellets[indexP].px = ranXPos.randomXpos(); //NextRanX();
                                         gamePellets[indexP].py = ranYPos.randomYpos(); //NextRanY();


                                         for (int b = 0; b < clientsEntered.Count; ++b )
                                         {

                                             clientsEntered[b].sw.WriteLine("newPell\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
                                             clientsEntered[i].clientNumber, (clientsEntered[i].playerSize),
                                             clientsEntered[i].playerSpeed, gamePellets[indexP].pellNum, gamePellets[indexP].px,
                                             gamePellets[indexP].py);

                                         }
                                     //    clientsEntered[1].sw.WriteLine("newPell\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
                                     //clientsEntered[indexC1].clientNumber, (clientsEntered[indexC1].playerSize),
                                     //clientsEntered[indexC1].playerSpeed, gamePellets[indexPell].pellNum, gamePellets[indexPell].px,
                                    // gamePellets[indexPell].py);

                                        // if (clientsEntered.Count == 3)
                                        // {
                                        //     clientsEntered[2].sw.WriteLine("newPell\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
                                        //   clientsEntered[indexC1].clientNumber, (clientsEntered[indexC1].playerSize),
                                        //   clientsEntered[indexC1].playerSpeed, gamePellets[indexPell].pellNum, gamePellets[indexPell].px,
                                        //   gamePellets[indexPell].py);

                                        // }


                                    
                                 }
         


  // CHECK FOR COLLISION WITH OTHER PLAYERS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                                 int hitCheckClient = clientsEntered.FindIndex(x => x.clientNumber == clientsEntered[i].clientNumber); //gd1.clientNum);

                                 List<Client> collideList2 = new List<Client>();


                                 collideList2 = clientsEntered.FindAll(x => (x.clientNumber != clientsEntered[hitCheckClient].clientNumber) && (x.playerSize != clientsEntered[hitCheckClient].playerSize)&&
                                     // compares top left corner
                                    (((x.posX + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posX - (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posX - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posX - (clientsEntered[hitCheckClient].playerSize * .6))
                                     && (x.posY + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posY + (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posY - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posY + (clientsEntered[hitCheckClient].playerSize * .6)))
                                     // bottom left
                                     || ((x.posX + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posX - (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posX - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posX - (clientsEntered[hitCheckClient].playerSize * .6))
                                     && (x.posY + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posY - (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posY - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posY - (clientsEntered[hitCheckClient].playerSize * .6)))
                                     // top right
                                     || ((x.posX + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posX + (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posX - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posX + (clientsEntered[hitCheckClient].playerSize * .6))
                                     && (x.posY + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posY + (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posY - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posY + (clientsEntered[hitCheckClient].playerSize * .6)))
                                     //bottom right
                                     || ((x.posX + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posX + (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posX - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posX + (clientsEntered[hitCheckClient].playerSize * .6))
                                     && (x.posY + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posY - (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posY - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posY - (clientsEntered[hitCheckClient].playerSize * .6)))));



                                 if (collideList2.Count != 0)
                                 {
                                     

                                     Console.WriteLine("\nCOLLIDED W/ SOMEONE-- cn1: {0}, cn2: {1}, |||| posxC1: {2}, y {3} posxC2: {4}, y2: {5} |||| SZ1: {6} , SZ2: {7} ",
                                         collideList2[0].clientNumber, clientsEntered[hitCheckClient].clientNumber, collideList2[0].posX, collideList2[0].posY
                                         , clientsEntered[hitCheckClient].posX, clientsEntered[hitCheckClient].posY, collideList2[0].playerSize, clientsEntered[hitCheckClient].playerSize);

                                     int indexC = clientsEntered.FindIndex(x => x.clientNumber == collideList2[0].clientNumber);


                                     if (clientsEntered[indexC].clientNumber != 0)
                                     {



                                         double reset1X = 0;
                                         double reset1Y = 0;

                                         double reset2X = 0;
                                         double reset2Y = 0;

                                         switch (clientsEntered[indexC].clientNumber)
                                         {
                                             case 1:
                                                 {
                                                     reset1X = -17;
                                                     reset1Y = 7;
                                                     break;
                                                 }
                                             case 2:
                                                 {
                                                     reset1X = 20;
                                                     reset1Y = 7;

                                                     break;
                                                 }

                                             case 3:
                                                 {
                                                     reset1X = 20;
                                                     reset1Y = -10;
                                                     break;
                                                 }
                                             default:
                                                 break;
                                         }

                                         switch (clientsEntered[hitCheckClient].clientNumber)//(compareGamePlay[i].clientsNum)
                                         {
                                             case 1:
                                                 {
                                                     reset2X = -17;
                                                     reset2Y = 7;
                                                     break;
                                                 }
                                             case 2:
                                                 {
                                                     reset2X = 20;
                                                     reset2Y = 7;

                                                     break;
                                                 }

                                             case 3:
                                                 {
                                                     reset2X = 20;
                                                     reset2Y = -10;
                                                     break;
                                                 }
                                             default:
                                                 break;
                                         }









                                         if (clientsEntered[indexC].playerSize == clientsEntered[hitCheckClient].playerSize)//compareGamePlay[i].size)
                                         {
                                             /*
                                             //reset both and loose their points
                                             clientsEntered[indexC].posX = randomGen.Next(-22, 22); //reset1X;
                                             clientsEntered[indexC].posY = randomGen.Next(-11, 13);  //reset1Y;
                                             clientsEntered[indexC].playerSpeed = 10;
                                             clientsEntered[indexC].playerSize = 2;

                                             clientsEntered[hitCheckClient].playerSize = 2; //compareGamePlay[i].size;
                                             clientsEntered[hitCheckClient].posX = randomGen.Next(-22, 22); //reset2X;
                                             clientsEntered[hitCheckClient].posY = randomGen.Next(-11, 13);
                                             clientsEntered[hitCheckClient].playerSpeed = 10;
                                             */

                                         }
                                         else if (clientsEntered[indexC].playerSize > clientsEntered[hitCheckClient].playerSize)//compareGamePlay[i].size)
                                         {

                                             double speedChange = (clientsEntered[indexC].playerSpeed - 2);

                                             if (speedChange < 1)
                                             {
                                                 clientsEntered[indexC].playerSpeed = clientsEntered[indexC].playerSpeed * .85;

                                             }
                                             else
                                             {
                                                 clientsEntered[indexC].playerSpeed = speedChange;
                                             }


                                             clientsEntered[indexC].playerSize += clientsEntered[i].playerSize; //compareGamePlay[i].size;

                                             clientsEntered[hitCheckClient].playerSize = 2;
                                             clientsEntered[hitCheckClient].playerSpeed = 10;
                                             clientsEntered[hitCheckClient].posX = reset2X; //randomGen.Next(-22, 22);
                                             clientsEntered[hitCheckClient].posY = reset2Y; //randomGen.Next(-11, 13);



                                         }
                                         else if (clientsEntered[indexC].playerSize < clientsEntered[hitCheckClient].playerSize)//compareGamePlay[i].size)
                                         {

                                             clientsEntered[hitCheckClient].playerSize += clientsEntered[indexC].playerSize;

                                             double speedChange = (clientsEntered[hitCheckClient].playerSpeed - 2);

                                             if (speedChange < 1)
                                             {
                                                 clientsEntered[hitCheckClient].playerSpeed = clientsEntered[indexC].playerSpeed * .85;

                                             }
                                             else
                                             {
                                                 clientsEntered[hitCheckClient].playerSpeed = speedChange;
                                             }


                                             clientsEntered[indexC].posX = reset1X; //randomGen.Next(-22, 22);
                                             clientsEntered[indexC].posY = reset1Y; //randomGen.Next(-11, 13);
                                             clientsEntered[indexC].playerSize = 2;
                                             clientsEntered[indexC].playerSpeed = 10;


                                         }


                                            for (int b = 0; b < clientsEntered.Count; ++b )
                                            {

                                              clientsEntered[b].sw.WriteLine("pH\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                             clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                             clientsEntered[indexC].posY, clientsEntered[hitCheckClient].clientNumber, clientsEntered[hitCheckClient].playerSize, clientsEntered[hitCheckClient].playerSpeed,
                                             clientsEntered[hitCheckClient].posX, clientsEntered[hitCheckClient].posY);
                                        
                                            }
                                         /*
                                         clientsEntered[1].sw.WriteLine("pH\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                             clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                             clientsEntered[indexC].posY, clientsEntered[hitCheckClient].clientNumber, clientsEntered[hitCheckClient].playerSize, clientsEntered[hitCheckClient].playerSpeed,
                                             clientsEntered[hitCheckClient].posX, clientsEntered[hitCheckClient].posY);

                                         if (clientsEntered.Count == 3)
                                         {
                                             clientsEntered[2].sw.WriteLine("pH\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                                 clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                                 clientsEntered[indexC].posY, clientsEntered[hitCheckClient].clientNumber, clientsEntered[hitCheckClient].playerSize, clientsEntered[hitCheckClient].playerSpeed,
                                                 clientsEntered[hitCheckClient].posX, clientsEntered[hitCheckClient].posY);
                                         }

                                        */
                                         Thread.Sleep(200);
                                         clientsEntered[i].clientQueue.Clear();

                                         /*
                                         gameData peek = new gameData();
                                        if(clientsEntered[i].clientQueue.Count!= 0)
                                          peek = (gameData)clientsEntered[i].clientQueue.Peek();
                                         if (peek.action == "move")
                                         {
                                             clientsEntered[i].clientQueue.Dequeue();
                                         }
                                         */
                                     }



                                 }
                                 else
                                 {

                                     // ***** SEND MISS ***** for debugging purposes

                                    // clientsEntered[0].sw.WriteLine("missHitOpp\\{0}", clientsEntered[i].clientNumber);

                                     //clientsEntered[1].sw.WriteLine("missHitOpp\\{0}", clientsEntered[i].clientNumber);

                                     //if (clientsEntered.Count == 3)
                                     //{
                                     //    clientsEntered[2].sw.WriteLine("missHitOpp\\{0}", clientsEntered[i].clientNumber);
                                     //}

                                 }






                                
                                break;
                            }
                            //  move\\gameobject.x\\gameobject.z\\1

                        case "userAndPass":
                            {
                                // enter database

                                string response = db.connect(gd1.userName, gd1.password);

                                if (response == "connect" || response == "added")
                                {
                                    //tempClient.sw.WriteLine("correctUserPass");
                                   // passwordAcepted = true;
                                   // tempClient.clientpasswordAcepted = true;
                                    numClientsPass += 1;
                                    Console.WriteLine("\n--- NUM OF CLIENTS SUCCESSFULLY LOGGED IN {0} ", numClientsPass );

                                    bool found= false;

                                        for(int j = 0; j< clientsEntered.Count && !found; ++j)
                                        {
                                          //Client tClient = clientsEntered.Find(x => x.clientNumber == j+1);

                                            if (!clientsEntered.Exists(x => x.clientNumber == j + 1))   //(tClient.clientNumber == 0) 
                                            {
                                                clientsEntered[i].clientNumber = j + 1; //tempClient.clientNumber = j+1;
                                               found = true;
                                            }
                                        }

                                        

                                    int initialX =0;
                                    int initialY=0;

                                    switch(clientsEntered[i].clientNumber)//(tempClient.clientNumber)
                                    {
                                        case 1:
                                            {
                                                initialX = -17;
                                                initialY = 7;
                                               break;
                                            }
                                        case 2:
                                            {
                                                initialX = 20;
                                                initialY = 7;
                                                break;
                                            }

                                        case 3:
                                            {
                                                initialX = 20;
                                                initialY = -10;
                                                break;
                                            }
                                        default:
                                            break;
                                    }

                                    clientsEntered[i].posX = initialX; //tempClient.posX = initialX;
                                    clientsEntered[i].posY = initialY; //tempClient.posY = initialY;
                                       
                                            //clientsEntered[i] = tempClient;
                                        
                                    string pellLoc ="";
                                    pellet tempP ; 
                                    for(int k=0; k < gamePellets.Count; ++k)
                                    {
                                      tempP = gamePellets[k];

                                      pellLoc = string.Concat(pellLoc, string.Format("\\{0}\\{1}", tempP.px, tempP.py));
                                    }

                                    //tempClient
                                    clientsEntered[i].sw.WriteLine(string.Concat(
                                        String.Format("clientNumber\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}",
                                        clientsEntered[i].clientNumber, initialX, initialY,
                                        (walls.leftWall-.95), (walls.rightWall+.95),(walls.topWall+.95),(walls.bottomWall-.95)), pellLoc));  // initPelletLocation
                                                                                   //tempClient


                                    //  updateNewClient = string.Concat(updateNewClient,
                                    //           string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}", clientsPlayingInfo[r].posX,
                                    //           clientsPlayingInfo[r].posY, clientsPlayingInfo[r].clientNumber,
                                    //          clientsPlayingInfo[r].playerSpeed, clientsPlayingInfo[r].playerSize));

                                    clientsConnectedInfo = string.Concat(clientsConnectedInfo,
                                    string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}", initialX, initialY,
                                    clientsEntered[i].clientNumber,clientsEntered[i].playerSpeed,clientsEntered[i].playerSize));
                                                                                         //tempClient                    
                                                    
                                    if (addAfterStart)
                                    {
                                        newClientsInfo = string.Concat(newClientsInfo,
                                              string.Format("\\{0}\\{1}\\{2}", initialX, initialY, clientsEntered[i].clientNumber));
                                                                                                //tempClient
                                                              //tempClient
                                        newClientaddedNum.Add(clientsEntered[i].clientNumber);
                                    }


                                }
                                else
                                {//tempClient
                                    clientsEntered[i].sw.WriteLine("incorrectUserPass");
                                    
                                }
                                
                                break;
                            }
                       /*
                        case "quit":
                            {
                                for (int b = 0; b < clientsEntered.Count; ++b)
                                {
                                    if(clientsEntered[b].clientNumber != 0)
                                    clientsEntered[b].sw.WriteLine("closed\\{0}",clientsEntered[i].clientNumber);
                                }
                                
                                clientsEntered[i].TCPclient.Close();
                                clientsEntered[i] = new Client();
                                break;
                            }
                        */
                        case "hitPell":
                            {
                               //playInfo tempPlayInfo = new playInfo();

                               //tempPlayInfo.hitType = "hitPell";

                               //tempPlayInfo.objectNum = gd1.clientNum;
                               //tempPlayInfo.size = gd1.size;
                               //tempPlayInfo.compareX = gd1.posX;
                               //tempPlayInfo.compareY = gd1.posY;
                               //tempPlayInfo.timeStamp = gd1.timeStamp;
                               //tempPlayInfo.clientsNum = clientsEntered[i].clientNumber;
                                                       //tempClient

                               //compareGamePlay.Add(tempPlayInfo); //.Insert(tempClient.clientNumber, tempPlayInfo);
/*
                               pellet hitPell = new pellet();
                                hitPell = gamePellets.Find(x=>x.pellNum == gd1.clientNum);

                               int indexPell  = gamePellets.FindIndex(x=>x.pellNum == hitPell.pellNum);

                               List<Client> tempC1 = new List<Client>();
                               tempC1 = clientsEntered.FindAll(x =>
     // compares the pellet corner: top left 
                                   ((x.posX + (x.playerSize * .55)) >= (gamePellets[indexPell].px - (.6)) && (x.posX - (x.playerSize * .55)) <= (gamePellets[indexPell].px - (.6))
                                   && (x.posY + (x.playerSize * .55)) >= (gamePellets[indexPell].py + (.6)) && (x.posY - (x.playerSize * .55)) <= (gamePellets[indexPell].py + (.6)))
                                       // bottom left
                                   || (x.posX + (x.playerSize * .55)) >= (gamePellets[indexPell].px - (.6)) && (x.posX - (x.playerSize * .55)) <= (gamePellets[indexPell].px - (.6))
                                   && (x.posY + (x.playerSize * .55)) >= (gamePellets[indexPell].py - (.6)) && (x.posY - (x.playerSize * .55)) <= (gamePellets[indexPell].py - (.6))
                                       // top right
                                   || ((x.posX + (x.playerSize * .55)) >= (gamePellets[indexPell].px + (.6)) && (x.posX - (x.playerSize * .55)) <= (gamePellets[indexPell].px + (.6))
                                   && (x.posY + (x.playerSize * .55)) >= (gamePellets[indexPell].py + (.6)) && (x.posY - (x.playerSize * .55)) <= (gamePellets[indexPell].py + (.6)))
                                       //bottom right
                                   || ((x.posX + (x.playerSize * .55)) >= (gamePellets[indexPell].px + (.6)) && (x.posX - (x.playerSize * .55)) <= (gamePellets[indexPell].px + (.6))
                                   && (x.posY + (x.playerSize * .55)) >= (gamePellets[indexPell].py - (.6)) && (x.posY - (x.playerSize * .55)) <= (gamePellets[indexPell].py - (.6))));



                               if (tempC1.Count != 0)
                               {
                                   // List<playInfo> orderTempList = (List<playInfo>)tempC1.OrderBy(x => x.timeStamp.Millisecond);

                                  int indexC1 = clientsEntered.FindIndex(x => x.clientNumber == tempC1[0].clientNumber);


                                   if (clientsEntered[indexC1].clientNumber != 0)
                                   {

                                       double speedChange = (clientsEntered[indexC1].playerSpeed - 2);

                                       if (speedChange < 1)
                                       {
                                           clientsEntered[indexC1].playerSpeed = clientsEntered[indexC1].playerSpeed * .85;

                                       }
                                       else
                                       {
                                           clientsEntered[indexC1].playerSpeed = speedChange;
                                       }


                                       clientsEntered[indexC1].playerSize = clientsEntered[indexC1].playerSize + growSize;

                                       int removePellX = generatedX.FindIndex(x => x == gamePellets[indexPell].px);
                                       int removePellY = generatedY.FindIndex(y => y == gamePellets[indexPell].py);

                                       generatedX.RemoveAt(removePellX);
                                       generatedY.RemoveAt(removePellY);

                                       gamePellets[indexPell].px = ranXPos.randomXpos(); //NextRanX();
                                       gamePellets[indexPell].py = ranYPos.randomYpos(); //NextRanY();
                                       


                                        clientsEntered[0].sw.WriteLine("newPell\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
                                    clientsEntered[indexC1].clientNumber, (clientsEntered[indexC1].playerSize),
                                    clientsEntered[indexC1].playerSpeed, gamePellets[indexPell].pellNum, gamePellets[indexPell].px,
                                    gamePellets[indexPell].py);

                                        clientsEntered[1].sw.WriteLine("newPell\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
                                    clientsEntered[indexC1].clientNumber, (clientsEntered[indexC1].playerSize),
                                    clientsEntered[indexC1].playerSpeed, gamePellets[indexPell].pellNum, gamePellets[indexPell].px,
                                    gamePellets[indexPell].py);

                                     if (clientsEntered.Count == 3)
                                     {
                                         clientsEntered[2].sw.WriteLine("newPell\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
                                       clientsEntered[indexC1].clientNumber, (clientsEntered[indexC1].playerSize),
                                       clientsEntered[indexC1].playerSpeed, gamePellets[indexPell].pellNum, gamePellets[indexPell].px,
                                       gamePellets[indexPell].py);
                                    
                                     }


                                   }
                               }
                               else
                               {

                                   // ***** SEND MISS ***** for debugging purposes

                                   clientsEntered[0].sw.WriteLine("missPell");

                                   clientsEntered[1].sw.WriteLine("missPell");

                                   if (clientsEntered.Count == 3)
                                   {
                                       clientsEntered[2].sw.WriteLine("missPell");                               
                                   }

                               }
                                
*/
                                break;
                            }
                        case "score":
                            {
                               /*
                                int newScore = db.updateScore(gd1.userName, gd1.score);
                                gd1.score = newScore;
                                scoreUpdateInfo = string.Concat(scoreUpdateInfo,
                                        string.Format("\\{0}", gd1.score));

                                clientsEntered[i].sw.WriteLine("score{0}", scoreUpdateInfo);
                                scoreUpdateInfo = "";
                               */
                                break;
                            }
                        case"hitOpp":
                            {
                               
//====================================================================================================================================
/*                                
                                // send from client, check if it got hit
                                 int hitCheckClient = clientsEntered.FindIndex(x => x.clientNumber == gd1.clientNum);

                                List<Client> collideList2 = new List<Client>();


                                collideList2 = clientsEntered.FindAll(x => (x.clientNumber != clientsEntered[hitCheckClient].clientNumber) &&
                                    // compares top left corner
                                   (((x.posX + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posX - (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posX - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posX - (clientsEntered[hitCheckClient].playerSize * .6))
                                    && (x.posY + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posY + (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posY - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posY + (clientsEntered[hitCheckClient].playerSize * .6)))
                                    // bottom left
                                    || ((x.posX + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posX - (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posX - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posX - (clientsEntered[hitCheckClient].playerSize * .6))
                                    && (x.posY + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posY - (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posY - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posY - (clientsEntered[hitCheckClient].playerSize * .6)))
                                    // top right
                                    || ((x.posX + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posX + (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posX - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posX + (clientsEntered[hitCheckClient].playerSize * .6))
                                    && (x.posY + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posY + (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posY - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posY + (clientsEntered[hitCheckClient].playerSize * .6)))
                                    //bottom right
                                    || ((x.posX + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posX + (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posX - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posX + (clientsEntered[hitCheckClient].playerSize * .6))
                                    && (x.posY + (x.playerSize * .6)) >= (clientsEntered[hitCheckClient].posY - (clientsEntered[hitCheckClient].playerSize * .6)) && (x.posY - (x.playerSize * .6)) <= (clientsEntered[hitCheckClient].posY - (clientsEntered[hitCheckClient].playerSize * .6)))));



                                if (collideList2.Count != 0)
                                {

                                    Console.WriteLine("\nCOLLIDED W/ SOMEONE-- cn1: {0}, cn2: {1}, posxC1 {2}, y {3} :: posxC2 {4}, y2 {5} -_- SZ1 {6} , SZ2{7} ",
                                        collideList2[0].clientNumber, clientsEntered[hitCheckClient].clientNumber, collideList2[0].posX, collideList2[0].posY
                                        , clientsEntered[hitCheckClient].posX, clientsEntered[hitCheckClient].posY, collideList2[0].playerSize, clientsEntered[hitCheckClient].playerSize);

                                    int indexC = clientsEntered.FindIndex(x => x.clientNumber == collideList2[0].clientNumber);


                                    if (clientsEntered[indexC].clientNumber != 0)
                                    {

                                        if (clientsEntered[indexC].playerSize == clientsEntered[hitCheckClient].playerSize)//compareGamePlay[i].size)
                                        {

                                            //reset both and loose their points
                                            clientsEntered[indexC].posX = randomGen.Next(-23, 23); //reset1X;
                                            clientsEntered[indexC].posY = randomGen.Next(-12, 14);  //reset1Y;
                                            clientsEntered[indexC].playerSpeed = 10;
                                            clientsEntered[indexC].playerSize = 2;

                                            clientsEntered[hitCheckClient].playerSize = 2; //compareGamePlay[i].size;
                                            clientsEntered[hitCheckClient].posX = randomGen.Next(-23, 23); //reset2X;
                                            clientsEntered[hitCheckClient].posY = randomGen.Next(-12, 14);
                                            clientsEntered[hitCheckClient].playerSpeed = 10;


                                        }
                                        else if (clientsEntered[indexC].playerSize > clientsEntered[hitCheckClient].playerSize)//compareGamePlay[i].size)
                                        {

                                            double speedChange = (clientsEntered[indexC].playerSpeed - 2);

                                            if (speedChange < 1)
                                            {
                                                clientsEntered[indexC].playerSpeed = clientsEntered[indexC].playerSpeed * .85;

                                            }
                                            else
                                            {
                                                clientsEntered[indexC].playerSpeed = speedChange;
                                            }


                                            clientsEntered[indexC].playerSize += clientsEntered[i].playerSize; //compareGamePlay[i].size;

                                            clientsEntered[hitCheckClient].playerSize = 2;
                                            clientsEntered[hitCheckClient].playerSpeed = 10;
                                            clientsEntered[hitCheckClient].posX = randomGen.Next(-23, 23);
                                            clientsEntered[hitCheckClient].posY = randomGen.Next(-12, 14);



                                        }
                                        else if (clientsEntered[indexC].playerSize < clientsEntered[hitCheckClient].playerSize)//compareGamePlay[i].size)
                                        {

                                            clientsEntered[hitCheckClient].playerSize += clientsEntered[indexC].playerSize;

                                            double speedChange = (clientsEntered[hitCheckClient].playerSpeed - 2);

                                            if (speedChange < 1)
                                            {
                                                clientsEntered[hitCheckClient].playerSpeed = clientsEntered[indexC].playerSpeed * .85;

                                            }
                                            else
                                            {
                                                clientsEntered[hitCheckClient].playerSpeed = speedChange;
                                            }


                                            clientsEntered[indexC].posX = randomGen.Next(-23, 23);
                                            clientsEntered[indexC].posY = randomGen.Next(-12, 14);
                                            clientsEntered[indexC].playerSize = 2;
                                            clientsEntered[indexC].playerSpeed = 10;
                                            

                                        }


                                            clientsEntered[0].sw.WriteLine("pH\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                                clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                                clientsEntered[indexC].posY, clientsEntered[hitCheckClient].clientNumber, clientsEntered[hitCheckClient].playerSize, clientsEntered[hitCheckClient].playerSpeed,
                                                clientsEntered[hitCheckClient].posX, clientsEntered[hitCheckClient].posY);

                                            clientsEntered[1].sw.WriteLine("pH\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                                clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                                clientsEntered[indexC].posY, clientsEntered[hitCheckClient].clientNumber, clientsEntered[hitCheckClient].playerSize, clientsEntered[hitCheckClient].playerSpeed,
                                                clientsEntered[hitCheckClient].posX, clientsEntered[hitCheckClient].posY);

                                        if(clientsEntered.Count == 3)
                                        {
                                            clientsEntered[2].sw.WriteLine("pH\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                                clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                                clientsEntered[indexC].posY, clientsEntered[hitCheckClient].clientNumber, clientsEntered[hitCheckClient].playerSize, clientsEntered[hitCheckClient].playerSpeed,
                                                clientsEntered[hitCheckClient].posX, clientsEntered[hitCheckClient].posY);
                                        }

                                    }

                                   

                                }
                                else
                                {

                                    // ***** SEND MISS ***** for debugging purposes

                                    clientsEntered[0].sw.WriteLine("missHitOpp\\{0}", clientsEntered[i].clientNumber);

                                    clientsEntered[1].sw.WriteLine("missHitOpp\\{0}", clientsEntered[i].clientNumber);

                                    if (clientsEntered.Count == 3)
                                    {
                                        clientsEntered[2].sw.WriteLine("missHitOpp\\{0}", clientsEntered[i].clientNumber);
                                    }

                                }



*/
//===================================================================================================================================
                                break;
                            }
                      /*
                        case "wall":
                            {
                                clientsEntered[i].playerSize = 2;
                                clientsEntered[i].playerSpeed = 10;
                                
                                switch(clientsEntered[i].clientNumber)//(tempClient.clientNumber)
                                    {
                                        case 1:
                                            {
                                                clientsEntered[i].posX = -17;
                                                clientsEntered[i].posY = 7;
                                                break;
                                            }
                                        case 2:
                                            {
                                                clientsEntered[i].posX = 20;
                                                clientsEntered[i].posY = 7;
                                                break;
                                            }

                                        case 3:
                                            {
                                                clientsEntered[i].posX = 20; 
                                                clientsEntered[i].posY = -10;
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                
                                
                                break;
                            }

                       */

                        default:
                            break;

                    }
                }

            }

////////////// -- PLACE TO BROADCAST TO ALL PLAYERS -- ///////////////////////////////////////////////////////////////
           
            if (numClientsPass > 1 && numClientsPass > currentNumPlayers) //&& !sentStartGame
            {

                if (addAfterStart)
                {
                   
                    // currentClientsMove = ""

                    for (int l = 0; l < clientsEntered.Count; ++l)
                    {
                        //Client tC = clientsEntered[l];

                        if ((newClientaddedNum.Find(x => x == clientsEntered[l].clientNumber)) != 0)
                        {
                            
                            List<Client> clientsPlayingInfo = new List<Client>();
                           clientsPlayingInfo  = clientsEntered.FindAll(i=>i.clientNumber != newClientaddedNum[0]);
                            
                            string updateNewClient ="";
                            for(int r=0; r< clientsPlayingInfo.Count; ++r)
                            {
                            
                               updateNewClient = string.Concat(updateNewClient,
                                       string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}", clientsPlayingInfo[r].posX,
                                        clientsPlayingInfo[r].posY, clientsPlayingInfo[r].clientNumber,
                                        clientsPlayingInfo[r].playerSpeed, clientsPlayingInfo[r].playerSize));
                            
                            }

                            clientsEntered[l].sw.WriteLine("startInitalGame{0}",updateNewClient);//clientsConnectedInfo);
                        }
                        else
                        {
                            
                            clientsEntered[l].sw.WriteLine("newEntry{0}", newClientsInfo);
                        }
                    }
                    newClientaddedNum.Clear();
                    newClientsInfo = "";
                }
                else
                {
                    for (int l = 0; l < clientsEntered.Count; ++l)
                    {
                        //Client tC = clientsEntered[l];
                        //tC
                        if (clientsEntered[l].clientNumber != 0)
                        {  // tC                                      //,numClientsPass
                            clientsEntered[l].sw.WriteLine("startInitalGame{0}", clientsConnectedInfo);
                        }
                    }
                    addAfterStart = true;
                }


                currentNumPlayers = numClientsPass;
            }


            //if (currentClientsMove != "")
            //{
            // send everyone all movements logged in currentClientsMove 
            // for (int l = 0; l < clientsEntered.Count; ++l)
            // {

            //     clientsEntered[l].sw.WriteLine("move{0}", currentClientsMove);

            //  }

            //  currentClientsMove = "";
            //}
          
         /*
            if (addedNewPellets != "")
            {
                for (int l = 0; l < clientsEntered.Count; ++l)
                {

                    clientsEntered[l].sw.WriteLine("newPell{0}", addedNewPellets);

                }
                addedNewPellets = "";
            }
        

            if (clientsHit != "")
            {
                for (int l = 0; l < clientsEntered.Count; ++l)
                {

                    clientsEntered[l].sw.WriteLine("pH{0}", clientsHit);

                }

                clientsHit = "";
            }

            if (wallHitClients != "")
            {
                for (int l = 0; l < clientsEntered.Count; ++l)
                {
                    clientsEntered[l].sw.WriteLine("wall{0}", wallHitClients);

                }
                wallHitClients = "";
            }

         */
   
        }
    }
}

/*
private class BroadCast
{

    public BroadCast()
    {
    }

    public void sendToAll()
    {
        while (clientsEntered.Count != 0)
        {
            if (numClientsPass > 1 && numClientsPass > currentNumPlayers) //&& !sentStartGame
            {


                if (addAfterStart)
                {
                    // TESTING (since still don't have movement sent over):
                    // currentClientsMove = ""

                    for (int l = 0; l < clientsEntered.Count; ++l)
                    {
                        //Client tC = clientsEntered[l];

                        if ((newClientaddedNum.Find(x => x == clientsEntered[l].clientNumber)) != 0)
                        {
                            // clients who just joined (could optimize by somehow getting most recent movements before )
                            //tC
                            clientsEntered[l].sw.WriteLine("startInitalGame{0}", clientsConnectedInfo);
                        }
                        else
                        {
                            //clients who already playing
                            //           // , numClientsPass
                            //tC
                            clientsEntered[l].sw.WriteLine("newEntry{0}", newClientsInfo);
                        }
                    }
                    newClientaddedNum.Clear();
                    newClientsInfo = "";
                }
                else
                {
                    for (int l = 0; l < clientsEntered.Count; ++l)
                    {
                        //Client tC = clientsEntered[l];
                        //tC
                        if (clientsEntered[l].clientNumber != 0)
                        {  // tC                                      //,numClientsPass
                            clientsEntered[l].sw.WriteLine("startInitalGame{0}", clientsConnectedInfo);
                        }
                    }
                    addAfterStart = true;
                }


                currentNumPlayers = numClientsPass;
            }


            //if (currentClientsMove != "")
            //{
                // send everyone all movements logged in currentClientsMove 
               // for (int l = 0; l < clientsEntered.Count; ++l)
               // {

               //     clientsEntered[l].sw.WriteLine("move{0}", currentClientsMove);

              //  }

              //  currentClientsMove = "";
            //}

            if (addedNewPellets != "")
            {
                for (int l = 0; l < clientsEntered.Count; ++l)
                {

                    clientsEntered[l].sw.WriteLine("newPell{0}", addedNewPellets);

                }
                addedNewPellets = "";
            }

            if (clientsHit != "")
            {
                for (int l = 0; l < clientsEntered.Count; ++l)
                {

                    clientsEntered[l].sw.WriteLine("pH{0}", clientsHit);

                }

                clientsHit = "";
            }

            if (wallHitClients != "")
            {
                for (int l = 0; l < clientsEntered.Count; ++l)
                {
                    clientsEntered[l].sw.WriteLine("wall{0}", wallHitClients);

                }
                wallHitClients = "";
            }


            //newClientaddedNum.Clear();
            //currentClientsMove = "";
            //newClientsInfo = "";
        }
    }

}

*/

// PROCESS CLASS (THREADED) (END) ========================================================================================

            // THREAD READ IN INFO =======================================================================================

            //class to handle the client incoming info
            private class ThreadSock
            {

                public ThreadSock(NetworkStream nwsIn, Socket sockIn)
                {

                    client1Queue = new Queue();

                }


                public void HandleClientComm(object client)
                {
                    TcpClient tcpClient = (TcpClient)client;
                    //TcpClient sendClient = (TcpClient)client;
                    NetworkStream clientStream = tcpClient.GetStream();
                    NetworkStream nws = tcpClient.GetStream();
                    StreamReader sr = new StreamReader(nws);
                    gameData gamedata = new gameData();

                    //int bytesRead;
                    string readData;
                    Object thisLock = new Object();
                    // Client tempClient= new Client();
                    //int clientIndex = -9;
                    int cliIndex = -999;

                    lock (thisLock)
                    {
                        // tempClient = clientsEntered.Find(x => x.TCPclient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint);
                        // clientIndex = clientsEntered.IndexOf(tempClient);

                        cliIndex = clientsEntered.FindIndex(x => x.TCPclient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint);
                        
                    }
                    if (cliIndex == -1 || cliIndex == -999)
                        Console.WriteLine("client read in not established " + cliIndex);
                        // tempClient.clientQueue = new Queue(); 
                    clientsEntered[cliIndex].clientQueue = new Queue();

                    while (true)
                    {

                        readData = "";

                        try
                        {

                            readData = sr.ReadLine();


                        }
                        catch
                        {
                            //a socket error has occured
                            break;
                        }

                        string[] data = readData.Split(delimiter);

                        //readData
                        if (data[0] == "close")
                        {
                            //the client has disconnected from the server
                            break;
                        }
                        //readData
                        else if (data[0] == "userAndPass")
                        {
                            gamedata.action = data[0];    // ???? might need to create new gamedata for every entry 
                            gamedata.userName = data[1];
                            gamedata.password = data[2];

                        }
                        else if (data[0] == "move")
                        {

                            gamedata.action = data[0];
                            gamedata.clientNum = Convert.ToInt32(data[1]); // pell or other client
                            gamedata.posX = Convert.ToDouble(data[2]);
                            gamedata.posY = Convert.ToDouble(data[3]);
                            //gamedata.timeStamp = Convert.ToDateTime(data[5]);

                        }
                        else if (data[0] == "score")
                        {
                            gamedata.action = data[0];
                            gamedata.score = Convert.ToInt32(data[1]); //Score
                        }
                        else if(data[0] == "quit")
                        {
                            gamedata.action = data[0];

                        }

                    /*
                        else if (data[0] == "wall")
                        {
                            gamedata.action = data[0];
                            //gamedata.clientNum = Convert.ToInt32(data[1]); 
                            //gamedata.posX = Convert.ToDouble(data[2]);
                            //gamedata.posY = Convert.ToDouble(data[3]);

                        }
                        else if (data[0] == "hitPell")
                        {
                            gamedata.action = data[0];
                            gamedata.clientNum = Convert.ToInt32(data[1]); // pell 
                            gamedata.size = Convert.ToInt32(data[2]);
                            gamedata.posX = Convert.ToDouble(data[3]);
                            gamedata.posY = Convert.ToDouble(data[4]);

                        }
                        else if (data[0] == "hitOpp")
                        {
                            gamedata.action = data[0];
                            gamedata.clientNum = Convert.ToInt32(data[1]); 
                            gamedata.size = Convert.ToInt32(data[2]);
                            gamedata.posX = Convert.ToDouble(data[3]);
                            gamedata.posY = Convert.ToDouble(data[4]);

                        }
                    */
                        else
                        {
                            gamedata.action = "";
                            
                        }

                       // if (gamedata.action == "hitOpp")
                       // {
                        //    Console.WriteLine("hitOPP :: client who sent {0}, opp {1}" ,
                        //        clientsEntered[cliIndex].clientNumber, gamedata.clientNum);
                        
                       // }

                          if(gamedata.action != "")
                          {
                            lock (thisLock)
                            {
                            
                                clientsEntered[cliIndex].clientQueue.Enqueue(gamedata);

                            }
                          }

                    }

                    tcpClient.Close();
                }
            }


            // THREAD READ IN INFO  (END) =======================================================================================

private class ClientGameState
{

    public StreamWriter sw1;
    public StreamWriter sw2;
    Object thisLock = new Object();
    //int waitTimeLoopCount;
    public bool calcPaddle;
    public Stopwatch collWaitTime = new Stopwatch();
    bool startedWaitTime;
    public double[] willBePosition;
    bool setSpeedIfHit;
    public double[] missSpeed;

    public double[] missPosition;
    public bool missed;

    bool wallHit;
    bool startedTimer;
    public Random randPellets;
    bool startedGame;
    int numberOfpellets;

    public int lastProcessRange;

    List<playInfo> resolvedPellets;

    //public Random randomGen;
    //List<int> generatedX;
    //List<int> generatedY;

    //TEMP
    bool round2;
    double lastLocx;
    double lastLocy;

    public ClientGameState()
    {
        ballLocation = new double[] { 0, 0, 0, 0 };
        // ballLocation[1] = 0;
        currentSpeed = new double[] { -7, 5 }; //-5,3

        willBePosition = new double[] { 0, 0 };
        setSpeedIfHit = false;

        // sw1 = new StreamWriter(nws1);
        //  sw1.AutoFlush = true;
        //  sw2 = new StreamWriter(nws2);
        //   sw2.AutoFlush = true;
        waitTimeLoopCount = 0;
        calcPaddle = false;
        startedWaitTime = false;
        Score = new int[] { 0, 0 };
        hitTime = 0;

        missSpeed = new double[] { 0, 0 };
        missPosition = new double[] { 0, 0 };
        missed = false;

        wallHit = false;
        startedTimer = false;
        randPellets = new Random();
        randomGen = new Random();

        startedGame = false;

        numberOfpellets = 5; // WHERE TO DETERMINE NUMBER OF PELLETS IN GAME
        //initPelletLocation = "";  //  "iniP\\";
        addedNewPellets = "";
        clientsHit = "";
        wallHitClients = "";
        generatedX = new List<int>();
        generatedY = new List<int>();
        resolvedPellets = new List<playInfo>();

        growSize = 1;
        moveSpeed = 10;

        lastProcessRange = 0;

        //TEMP
        round2 = false;
        lastLocx = 0;
        lastLocy = 0;



        //  player 1 x = -17, y = 7;

    }

    public void gameState()  //NOTHING IS SENT OUT HERE, JUST CALCULATED
    {
        //while (true)
        //{

            if (!startedGame) //(client1Start && client2Start)
            {

                for (int i = 0; i < numberOfpellets; ++i)
                {
                    pellet p = new pellet();
                    p.px = ranXPos.randomXpos(); //NextRanX();//randPellets.Next(-23,23);
                    p.py = ranYPos.randomYpos(); //NextRanY();//randPellets.Next(-12, 14);
                    p.pellNum = i + 1;

                    gamePellets.Add(p);

                    // string tempstring = string.Format("\\{0}\\{1}",p.px,p.py);
                    // initPelletLocation= String.Concat(initPelletLocation,tempstring); 
                }

                startedGame = true;

            }
            // if(!startedTimer&& numClientsPass >1)
            // {
            //     physTimer.Start();
            //     startedTimer = true;
            // }



// ___________________________________________________________________________________________________________________________
/*
            if (numClientsPass > 1) // physTimer.Elapsed.Milliseconds >= 20 && 
            {
                List<Client> tempC = new List<Client>();

                tempC = clientsEntered.FindAll(x => x.posX >= walls.rightWall || x.posX <= walls.leftWall
                                        || x.posY >= walls.topWall || x.posY <= walls.bottomWall);
                if (tempC.Count != 0)
                {
                    for (int i = 0; i < tempC.Count; ++i)
                    {
                        int wallHitIndex = clientsEntered.IndexOf(tempC[i]);

                        switch (clientsEntered[wallHitIndex].clientNumber)
                        {
                            case 1:
                                {
                                    clientsEntered[wallHitIndex].posX = -17;
                                    clientsEntered[wallHitIndex].posX = 7;
                                    break;
                                }
                            case 2:
                                {
                                    clientsEntered[wallHitIndex].posX = 20;
                                    clientsEntered[wallHitIndex].posX = 7;
                                    break;
                                }

                            case 3:
                                {
                                    clientsEntered[wallHitIndex].posX = 20;
                                    clientsEntered[wallHitIndex].posX = -10;
                                    break;
                                }
                            default:
                                break;
                        }


                        clientsEntered[wallHitIndex].playerSize = 2;
                        clientsEntered[wallHitIndex].playerSpeed = 10;

                        wallHitClients = string.Concat(wallHitClients, string.Format("\\{0}\\{1}\\{2}",
                            clientsEntered[wallHitIndex].clientNumber, clientsEntered[wallHitIndex].posX, clientsEntered[wallHitIndex].posY));

                    }

                }

                            

                            

                    //IEnumerable

                    //  List<playInfo> orderTempList = (List<playInfo>)compareGamePlay.OrderBy(x => x.timeStamp.Millisecond);

                    // if ((newClientaddedNum.Find(x => x == clientsEntered[l].clientNumber)) != 0 )


                    //List<playInfo> tempList = compareGamePlay.FindAll((x => x.objectNum == i+1) );

                    // / * 
                    // ?? wait to see if another "hit" is on it's way.
                    for (int i = 0; i < gamePellets.Count; ++i)
                    {
                        int indexC1=0;

                        List<Client> tempC1 = new List<Client>();
                        tempC1 = clientsEntered.FindAll(x =>
                        // compares top left corner
                            ((x.posX + (x.playerSize / 2)) >= (gamePellets[i].px - (.5)) && (x.posX - (x.playerSize / 2)) <= (gamePellets[i].px - (.5))
                            && (x.posY + (x.playerSize / 2)) >= (gamePellets[i].py + (.5)) && (x.posY - (x.playerSize / 2)) <= (gamePellets[i].py + (.5)))
                            // bottom left
                            || (x.posX + (x.playerSize / 2)) >= (gamePellets[i].px - (.5)) && (x.posX - (x.playerSize / 2)) <= (gamePellets[i].px - (.5))
                            && (x.posY + (x.playerSize / 2)) >= (gamePellets[i].py - (.5)) && (x.posY - (x.playerSize / 2)) <= (gamePellets[i].py - (.5))
                            // top right
                            || ((x.posX + (x.playerSize / 2)) >= (gamePellets[i].px + (.5)) && (x.posX - (x.playerSize / 2)) <= (gamePellets[i].px + (.5))
                            && (x.posY + (x.playerSize / 2)) >= (gamePellets[i].py + (.5)) && (x.posY - (x.playerSize / 2)) <= (gamePellets[i].py + (.5)))
                            //bottom right
                            || ((x.posX + (x.playerSize / 2)) >= (gamePellets[i].px + (.5)) && (x.posX - (x.playerSize / 2)) <= (gamePellets[i].px + (.5))
                            && (x.posY + (x.playerSize / 2)) >= (gamePellets[i].py - (.5)) && (x.posY - (x.playerSize / 2)) <= (gamePellets[i].py - (.5))));
   
                        
                        if (tempC1.Count != 0)
                        {
                            // List<playInfo> orderTempList = (List<playInfo>)tempC1.OrderBy(x => x.timeStamp.Millisecond);


                         indexC1 = clientsEntered.FindIndex(x => x.clientNumber == tempC1[0].clientNumber);

                                    
                            if (clientsEntered[indexC1].clientNumber != 0)
                            {
                                            
                                //if ((clientsEntered[indexC1].posX + (clientsEntered[indexC1].playerSize / 2) >= compareGamePlay[i].compareX - (compareGamePlay[i].size / 2))
                                //  || (clientsEntered[indexC1].posX - (clientsEntered[indexC1].playerSize / 2) <= compareGamePlay[i].compareX + (compareGamePlay[i].size / 2))
                                //  || (clientsEntered[indexC1].posY + (clientsEntered[indexC1].playerSize / 2) >= compareGamePlay[i].compareY - (compareGamePlay[i].size / 2))
                                //  || (clientsEntered[indexC1].posY - (clientsEntered[indexC1].playerSize / 2) <= compareGamePlay[i].compareY + (compareGamePlay[i].size / 2))
                                //  && !resolvedPellets.Exists(x => (x.objectNum == compareGamePlay[i].objectNum)
                                //      && (x.compareX == compareGamePlay[i].compareX) && (x.compareY == compareGamePlay[i].compareY)))
                                //{
                                    double speedChange = (clientsEntered[indexC1].playerSpeed - 2);

                                    if (speedChange < 1)
                                    {
                                        clientsEntered[indexC1].playerSpeed = clientsEntered[indexC1].playerSpeed * .85;

                                    }
                                    else
                                    {
                                        clientsEntered[indexC1].playerSpeed = speedChange;
                                    }

                                    // clientsEntered[indexC].sw.WriteLine("getPell\\{0}\\{1}", (clientsEntered[indexC].playerSize + growSize), clientsEntered[indexC].playerSpeed);
                                    //, (((dt.AddMinutes(uniClock.Elapsed.Minutes).AddSeconds(uniClock.Elapsed.Seconds).AddMilliseconds(uniClock.Elapsed.Milliseconds)).Ticks))


                                    clientsEntered[indexC1].playerSize = clientsEntered[indexC1].playerSize + growSize;
                                    //orderTempList[i]
                                    //pellet tempPel = gamePellets.Find(x => x.pellNum == compareGamePlay[i].objectNum);
                                    //int indexPel = gamePellets.IndexOf(tempPel);

                                    //int indPell = gamePellets.FindIndex(x => x.pellNum == compareGamePlay[i].objectNum);

                                    //tempPel

                                    int removePellX = generatedX.FindIndex(x=>x == gamePellets[i].px);
                                    int removePellY = generatedY.FindIndex(y=>y == gamePellets[i].py);    
                                
                                    generatedX.RemoveAt(removePellX);
                                    generatedY.RemoveAt(removePellY);

                                    gamePellets[i].px = NextRanX();
                                    gamePellets[i].py = NextRanY();
                                    //orderTempList
                                    // gamePellets[indPell].pellNum = compareGamePlay[i].objectNum;
                                    //gamePellets[indexPel] = tempPel;

                                    // int clientIndex = clientsEntered.IndexOf(c);
                                    //clientsEntered[clientIndex] = c;

                                    //lock (thisLock)
                                    //{
                                            //addedNewPellets = string.Concat(addedNewPellets, string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
                                            //clientsEntered[indexC1].clientNumber, (clientsEntered[indexC1].playerSize), clientsEntered[indexC1].playerSpeed,
                                            //gamePellets[i].pellNum, gamePellets[i].px, gamePellets[i].py));

                                    //}

//  METHOD:  send through w/o input from client 

//                                    clientsEntered[0].sw.WriteLine("newPell\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
//                                clientsEntered[indexC1].clientNumber, (clientsEntered[indexC1].playerSize), clientsEntered[indexC1].playerSpeed,
//                                gamePellets[i].pellNum, gamePellets[i].px, gamePellets[i].py);

//                                    clientsEntered[1].sw.WriteLine("newPell\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}",
//                                clientsEntered[indexC1].clientNumber, (clientsEntered[indexC1].playerSize), clientsEntered[indexC1].playerSpeed,
//                                gamePellets[i].pellNum, gamePellets[i].px, gamePellets[i].py);

                                //add a thrid client if there is one
                                           
                            }
   
                    }
       
                }


// ========================== TEST FOR PLAYERS COLLISION ===========================================================================================

                   
                if (numClientsPass > 1 && addAfterStart) // physTimer.Elapsed.Milliseconds >= 20 
                    {

                        for (int i = 0; i < clientsEntered.Count; ++i)
                        {

                            //if (compareGamePlay[i].hitType == "hitPell")
                            //{

                            //orderTempList[i]
                            //Client c =  clientsEntered.Find(x => x.clientNumber == compareGamePlay[i].clientsNum);  

                            // int indexC1 = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum);

                            List<Client> collideList2 = new List<Client>();


                            collideList2 = clientsEntered.FindAll(x => (x.clientNumber != clientsEntered[i].clientNumber) &&
                                // compares top left corner
                               (((x.posX + (x.playerSize / 2)) >= (clientsEntered[i].posX - (clientsEntered[i].playerSize / 2)) && (x.posX - (x.playerSize / 2)) <= (clientsEntered[i].posX - (clientsEntered[i].playerSize / 2))
                                && (x.posY + (x.playerSize / 2)) >= (clientsEntered[i].posY + (clientsEntered[i].playerSize / 2)) && (x.posY - (x.playerSize / 2)) <= (clientsEntered[i].posY + (clientsEntered[i].playerSize / 2)))
                                // bottom left
                                || ((x.posX + (x.playerSize / 2)) >= (clientsEntered[i].posX - (clientsEntered[i].playerSize / 2)) && (x.posX - (x.playerSize / 2)) <= (clientsEntered[i].posX - (clientsEntered[i].playerSize / 2))
                                && (x.posY + (x.playerSize / 2)) >= (clientsEntered[i].posY - (clientsEntered[i].playerSize / 2)) && (x.posY - (x.playerSize / 2)) <= (clientsEntered[i].posY - (clientsEntered[i].playerSize / 2)))
                                // top right
                                || ((x.posX + (x.playerSize / 2)) >= (clientsEntered[i].posX + (clientsEntered[i].playerSize / 2)) && (x.posX - (x.playerSize / 2)) <= (clientsEntered[i].posX + (clientsEntered[i].playerSize / 2))
                                && (x.posY + (x.playerSize / 2)) >= (clientsEntered[i].posY + (clientsEntered[i].playerSize / 2)) && (x.posY - (x.playerSize / 2)) <= (clientsEntered[i].posY + (clientsEntered[i].playerSize / 2)))
                                //bottom right
                                || ((x.posX + (x.playerSize / 2)) >= (clientsEntered[i].posX + (clientsEntered[i].playerSize / 2)) && (x.posX - (x.playerSize / 2)) <= (clientsEntered[i].posX + (clientsEntered[i].playerSize / 2))
                                && (x.posY + (x.playerSize / 2)) >= (clientsEntered[i].posY - (clientsEntered[i].playerSize / 2)) && (x.posY - (x.playerSize / 2)) <= (clientsEntered[i].posY - (clientsEntered[i].playerSize / 2)))));


                            



                            //collideList2 = clientsEntered.FindAll(x => (x.posX + (x.playerSize / 2) >= clientsEntered[i].posX - (clientsEntered[i].playerSize / 2))
                            //                || (x.posX - (x.playerSize / 2) <= clientsEntered[i].posX + (clientsEntered[i].playerSize / 2))
                            //                || (x.posY + (x.playerSize / 2) >= clientsEntered[i].posY - (clientsEntered[i].playerSize / 2))
                            //                || (x.posY - (x.playerSize / 2) <= clientsEntered[i].posY + (clientsEntered[i].playerSize / 2))
                            //                && x.clientNumber != clientsEntered[i].clientNumber);


                            if (collideList2.Count != 0)
                            {
                                
                                Console.WriteLine("\nCOLLIDED W/ SOMEONE-- cn1 {0}, cn2 {1}, posxC1 {2}, y {3} :: posxC2 {4}, y2 {5} -_- SZ1 {6} , SZ2{7} ",
                                    collideList2[0].clientNumber, clientsEntered[i].clientNumber, collideList2[0].posX, collideList2[0].posY
                                    , clientsEntered[i].posX, clientsEntered[i].posY, collideList2[0].playerSize, clientsEntered[i].playerSize);

                                int indexC = clientsEntered.FindIndex(x => x.clientNumber == collideList2[0].clientNumber);


                                if (clientsEntered[indexC].clientNumber != 0)
                                {

                                    // if ((clientsEntered[indexC].posX + (clientsEntered[indexC].playerSize / 2) >= compareGamePlay[i].compareX - (compareGamePlay[i].size / 2))
                                    //|| (clientsEntered[indexC].posX - (clientsEntered[indexC].playerSize / 2) <= compareGamePlay[i].compareX + (compareGamePlay[i].size / 2))
                                    //|| (clientsEntered[indexC].posY + (clientsEntered[indexC].playerSize / 2) >= compareGamePlay[i].compareY - (compareGamePlay[i].size / 2))
                                    // || (clientsEntered[indexC].posY - (clientsEntered[indexC].playerSize / 2) <= compareGamePlay[i].compareY + (compareGamePlay[i].size / 2)))
                                    //&& !resolvedPellets.Exists(x => (x.objectNum == compareGamePlay[i].objectNum)
                                    //                            && (x.compareX == compareGamePlay[i].compareX) && (x.compareY == compareGamePlay[i].compareY)))
                                    //{
                                

                                    if (clientsEntered[indexC].playerSize == clientsEntered[i].playerSize)//compareGamePlay[i].size)
                                    {


                                        //reset both and loose their points
                                        clientsEntered[indexC].posX = randomGen.Next(-23, 23); //reset1X;
                                        clientsEntered[indexC].posY = randomGen.Next(-12, 14);  //reset1Y;
                                        clientsEntered[indexC].playerSpeed = 10;
                                        clientsEntered[indexC].playerSize = 2;

                                        //int tempi = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum2);

                                        clientsEntered[i].playerSize = 2; //compareGamePlay[i].size;
                                        clientsEntered[i].posX = randomGen.Next(-23, 23); //reset2X;
                                        clientsEntered[i].posY = randomGen.Next(-12, 14);
                                        clientsEntered[i].playerSpeed = 10;

                                        //clientsHit = string.Concat(clientsHit, string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                        //        clientsEntered[indexC].clientNumber, 2, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                        //        clientsEntered[indexC].posY, clientsEntered[i].clientNumber, 2, clientsEntered[i].playerSpeed,
                                        //        clientsEntered[i].posX, clientsEntered[i].posY));

                                    }
                                    else if (clientsEntered[indexC].playerSize > clientsEntered[i].playerSize)//compareGamePlay[i].size)
                                    {

                                        double speedChange = (clientsEntered[indexC].playerSpeed - 2);

                                        if (speedChange < 1)
                                        {
                                            clientsEntered[indexC].playerSpeed = clientsEntered[indexC].playerSpeed * .85;

                                        }
                                        else
                                        {
                                            clientsEntered[indexC].playerSpeed = speedChange;
                                        }


                                        clientsEntered[indexC].playerSize += clientsEntered[i].playerSize; //compareGamePlay[i].size;

                                        //int tempi = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum2);

                                        clientsEntered[i].playerSize = 2;
                                        clientsEntered[i].playerSpeed = 10;
                                        clientsEntered[i].posX = randomGen.Next(-23, 23);
                                        clientsEntered[i].posY = randomGen.Next(-12, 14);

                                        // clientsEntered[indexC].clientNumber,
                                        //clientsHit = string.Concat(clientsHit, string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                        //        clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                        //        clientsEntered[indexC].posY, clientsEntered[i].clientNumber, clientsEntered[i].playerSize, clientsEntered[i].playerSpeed,
                                        //        clientsEntered[i].posX, clientsEntered[i].posY));


                                    }
                                    else if (clientsEntered[indexC].playerSize < clientsEntered[i].playerSize)//compareGamePlay[i].size)
                                    {

                                        //int tempi = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum2);

                                        clientsEntered[i].playerSize += clientsEntered[indexC].playerSize;

                                        double speedChange = (clientsEntered[i].playerSpeed - 2);

                                        if (speedChange < 1)
                                        {
                                            clientsEntered[i].playerSpeed = clientsEntered[indexC].playerSpeed * .85;

                                        }
                                        else
                                        {
                                            clientsEntered[i].playerSpeed = speedChange;
                                        }


                                        //areGamePlay[i].size;

                                        clientsEntered[indexC].posX = randomGen.Next(-23, 23);
                                        clientsEntered[indexC].posY = randomGen.Next(-12, 14);
                                        clientsEntered[indexC].playerSize = 2;
                                        clientsEntered[indexC].playerSpeed = 10;

                                        //clientsHit = string.Concat(clientsHit, string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                        //    clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                        //        clientsEntered[indexC].posY, clientsEntered[i].clientNumber, clientsEntered[i].playerSize, clientsEntered[i].playerSpeed,
                                        //        clientsEntered[i].posX, clientsEntered[i].posY));


                                    }


//                                    clientsEntered[0].sw.WriteLine("pH\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
//                                       clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
//                                      clientsEntered[indexC].posY, clientsEntered[i].clientNumber, clientsEntered[i].playerSize, clientsEntered[i].playerSpeed,
//                                       clientsEntered[i].posX, clientsEntered[i].posY);

//                                    clientsEntered[1].sw.WriteLine("pH\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
//                                            clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
//                                           clientsEntered[indexC].posY, clientsEntered[i].clientNumber, clientsEntered[i].playerSize, clientsEntered[i].playerSpeed,
//                                           clientsEntered[i].posX, clientsEntered[i].posY);

                                }

                                //lastProcessRange = compareGamePlay.Count;

                            }

                        }
                    }


                resolvedPellets.Clear();

            }
*/ 


       // } //MAIN WHILE LOOP


    }
 

}

public class NextRanX
{
    public int randomXpos()
    {
      int r;
      do
      {
        r = randomGen.Next(-23, 23);
      }
      while (generatedX.Contains(r));

      generatedX.Add(r);
      return r;
    }
}

 public class NextRanY
 {
     public int randomYpos()
     {
        int r;
        do
        {
            r = randomGen.Next(-12, 14);
        }
        while (generatedY.Contains(r));

        generatedY.Add(r);
        return r;
     }
}
private class handleCollisions
{

    public handleCollisions()
    {

    }

    public void collisions()
    {
        while (true)
        {
            if (numClientsPass > 1 && addAfterStart) // physTimer.Elapsed.Milliseconds >= 20 
            {

                for (int i = 0; i < clientsEntered.Count; ++i)
                {

                    //if (compareGamePlay[i].hitType == "hitPell")
                    //{

                    //orderTempList[i]
                    //Client c =  clientsEntered.Find(x => x.clientNumber == compareGamePlay[i].clientsNum);  

                    // int indexC1 = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum);

                    List<Client> collideList2 = new List<Client>();


                    collideList2 = clientsEntered.FindAll(x => ( x.clientNumber != clientsEntered[i].clientNumber) &&
                        // compares top left corner
                       ( ((x.posX + (x.playerSize / 2)) >= (clientsEntered[i].posX - (clientsEntered[i].playerSize / 2)) && (x.posX - (x.playerSize / 2)) <= (clientsEntered[i].posX - (clientsEntered[i].playerSize / 2))
                        && (x.posY + (x.playerSize / 2)) >= (clientsEntered[i].posY + (clientsEntered[i].playerSize / 2)) && (x.posY - (x.playerSize / 2)) <= (clientsEntered[i].posY + (clientsEntered[i].playerSize / 2)))
                            // bottom left
                        || ((x.posX + (x.playerSize / 2)) >= (clientsEntered[i].posX - (clientsEntered[i].playerSize / 2)) && (x.posX - (x.playerSize / 2)) <= (clientsEntered[i].posX - (clientsEntered[i].playerSize / 2))
                        && (x.posY + (x.playerSize / 2)) >= (clientsEntered[i].posY - (clientsEntered[i].playerSize / 2)) && (x.posY - (x.playerSize / 2)) <= (clientsEntered[i].posY - (clientsEntered[i].playerSize / 2)))
                            // top right
                        || ((x.posX + (x.playerSize / 2)) >= (clientsEntered[i].posX + (clientsEntered[i].playerSize / 2)) && (x.posX - (x.playerSize / 2)) <= (clientsEntered[i].posX + (clientsEntered[i].playerSize / 2))
                        && (x.posY + (x.playerSize / 2)) >= (clientsEntered[i].posY + (clientsEntered[i].playerSize / 2)) && (x.posY - (x.playerSize / 2)) <= (clientsEntered[i].posY + (clientsEntered[i].playerSize / 2)))
                            //bottom right
                        || ((x.posX + (x.playerSize / 2)) >= (clientsEntered[i].posX + (clientsEntered[i].playerSize / 2)) && (x.posX - (x.playerSize / 2)) <= (clientsEntered[i].posX + (clientsEntered[i].playerSize / 2))
                        && (x.posY + (x.playerSize / 2)) >= (clientsEntered[i].posY - (clientsEntered[i].playerSize / 2)) && (x.posY - (x.playerSize / 2)) <= (clientsEntered[i].posY - (clientsEntered[i].playerSize / 2)))));


                    /*
                     tempC1 = clientsEntered.FindAll(x =>
                        // compares top left corner
                            ((x.posX + (x.playerSize / 2)) >= (gamePellets[i].px - (.5)) && (x.posX - (x.playerSize / 2)) <= (gamePellets[i].px - (.5))
                            && (x.posY + (x.playerSize / 2)) >= (gamePellets[i].py + (.5)) && (x.posY - (x.playerSize / 2)) <= (gamePellets[i].py + (.5)))
                            // bottom left
                            || (x.posX + (x.playerSize / 2)) >= (gamePellets[i].px - (.5)) && (x.posX - (x.playerSize / 2)) <= (gamePellets[i].px - (.5))
                            && (x.posY + (x.playerSize / 2)) >= (gamePellets[i].py - (.5)) && (x.posY - (x.playerSize / 2)) <= (gamePellets[i].py - (.5))
                            // top right
                            || ((x.posX + (x.playerSize / 2)) >= (gamePellets[i].px + (.5)) && (x.posX - (x.playerSize / 2)) <= (gamePellets[i].px + (.5))
                            && (x.posY + (x.playerSize / 2)) >= (gamePellets[i].py + (.5)) && (x.posY - (x.playerSize / 2)) <= (gamePellets[i].py + (.5)))
                            //bottom right
                            || ((x.posX + (x.playerSize / 2)) >= (gamePellets[i].px + (.5)) && (x.posX - (x.playerSize / 2)) <= (gamePellets[i].px + (.5))
                            && (x.posY + (x.playerSize / 2)) >= (gamePellets[i].py - (.5)) && (x.posY - (x.playerSize / 2)) <= (gamePellets[i].py - (.5))));
    
                      */



                    //collideList2 = clientsEntered.FindAll(x => (x.posX + (x.playerSize / 2) >= clientsEntered[i].posX - (clientsEntered[i].playerSize / 2))
                    //                || (x.posX - (x.playerSize / 2) <= clientsEntered[i].posX + (clientsEntered[i].playerSize / 2))
                    //                || (x.posY + (x.playerSize / 2) >= clientsEntered[i].posY - (clientsEntered[i].playerSize / 2))
                    //                || (x.posY - (x.playerSize / 2) <= clientsEntered[i].posY + (clientsEntered[i].playerSize / 2))
                    //                && x.clientNumber != clientsEntered[i].clientNumber);


                    if (collideList2.Count != 0)
                    {
                        /*
                        Console.WriteLine("resolvePel count: {0} ", resolvedPellets.Count);
                        for (int w = 0; w < resolvedPellets.Count; ++w)
                        {

                            Console.WriteLine("\nResolved pellets == clin#1 {0}, clin#2 {1}, Obj# {2} ",
                                resolvedPellets[i].clientsNum, resolvedPellets[i].clientsNum2, resolvedPellets[i].objectNum);
                        }
                        */
                        Console.WriteLine("\nCOLLIDED W/ SOMEONE-- cn1 {0}, cn2 {1}, posxC1 {2}, y {3} :: posxC2 {4}, y2 {5} -_- SZ1 {6} , SZ2{7} ",
                            collideList2[0].clientNumber, clientsEntered[i].clientNumber, collideList2[0].posX, collideList2[0].posY
                            , clientsEntered[i].posX, clientsEntered[i].posY, collideList2[0].playerSize, clientsEntered[i].playerSize);

                        int indexC = clientsEntered.FindIndex(x => x.clientNumber == collideList2[0].clientNumber);


                        if (clientsEntered[indexC].clientNumber != 0)
                        {

                            // if ((clientsEntered[indexC].posX + (clientsEntered[indexC].playerSize / 2) >= compareGamePlay[i].compareX - (compareGamePlay[i].size / 2))
                            //|| (clientsEntered[indexC].posX - (clientsEntered[indexC].playerSize / 2) <= compareGamePlay[i].compareX + (compareGamePlay[i].size / 2))
                            //|| (clientsEntered[indexC].posY + (clientsEntered[indexC].playerSize / 2) >= compareGamePlay[i].compareY - (compareGamePlay[i].size / 2))
                            // || (clientsEntered[indexC].posY - (clientsEntered[indexC].playerSize / 2) <= compareGamePlay[i].compareY + (compareGamePlay[i].size / 2)))
                            //&& !resolvedPellets.Exists(x => (x.objectNum == compareGamePlay[i].objectNum)
                            //                            && (x.compareX == compareGamePlay[i].compareX) && (x.compareY == compareGamePlay[i].compareY)))
                            //{

                            double reset1X = 0;
                            double reset1Y = 0;

                            double reset2X = 0;
                            double reset2Y = 0;

                            switch (clientsEntered[indexC].clientNumber)
                            {
                                case 1:
                                    {
                                        reset1X = -17;
                                        reset1Y = 7;
                                        break;
                                    }
                                case 2:
                                    {
                                        reset1X = 20;
                                        reset1Y = 7;

                                        break;
                                    }

                                case 3:
                                    {
                                        reset1X = 20;
                                        reset1Y = -10;
                                        break;
                                    }
                                default:
                                    break;
                            }

                            switch (clientsEntered[i].clientNumber)//(compareGamePlay[i].clientsNum)
                            {
                                case 1:
                                    {
                                        reset2X = -17;
                                        reset2Y = 7;
                                        break;
                                    }
                                case 2:
                                    {
                                        reset2X = 20;
                                        reset2Y = 7;

                                        break;
                                    }

                                case 3:
                                    {
                                        reset2X = 20;
                                        reset2Y = -10;
                                        break;
                                    }
                                default:
                                    break;
                            }


                            if (clientsEntered[indexC].playerSize == clientsEntered[i].playerSize)//compareGamePlay[i].size)
                            {


                                //reset both and loose their points
                                clientsEntered[indexC].posX = reset1X;
                                clientsEntered[indexC].posY = reset1Y;
                                clientsEntered[indexC].playerSpeed = 10;
                                clientsEntered[indexC].playerSize = 2;

                                //int tempi = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum2);

                                clientsEntered[i].playerSize = 2; //compareGamePlay[i].size;
                                clientsEntered[i].posX = reset2X;
                                clientsEntered[i].posY = reset2Y;
                                clientsEntered[i].playerSpeed = 10;

                                clientsHit = string.Concat(clientsHit, string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                        clientsEntered[indexC].clientNumber, 2, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                        clientsEntered[indexC].posY, clientsEntered[i].clientNumber, 2, clientsEntered[i].playerSpeed,
                                        clientsEntered[i].posX, clientsEntered[i].posY));

                            }
                            else if (clientsEntered[indexC].playerSize > clientsEntered[i].playerSize)//compareGamePlay[i].size)
                            {

                                double speedChange = (clientsEntered[indexC].playerSpeed - 2);

                                if (speedChange < 1)
                                {
                                    clientsEntered[indexC].playerSpeed = clientsEntered[indexC].playerSpeed * .85;

                                }
                                else
                                {
                                    clientsEntered[indexC].playerSpeed = speedChange;
                                }


                                clientsEntered[indexC].playerSize += clientsEntered[i].playerSize; //compareGamePlay[i].size;

                                //int tempi = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum2);

                                clientsEntered[i].playerSize = 2;
                                clientsEntered[i].playerSpeed = 10;
                                clientsEntered[i].posX = reset2X;
                                clientsEntered[i].posY = reset2Y;

                                // clientsEntered[indexC].clientNumber,
                                clientsHit = string.Concat(clientsHit, string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                        clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                        clientsEntered[indexC].posY, clientsEntered[i].clientNumber, clientsEntered[i].playerSize, clientsEntered[i].playerSpeed,
                                        clientsEntered[i].posX, clientsEntered[i].posY));


                            }
                            else if (clientsEntered[indexC].playerSize < clientsEntered[i].playerSize)//compareGamePlay[i].size)
                            {

                                //int tempi = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum2);

                                clientsEntered[i].playerSize += clientsEntered[indexC].playerSize;

                                double speedChange = (clientsEntered[i].playerSpeed - 2);

                                if (speedChange < 1)
                                {
                                    clientsEntered[i].playerSpeed = clientsEntered[indexC].playerSpeed * .85;

                                }
                                else
                                {
                                    clientsEntered[i].playerSpeed = speedChange;
                                }


                                //areGamePlay[i].size;

                                clientsEntered[indexC].posX = reset1X;
                                clientsEntered[indexC].posY = reset1Y;
                                clientsEntered[indexC].playerSize = 2;
                                clientsEntered[indexC].playerSpeed = 10;

                                clientsHit = string.Concat(clientsHit, string.Format("\\{0}\\{1}\\{2}\\{3}\\{4}\\{5}\\{6}\\{7}\\{8}\\{9}",
                                    clientsEntered[indexC].clientNumber, clientsEntered[indexC].playerSize, clientsEntered[indexC].playerSpeed, clientsEntered[indexC].posX,
                                        clientsEntered[indexC].posY, clientsEntered[i].clientNumber, clientsEntered[i].playerSize, clientsEntered[i].playerSpeed,
                                        clientsEntered[i].posX, clientsEntered[i].posY));


                            }


                            //playInfo addToResolve = new playInfo();
                            //addToResolve.objectNum = compareGamePlay[i].objectNum;
                            //addToResolve.compareX = compareGamePlay[i].compareX;
                            //addToResolve.compareY = compareGamePlay[i].compareY;

                            //resolvedPellets.Add(addToResolve);

                            //}

                        }

                        //lastProcessRange = compareGamePlay.Count;

                    }

                }
            }
        }
    }// end of collisions

}
// end of handleCollisions class




        }
    }
