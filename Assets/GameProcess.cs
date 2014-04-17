//SWARCH


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
	
	//public Sockets socks;
	
	private byte byteBuffer;
	private byte tempBuffer;
	private GuiSwarch guiSwarch;
	private string data;
	public string[] splitData;
	private char delemeter;
	
	public int numOfPlayer;
	public DateTime t1; 
	public DateTime t4;
	public DateTime tServer;
	public bool canSendStart;
	public double totalLat;
	
	void Start () 
	{
		
		//socks = new Sockets();
		
		startGame = false;
		startNextRound = false;
		hitGoal = false;
		splitData = new string[]{"",""};
		
		guiSwarch = GameObject.Find("GuiSwarch").GetComponent<GuiSwarch>();

		delemeter = ('\\');
		
		winningMove = 0;
		t1 = new DateTime();
		t4 = new DateTime();
		tServer = new DateTime();
		canSendStart = false;
		totalLat = 0;
		//t1 = DateTime.UtcNow;  // ******* WHERE TO PUT THIS , timestamp
		
		
	}
	
	void Update () 
	{
		//lock(socks.recvBuffer)
		//{
	/*	
		if(true)//socks.recvBuffer.Count > 0)
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
	  */	
	}
	//public Sockets returnSocket()
	//{
	//	return socks;
	//}
	
	
	
	public void send(string toSend )
	{
		
		//socks.SendTCPPacket(toSend);
		
	}
	
	
	public void sendEndGame ()
	{


	}
	

	
	public void printGui( string printStr )
	{
		this.guiSwarch.printGui(printStr );
	}
	
	
	
	
}
