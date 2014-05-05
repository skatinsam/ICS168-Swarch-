﻿using System;
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

        protected static TcpClient[] clientsArray;
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


        protected static gameData p1CollisionCompare;
        protected static gameData p2CollisionCompare;

        protected static bool sendHit;
        protected static bool sendScore;

        protected static int waitTimeLoopCount;
        protected static int[] Score;
        protected static double hitTime;

        // wallBounds[0] -- Top 
        // wallBounds[1] -- Bottom
        protected static double[] wallBounds;

        // goalWall[0] -- left(p1)
        // goalWall[1] -- right(p2)
        protected static double[] goalWall;
        protected static int sleepTime;


        protected static ArrayList clientsEntered = new ArrayList();

        protected struct gameData
        {
            public string action;
            public double posX;
            public double posY;
            public double movement1;
            public double movement2;
            public DateTime timeStamp;

        }

        //protected static List<gameData> clientData = new List<gameData>(); // array of sturct for each client info

        public Server()
        {
            //this.tcpListener = new TcpListener(IPAddress.Any, 4040);
            Socket sock = new Socket();
            sock.startSock();
            // this.listenThread = new Thread(new ThreadStart(sock.startSock)); //(ListenForClients));
            //  this.listenThread.Start();
            //clientsArray = new TcpClient[2] { null, null };
        }

// THREAD ESTABLISH CONNECTIONS =======================================================================================
private class Socket//private void ListenForClients()  
{
    public TcpListener tcpListener;
    int clientsConnected;
    //TcpClient[] clientsArray;
    Thread clientThread;
    //Thread clientThread2;
    Thread BallThread; 
    bool gameStarted;

    public StreamWriter sw1;
    public StreamWriter sw2;
    Object thisLock = new Object();
    public DateTime p1T2;
    public DateTime p2T2;

