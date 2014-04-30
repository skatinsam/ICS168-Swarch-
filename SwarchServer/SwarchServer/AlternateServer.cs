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


public class PongServer
{
    static TcpListener listener;

    protected static int ballVelocityX;
    protected static int ballVelocityY;

    protected static int ballPositionX;
    protected static int ballPositionY;

    protected static float leftPaddlePosition;
    protected static float rightPaddlePosition;

    int connectedPlayers = 0;
    IPAddress localAddr = IPAddress.Parse("128.195.11.131");
    const int serverPort = 4502;

    static NetworkStream stream1;
    static NetworkStream stream2;

    PaddleCommunication leftPaddle;
    PaddleCommunication rightPaddle;

    private bool start1 = false;
    private bool start2 = false;

    Queue commandQueue;
    private static object theLock = new object();

    public static void Main()
    {
        PongServer pong = new PongServer();
    }

    public PongServer()
    {
        listener = new System.Net.Sockets.TcpListener(serverPort);
        BallCalculation ball = new BallCalculation();
        Thread ballComs = new Thread(new ThreadStart(ball.threadRun));
        //ballComs.Start();
        
        while (true)
        {
            listener.Start();

            if (connectedPlayers < 2)
            {
                //Connect to Stream
                TcpClient connection = listener.AcceptTcpClient();
                NetworkStream s = connection.GetStream();

                //Get initial packet information
                /*byte[] initial = new byte[128];
                int size = s.Read(initial, 0, initial.Length);
                Array.Resize(ref initial, size);
                string c = System.Text.Encoding.ASCII.GetString(initial);
                string[] commandLine = c.Split('\\');
                string command = commandLine[0];*/

                //Set info with initial packet information
                int x = 4;
                float f = (float)x;

                //Increment Players
                connectedPlayers++;

                //Set paddle of player
                if (connectedPlayers == 1)
                {
                    Console.WriteLine("Connected: {0} as player {1}", connection.Client.RemoteEndPoint, connectedPlayers);

                    string connect = "c\\1";
                    byte[] send = System.Text.Encoding.ASCII.GetBytes(connect);
                    s.Write(send, 0, send.Length);
                    leftPaddle = new PaddleCommunication(connection, f, 1);
                    stream1 = s;
                }
                if (connectedPlayers == 2)
                {
                    Console.WriteLine("Connected: {0} as player {1}", connection.Client.RemoteEndPoint, connectedPlayers);
                    rightPaddle = new PaddleCommunication(connection, f, 2);
                    string connect = "c\\2";
                    byte[] send = System.Text.Encoding.ASCII.GetBytes(connect);
                    s.Write(send, 0, send.Length);
                    stream2 = s;
                }
            }
            else
            {
                //if start is false wait til both are ready

                //else we are good to go
                leftPaddlePosition = leftPaddle.getPaddlePosition();
                rightPaddlePosition = rightPaddle.getPaddlePosition();
                Console.WriteLine("Left Paddle Position is {0}", leftPaddlePosition);
                Console.WriteLine("Right Paddle Position is {0}", rightPaddlePosition);
            }
        }
    }

    public class PaddleCommunication
    {
        TcpClient client;
        float paddlePosition;
        int clientNumber;
        Thread thread;

        public PaddleCommunication(TcpClient theClient, float startPosition, int number)
        {
            this.client = theClient;
            this.paddlePosition = startPosition;
            this.clientNumber = number;
            thread = new Thread(threadRun);
            thread.Start();
        }

        public void beginGame()
        {

        }

        //Thread to run
        public void threadRun()
        {
            try
            {
                //read information
                byte[] receiveMessage = new byte[128];
                if (client.GetStream().CanRead)
                {
                    int size = client.GetStream().Read(receiveMessage, 0, receiveMessage.Length);
                    Array.Resize(ref receiveMessage, size);
                    string c = System.Text.Encoding.ASCII.GetString(receiveMessage);
                    string[] commandLine = c.Split('\\');
                    string clientd = commandLine[1];
                    lock (theLock)
                    {
                        if (clientd.Equals("1"))
                        {
                            stream2.Write(receiveMessage, 0, receiveMessage.Length);
                            stream2.Flush();
                        }
                        else if (clientd.Equals("2"))
                        {
                            stream1.Write(receiveMessage, 0, receiveMessage.Length);
                            stream1.Flush();
                        }
                        paddlePosition = (float)Int16.Parse(commandLine[2]);
                    }
                }
            }
            catch { Console.WriteLine("Something went Wrong"); }
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
        //Thread to run
        public BallCalculation()
        {
        }

        public void threadRun()
        {
            
        }
    }
};