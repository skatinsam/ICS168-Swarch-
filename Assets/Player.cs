using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
	// how fast the paddle can move
	public float MoveSpeed = 10f;
	
	//public int playerNum;
	
	// how far up and down the paddle can move
	public float MoveRangeL = -5.712811f;//10f;
	public float MoveRangeH = 12.90477f;
	
	// whether this paddle can accept player input
	public bool AcceptsInput = true;
	
	public double thresholdMove;
	
	private GameProcess gp; 
	public float timeLerp;
	public float moveDiff;
	public GameObject p;
	public bool hasPadNum;
	Vector3 lastPos;
	float input; 
	float lastInput;
	public Vector3 pos;
	
	// new additions
	public double upperThreshold;
	public double lowerThreshold;
	public bool sendPacket;
	public double change;
	
	void Start()
	{
		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		hasPadNum = false;
		//thresholdMove = .2; //.5;
		
		thresholdMove = Screen.height / 100; //.5;
		sendPacket = false;
		change = 0;
		
		//***** co-rutine
		//StartCoroutine(SendDelay());
	}
	
	void Update()   /// ****** try fixed update to give better change vaule
	{
		
		// does not accept input, abort
		if( !AcceptsInput )
			return;
		
		if(hasPadNum && gp.startGame)
		{
			
			//get user input
			
			
			
			//if(Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow) )
			//{
			input = Input.GetAxis( "Vertical" );
			//}
			//if(Input.GetKeyUp(KeyCode.UpArrow)||Input.GetKeyUp(KeyCode.DownArrow))
			//{
			//	lastInput = input;
			//}
			
			
			lastPos = p.transform.position;
			
			if(sendPacket == false)
			{
				sendPacket = true;
				upperThreshold = lastPos.z + 0.5;
				lowerThreshold = lastPos.z - 0.5;
			}
			
			
			// move paddle
			//Vector3 pos = p.transform.position;
			pos.z += input * MoveSpeed * Time.deltaTime;
			//input
			// clamp paddle position
			pos.z = Mathf.Clamp( pos.z, MoveRangeL, MoveRangeH );
			
			timeLerp = Time.time;
			
			change = Mathf.Abs(lastPos.z - pos.z );
			
			
			if(change > 0)
			{
				//print("\n UPThresh: " +upperThreshold + " LowThresh: " + " pos.z " + pos.z );   //***** difference isn't becomeing greater than .5
				
				//print("\n PLAY  : "+ (pos.z));
				//print("\n greater " + (thresholdMove+pos.z));
				//print("\n less " + (pos.z - thresholdMove));
			}//moveDiff
			
			if(change != 0 && (pos.z >= upperThreshold || pos.z <= lowerThreshold)) //(change != 0 && ((change) >= thresholdMove))// ||((lastPos.z + change) <= lastPos.z - thresholdMove) ) //&&  // 
			{
				
				//gp.socks.SendTCPPacket("pad\\" + pos.z);
				
				//gp.socks.sendQueue.Enqueue("pad\\" + pos.z); // THREAD METHOD
				
				//print ("\nSENT OUT ------>  " + pos.z);
				
				sendPacket = false;
				
				
			}
			// set position
			p.transform.position = pos;
		}
		
	}
  /*
	IEnumerator SendDelay() 
	{
		
		while ( true )
		{
			yield return new WaitForSeconds ( .1f ) ; //0.1f  // --> 100ms
			
			if (change != 0 && (pos.z >= upperThreshold || pos.z <= lowerThreshold))
			{
				gp.socks.SendTCPPacket("pad\\" + pos.z);
				print ("\nSENT PAD ------>  " + pos.z);
				
				sendPacket = false;
			}
		}
		
	}
  */
  /*
	public void moveOpponent(string[] opponentInfo)
	{
		// / * 
		float movePad = Convert.ToSingle(gp.splitData[1]);
		float distCovered = (Time.time - timeLerp) * MoveSpeed;
		float fracJourney = distCovered / moveDiff;  //<-- ?  //journeyLength;
		Vector3 player2Pad; 
		GameObject p;
		
		if(gp.numOfPlayer == 1)
		{
			p = GameObject.Find("PaddleR");
			player2Pad = GameObject.Find("PaddleR").transform.position;
		}
		else
		{
			p = GameObject.Find("PaddleL");
			player2Pad = GameObject.Find("PaddleL").transform.position;
		}
		// * /
		//p.transform.position =  Vector3.Lerp(player2Pad , new Vector3(  player2Pad.x ,player2Pad.y, movePad ), distCovered);
		
		p.transform.position = new Vector3(player2Pad.x ,player2Pad.y, movePad);
		
	}
  */
}