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
	private Sockets socks;
	private StreamReader sr;
	private System.Object thisLock = new System.Object();
	
	//********* COMPLETE THE FOLLOWING CODE
	public ThreadSock (NetworkStream nwsIn, Sockets inSocket )
	{
		//?? nws = new NetworkStream(; //nwsIn;
		nws = nwsIn;
		
		//?? socks = new Sockets();
		socks = inSocket;
		
		sr = new StreamReader(nws);
		
		
	}
	public void Service ()
	{	
		//lock inside while loop
		try
		{
			lock( thisLock)
			{
				
				// Create an instance of StreamReader to read from a file. 
				// The using statement also closes the StreamReader. 
				//using (socks.sr) 
				//{
				// while(true)
				//{
				string line;
				// Read and display lines from the file until the end of  
				// the file is reached. 
				while ((line = sr.ReadLine()) != null) 
				{
					
					print("Received this <------ " + line);
					
					socks.recvBuffer.Enqueue(line);
					
				}
				
			}
			
			
			//while(true)
			//{
			/*
				if((byteBuffer = (byte)nws.ReadByte()) != -1 )
				{
					lock(socks.recvBuffer)
					{

						socks.recvBuffer.Enqueue(byteBuffer);
					}
				}
              */
			//	}
			
		}
		catch ( Exception ex )
		{
			print ( ex.Message + " : ThreadSocket loop" );
			
		}
		
	}
	
}
