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
        //protected static List<pellet>  = new List<pellet>();
        protected static string addedNewPellets; 
       
        protected static List<playInfo> compareGamePlay = new List<playInfo>();


        protected static Stopwatch uniClock = new Stopwatch();
        protected static DateTime dt = new DateTime();

     ///*  
       protected struct playInfo
        {
           public int objectNum;
           public int clientsNum;
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
            public int clientPell;
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
    Thread processGameThread;
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

                tempClient.playerSize = 2;
                tempClient.playerSpeed = 10;//moveSpeed;
                
            // array that holds clients to be used globally 
            clientsEntered.Add(tempClient);
                
            tempClient.clientThread.Start(tempClient.TCPclient);

                

            //===========================================================
            if(!startedGameState)
            {
            ClientGameState gs = new ClientGameState();

                stateThread = new Thread(
                        new ThreadStart(gs.gameState));

                stateThread.Start();
                startedGameState = true;
            }
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
    public bool addAfterStart;
    public int currentNumPlayers;

    public string clientsConnectedInfo;
    public string newClientsInfo;
    public int numClientsPass;
    public string currentClientsMove;

    List<int> newClientaddedNum = new List<int>();

  public processGame()
  {
      addAfterStart = false;
      currentNumPlayers = 0;
      clientsConnectedInfo = ""; //new List<int>();
      newClientsInfo = "";
      numClientsPass=0;
      currentClientsMove = "";

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
                                 //*** MAKE SURE MOVEMENTS ARE SENT NO BIGGER THEN SIZE OF PELLET

                                currentClientsMove = string.Concat(currentClientsMove,
                                                            string.Format("\\{0}\\{1}\\{2}", gd1.movement1, gd1.movement2, clientsEntered[i].clientNumber)); //tempClient.clientNumber));
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
                                        String.Format("clientNumber\\{0}\\{1}\\{2}", clientsEntered[i].clientNumber, initialX, initialY), pellLoc));  // initPelletLocation
                                                                                   //tempClient

                                    clientsConnectedInfo = string.Concat(clientsConnectedInfo,
                                        string.Format("\\{0}\\{1}\\{2}", initialX, initialY, clientsEntered[i].clientNumber));
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
                        case "hit":
                            {
                               playInfo tempPlayInfo = new playInfo();

                               tempPlayInfo.objectNum = gd1.clientNum;
                               tempPlayInfo.compareX = gd1.posX;
                               tempPlayInfo.compareY = gd1.posY;
                               tempPlayInfo.timeStamp = gd1.timeStamp;
                               tempPlayInfo.clientsNum = clientsEntered[i].clientNumber;
                                                       //tempClient

                               compareGamePlay.Add(tempPlayInfo); //.Insert(tempClient.clientNumber, tempPlayInfo);
                               
                                break;
                            }
                        case "score":
                            {
                                int newScore = db.updateScore(gd1.userName, gd1.score);
                                gd1.score = newScore;

                                break;
                            }

                        default:
                            break;

                    }
                }

            }

