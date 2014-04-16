
using UnityEngine;
using System;
using System.Collections;

public class GuiSwarch : MonoBehaviour {
	
	
	
	bool show = false;
	public double delTime;
	public GUIText guiText;
	private GameProcess process;
	bool startedGame = false;
	public bool gameOver;
	
	void Start () 
	{
		//process = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		gameOver = false;
	}
	
	void OnGUI ()
	{
		
		if ( !show )
		{
		  /*
			if (GUI.Button (new Rect (150,60,100,20), "Connect")) 
			{
				print("Connecting...");
				if ( process.returnSocket().Connect() )
				{	
					guiText.text = "";
					show = !show;
					print("Connect Succeeded");
					
				}
				else guiText.text = "Connect Failed";
			}
		  */
		}
		
		if ( show ) 
		{
			if ( GUI.Button( new Rect( 0, 0, 100, 20), "Disconnect"))
			{
				//********* COMPLETE THE FOLLOWING CODE
				//********* KILL THREAD AND SEVER CONNECTION
				
				//returnSocket().SendTCPPacket ((byte) (process.commands[(int)GameProcess.codes.roll]));
				
				//process.sendEndGame();
				
				//process.returnSocket().endThread();
				//process.returnSocket().Disconnect();
				
				show = !show;
			}
			
			
		}
		if(!startedGame && show && process.canSendStart)
		{
			if( GUI.Button( new Rect( 0, 40, 100, 20), "Start Game"))
			{

				if(process.numOfPlayer == 1)
				{
					Vector3 LB = GameObject.Find("LowerBound").transform.position;
					Vector3 UB = GameObject.Find("UpperBound").transform.position;
					Vector3 Ballsize = GameObject.Find("Ball").transform.localScale;
					Vector3 initBallPos = GameObject.Find("Ball").transform.position;
					Vector3 LGoal = GameObject.Find("LeftGoal").transform.position;
					Vector3 RGoal = GameObject.Find("RightGoal").transform.position;
					/*
				 process.send("start\\"
					         + padPos.x + "\\" 
					         + padPos.z + "\\"
					         +((UB.z - .5))+ "\\"  // - (Ballsize.z + .5)
					         +((LB.z + .5))+ "\\"  // + (Ballsize.z - .5)
					         +((LGoal.x + .5))+ "\\"
					         +((RGoal.x - .5 ))+ "\\"
					         + initBallPos.x + "\\"
					         + initBallPos.z );
			*/
					/*
					process.socks.sendQueue.Enqueue("start\\"
					                                + padPos.x + "\\" 
					                                + padPos.z + "\\"
					                                +((UB.z - .5))+ "\\"  // - (Ballsize.z + .5)
					                                +((LB.z + .5))+ "\\"  // + (Ballsize.z - .5)
					                                +((LGoal.x + .5))+ "\\"
					                                +((RGoal.x - .5 ))+ "\\"
					                                + initBallPos.x + "\\"
					                                + initBallPos.z);
                  */
				}
				else
				{
					//process.send("start\\"+ padPos.x+ "\\" + padPos.z);
					
					//process.socks.sendQueue.Enqueue("start\\"+ padPos.x+ "\\" + padPos.z);
				}
				
				
				startedGame = true;
			}
			
			
		}
		if(process.hitGoal && show && !gameOver)
		{
			if( GUI.Button( new Rect( 0, 40, 120, 20), "Next Round"))
			{
				guiText.text = "";
				//process.send("startNextRound");
				//process.socks.sendQueue.Enqueue("startNextRound");
				process.hitGoal = false;
			}
		}
		

		
		if(process.canSendStart && show)
		{
			if( GUI.Button( new Rect( 120, 5, 150, 40), " Measure Latency "))
			{      
				guiText.text = "";
				
				//stamp time to send out
				//float timeLatStart = Time.time.ToString();
				
				process.t1 = DateTime.Now; //Now;
				//process.socks.sendQueue.Enqueue("lat" ); // \\"+ process.t1);
			}
			
		}
		if(process.totalLat!= 0)
		{
			guiText.text = ("Total Latency: "+ process.totalLat);
			print ("TOTAL LAT: " + process.totalLat);
			process.totalLat = 0;
		}
		
		//if ( GUI.Button ( new Rect ( 500, 300, 100, 20 ), "Latency" ))
		//{
		//process.returnSocket().measureLatency() ;
		//}
		
		//GUI.Label ( new Rect ( 500, 330, 100, 20 ) , "Latency : " + process.returnSocket().returnLatency()  );
		
	}
	
	
	void Update () 
	{
	}
	
	public void printGui ( string printStr )
	{
		int wordCount = 0 ;
		string[] words = printStr.Split(' ');
		
		printStr = "";
		
		for ( int i = 0 ; i < words.Length ; ++ i )
		{
			if ( wordCount <= 4 )
			{
				printStr += words[i] + " " ;
				wordCount ++ ;
			}
			else
			{
				printStr += words[i] + "\n" ;
				wordCount = 0;
				
			}	
		}
		
		guiText.text = printStr ;
	}
	
	
	
}
