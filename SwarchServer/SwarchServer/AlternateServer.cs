using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Data.SQLite;


public class AlternateServer
{
    static float ballVelocityX;
    static float ballVelocityY;

    static float ballPositionX;
    static float ballPositionY;

    protected static float leftPaddlePosition;
    protected static float rightPaddlePosition;
    static float leftPaddleX = -10.37f;
    static float rightPaddleX = 10.37f;

    int connectedPlayers = 0;

    IPAddress localAddr = IPAddress.Parse("128.195.11.131");
    Int32 serverPort = 4645;

    static StreamReader streamRead1;
    static StreamReader streamRead2;
    static StreamReader streamRead3;
    static StreamReader streamRead4;
    static StreamWriter streamWrite1;
    static StreamWriter streamWrite2;
    static StreamWriter streamWrite3;
    static StreamWriter streamWrite4;
    static StreamReader[] sr;
    static StreamWriter[] sw;

    private static Stopwatch uniClock;
    private static DateTime dt;
    static Stopwatch ballTimer;

    PlayerCommunication playerOne;
    PlayerCommunication playerTwo;
    PlayerCommunication playerThree;
    PlayerCommunication playerFour;

    private int i;

    private bool start1 = false;
    private bool start2 = false;
    static bool hit = false;
    static bool pointScored = false;

    static int score1 = 0;
    static int score2 = 0;

    static float upBound = 7.5f;
    static float leftBound = -11.5f;
    static float downBound = -7.5f;
    static float rightBound = 11.5f;

    public static bool getOut;

    Queue commandQueue;
    private static object theLock = new object();