    public Socket()
    {
        this.tcpListener = new TcpListener(IPAddress.Any, 4040);
        this.tcpListener.Start();
        clientsArray = new TcpClient[2] { null, null };
        gameStarted = false;

        pad1 = new double[] { 0, 0 };
        pad2 = new double[] { 0, 0 };
        wallBounds = new double[] { 0, 0 };
        goalWall = new double[] { 0, 0 };

        // gameData p1CollisionCompare = new gameData();
        // gameData p2CollisionCompare = new gameData();
        sendHit = false;
        sendScore = false;
        p1T2 = new DateTime();
        p2T2 = new DateTime();

        sleepTime = 100; // change time to simulate delay (in ms)

    }
    public void startSock()
    {
        Console.WriteLine("-- SWARCH: Server Started --- ");

        while (true)
        {

            //if (clientsConnected < 2)
           // {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //display client that just connected
                TcpClient tcpClientTemp = client;
                Console.WriteLine("client localEndPt: " +
                                    tcpClientTemp.Client.LocalEndPoint);
                Console.WriteLine("client RemoteEndPt: " +
                                    tcpClientTemp.Client.RemoteEndPoint);

                //store the clients in array

                clientsArray[clientsConnected] = tcpClientTemp;

                clientsConnected++;

                NetworkStream clientnws = client.GetStream();

                StreamWriter swClientNum = new StreamWriter(clientnws);
                swClientNum.AutoFlush = true;

                swClientNum.WriteLine("playNum\\{0}", clientsConnected);

                Console.WriteLine("clients Connected " + clientsConnected);
           // }


           // if (clientsConnected == 2 && !gameStarted)
            //{
              //  Console.WriteLine("\n Let The Game Begin! ");



                NetworkStream nws1 = clientsArray[0].GetStream(); //client.GetStream();
               // NetworkStream nws2 = clientsArray[1].GetStream();

                //sw1 = new StreamWriter(nws1);
                sw1.AutoFlush = true;

                //sw2 = new StreamWriter(nws2);
                //sw2.AutoFlush = true;


                //sw1.WriteLine("canSendStart");
                //sw2.WriteLine("canSendStart");

                


                ThreadSock tSock1 = new ThreadSock(nws1, this);

                //ThreadSock tSock2 = new ThreadSock(nws2, this);


                //clientThread = new Thread(
                //    new ParameterizedThreadStart(tSock1.HandleClientComm)); //(HandleClientComm));

                //clientThread2 = new Thread(
                //        new ParameterizedThreadStart(tSock2.HandleClientComm));


                //clientThread.Start(clientsArray[0]);
                //clientThread2.Start(clientsArray[1]);




               clientsEntered.Add(new Thread(
                        new ParameterizedThreadStart(tSock1.HandleClientComm)));


              // clientsEntered[clientsEntered.Count - 1];
            
            //===========================================================
                //BallControl ballc = new BallControl(nws1, nws2);

               // BallThread = new Thread(
               //         new ThreadStart(ballc.ballLoop));

               // BallThread.Start();
           //===========================================================


                 // *** THIS WILL BE A SEPEATE THREAD
                processGame();
                //gameStarted = true;
            //}



        }
    }
    // process the game state to display for both players
    public void processGame()
    {
        //data[0] - action       
        //data[1] - movement

        //receive string: "pad\\ 1\\ 50" - [action, player, position] 

        while (clientsConnected == 2)  // clientsConnected--; => when a client disconnects
        {
            gameData gd1;
            gameData gd2;

            //**** DELAY HIT, PADDLE, START
            //lock (thisLock)
            //{
            if (client1Queue.Count != 0)
            {
                lock (thisLock)
                {
                    gd1 = (gameData)client1Queue.Dequeue();
                }

                switch (gd1.action)
                {
                    case "pad":
                        {
                            pad1[1] = gd1.movement1;

                            Thread.Sleep(sleepTime);
                            sw2.WriteLine("pad\\{0}", gd1.movement1);
                            // Console.WriteLine("\nWROTE TO SW2 -- {0}", gd1.movement1);
                            break;
                        }
                    case "hit":
                        {

                            // DELAY
                            //gotHit = true;
                            //p1CollisionCompare = gd1;



                            // if Wall hit just send -Y speed 
                            //sw1.WriteLine("hit\\{0}\\{1}", currentSpeed[0], (currentSpeed[1]*-1));  

                            break;
                        }
                    case "lat":
                        {
                            p1T2 = DateTime.Now;

                            Thread.Sleep(sleepTime);
                            sw1.WriteLine("lat\\" + p1T2);
                            break;
                        }



                    default:
                        break;

                }
            }
            //}
            //lock (thisLock)
            // {
            if (client2Queue.Count != 0)
            {
                lock (thisLock)
                {
                    gd2 = (gameData)client2Queue.Dequeue();
                }

                switch (gd2.action)
                {
                    case "pad":
                        {
                            pad2[1] = gd2.movement1;

                            Thread.Sleep(sleepTime);
                            sw1.WriteLine("pad\\{0}", gd2.movement1);
                            //  Console.WriteLine("\nWROTE TO SW1 -- {0}", gd2.movement1);
                            break;
                        }
                    case "hit":
                        {

                            // DELAY
                            // gotHit = true;
                            // p2CollisionCompare = gd2;

                            //sw2.WriteLine("hit\\{0}\\{1}", currentSpeed[0], (currentSpeed[1]*-1));

                            break;
                        }
                    case "lat":
                        {
                            p2T2 = DateTime.Now;

                            Thread.Sleep(sleepTime);
                            sw2.WriteLine("lat\\" + p2T2); //
                            break;
                        }

                    default:
                        break;
                }
            }


            // }

            ///*
            if (sendHit)
            {
                double[] speedToSend = currentSpeed;
                double[] ballToSend = ballLocation;
                double timeToSend = hitTime; //physTimer.Elapsed.TotalSeconds;

                Thread.Sleep(sleepTime);
                sw1.WriteLine("hit\\{0}\\{1}\\{2}\\{3}\\{4}",
                            speedToSend[0], speedToSend[1], ballToSend[0],
                            ballToSend[1], timeToSend);


                sw2.WriteLine("hit\\{0}\\{1}\\{2}\\{3}\\{4}",
                            speedToSend[0], speedToSend[1], ballToSend[0],
                            ballToSend[1], timeToSend);

                sendHit = false;
                physTimer.Restart();

            }


            if (sendScore)
            {
                sw1.WriteLine("points\\{0}\\{1}", Score[0], Score[1]);
                sw2.WriteLine("points\\{0}\\{1}", Score[0], Score[1]);
                sendScore = false;
            }
            // */
            //}
        }
    }

}
// THREAD ESTABLISH CONNECTIONS  (END)=======================================================================================

// THREAD READ IN INFO =======================================================================================
private class ThreadSock
{
    //private Socket socket;
    //private NetworkStream nws;
    //StreamWriter sw;        



