//SWARCH


using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

public class GameProcess : MonoBehaviour 
{
	
	//PUBLIC MEMBERS
	//public int clientNumber;

	public bool startedGame;
	public bool startNextRound;
	public bool hitGoal;
	public int winningMove;

	public int mainPlayerNumber;

	private Sockets socks;
	
	private byte byteBuffer;
	private byte tempBuffer;
	private string data;
	public string[] splitData;
	public char delemeter;

	public Transform pell;
	public Transform opponents;

	//public ArrayList 
	public List<Pellets> pellets;
	public List<float> pelletsLocation;
	public int numOfPlayer;
	public DateTime t1; 
	public DateTime t4;
	public DateTime tServer;
	public bool canSendStart;
	public double totalLat;

	public Player player;
	public Vector3 initPlayerPos;

	public Stopwatch uniClock; // = new Stopwatch();
	public DateTime dt; // = new DateTime();
	
	//public int startOpponents;

	public string[] initOpponentData;

	public GUIText guiTGP;

	public bool loadPellets;

	void Start () 
	{
		socks = new Sockets();

		pellets = new List<Pellets>();//new ArrayList();
		pelletsLocation = new List<float>();
		data = "";
		loadPellets = false;
		 
		mainPlayerNumber= 0;

		startedGame = false;
		startNextRound = false;
		hitGoal = false;
		splitData = new string[]{"",""};
		initOpponentData = new string[]{"",""};
		delemeter = ('\\');
		
		winningMove = 0;
		t1 = new DateTime();
		t4 = new DateTime();
		tServer = new DateTime();
		canSendStart = false;
		totalLat = 0;

		Stopwatch uniClock = new Stopwatch();
		DateTime dt = new DateTime();

		//dt = SwarchServer.NTPTime.getNTPTime(ref uniClock);

		//player = GameObject.Find("Player").GetComponent<Player>();

		//player.playerNum = mainPlayerNumber;


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
			//dt = SwarchServer.NTPTime.getNTPTime(ref uniClock);

			player = GameObject.Find("Player").GetComponent<Player>();

			player.pos = initPlayerPos;
			player.playerNum = mainPlayerNumber;

			guiTGP.text = "";

			//int startNumOpponents = Convert.ToInt32( initOpponentData[1]);

			for(int k=1; k < initOpponentData.Length; k = k+3)
			{

				if(Convert.ToInt32(initOpponentData[k+2]) != player.playerNum)
				{
					
					Transform tempOpponents = Instantiate(opponents, new Vector3(float.Parse(initOpponentData[k]), 
					                                   0, float.Parse(initOpponentData[k+1])), Quaternion.identity) as Transform;


					opponent opp =	(opponent)tempOpponents.GetComponent("opponent");
					opp.pos = new Vector3(float.Parse(initOpponentData[k]), 0, float.Parse(initOpponentData[k+1]));
					opp.name = "opponent"+initOpponentData[k+2];
					
					opp.opponentNum = Convert.ToInt32(initOpponentData[k+2]);

				}

			}
			int n = 1;
		for(int i =0; i < 10; i = i+2)
		{

				Transform tempPell = Instantiate(pell, new Vector3(pelletsLocation[i], 
				                        0,  pelletsLocation[i+1]), Quaternion.identity) as Transform;

				Pellets tempP = (Pellets)tempPell.GetComponent("Pellets"); 

				tempP.pellNumber = (n);
				tempP.name = "pellet"+n;

			pellets.Add(tempP ); 
			++n;
		}
			loadPellets = false;    
	  }
		
	
	 if(startedGame)
	 {
		if(socks.recvBuffer.Count > 0 )
		{
			
			data = (string)socks.recvBuffer.Dequeue();

			splitData = data.Split(delemeter);
		
			if(splitData[0] == "newEntry")
			{
				//print ("GOT INTO NEWENTRY _-_-_-_");  
				for(int k=1; k < splitData.Length; k = k+3)
				{
					
					//if(Convert.ToInt32(splitData[k+2]) != player.playerNum)
					//{
						
						Transform tempOpponents = Instantiate(opponents, new Vector3(float.Parse(splitData[k]), 
						                                                             0, float.Parse(splitData[k+1])), Quaternion.identity) as Transform;
						
						
						opponent opp =	(opponent)tempOpponents.GetComponent("opponent");
						opp.pos = new Vector3(float.Parse(splitData[k]), 0, float.Parse(splitData[k+1]));
						opp.name = "opponent"+splitData[k+2];
						
						opp.opponentNum = Convert.ToInt32(splitData[k+2]);
						
					//}
					
				}

			}
			else if(splitData[0] == "getPell")
			{
				// splitData[1] -- timeStamp
		      /*
				player.MoveSpeed = float.Parse( splitData[2]);



					//score += growSize;


					player.transform.localScale = new Vector3(Convert.ToInt32(splitData[1]),
					                                        player.transform.localScale.y,
					                                          Convert.ToInt32(splitData[1]));

              */
			}
			else if(splitData[0] == "newPell")
			{
					print ("GOT INTO NEWpELL _-_-_-_");
			  for(int r = 1; r< splitData.Length;  r=r+6)
		      {
				    if(Convert.ToInt32(splitData[r]) == player.playerNum)
					{
						player.MoveSpeed = float.Parse( splitData[r+2]);
						//score += growSize;

						player.transform.localScale = new Vector3(Convert.ToInt32(splitData[r+1]),
						                                          player.transform.localScale.y,
						                                          Convert.ToInt32(splitData[r+1]));
					}
					else
					{
							opponent tempClient = GameObject.Find("opponent"+splitData[r]).GetComponent<opponent>();
						
						tempClient.MoveSpeed = float.Parse( splitData[r+2]);

						tempClient.transform.localScale = new Vector3(Convert.ToInt32(splitData[r+1]),
						                                          tempClient.transform.localScale.y,
						                                          Convert.ToInt32(splitData[r+1]));


					}

					  
				  //for(int i =1; i < splitData.Length; i = i+3)
				  //{
						if(pellets.Exists(x=>x.pellNumber == Convert.ToInt32(splitData[r+3])))
						{
							print ("\nSAME PELLET DOES EXIST ::"+ splitData[r+3]);
						   ///*
							int tempPellLoc1 = pellets.FindIndex(x=> x.pellNumber == Convert.ToInt32(splitData[r+4]));
							
							pellets.RemoveAt(tempPellLoc1);

							GameObject oldPell = GameObject.Find("pellet"+splitData[r+4]);

							Destroy(oldPell);
                          //*/
						}
					
						Transform tempPell = Instantiate(pell, new Vector3(float.Parse(splitData[r+4]), 
						                                                   0,  float.Parse(splitData[r+5])), Quaternion.identity) as Transform;
					
					  Pellets tempP = (Pellets)tempPell.GetComponent("Pellets"); 
					
						tempP.pellNumber = Convert.ToInt32(splitData[r+3]);
						tempP.name = "pellet"+splitData[r+3];

						//int tempPellLoc = pellets.FindIndex(x=>x.pellNumber == Convert.ToInt32(splitData[r+3]));

						//print ("tempPellLoc " );
						pellets.Add(tempP ); 
					
				  //}  
			  }

			}
			//else if(splitData[0] == "missPell")
			//{
			   
				
			//}
            else if (splitData[0] == "score")
            {
                print("\nGOT TO UPDATE SCORE TO ::" + splitData[1]);
                player.totalScore = Convert.ToInt32(splitData[1]);
            }


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
