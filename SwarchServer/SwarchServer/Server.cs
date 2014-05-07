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
        protected static bool startedProcess;

        protected static List<Client> clientsEntered = new List<Client>();
        protected static DatabaseManager db = new DatabaseManager();

       
        // used to stores inportant info about the game
        protected struct gameData
        {
            public string userName;
            public string password;
           
            public string action;
            public double posX;
            public double posY;
            public double movement1;
            public double movement2;
            public DateTime timeStamp;

        }

        // used to store client communication for thread 
        // and reading and writing 
        protected struct Client
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
        }

        //protected static List<gameData> clientData = new List<gameData>(); // array of sturct for each client info

        public Server()
        {
            //this.tcpListener = new TcpListener(IPAddress.Any, 4040);
            Socket sock = new Socket();
            sock.startSock();
            
        }

// THREAD ESTABLISH CONNECTIONS =======================================================================================

private class Socket
{
    public TcpListener tcpListener;
    public int clientsConnected;
    //TcpClient[] clientsArray;
    Thread clientThread;
    //Thread clientThread2;
    Thread BallThread;
    Thread processGameThread;
    bool gameStarted;

    
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

        sleepTime = 100; // change time to simulate delay (in ms)

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
           
                clientsArray.Add( tcpClientTemp);


                Console.WriteLine("clients Connected " + clientsConnected);

                // client sream for reading and writing
                NetworkStream nws1 = (((TcpClient)clientsArray[clientsConnected-1]).GetStream());

                StreamWriter sw1 = new StreamWriter(nws1);
                sw1.AutoFlush = true;

                sw1.WriteLine("connected");
               
                ThreadSock tSock1 = new ThreadSock(nws1, this);

                // client sturt that holds thread, stream, client Number
                Client tempClient = new Client();

                tempClient.clientThread = new Thread(
                            new ParameterizedThreadStart(tSock1.HandleClientComm));

                tempClient.TCPclient= tcpClientTemp;

                tempClient.sw = sw1;

                tempClient.clientNumber = clientsConnected;
                
            // array that holds clients to be used globally 
            clientsEntered.Add(tempClient);
                
            tempClient.clientThread.Start(tempClient.TCPclient);

                

            //===========================================================
                //BallControl ballc = new BallControl(nws1, nws2);

               // BallThread = new Thread(
               //         new ThreadStart(ballc.ballLoop));

               // BallThread.Start();
           //===========================================================

                
                if (!startedProcess)
                {
                    processGame PG = new processGame();
                    processGameThread = new Thread(new ThreadStart( PG.startProcessGame));
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


  public processGame()
  {
      

  }

    public void startProcessGame()
    {
        while (clientsEntered.Count != 0)
        {
            gameData gd1;

            for (int i = 0; i < clientsEntered.Count; ++i)
            {
                Client tempClient = clientsEntered[i];

                if (tempClient.clientQueue != null && tempClient.clientQueue.Count != 0)
                {
                    lock (thisLock)
                    {

                        gd1 = (gameData)tempClient.clientQueue.Dequeue();
                    }

                    switch (gd1.action)
                    {
                        case "userAndPass":
                            {
                                // enter database
                                
                                string response = db.connect(gd1.userName, gd1.password);

                                if(response == "connect" || response == "added")
                                    tempClient.sw.WriteLine("correctUserPass");
                                else
                                    tempClient.sw.WriteLine("incorrectUserPass");
                                //sw2.WriteLine("pad\\{0}", gd1.movement1);

                                break;
                            }

                        default:
                            break;

                    }
                }
            }

          
        }
    }
}

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
        Client tempClient= new Client();
        int clientIndex = -9;

        lock (thisLock)
        {
          tempClient = clientsEntered.Find(x => x.TCPclient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint);
          clientIndex = clientsEntered.IndexOf(tempClient);
        }
    /*   // MANUALLY SEARCHING OF MATCHING CLIENT ****************************************
        int clientIndex = -9;
        bool found =false;
        

        for (int j = 0; j <= clientsEntered.Count-1 && !found; ++j )
        {
            tempClient = clientsEntered[j];
           
            if (tempClient.TCPclient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint)
            {
                found = true;
                clientIndex = j;
            }
        }
    */   // MANUALLY SEARCHING OF MATCHING CLIENT (END)****************************************
        
        
        tempClient.clientQueue = new Queue(); 
            

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
            if (data[0] == "userAndPass")
            {
                gamedata.action = data[0];
                gamedata.userName = data[1];
                gamedata.password = data[2];
                
            }

            lock (thisLock)
            {
                tempClient.clientQueue.Enqueue(gamedata);
                clientsEntered[clientIndex] = tempClient;
            }

            
        }

        tcpClient.Close();
    }
}


// THREAD READ IN INFO  (END) =======================================================================================

// **** handle other info about the game (NOT NEED YET)
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
