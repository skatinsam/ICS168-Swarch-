//SWARCH

using UnityEngine;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;
using System.Diagnostics;
using System.Threading;

public class Sockets : MonoBehaviour {
	
	
	//const string SERVER_LOCATION =   "128.195.77.35";  
	
	const string SERVER_LOCATION =    "128.195.11.129";  //<-- REMOTE CONNECTION
	
	const int SERVER_PORT = 4040; //YOUR PORT NUMBER; //FILL THESE OUT FOR YOUR OWN SERVER
	
	public TcpClient client; 
	
	public NetworkStream nws;
	public int clientNumber;
	public bool startGame ;
	public bool connected;
	public StreamReader sr; 
	public StreamWriter sw;
	
	public DateTime dt;
	
	public Thread t = null; 
	public Thread ts =null;
	
	protected static bool threadState = false;
	
	public Queue recvBuffer;
	public Queue sendQueue;
	
	public Sockets()
	{
		
		connected = false;
		recvBuffer = new Queue();
		sendQueue = new Queue();
		//uniClock = new Stopwatch();
		//dt = NTPTime.getNTPTime ( dt, ref uniClock );
		
	}
	
	public bool Connect ()
	{
		
		try
		{    	
			client = new TcpClient(SERVER_LOCATION, SERVER_PORT ); 
			
			//?? nws = new NetworkStream; //??
			
			
			if(client.Connected)
			{
				nws = client.GetStream();
				sr = new StreamReader(nws);
				sw = new StreamWriter(nws);
				sw.AutoFlush = true;
				connected = true;
				
				ThreadSock tSock = new ThreadSock(nws, this);
				t = new Thread(new ThreadStart(tSock.Service));
				t.IsBackground = true;
				
				t.Start();
				
				
				SendThread tSend = new SendThread(nws, this);
				ts = new Thread(new ThreadStart(tSend.Service));
				ts.IsBackground = true;
				
				ts.Start();
				
				threadState = true;
				
			}
			
			
		}
		catch ( Exception ex )
		{
			print ( ex.Message + " : OnConnect");
			
		}
		
		if ( client == null ) return false;
		return client.Connected;
	}
	
	public bool Disconnect ()
	{
		//!! //********* COMPLETE THE FOLLOWING CODE
		
		try
		{
			
			if (!connected)
			{
				return false;
			}
			
			sr.Close();
			sw.Close();
			client.Close();
			connected = false;
			
			
			
			
		}
		catch ( Exception ex )
		{
			print ( ex.Message + " : OnDisconnect" );
			return false;
			
		}
		return true;
	}
	
	public void SendTCPPacket(string toSend) //( byte toSend )
	{
		//??  //********* COMPLETE THE FOLLOWING CODE
		try
		{ 
			
			//nws.WriteByte(toSend);
			sw.WriteLine (toSend);
			//sw.Flush();
			
		}
		catch ( Exception ex )
		{
			print ( ex.Message + ": OnTCPPacket" );
		}	
	}
	
	public void measureLatency () //UN-NECESSARY
	{
	}
	
	public int returnLatency(){//UN-NECESSARY
		//return latency;
		return 0;
	}
	
	
	public void endThread(){
		threadState = false;
	}
	
	public void testThread()
	{
		
		try
		{
			if ( t!= null && !threadState  )
			{
				print ( "thread aborted");
				t.Abort();
				threadState = !threadState;	
			}
		}
		catch ( Exception ex )
		{
			print ( ex.Message + " : testThread ");
		}
	}
	
}
