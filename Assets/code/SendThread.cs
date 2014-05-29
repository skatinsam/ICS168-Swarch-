// SWARCH

using UnityEngine;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

public class SendThread : MonoBehaviour 
{
	private NetworkStream nws;
	private byte[] streamBuffer;
	private byte byteBuffer;
	private byte tempBuffer;
	private Sockets socks;
	private StreamWriter sw;
	
	private System.Object thisLock = new System.Object();
	
	public SendThread (NetworkStream nwsIn, Sockets inSocket )
	{
		nws = nwsIn;
		
		socks = inSocket;
		sw = new StreamWriter(nws);
		sw.AutoFlush = true;
		
		
		
	}
	void Start()
	{
		
	}
	public void Service()
	{
		///*
		while(socks.connected)
		{ 

				
				try
				{ 
					
					
					while(socks.sendQueue.Count > 0)
					{
					string sendout= "";
					lock(thisLock)
					{			
						sendout = (string)socks.sendQueue.Dequeue();
					}	
						//if(sendout)
						//sendoutSubstring(1,3); 
						// if( starts with "start" )
						//Thread.Sleep(100);

						sw.WriteLine(sendout); //(toSend);

						if(!sendout.Contains("move"))
						print ("\nSENT OUT(thread) ------>  " + sendout);
						
					}	
				}
				catch ( Exception ex )
				{
					print ( ex.Message + ": On SEND OUT (thread)" );
				}	
			
		}
		//*/
	}
	
	
	
}