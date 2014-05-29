// SWARCH

using UnityEngine;
using System;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;

public class ThreadSock : MonoBehaviour 
{
	private NetworkStream nws;
	private byte[] streamBuffer;
	private byte byteBuffer;
	private byte tempBuffer;
	private Sockets socks = new Sockets();
	private StreamReader sr;
	private System.Object thisLock = new System.Object();



	public ThreadSock (NetworkStream nwsIn, Sockets inSocket )
	{

		nws = nwsIn;

		socks = inSocket;
		
		sr = new StreamReader(nws);
		
		
	}
	public void Service ()
	{	
		try
		{


				string line;
			
				while ((line = sr.ReadLine()) != null) 
				{
					
			
				    lock( thisLock)
				    {
					    if(!line.Contains("move"))
						print("Received this <------ " + line);
					 socks.recvBuffer.Enqueue(line);
					}
				}
				


			
		}
		catch ( Exception ex )
		{
			print ( ex.Message + " : ThreadSocket loop" );
			
		}
		
	}
	
}