    public AlternateServer()
    {

        TcpListener server = new TcpListener(localAddr, serverPort);

        server.Start();
        ballTimer = new Stopwatch();
        BallCalculation ballc = new BallCalculation();
        Thread ballComs = new Thread(new ThreadStart(ballc.threadRun));
        //ballVelocityX = 0.2f;
        //ballVelocityY = 0.2f;

        //ballPositionX = 0.0f;
        //ballPositionY = 0.0f;
        
        
        while (!getOut)
        {
            if (server.Pending() && connectedPlayers < 4)
            {
                Console.WriteLine("Waiting for a connection... ");
                //Connect to Stream
                TcpClient connection = server.AcceptTcpClient();

                //Set info with initial packet information
                int x = 0;
                float f = (float)x;

                //Increment Players
                connectedPlayers++;

                //Set paddle of player
                if (connectedPlayers == 1)
                {

                    streamRead1 = new StreamReader(connection.GetStream());
                    streamWrite1 = new StreamWriter(connection.GetStream());
                    string connect = "c\\1";
                    streamWrite1.WriteLine(connect);
                    streamWrite1.Flush();
                    Console.WriteLine("Connected: {0} as player {1}", connection.Client.RemoteEndPoint, connectedPlayers);
                    playerOne = new PlayerCommunication(f, 1);

                    //sr[connectedPlayers] = streamRead1;
                }
                if (connectedPlayers == 2)
                {

                    streamRead2 = new StreamReader(connection.GetStream());
                    streamWrite2 = new StreamWriter(connection.GetStream());
                    playerTwo = new PlayerCommunication(f, 2);
                    string connect = "c\\2";
                    streamWrite2.WriteLine(connect);
                    streamWrite2.Flush();
                    Console.WriteLine("Connected: {0} as player {1}", connection.Client.RemoteEndPoint, connectedPlayers);

                    playerOne.setPaddleStreams(streamRead1, streamWrite2);
                    playerTwo.setPaddleStreams(streamRead2, streamWrite1);
                }
                if (connectedPlayers == 3)
                {

                    streamRead3 = new StreamReader(connection.GetStream());
                    streamWrite3 = new StreamWriter(connection.GetStream());
                    playerThree = new PlayerCommunication(f, 3);
                    string connect = "c\\3";
                    streamWrite3.WriteLine(connect);
                    streamWrite3.Flush();
                    Console.WriteLine("Connected: {0} as player {1}", connection.Client.RemoteEndPoint, connectedPlayers);

                    //leftPaddle.setPaddleStreams(streamRead1, streamWrite2);
                    //rightPaddle.setPaddleStreams(streamRead2, streamWrite1);
                }
                if (connectedPlayers == 4)
                {

                    streamRead4 = new StreamReader(connection.GetStream());
                    streamWrite4 = new StreamWriter(connection.GetStream());
                    playerFour = new PlayerCommunication(f, 2);
                    string connect = "c\\4";
                    streamWrite4.WriteLine(connect);
                    streamWrite4.Flush();
                    Console.WriteLine("Connected: {0} as player {1}", connection.Client.RemoteEndPoint, connectedPlayers);

                    //leftPaddle.setPaddleStreams(streamRead1, streamWrite2);
                    //rightPaddle.setPaddleStreams(streamRead2, streamWrite1);
                }
            }

            //Find a way to convert this over to something else
            ballComs.Start();

            string ball = "b\\" + ballPositionX.ToString() + "\\" + ballPositionY.ToString() + "\\" + ballVelocityX.ToString() + "\\" + ballVelocityY.ToString();
            Console.WriteLine(ball);
            streamWrite1.WriteLine(ball);
            streamWrite1.Flush();
            streamWrite2.WriteLine(ball);
            streamWrite2.Flush();
            Console.WriteLine("Ball Velocity: {0}, {1} Ball Position: {2}, {3}", ballVelocityX, ballVelocityY, ballPositionX, ballPositionY);

            
            leftPaddlePosition = playerOne.getPaddlePosition();
            rightPaddlePosition = playerTwo.getPaddlePosition();

                /*if (ballTimer.ElapsedMilliseconds >= 3000)
                {
                    string ball = "b\\" + 0 + "\\" + 0 + "\\" + -0.2f + "\\" + -0.2f;
                    streamWrite1.WriteLine(ball);
                    streamWrite1.Flush();
                    streamWrite2.WriteLine(ball);
                    streamWrite2.Flush();
                    ballTimer.Reset();

                }*/

            //Hit is not pertinent yet
            if (hit)
            {
                string ballw = "b\\" + ballPositionX.ToString() + "\\" + ballPositionY.ToString() + "\\" + ballVelocityX.ToString() + "\\" + ballVelocityY.ToString();
                streamWrite1.WriteLine(ballw);
                streamWrite1.Flush();
                streamWrite2.WriteLine(ballw);
                streamWrite2.Flush();   
                hit = false;

                    if (pointScored)
                    {
                        string p = "s\\" + score1 + "\\" + score2;
                        pointScored = false;
                        streamWrite2.WriteLine(p);
                        streamWrite2.Flush();
                        streamWrite1.WriteLine(p);
                        streamWrite1.Flush();
                    }
                }

                if (score1 == 10)
                {
                    string disconnect = "d\\1";
                    streamWrite1.WriteLine(disconnect);
                    streamWrite1.Flush();
                    streamWrite2.WriteLine(disconnect);
                    streamWrite2.Flush();
                }
                else if (score2 == 10)
                {
                    string disconnect = "d\\2";
                    streamWrite1.WriteLine(disconnect);
                    streamWrite1.Flush();
                    streamWrite2.WriteLine(disconnect);
                    streamWrite2.Flush();
                }
            }
        }



    public class PlayerCommunication
    {
        float paddlePosition;
        int clientNumber;
        Thread thread;
        StreamReader paddleReader;
        StreamWriter otherPaddleWriter;

        public PlayerCommunication(float startPosition, int number)
        {
            this.paddlePosition = startPosition;
            this.clientNumber = number;
            thread = new Thread(threadRun);
            thread.IsBackground = true;
        }

        public void beginGame()
        {

        }
        public void startThread()
        {
            thread.Start();
        }

        public void setPaddleStreams(StreamReader p, StreamWriter o)
        {
            paddleReader = p;
            otherPaddleWriter = o;
        }