    public ThreadSock(NetworkStream nwsIn, Socket sockIn)
    {
        //nws = nwsIn;
        //socket = sockIn;


        //sw = new StreamWriter(nws);
        //physTimer.Start();
        //sw.WriteLine("Start\\5");

        client1Queue = new Queue();
        client2Queue = new Queue();

    }


    public void HandleClientComm(object client)
    {
        TcpClient tcpClient = (TcpClient)client;
        //TcpClient sendClient = (TcpClient)client;
        NetworkStream clientStream = tcpClient.GetStream();
        NetworkStream nws = tcpClient.GetStream();
        StreamReader sr = new StreamReader(nws);
        gameData gd = new gameData();

        byte[] message = new byte[4096];
        //int bytesRead;
        string readData;
        Object thisLock = new Object();

        while (true)
        {
            //lock (thisLock)
            //{
            //bytesRead = 0;
            readData = "";

            try
            {

                //bytesRead = clientStream.Read(message, 0, 4096); // waits until it receives data
                readData = sr.ReadLine();


                // ASCIIEncoding encoder = new ASCIIEncoding();
                // readData = encoder.GetString(message, 0, bytesRead);

            }
            catch
            {
                //a socket error has occured
                break;
            }

            string[] data = readData.Split(delimiter);

            //readData
            if (data[0] == "close") //(bytesRead == 0)  // close the client (enter here) when disconnect data reveiced
            {
                //the client has disconnected from the server
                break;
            }
            //readData
            if (data[0] == "start")
            {
                if (tcpClient.Client.RemoteEndPoint == clientsArray[0].Client.RemoteEndPoint) //gd.action = readData;  // when game is starting
                {
                    client1Start = true;

                    //string[] data = readData.Split(delimiter);

                    pad1[0] = Convert.ToDouble(data[1]);
                    pad1[1] = Convert.ToDouble(data[2]);


                    wallBounds[0] = Convert.ToDouble(data[3]);
                    wallBounds[1] = Convert.ToDouble(data[4]);

                    goalWall[0] = Convert.ToDouble(data[5]);
                    goalWall[1] = Convert.ToDouble(data[6]);

                    initBallPosX = Convert.ToDouble(data[7]);
                    initBallPosY = Convert.ToDouble(data[8]);




                }
                else if (tcpClient.Client.RemoteEndPoint == clientsArray[1].Client.RemoteEndPoint) //gd.action = readData;  // when game is starting
                {
                    client2Start = true;

                    //string[] data = readData.Split(delimiter);

                    pad2[0] = Convert.ToDouble(data[1]);
                    pad2[1] = Convert.ToDouble(data[2]);


                }
                //clientData.Add(gd);
            }
            if (data[0] == "startNextRound")
            {
                if (tcpClient.Client.RemoteEndPoint == clientsArray[0].Client.RemoteEndPoint) //gd.action = readData;  // when game is starting
                {
                    nextRoundP1 = true;
                }
                else if (tcpClient.Client.RemoteEndPoint == clientsArray[1].Client.RemoteEndPoint) //gd.action = readData;  // when game is starting
                {
                    nextRoundP2 = true;
                }
            }
            if (data[0] == "startNewGame")
            {
                if (tcpClient.Client.RemoteEndPoint == clientsArray[0].Client.RemoteEndPoint) //gd.action = readData;  // when game is starting
                {
                    nextRoundP1 = true;
                    Score[0] = 0;
                }
                else if (tcpClient.Client.RemoteEndPoint == clientsArray[1].Client.RemoteEndPoint) //gd.action = readData;  // when game is starting
                {
                    nextRoundP2 = true;
                    Score[1] = 0;
                }

            }


            else if (readData != "" || readData != null)
            {
                //data[0] - action  
                //data[1] - pos x
                //data[2] - pos y
                //data[3] - movement x
                //data[4] - movement y
                //data[5] - timeStamp


                //string[] data = readData.Split(delimiter);

                gd.action = data[0];

                if (data.Length == 2)
                {
                    double move1 = Convert.ToDouble(data[1]);

                    gd.movement1 = move1;
                }
                /*
                                        if (data.Length == 3)
                                        {
                                            double move2 = Convert.ToDouble(data[2]);
                                            gd.movement2 = move2;
                                        }
                                        if (data.Length == 4)
                                        {
                                            DateTime time = Convert.ToDateTime(data[3]);
                                            gd.timeStamp = time;
                                        }
                                        if(data.Length == 5)
                                        {
                                            DateTime time = Convert.ToDateTime(data[3]);
                                            gd.timeStamp = time;


                                        }
                    */
                if (data.Length == 6)
                {
                    double posx = Convert.ToDouble(data[1]);
                    double posy = Convert.ToDouble(data[2]);

                    gd.posX = posx;
                    gd.posY = posy;

                    double move1 = Convert.ToDouble(data[3]);
                    double move2 = Convert.ToDouble(data[4]);

                    gd.movement1 = move1;
                    gd.movement2 = move2;


                    DateTime time = Convert.ToDateTime(data[5]);
                    gd.timeStamp = time;
                }

                // if (data[0] == "lat")
                //{
                //     gd.timeStamp = (data[1]);
                // }




                //place in own clients queue
                if (tcpClient.Client.RemoteEndPoint ==
                        clientsArray[0].Client.RemoteEndPoint)
                {
                    lock (thisLock)
                    {
                        client1Queue.Enqueue(gd);
                    }
                }
                else if (tcpClient.Client.RemoteEndPoint ==
                        clientsArray[1].Client.RemoteEndPoint)
                {
                    lock (thisLock)
                    {
                        client2Queue.Enqueue(gd);
                    }
                }


            }
            //} 
        }

        tcpClient.Close();
    }


}
// THREAD READ IN INFO  (END) =======================================================================================

