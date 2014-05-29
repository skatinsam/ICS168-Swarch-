using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
	// how fast the paddle can move
	public float MoveSpeed;
	
	public int playerNum;
	
	public int growSize = 2;
	public int score = 0;
    public int totalScore = 0;
	
	// whether this paddle can accept player input
	public bool AcceptsInput = true;

	private GameProcess gp; 

	float input; 
	float input2;
	public Vector3 pos;

	public bool resetting = false;
    public float move = 1.0f;

	public TextMesh scoreDisplay;

	Vector3 lastPos;
	public bool sendPacket;
	public double upperThreshold;
	public double lowerThreshold;
	public double leftThreshold;
	public double rightThreshold;


	void Start()
	{

		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>(); 

		scoreDisplay.transform.position = this.transform.position;


		MoveSpeed = 10f;
		playerNum = 0;
		sendPacket = false;
       
		//StartCoroutine(sendPosition());
	}
	
	void Update()
	{
		scoreDisplay.text = score.ToString();


		if( resetting )
			return;
		

		if( !AcceptsInput )
			return;

		//added
		lastPos = this.transform.position;

		//added
		if(sendPacket == false)
		{
			sendPacket = true;
			upperThreshold = lastPos.z + 0.3;
			lowerThreshold = lastPos.z - 0.3;
			leftThreshold = lastPos.x - 0.3;
			rightThreshold = lastPos.x + 0.3;

		}

		input = Input.GetAxis( "Vertical" );
		input2 = Input.GetAxis("Horizontal");	
	    pos.z += input * MoveSpeed * Time.deltaTime;
		pos.x += input2 *MoveSpeed * Time.deltaTime;	
	    
        


		scoreDisplay.transform.position = pos;





		//added
		if( (pos.z >= upperThreshold || pos.z <= lowerThreshold 
		     || pos.x >= rightThreshold || pos.x <= leftThreshold )) //(change != 0 && ((change) >= thresholdMove))// ||((lastPos.z + change) <= lastPos.z - thresholdMove) ) //&&  // 
		{
			gp.returnSocket().sendQueue.Enqueue("move\\" + playerNum + "\\" + pos.x + "\\" + pos.z);
			sendPacket = false;

		}
		this.transform.position = pos;

	}

	void OnTriggerEnter( Collider c )
	{

		if(c.tag == "Wall")
		{
			//StartCoroutine( resetBall());

			//print ("\nHIT WALL");

			//Player p = GameObject.Find("Player").GetComponent<GameProcess>();
													//+ this.playerNum 
			gp.returnSocket().sendQueue.Enqueue("wall\\"); 

			gp.returnSocket().sendQueue.Enqueue("score\\" + score);
			//pos =  Vector3.zero; // notify the server ??

			switch(this.playerNum)//(tempClient.clientNumber)
			{
			case 1:
			{
				this.pos = new Vector3(-17.0f,0f,7.0f);
				break;
			}
			case 2:
			{
				this.pos = new Vector3(20.0f,0f,7.0f);
				break;
			}
				
			case 3:
			{
				this.pos = new Vector3(20.0f,0f,-10.0f);
				break;
			}
			default:
				break;
			}

			this.transform.localScale = new Vector3(2,2,2);
			this.MoveSpeed = 10;
			//score = 0;

		}
		if(c.tag == "Opponent")
		{

			//float oppX = c.transform.position.x;
			//float oppZ = c.transform.position.z;
			//opponent tempOpp = (opponent)c.GetComponent("opponent");

			//gp.returnSocket().sendQueue.Enqueue("hitOpp\\"+ tempOpp.opponentNum +"\\"
			//                                    + tempOpp.transform.localScale.x 
			//                                    +"\\" + oppX +"\\"+ oppZ); 

		}
		if(c.tag == "Pellet")
		{
			//float pellX = c.transform.position.x;
			//float pellZ = c.transform.position.z;
			Pellets tempPell = (Pellets)c.GetComponent("Pellets");

			 
			//gp.returnSocket().sendQueue.Enqueue("hitPell\\"+ tempPell.pellNumber +"\\"
			 //                                   + tempPell.transform.localScale.x  
			 //                                   +  "\\" +pellX +"\\"+ pellZ); //+ "\\"
			                                    //+(((gp.dt.AddMinutes(gp.uniClock.Elapsed.Minutes).AddSeconds(gp.uniClock.Elapsed.Seconds).AddMilliseconds(gp.uniClock.Elapsed.Milliseconds)).Ticks)) ); 

  
// CLIENT SIDE REMOVEAL OF PELLETS
			//int tempPellLoc = gp.pellets.FindIndex(x=> x.pellNumber == tempPell.pellNumber);
			//gp.pellets.RemoveAt(tempPellLoc);
			//Destroy(c.gameObject);

		}

	}
  /*
	IEnumerator resetBall()
	{
		// reset position, speed, and direction
		resetting = true;
		this.transform.position = Vector3.zero;
		
		//currentDir = Vector3.zero;
		MoveSpeed = 0f;
		
		// wait for 3 seconds before starting the round
		yield return new WaitForSeconds( 0f );
		
		Start();
		
		resetting = false;
	}
  */
/*
    IEnumerator sendPosition()
    {
	  while ( true )
	  {
			
        yield return new WaitForSeconds(.00000000000001f);
         
		 if(playerNum!=0)
		 {
			//gp.returnSocket().sendQueue.Enqueue("move\\" + playerNum + "\\" + pos.x + "\\" + pos.z);
		 
			//added

		    if ( (pos.z >= upperThreshold || pos.z <= lowerThreshold
				 || pos.x >= rightThreshold || pos.x <= leftThreshold ))
			{

				gp.returnSocket().SendTCPPacket("move\\" + playerNum + "\\" + pos.x + "\\" + pos.z);
					//print ("\nSENT MOVE ------> : " + pos.x+" y " + pos.z);

				sendPacket = false;
			}

		 }
	  }
   }
*/
 }


