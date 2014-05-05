﻿//SWARCH


using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

public class GameProcess : MonoBehaviour {
	
	//PUBLIC MEMBERS
	public int clientNumber;

	public bool startGame;
	public bool startNextRound;
	public bool hitGoal;
	public int winningMove ;
	
	private Sockets socks;
	
	private byte byteBuffer;
	private byte tempBuffer;
	private string data;
	public string[] splitData;
	public char delemeter;

	public Transform pell;
	public ArrayList pellets;

	public int numOfPlayer;
	public DateTime t1; 
	public DateTime t4;
	public DateTime tServer;
	public bool canSendStart;
	public double totalLat;

	public bool loadPellets;

	void Start () 
	{
		socks = new Sockets();

		pellets = new ArrayList();
		data = "";
		loadPellets = false;


		startGame = false;
		startNextRound = false;
		hitGoal = false;
		splitData = new string[]{"",""};

		delemeter = ('\\');
		
		winningMove = 0;
		t1 = new DateTime();
		t4 = new DateTime();
		tServer = new DateTime();
		canSendStart = false;
		totalLat = 0;
		//t1 = DateTime.UtcNow; 
		
		
	}

	void OnGUI()
	{
		if ( GUI.Button( new Rect( 0, 0, 100, 20), "Disconnect"))
		{
			//********* COMPLETE THE FOLLOWING CODE
			//********* KILL THREAD AND SEVER CONNECTION
			
			//returnSocket().SendTCPPacket ((byte) (process.commands[(int)GameProcess.codes.roll]));
			
			//process.sendEndGame();
			
			socks.endThread();
			socks.Disconnect();
			print("\nDISCONNECTED ");
		}

	}

	void Update () 
	{

	  if(loadPellets)
	  {
		for(int i =0; i < 5; ++i)
		{
			//pell;
			Transform tempPell = Instantiate(pell, new Vector3(UnityEngine.Random.Range(-23.5F, 23.5F), 
			                                                   0, UnityEngine.Random.Range(-12.5F, 14.5F)), Quaternion.identity) as Transform;
			
			//temp.name = (i.ToString());
			//print (temp.name);
			
			pellets.Add(tempPell ); 
			
				loadPellets = false;
			
		}
	        
			//FindObjectOfType(typeof (YourObject) )
	  }
		


		//lock(socks.recvBuffer)
		//{
	///*	
		if(socks.recvBuffer.Count > 0)
		{
			
			//data = (string)socks.recvBuffer.Dequeue();
			
			splitData = data.Split(delemeter);
			
			if(splitData[0] == "canSendStart")
			{
				canSendStart = true;
			}
			
			if(splitData[0] == "playNum")
			{
				//setPlayerPaddle(Convert.ToInt32(splitData[1]));
			}
			
			if(splitData[0] == "start") //from server
			{
				startGame = true;

				
			}

			
		}
	 // */	
	}
	public Sockets returnSocket()
	{
		return socks;
	}
	
	
	
	public void send(string toSend )
	{
		
		//socks.SendTCPPacket(toSend);
		
	}
	
	
	public void sendEndGame ()
	{


	}
	


	
	
	
	
}