private class BallControl
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


    //TEMP
    bool round2;
    double lastLocx;
    double lastLocy;

    public BallControl(NetworkStream nws1, NetworkStream nws2)
    {
        ballLocation = new double[] { 0, 0, 1, 1 };
        // ballLocation[1] = 0;
        currentSpeed = new double[] { -7, 5 }; //-5,3

        willBePosition = new double[] { 0, 0 };
        setSpeedIfHit = false;

        sw1 = new StreamWriter(nws1);
        sw1.AutoFlush = true;
        sw2 = new StreamWriter(nws2);
        sw2.AutoFlush = true;
        waitTimeLoopCount = 0;
        calcPaddle = false;
        startedWaitTime = false;
        Score = new int[] { 0, 0 };
        hitTime = 0;

        missSpeed = new double[] { 0, 0 };
        missPosition = new double[] { 0, 0 };
        missed = false;

        wallHit = false;

        //TEMP
        round2 = false;
        lastLocx = 0;
        lastLocy = 0;

    }

    public void ballLoop()  //NOTHING IS SENT OUT HERE, JUST CALCULATED
    {
        while (true)
        {
            // if (clientData.Count != 0)
            //{
            //lock(thisLock)
            //{
            if (client1Start && client2Start)
            {
                physTimer.Start();

                //initial ball move 
                ballLocation[0] = initBallPosX;
                ballLocation[1] = initBallPosY;



                // send start game for both clients here


                //                     sw2 = new StreamWriter(nws2))
                //
                Thread.Sleep(sleepTime);
                sw1.WriteLine("start\\{0}\\{1}", currentSpeed[0], currentSpeed[1]);
                sw2.WriteLine("start\\{0}\\{1}", currentSpeed[0], currentSpeed[1]);


                client1Start = false;

            }

            if (nextRoundP1 && nextRoundP2)
            {
                Thread.Sleep(sleepTime);
                physTimer.Start();
                sw1.WriteLine("nextRound\\{0}\\{1}", (currentSpeed[0]), currentSpeed[1]);
                sw2.WriteLine("nextRound\\{0}\\{1}", (currentSpeed[0]), currentSpeed[1]);
                nextRoundP1 = false;
                nextRoundP2 = false;
                round2 = true;
            }


            if (physTimer.Elapsed.Milliseconds >= 20)  // .Elapsed.Milliseconds
            {
                if ((ballLocation[0] - .5 <= pad1[0] + .5) ||
                    (ballLocation[0] + .5 >= pad2[0] - .5)) //&& !missed
                {
                    //to calculate latency, not adjust info. --> (t4-t1/2)  

                    collisionHit = true;

                    if (waitTimeLoopCount == 3)
                    {
                        calcPaddle = true;
                    }

                    waitTimeLoopCount++;

                }

                if ((ballLocation[1] + .5 >= wallBounds[0]) ||
                            (ballLocation[1] - .5 <= wallBounds[1]))      // && ballLocation[1] - .5 >= wallBounds[1]-.1 
                {

                    Console.WriteLine("\nHit Top or Bottom (BEFORE)[x,y] " + ballLocation[0] + " , " + ballLocation[1]
                                        + " WALLpos: " + (wallBounds[0]) + " , " + (wallBounds[1]));
                    currentSpeed[1] = currentSpeed[1] * -1;

                    if (ballLocation[1] + .5 >= wallBounds[0])
                    {
                        ballLocation[1] = wallBounds[0] - .534;
                        Console.WriteLine("------- MOVED DOWN ------- ");
                    }
                    else if (ballLocation[1] - .5 <= wallBounds[1])
                    {
                        ballLocation[1] = wallBounds[1] + .534;
                        Console.WriteLine("------- MOVED UP --------- ");

                    }

                    Console.WriteLine("\nHit Top or Bottom (AFTER)[x,y] " + ballLocation[0] + " , " + ballLocation[1]
                                            + " Movedpos: " + (wallBounds[0] - .2) + " , " + (wallBounds[1] + .2));

                    wallHit = true;
                }


                if (!collisionHit && !wallHit)
                {
                    ballLocation[0] += currentSpeed[0] * physTimer.Elapsed.TotalSeconds; //Milliseconds; 
                    ballLocation[1] += currentSpeed[1] * physTimer.Elapsed.TotalSeconds; //Milliseconds;
                    physTimer.Restart();


                }
                else if (collisionHit && waitTimeLoopCount <= 3)
                {
                    if (!setSpeedIfHit)
                    {

                        missSpeed[0] = currentSpeed[0];
                        missSpeed[1] = currentSpeed[1];
                        currentSpeed[0] = currentSpeed[0] * -1;
                        lastLocx = ballLocation[0];
                        lastLocy = ballLocation[1];
                        willBePosition[0] = lastLocx;
                        willBePosition[1] = lastLocy;

                        missPosition[0] = lastLocx;
                        missPosition[1] = lastLocy;

                        setSpeedIfHit = true;
                    }

                    //position if hit (reverse direction)
                    willBePosition[0] += currentSpeed[0] * physTimer.Elapsed.TotalSeconds;
                    willBePosition[1] += currentSpeed[1] * physTimer.Elapsed.TotalSeconds;


                    //position if missed
                    missPosition[0] += missSpeed[0] * physTimer.Elapsed.TotalSeconds;
                    missPosition[1] += missSpeed[1] * physTimer.Elapsed.TotalSeconds;


                    physTimer.Restart();

                    //collWaitTime.Start();
                    //startedWaitTime = true;
                }

                wallHit = false;

                if (calcPaddle)
                {
                    // if y-pos of pad is same pos as ball y
                    //(ballLocation[0] - .5 <= pad1[1] + 2 || ballLocation[0] - .5 <= pad1[1] - 2 ||
                    //    ballLocation[0] + .5 >= pad2[1] + 2 || ballLocation[0] + .5 <= pad2[1] - 2)

                    //(ballLocation[0] - .5 <= pad1[0] + .5 && (ballLocation[1]+.5 <= pad1[1]+2 && ballLocation[1]+.5 >= pad1[1]-2 ))

                    double topOfBall = (ballLocation[1] + .5);
                    double bottomOfBall = (ballLocation[1] - .5);
                    double ballFrontP1 = (ballLocation[0] - .5);
                    double ballFrontP2 = (ballLocation[0] + .5);
                    double pad1Front = (pad1[0] + .5);
                    double pad2Front = (pad2[0] - .5);
                    double pad1TopRange = (pad1[1] + 2);
                    double pad1BottomRange = (pad1[1] - 2);
                    double pad2TopRange = (pad2[1] + 2);
                    double pad2BottomRange = (pad2[1] - 2);
                    // Console.WriteLine("calc pad entered ");
                    //(ballLocation[0]-.5 <= pad1[0]+.5 && ballLocation[0]-.5 >= pad1[0]+.4)
                    if ((ballFrontP1 <= pad1Front && ((topOfBall <= pad1TopRange && topOfBall >= pad1BottomRange) || (bottomOfBall <= pad1TopRange && bottomOfBall >= pad1BottomRange))) ||
                        ((ballFrontP2 >= pad2Front) && ((topOfBall <= pad2TopRange && topOfBall >= pad2BottomRange) || (bottomOfBall <= pad2TopRange && bottomOfBall >= pad2BottomRange))))
                    {

                        Console.WriteLine("\nPadHit!(before) " + ballLocation[0] + " willBePos " + willBePosition[0] + " s " + (currentSpeed[0] * -1)
                                            + " t " + physTimer.Elapsed.TotalSeconds + " padL: " + pad1[0] + " padR: " + pad2[0]);
                        //currentSpeed[0] = currentSpeed[0] * -1;
                        // ballLocation[0] = willBePosition[0]; //currentSpeed[0] * physTimer.Elapsed.TotalSeconds; //Milliseconds; 
                        // ballLocation[1] = willBePosition[1]; //currentSpeed[1] * physTimer.Elapsed.TotalSeconds; //Milliseconds;
                        //   physTimer.Restart();

                        if ((ballFrontP1 <= pad1Front && ((topOfBall <= pad1TopRange && topOfBall >= pad1BottomRange) || (bottomOfBall <= pad1TopRange && bottomOfBall >= pad1BottomRange))))
                        {
                            ballLocation[0] = pad1Front + .534;
                            Console.WriteLine("------- MOVED RIGHT --------- ");


                        }
                        else if (((ballFrontP2 >= pad2Front) && ((topOfBall <= pad2TopRange && topOfBall >= pad2BottomRange) || (bottomOfBall <= pad2TopRange && bottomOfBall >= pad2BottomRange))))
                        {
                            ballLocation[0] = pad2Front - .534;
                            Console.WriteLine("------- MOVED LEFT --------- ");


                        }



                        //Console.WriteLine("\nPadHit!(after) " + ballLocation[0] + " t " + physTimer.Elapsed.TotalSeconds);
                        hitTime = physTimer.Elapsed.TotalSeconds;
                        sendHit = true;


                    }
                    else
                    {
                        // ---------- ORIGINALLY ------------------------------------
                        ///*
                        //MISSED.
                        if (ballLocation[0] - .5 <= pad1[0] + .5)
                        {
                            Score[1] += 1;
                        }
                        if (ballLocation[0] + .5 >= pad2[0] - .5)
                        {
                            Score[0] += 1;
                        }

                        // when missed send position 

                        ballLocation[0] = initBallPosX;
                        ballLocation[1] = initBallPosY;
                        sendScore = true;

                        //Console.WriteLine("\n score is: (p1,p2) " + Score[0] + " TO " + Score[1] + " Time " + physTimer.Elapsed.TotalSeconds);
                        physTimer.Reset();
                        //*/
                        // ---------- ORIGINALLY  END ------------------------------------  
                        ///*
                        //double elapseCollWait = collWaitTime.Elapsed.Milliseconds;
                        //collWaitTime.Reset();


                        //ballLocation[0] = missPosition[0]; //+= currentSpeed[0] * (physTimer.Elapsed.TotalSeconds + elapseCollWait); //Milliseconds; 
                        //ballLocation[1] = missPosition[1]; //+= currentSpeed[1] * (physTimer.Elapsed.TotalSeconds + elapseCollWait); //Milliseconds;
                        Console.WriteLine("\nMISSED: " + "ball loc " + ballLocation[0] + " missSpeed " + missSpeed[0]);
                        // missed = true;
                        //physTimer.Restart();
                        //*/
                    }

                    waitTimeLoopCount = 0;
                    collisionHit = false;
                    calcPaddle = false;
                    startedWaitTime = false;
                    setSpeedIfHit = false;
                }
                /*
                    if (ballLocation[0] + .5 >= goalWall[1]-.5)
                    {
                        // score for player 1
                        // send start
                        Score[0] += 1;

                        ballLocation[0] = initBallPosX;
                        ballLocation[1] = initBallPosY;
                        sendScore = true;

                        Console.WriteLine("\n GOAL LEFT");
                        missed = false;
                        physTimer.Reset();
                        //sw1.WriteLine("reStart\\{0}\\{1}", (currentSpeed[0] * -1), currentSpeed[1]);
                        //sw2.WriteLine("reStart\\{0}\\{1}", (currentSpeed[0] * -1), currentSpeed[1]);

                    }
                    else if (ballLocation[0] - .5 <= goalWall[0]+.5)
                    {
                        // score for player 2
                        Score[1] += 1;

                        ballLocation[0] = initBallPosX;
                        ballLocation[1] = initBallPosY;
                        sendScore = true;
                        Console.WriteLine("\n GOAL RIGHT");
                        missed = false;
                        physTimer.Reset();
                        //sw1.WriteLine("reStart\\{0}\\{1}", (currentSpeed[0] * -1), currentSpeed[1]);
                        //sw2.WriteLine("reStart\\{0}\\{1}", (currentSpeed[0] * -1), currentSpeed[1]);
                    }

                */

            }
            //}


            //}


        }


    }

}
    
    
    }
}