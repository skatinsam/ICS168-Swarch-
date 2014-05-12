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
      
        protected static bool startedProcess;

        protected static List<Client> clientsEntered = new List<Client>();
        protected static DatabaseManager db = new DatabaseManager();
        protected static bool passwordAcepted;
        protected static string initPelletLocation;
        protected static List<playing> clientsPlaying = new List<playing>();

        protected struct playing
        {
           public int clientNumber;
           public bool isPlaying;
        }

        protected struct pellet
        {
            public int px;
            public int py;

        }

        protected static List<pellet> gamePellets = new List<pellet>();
        
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

           // public bool connected = false;

            //public bool clientpasswordAcepted = false;
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
    Thread stateThread;
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

        for(int i=0; i<4; ++i)  // 4 -- number of player allowable pergame
        {
            playing tempP;
            tempP.clientNumber = i+1;
            tempP.isPlaying = false;

            clientsPlaying.Add(tempP);
        
        }

     

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
                tempClient.sw.AutoFlush = true;

                tempClient.clientNumber = clientsConnected;
                
            // array that holds clients to be used globally 
            clientsEntered.Add(tempClient);
                
            tempClient.clientThread.Start(tempClient.TCPclient);

                

            //===========================================================
            ClientGameState gs = new ClientGameState();

                stateThread = new Thread(
                        new ThreadStart(gs.gameState));

                stateThread.Start();
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

                                if (response == "connect" || response == "added")
                                {
                                    //tempClient.sw.WriteLine("correctUserPass");
                                   // passwordAcepted = true;
                                   // tempClient.clientpasswordAcepted = true;
                                   
                                    bool found= false;

                                    for (int j = 0; j < clientsPlaying.Count && !found; ++j )
                                    {
                                        playing play = clientsPlaying[j];

                                        if (!play.isPlaying)
                                        {
                                            tempClient.clientNumber = play.clientNumber;
                                            play.isPlaying = true;

                                            clientsPlaying[j] = play;
                                            clientsEntered[i] = tempClient;

                                            found = true;
                                        }
                                    }
                                     
                                   // string stringTemp = String.Format("clientNumber\\{0}", tempClient.clientNumber);

                                    tempClient.sw.WriteLine(string.Concat(String.Format("clientNumber\\{0}", tempClient.clientNumber), initPelletLocation));

                                    //tempClient.sw.WriteLine(initPelletLocation);
                                }
                                else
                                {
                                    tempClient.sw.WriteLine("incorrectUserPass");
                                    //sw2.WriteLine("pad\\{0}", gd1.movement1);
                                }
                                
                                break;
                            }

                        default:
                            break;

                    }
                }

                //if(tempClient.clientpasswordAcepted)
                //{
               //     tempClient.sw.WriteLine(initPelletLocation);
               // }


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
                gamedata.action = data[0];    // ???? might need to create new gamedata for every entry 
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

    public Random randPellets;
    bool startedGame;
    int numberOfpellets;

    public Random randomGen;
    List<int> generatedX;
    List<int> generatedY;

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

         randPellets = new Random();
         randomGen = new Random();
        
        startedGame = false;

        numberOfpellets = 5; // WHERE TO DETERMINE NUMBER OF PELLETS IN GAME
        initPelletLocation = "";  //  "iniP\\";

         generatedX = new List<int>();
         generatedY = new List<int>();

        //TEMP
        round2 = false;
        lastLocx = 0;
        lastLocy = 0;

    }

    public void gameState()  //NOTHING IS SENT OUT HERE, JUST CALCULATED
    {
        while (true)
        {
         
            if (!startedGame) //(client1Start && client2Start)
            {
                
                for(int i=0; i < numberOfpellets; ++i)
                {
                  pellet p = new pellet();
                  p.px = NextRanX();//randPellets.Next(-23,23);
                  p.py = NextRanY();//randPellets.Next(-12, 14);

                  gamePellets.Add(p);
                    
                    string tempstring = string.Format("\\{0}\\{1}",p.px,p.py);
                    initPelletLocation= String.Concat(initPelletLocation,tempstring); 
                } 
               
                startedGame = true;
               
               
                physTimer.Start();  //??? may not need

            }

            if (physTimer.Elapsed.Milliseconds >= 20) 
            {
              

             

            }
            


        }


    }

    public int NextRanX()
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

    public int NextRanY()
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
    
    
    }
}
