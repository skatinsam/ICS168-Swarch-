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


	void Start()
	{

		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>(); 

		scoreDisplay.transform.position = this.transform.position;


		MoveSpeed = 10f;
		playerNum = 0;

        StartCoroutine(sendPosition());
	}
	
	void Update()
	{
		scoreDisplay.text = score.ToString();


		if( resetting )
			return;
		

		if( !AcceptsInput )
			return;
		

		input = Input.GetAxis( "Vertical" );
		input2 = Input.GetAxis("Horizontal");	
	    pos.z += input * MoveSpeed * Time.deltaTime;
		pos.x += input2 *MoveSpeed * Time.deltaTime;	
	    
        

		this.transform.position = pos;
		scoreDisplay.transform.position = pos;
	}

	void OnTriggerEnter( Collider c )
	{

		if(c.tag == "Wall")
		{
			//StartCoroutine( resetBall());

			print ("\nHIT WALL");

			//Player p = GameObject.Find("Player").GetComponent<GameProcess>();

			gp.returnSocket().sendQueue.Enqueue("wall\\"+ this.playerNum + "\\" + this.pos.x +"\\"+ this.pos.z); //+ "\\"

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
			//score = 0;

		}
		if(c.tag == "Opponent")
		{

			float oppX = c.transform.position.x;
			float oppZ = c.transform.position.z;
			opponent tempOpp = (opponent)c.GetComponent("opponent");

			gp.returnSocket().sendQueue.Enqueue("hitOpp\\"+ tempOpp.opponentNum +"\\"
			                                    + tempOpp.transform.localScale.x 
			                                    +"\\" + oppX +"\\"+ oppZ); 

		}
		if(c.tag == "Pellet")
		{
			float pellX = c.transform.position.x;
			float pellZ = c.transform.position.z;
			Pellets tempPell = (Pellets)c.GetComponent("Pellets");

			 
			gp.returnSocket().sendQueue.Enqueue("hitPell\\"+ tempPell.pellNumber +"\\"
			                                    + tempPell.transform.localScale.x  
			                                    +  "\\" +pellX +"\\"+ pellZ); //+ "\\"
			                                    //+(((gp.dt.AddMinutes(gp.uniClock.Elapsed.Minutes).AddSeconds(gp.uniClock.Elapsed.Seconds).AddMilliseconds(gp.uniClock.Elapsed.Milliseconds)).Ticks)) ); 


			int tempPellLoc = gp.pellets.FindIndex(x=> x.pellNumber == tempPell.pellNumber);

			gp.pellets.RemoveAt(tempPellLoc);
			//gp.pellets.RemoveAt(0); // server will determine who ate it.
			Destroy(c.gameObject);

// his now set when server sends it
			//print("size of pellet array(AFTER): ----> "+gp.pellets.Count);
		
			//Transform tempPell = Instantiate(gp.pell, new Vector3(UnityEngine.Random.Range(-23.5F, 23.5F), 
			//                                               0, UnityEngine.Random.Range(-12.5F, 14.5F)), Quaternion.identity) as Transform;

			//gp.pellets.Add(tempPell); 

			 //int growSide = UnityEngine.Random.Range(1,10);

			//score += growSize;
		
			//this.transform.localScale = new Vector3(growSize,
			//                                        this.transform.localScale.y,
			 //                                       growSize);

		/*
		    if(growSide <= 5)
			{
			this.transform.localScale = new Vector3(this.transform.localScale.x+growSize,
			                                        this.transform.localScale.y,
			                                       this.transform.localScale.z);

			}
			else
			{
				this.transform.localScale = new Vector3(this.transform.localScale.x,
				                                        this.transform.localScale.y,
				                                        this.transform.localScale.z+growSize);
			}
		*/
		/*
			float speedChange = (MoveSpeed-2);

			if(speedChange < 1)
			{
			  MoveSpeed = MoveSpeed * .85F;
			
			}
			else
			{
				MoveSpeed = speedChange;
			}
		*/

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
    IEnumerator sendPosition()
    {
        yield return new WaitForSeconds(.4f);
        gp.returnSocket().sendQueue.Enqueue("move\\" + playerNum + "\\" + pos.x + "\\" + pos.z);
    }
 }