        //Thread to run
        public void threadRun()
        {
            while (true)
            {
                try
                {
                    //read information

                    string receiveMessage = paddleReader.ReadLine();
                    string[] commandLine = receiveMessage.Split('\\');
                    lock (theLock)
                    {
                        otherPaddleWriter.WriteLine(receiveMessage);
                        otherPaddleWriter.Flush();
                        paddlePosition = Single.Parse(commandLine[1]);
                        Console.WriteLine("Paddle Number " + clientNumber + ": " + paddlePosition);
                    }
                }
                catch
                {
                    Console.WriteLine("Something went Wrong");
                    lock (theLock)
                    {
                        getOut = true;
                    }
                }
            }
        }

        private float calculateLeftPaddle(float leftPaddlePosition)
        {
            Console.WriteLine("In Calculate Left Paddle");
            return 0;
        }
        
        public float getPaddlePosition()
        {
            return paddlePosition;
        }
    }

    public class BallCalculation
    {
        private int i;
        //Thread to run
        public BallCalculation()
        {
            i = 0;

        }

        public void threadRun()
        {
            while (true)
            {
                try
                {
                    lock (theLock)
                    {
                       if(ballTimer.ElapsedMilliseconds > 20)
                       {
                            if (ballPositionY > upBound)
                            {
                                ballVelocityY = -ballVelocityY;
                                Console.WriteLine("Ball Velocity: {0}, {1} Ball Position: {2}, {3}", ballVelocityX, ballVelocityY, ballPositionX, ballPositionY);
                                Console.WriteLine("Hit Top");
                                hit = true;
                            }
                            else if (ballPositionY < downBound)
                            {
                                ballVelocityY = -ballVelocityY;
                                Console.WriteLine("Ball Velocity: {0}, {1} Ball Position: {2}, {3}", ballVelocityX, ballVelocityY, ballPositionX, ballPositionY);
                                Console.WriteLine("Hit bottom");
                                hit = true;
                            }
                            else if (ballPositionX < leftBound)
                            {
                                ballVelocityX = -ballVelocityX;
                                Console.WriteLine("Ball Velocity: {0}, {1} Ball Position: {2}, {3}", ballVelocityX, ballVelocityY, ballPositionX, ballPositionY);
                                Console.WriteLine("Hit Left");
                                score2++;
                                hit = true;
                                pointScored = true;
                            }
                            else if (ballPositionX > rightBound)
                            {
                                ballVelocityX = -ballVelocityX;
                                Console.WriteLine("Ball Velocity: {0}, {1} Ball Position: {2}, {3}", ballVelocityX, ballVelocityY, ballPositionX, ballPositionY);
                                Console.WriteLine("Hit Right");
                                score1++;
                                hit = true;
                                pointScored = true;
                            }
                            //paddle x
                            else if (ballPositionX < leftPaddleX)
                            {
                                if (ballPositionY < (leftPaddlePosition + 1) && ballPositionY > (leftPaddlePosition - 1))
                                {
                                    ballVelocityX = -ballVelocityX;
                                    ballVelocityY = -ballVelocityY;
                                    Console.WriteLine("Ball Velocity: {0}, {1} Ball Position: {2}, {3}", ballVelocityX, ballVelocityY, ballPositionX, ballPositionY);
                                    Console.WriteLine("Hit Left Paddle");
                                    hit = true;
                                }
                            }
                            else if (ballPositionX > rightPaddleX)
                            {
                                if (ballPositionY < (rightPaddlePosition + 1) && ballPositionY > (rightPaddlePosition - 1))
                                {
                                    ballVelocityX = -ballVelocityX;
                                    ballVelocityY = -ballVelocityY;
                                    Console.WriteLine("Ball Velocity: {0}, {1} Ball Position: {2}, {3}", ballVelocityX, ballVelocityY, ballPositionX, ballPositionY);
                                    Console.WriteLine("Hit Right Paddle");
                                    hit = true;
                                }
                            }
                            ballPositionX += ballVelocityX;
                            ballPositionY += ballVelocityY;
                            ballTimer.Restart();
                        }
                    }
                }
                catch { Console.WriteLine("Something Went Wrong in Ball Thread"); }
            }
        }
    }

    public static void setBallPosition(float x, float y)
    {
        ballPositionX = x;
        ballPositionY = y;
    }
    public static void setBallVelocity(float x, float y)
    {
        ballVelocityX = x;
        ballVelocityY = y;
    }
};