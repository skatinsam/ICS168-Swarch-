//SWARCH

/*
 * Scoreboard status:
 * Made GUI.Box in Player.cs and opponent.cs
 * Placement of box dependent on client number
 * 
 * 
 * 
*/

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
    public String[] highestScorers;
    public int[] highestScores;
	public int numOfPlayer;
	public DateTime t1; 
	public DateTime t4;
	public DateTime tServer;
	public bool canSendStart;
	public double totalLat;
	public double[] levelBounds;
	public Player player;
	public Vector3 initPlayerPos;
	System.Object thisLock;
    public opponent[] opps;

	public Stopwatch uniClock; // = new Stopwatch();
	public DateTime dt; // = new DateTime();
	
	//public int startOpponents;

	public string[] initOpponentData;

	public GUIText guiTGP;

	public bool loadPellets;

	void Start () 
	{
		//Sockets socketThread = GameObject.Find("socketHolder").GetComponent<Sockets>();

		socks =new Sockets(); //socketThread;//

		pellets = new List<Pellets>();//new ArrayList();
		pelletsLocation = new List<float>();
		data = "";
		loadPellets = false;

        highestScorers = new String[5];
        highestScores = new int[5];

		mainPlayerNumber= 0;

		startedGame = false;
		startNextRound = false;
		hitGoal = false;
		splitData = new string[]{"",""};

		// 0-left,1-right,2-up,3-down
		levelBounds = new double[]{0.0,0.0,0.0,0.0};
		initOpponentData = new string[]{"",""};
		delemeter = ('\\');
		
		winningMove = 0;
		t1 = new DateTime();
		t4 = new DateTime();
		tServer = new DateTime();
		canSendStart = false;
		totalLat = 0;
		thisLock = new System.Object();


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
			//returnSocket().sendQueue.Enqueue("quit");

			socks.endThread();
			socks.Disconnect();
			print("\nDISCONNECTED ");
		}
    
        

	}

	void Update () 
	{

	  if(loadPellets)
	  {

			GameObject leftBound =  GameObject.Find("leftBound");
			leftBound.transform.position = new Vector3((float)levelBounds[0],
			                                        leftBound.transform.position.y,
			                                        leftBound.transform.position.z);
			GameObject rightBound =  GameObject.Find("rightBound");
			rightBound.transform.position = new Vector3((float)levelBounds[1],
			                                           rightBound.transform.position.y,
			                                           rightBound.transform.position.z);
			GameObject topBound =  GameObject.Find("TopBound");
			topBound.transform.position = new Vector3(topBound.transform.position.x,
			                                           topBound.transform.position.y,
			                                           (float)levelBounds[2]);
			GameObject bottomBound =  GameObject.Find("bottomBound");
			bottomBound.transform.position = new Vector3(bottomBound.transform.position.x,
			                                           bottomBound.transform.position.y,
			                                           (float)levelBounds[3]);


			//dt = SwarchServer.NTPTime.getNTPTime(ref uniClock);
			GameObject destroyLogin = GameObject.Find("Login Manager");
			Destroy(destroyLogin);
			player = GameObject.Find("Player").GetComponent<Player>();

			player.pos = initPlayerPos;
			player.playerNum = mainPlayerNumber;

			guiTGP.text = "";

			//int startNumOpponents = Convert.ToInt32( initOpponentData[1]);

			for(int k=1; k < initOpponentData.Length; k = k+5)
			{

				if(Convert.ToInt32(initOpponentData[k+2]) != player.playerNum)
				{
					
					Transform tempOpponents = Instantiate(opponents, new Vector3(float.Parse(initOpponentData[k]), 
					                                   0, float.Parse(initOpponentData[k+1])), Quaternion.identity) as Transform;


					opponent opp =	(opponent)tempOpponents.GetComponent("opponent");

					opp.MoveSpeed = float.Parse( initOpponentData[k+3]);

					opp.transform.localScale = new Vector3(Convert.ToInt32(initOpponentData[k+4]),
					                                    tempOpponents.transform.localScale.y,
					                                      Convert.ToInt32(initOpponentData[k+4]));
					


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
				//print("\npeek in Queue **** "+ socks.recvBuffer.Peek());
			
			lock(socks.recvBuffer)
			{
			 data = (string)socks.recvBuffer.Dequeue();
			}

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
			else if(splitData[0] == "move")
			{
				for(int i=1; i<splitData.Length; i=i+3)
				{

					if(Convert.ToInt32(splitData[i+2]) == player.playerNum) 
					{

							//player.transform.position = new Vector3(float.Parse(splitData[i]),0f,float.Parse(splitData[i+1]));
							//player.pos = new Vector3(float.Parse(splitData[i]),0f,float.Parse(splitData[i+1]));
							
					}
					else
					{
						opponent tempClient = GameObject.Find("opponent"+splitData[i+2]).GetComponent<opponent>();
						
							tempClient.pos = new Vector3(float.Parse(splitData[i]),0f,float.Parse(splitData[i+1]));


					}
				}
			
			}
			else if(splitData[0] == "hitwall")
			{
				if(Convert.ToInt32(splitData[1]) == player.playerNum) 
				{
						player.transform.localScale = new Vector3(Convert.ToInt32(splitData[2]),
						                                          player.transform.localScale.y,
						                                          Convert.ToInt32(splitData[2]));

						player.MoveSpeed = float.Parse( splitData[3]);

						player.pos = new Vector3(float.Parse(splitData[4]),0f,
						                         float.Parse(splitData[5]));
					
				}
				else
				{
						opponent tempClient = GameObject.Find("opponent"+splitData[1]).GetComponent<opponent>();

						tempClient.transform.localScale = new Vector3(Convert.ToInt32(splitData[2]),
						                                          tempClient.transform.localScale.y,
						                                          Convert.ToInt32(splitData[2]));
						
						tempClient.MoveSpeed = float.Parse( splitData[3]);
						
						tempClient.pos = new Vector3(float.Parse(splitData[4]),0f,
						                         float.Parse(splitData[5]));


				}
				
			}
			else if(splitData[0] == "newPell")
			{

			  for(int r = 1; r< splitData.Length;  r=r+6)
		      {
				    if(Convert.ToInt32(splitData[r]) == player.playerNum)
					{
						player.MoveSpeed = float.Parse( splitData[r+2]);
						//score += growSize;

						player.transform.localScale = new Vector3(Convert.ToInt32(splitData[r+1]),
						                                          player.transform.localScale.y,
						                                          Convert.ToInt32(splitData[r+1]));
                        player.score += 2;
					}
					else
					{
					   opponent tempClient = GameObject.Find("opponent"+splitData[r]).GetComponent<opponent>();
						
						tempClient.MoveSpeed = float.Parse( splitData[r+2]);

						tempClient.transform.localScale = new Vector3(Convert.ToInt32(splitData[r+1]),
						                                          tempClient.transform.localScale.y,
						                                          Convert.ToInt32(splitData[r+1]));


					}

					
						if(pellets.Exists(x=>x.pellNumber == Convert.ToInt32(splitData[r+3])))
						{
							print ("\nSAME PELLET DOES EXIST ::"+ splitData[r+3]);
						   ///*
							int tempPellLoc1 = pellets.FindIndex(x=> x.pellNumber == Convert.ToInt32(splitData[r+3]));

							print("find index :" + tempPellLoc1);

							pellets.RemoveAt(tempPellLoc1);

							GameObject oldPell = GameObject.Find("pellet"+splitData[r+3]);

							Destroy(oldPell);
                         
						}
					
						Transform tempPell = Instantiate(pell, new Vector3(float.Parse(splitData[r+4]), 
						                                                   0,  float.Parse(splitData[r+5])), Quaternion.identity) as Transform;
					
					  Pellets tempP = (Pellets)tempPell.GetComponent("Pellets"); 
					
						tempP.pellNumber = Convert.ToInt32(splitData[r+3]);
						tempP.name = "pellet"+splitData[r+3];


						print ("\nadded : "+ splitData[r+3] );
						pellets.Add(tempP ); 
					
				  
			  }

			}
			else if (splitData[0] == "score")
			{
				print("\nGOT TO UPDATE SCORE TO ::" + splitData[1]);
				player.totalScore = Convert.ToInt32(splitData[1]);
			}
            else if (splitData[0] == "highscore")
            {
                int j = 0;
                //add scores to this thing
                for (int i = 1; i < 11; i += 2)
                {
                    highestScorers[j] = splitData[i];
                    highestScores[j] = Int32.Parse(splitData[i + 1]);
                    j++;
                }
            }
            else if (splitData[0] == "pH")
            {

                for (int r = 1; r < splitData.Length; r = r + 5)
                {


                    if (Convert.ToInt32(splitData[r]) == player.playerNum)
                    {

                        player.transform.localScale = new Vector3(Convert.ToInt32(splitData[r + 1]),
                                                                  player.transform.localScale.y,
                                                                  Convert.ToInt32(splitData[r + 1]));

                        player.MoveSpeed = float.Parse(splitData[r + 2]);


                        //player.transform.position = new Vector3(float.Parse(splitData[r+3]),
                        //                                       player.transform.position.y,
                        //                                      float.Parse(splitData[r+4]));		


                        //**** USE PLAYER.POS ALSO FOR OPPONENTS

                        player.pos = new Vector3(float.Parse(splitData[r + 3]),
                                                  player.transform.position.y,
                                                  float.Parse(splitData[r + 4]));



                    }
                    else
                    {
                        //  UPDATE THE OPPONENT //

                        opponent tempClient = GameObject.Find("opponent" + splitData[r]).GetComponent<opponent>();



                        tempClient.transform.localScale = new Vector3(Convert.ToInt32(splitData[r + 1]),
                                                                      tempClient.transform.localScale.y,
                                                                      Convert.ToInt32(splitData[r + 1]));

                        tempClient.MoveSpeed = float.Parse(splitData[r + 2]);

                        tempClient.pos = new Vector3(float.Parse(splitData[r + 3]),
                                                     tempClient.transform.position.y,
                                                      float.Parse(splitData[r + 4]));


                    }

                }

            }
		 /*
			else if(splitData[0] == "closed")
			{

			  if(Convert.ToInt32(splitData[1]) == player.playerNum)
			  {
			     socks.endThread();
				 socks.Disconnect();
				 Application.Quit();
			  }
			  else
			  {
			    // clear the score here
			  }
			}
		  */

		}
	 }
		
	}
	public Sockets returnSocket()
	{
		return socks;
	}
	
	
	
	public void send(string toSend )
	{
		print ("\nsent through GP ");
		socks.SendTCPPacket(toSend);
		
	}
	
	
	public void sendEndGame ()
	{


	}
	


	
	
	
	
}
