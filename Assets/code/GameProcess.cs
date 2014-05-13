/* Things to do
 * 
 * Multiuser. The goal of this phase of the project is to coordinate the game 
between 2 or more game clients. Your game should have player collision 
detection, ensure that players win/lose when appropriate, prevent 
inconsistencies (clients reaching the same pallet at the same time), and avoid the 
bugs that you studied in Milestone 1.
 * 
 * More and better. The goal of this phase of the project is to make your multiuser 
game more solid and demoable to outsiders. Here is the list of things you need 
to do in this last phase: 
(a) If there were network-related bugs in M4, you need to fix them. 
(b) Add a score board that shows the clients currently connected along with 
their scores. The score is computed as follows: 
Get a pallet: +1 point 
Get another player: +10 points 
(c) Detect client disconnections, and update the score board accordingly 
(i.e. delete that user from the score board) 
 Parts to be done:
 * Collision detection (Collision with wall handled on client side, the rest on server)
 * Win/lose condition (implement scores/points for pellet and players, implement scoreboard if extra time)
 * Client prediction if necessary
 * TIME WARPP
 * Disconnections
*/



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
	
	private Sockets socks;
	
	private byte byteBuffer;
	private byte tempBuffer;
	private string data;
	public string[] splitData;
	public char delemeter;

	public Transform pell;
	public ArrayList pellets;
	public List<float> pelletsLocation;
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
		pelletsLocation = new List<float>();
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

		
	}

	void OnGUI()
	{
		if ( GUI.Button( new Rect( 0, 50, 100, 20), "Disconnect"))
		{
			
			socks.endThread();
			socks.Disconnect();
			print("\nDISCONNECTED ");
		}

	}

	void Update () 
	{

	  if(loadPellets)
	  {
		for(int i =0; i < 10; i = i+2)
		{
			

			//Transform tempPell = Instantiate(pell, new Vector3(UnityEngine.Random.Range(-23.5F, 23.5F), 
			//                                                   0, UnityEngine.Random.Range(-12.5F, 14.5F)), Quaternion.identity) as Transform;

			

				Transform tempPell = Instantiate(pell, new Vector3(pelletsLocation[i], 
				                        0,  pelletsLocation[i+1]), Quaternion.identity) as Transform;

			pellets.Add(tempPell ); 
			

			
		}
			loadPellets = false;    
	  }
		
	
		if(socks.recvBuffer.Count > 0)
		{

			
			splitData = data.Split(delemeter);
			
			if(splitData[0] == "canSendStart")
			{
				canSendStart = true;
			}
			
			if(splitData[0] == "playNum")
			{
				//setPlayerPaddle(Convert.ToInt32(splitData[1]));
			}
			
			if(splitData[0] == "start") 
			{
				startGame = true;

				
			}

			
		}
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