////////////// -- PLACE TO BROADCAST TO ALL PLAYERS -- ///////////////////////////////////////////////////////////////

            if (numClientsPass > 1 &&  numClientsPass > currentNumPlayers ) //&& !sentStartGame
            {


                if (addAfterStart)
                {
                    // TESTING (since still don't have movement sent over):
                    // currentClientsMove = ""

                    for (int l = 0; l < clientsEntered.Count; ++l)
                    {
                        //Client tC = clientsEntered[l];

                        if ((newClientaddedNum.Find(x => x == clientsEntered[l].clientNumber)) != 0 )
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

            if (currentClientsMove != "")
            {
                // send everyone all movements logged in currentClientsMove 
                
                
                currentClientsMove = "";
            }

            if (addedNewPellets != "")
            {
              for (int l = 0; l < clientsEntered.Count; ++l)
              {
                 
                  clientsEntered[l].sw.WriteLine("newPell{0}", addedNewPellets);
                  
              }
              addedNewPellets = "";
            }


            //newClientaddedNum.Clear();
            //currentClientsMove = "";
            //newClientsInfo = "";
            
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
       // Client tempClient= new Client();
        //int clientIndex = -9;
        int cliIndex = -999;

        lock (thisLock)
        {
         // tempClient = clientsEntered.Find(x => x.TCPclient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint);
         // clientIndex = clientsEntered.IndexOf(tempClient);

             cliIndex = clientsEntered.FindIndex(x => x.TCPclient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint);
        }

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
            if (data[0] == "userAndPass")
            {
                gamedata.action = data[0];    // ???? might need to create new gamedata for every entry 
                gamedata.userName = data[1];
                gamedata.password = data[2];
                
            }
            if(data[0] == "hit" || data[0] == "move" )
            {
                gamedata.action = data[0];
                gamedata.clientNum = Convert.ToInt32(data[1]); // pell or other client
                gamedata.posX = Convert.ToDouble(data[2]);
                gamedata.posY = Convert.ToDouble(data[3]);
                //gamedata.timeStamp = Convert.ToDateTime(data[4]);
                
            }
            if (data[0] == "score")
            {
                gamedata.action = data[0];
                gamedata.userName = data[1];    //User Name
                gamedata.score = Convert.ToInt32(data[2]); //Score
            }

            lock (thisLock)
            {
                //tempClient.clientQueue.Enqueue(gamedata);
                //clientsEntered[clientIndex] = tempClient;

                clientsEntered[cliIndex].clientQueue.Enqueue(gamedata);
                
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

    public int lastProcessRange;

    List<playInfo> resolvedPellets;

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
        //initPelletLocation = "";  //  "iniP\\";
        addedNewPellets = "";

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
        while (true)
        {
         
            if (!startedGame) //(client1Start && client2Start)
            {
                
                for(int i=0; i < numberOfpellets; ++i)
                {
                  pellet p = new pellet();
                  p.px = NextRanX();//randPellets.Next(-23,23);
                  p.py = NextRanY();//randPellets.Next(-12, 14);
                  p.pellNum = i + 1;

                  gamePellets.Add(p);
                    
                   // string tempstring = string.Format("\\{0}\\{1}",p.px,p.py);
                   // initPelletLocation= String.Concat(initPelletLocation,tempstring); 
                } 
               
                startedGame = true;
               
               
                physTimer.Start(); 

            }

            if (physTimer.Elapsed.Milliseconds >= 20) 
            {

                if (compareGamePlay.Count != 0)
                {

                    //IEnumerable
                  
                //  List<playInfo> orderTempList = (List<playInfo>)compareGamePlay.OrderBy(x => x.timeStamp.Millisecond);
                    
                    // if ((newClientaddedNum.Find(x => x == clientsEntered[l].clientNumber)) != 0 )

                        
                        //List<playInfo> tempList = compareGamePlay.FindAll((x => x.objectNum == i+1) );

                     
                       // ?? wait to see if another "hit" is on it's way.
                      for (int i = 0; i < compareGamePlay.Count; ++i)
                      { 
                                                                                //orderTempList[i]
                         //Client c =  clientsEntered.Find(x => x.clientNumber == compareGamePlay[i].clientsNum);  

                          int indexC = clientsEntered.FindIndex(x => x.clientNumber == compareGamePlay[i].clientsNum);

                          //c
                          if (clientsEntered[indexC].clientNumber != 0)
                          {
                                                               //orderTempList[i] <--> all compareGamePlay[i]
                            //c <--> clientsEntered[indexC]
                              if ((clientsEntered[indexC].posX + (clientsEntered[indexC].playerSize / 2) >= compareGamePlay[i].compareX - .5) || (clientsEntered[indexC].posX - (clientsEntered[indexC].playerSize / 2) <= compareGamePlay[i].compareX + .5)
                                || (clientsEntered[indexC].posY + (clientsEntered[indexC].playerSize / 2) >= compareGamePlay[i].compareY - .5) || (clientsEntered[indexC].posY - (clientsEntered[indexC].playerSize / 2) <= compareGamePlay[i].compareY + .5)
                                && !resolvedPellets.Exists(x => (x.objectNum == compareGamePlay[i].objectNum) 
                                    && (x.compareX == compareGamePlay[i].compareX) && (x.compareY == compareGamePlay[i].compareY)))
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

                                 // clientsEntered[indexC].sw.WriteLine("getPell\\{0}\\{1}", (clientsEntered[indexC].playerSize + growSize), clientsEntered[indexC].playerSpeed);
                                      //, (((dt.AddMinutes(uniClock.Elapsed.Minutes).AddSeconds(uniClock.Elapsed.Seconds).AddMilliseconds(uniClock.Elapsed.Milliseconds)).Ticks))


                                  clientsEntered[indexC].playerSize = clientsEntered[indexC].playerSize + growSize;
                                                                                    //orderTempList[i]
                                  //pellet tempPel = gamePellets.Find(x => x.pellNum == compareGamePlay[i].objectNum);
                                  //int indexPel = gamePellets.IndexOf(tempPel);

                                int indPell = gamePellets.FindIndex(x => x.pellNum == compareGamePlay[i].objectNum);
                                  
                                  //tempPel
                                gamePellets[indPell].px = NextRanX();
                                gamePellets[indPell].py = NextRanY();
                                                   //orderTempList
                               // gamePellets[indPell].pellNum = compareGamePlay[i].objectNum;
                                  //gamePellets[indexPel] = tempPel;

                                 // int clientIndex = clientsEntered.IndexOf(c);
                                  //clientsEntered[clientIndex] = c;

                                lock(thisLock)
                                {
                                  addedNewPellets = string.Concat(addedNewPellets, string.Format("\\{0}\\{1}\\{2}\\{3}\\{3}\\{5}",
                                    clientsEntered[indexC].clientNumber, (clientsEntered[indexC].playerSize),  clientsEntered[indexC].playerSpeed,
                                    gamePellets[indPell].pellNum, gamePellets[indPell].px, gamePellets[indPell].py));
                                                        
                                }  
                                  playInfo addToResolve = new playInfo();
                                  addToResolve.objectNum = compareGamePlay[i].objectNum;
                                  addToResolve.compareX = compareGamePlay[i].compareX;
                                  addToResolve.compareY = compareGamePlay[i].compareY;
                                                        //orderTempList[i]
                                  resolvedPellets.Add(addToResolve);

                              }
                              else
                              {
                                 // clientsEntered[indexC].sw.WriteLine("missPell"); //+ "\\"
                                      //+ (((dt.AddMinutes(uniClock.Elapsed.Minutes).AddSeconds(uniClock.Elapsed.Seconds).AddMilliseconds(uniClock.Elapsed.Milliseconds)).Ticks)) );
                              }
                          }


                          lastProcessRange = compareGamePlay.Count;
                      }
                      compareGamePlay.RemoveRange(0, lastProcessRange);
                      

                }
                    resolvedPellets.Clear();

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
