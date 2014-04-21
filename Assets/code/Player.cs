using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
	// how fast the paddle can move
	public float MoveSpeed;
	
	//public int playerNum;
	
	public int growSize = 2;
	
	// whether this paddle can accept player input
	public bool AcceptsInput = true;

	private GameProcess gp; //= GameObject.Find("GameProcess").GetComponent<GameProcess>(); 

	float input; 
	float input2;
	public Vector3 pos;

	public bool resetting = false;


	void Start()
	{

		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>(); 


		//p = GameObject.Find("Player");

		MoveSpeed = 10f;

	}
	
	void Update()
	{

		if( resetting )
			return;
		
		// move the ball in the current direction
		//Vector2 moveDir = currentDir * currentSpeed * Time.deltaTime;
		//transform.Translate( new Vector3( moveDir.x, 0f, moveDir.y ) );


	  ///*	
		// does not accept input, abort
		if( !AcceptsInput )
			return;
		

			
			//if(Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow) )
			//{
			input = Input.GetAxis( "Vertical" );
		    input2 = Input.GetAxis("Horizontal");	
		//}
			//if(Input.GetKeyUp(KeyCode.UpArrow)||Input.GetKeyUp(KeyCode.DownArrow))
			//{
			//	lastInput = input;
			//}
			
			
			//lastPos = p.transform.position;
			

			// move paddle
			//Vector3 pos = p.transform.position;
			pos.z += input * MoveSpeed * Time.deltaTime;
		    pos.x += input2 *MoveSpeed * Time.deltaTime;	
	    // p 


		this.transform.position = pos;

		//if(this.transform.position.x >= 5 && !movedBack)
		//{

			//this.transform.position = new Vector3(0,0,0);
			//movedBack = true;
		//}

		//print(p.transform.position);

    //*/
		
	}

	void OnTriggerEnter( Collider c )
	{

		if(c.tag == "Wall")
		{
			StartCoroutine( resetBall());

			print ("\n\nHIT WALL");

			GameObject p = GameObject.Find("Player");
			pos =  Vector3.zero;

			this.transform.localScale = new Vector3(2,2,2);


		}
		if(c.tag == "Pellet")
		{
			print("size of pellet array(BEFORE): " +gp.pellets.Count + " c NAME: " + c.gameObject.name);
			gp.pellets.RemoveAt(0); //(Convert.ToInt32(c.gameObject.name));
			Destroy(c.gameObject);
			print("size of pellet array(AFTER): ----> "+gp.pellets.Count);
		
			Transform tempPell = Instantiate(gp.pell, new Vector3(UnityEngine.Random.Range(-23.5F, 23.5F), 
			                                               0, UnityEngine.Random.Range(-12.5F, 14.5F)), Quaternion.identity) as Transform;

			gp.pellets.Add(tempPell); 

			 int growSide = UnityEngine.Random.Range(1,10);

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

			float speedChange = (MoveSpeed-2);

			if(speedChange < 1)
			{
			  MoveSpeed = MoveSpeed * .85F;
			
			}
			else
			{
				MoveSpeed = speedChange;
			}
			print("MoveSpeed " +MoveSpeed);
		}
	}
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

 }


/*

public class Ball : MonoBehaviour
{
	// the speed the ball starts with
	public float StartSpeed = 5f;
	
	// the maximum speed of the ball
	public float MaxSpeed = 20f;
	
	// how much faster the ball gets with each bounce
	public float SpeedIncrease = 0.25f;
	
	// the current speed of the ball
	private float currentSpeed;
	
	// the current direction of travel
	private Vector2 currentDir
		
		// whether or not the ball is resetting
		private bool resetting = false;
	
	void Start()
	{
		// initialize starting speed
		currentSpeed = StartSpeed;
		
		// initialize direction
		currentDir = Random.insideUnitCircle.normalized;
	}
	
	void Update()
	{
		// don’t move the ball if it’s resetting
		if( resetting )
			return;
		
		// move the ball in the current direction
		Vector2 moveDir = currentDir * currentSpeed * Time.deltaTime;
		transform.Translate( new Vector3( moveDir.x, 0f, moveDir.y ) );
	}
	
	void OnTriggerEnter( Collider other )
	{
		if( other.tag == "Boundary" )
		{
			// vertical boundary, reverse Y direction
			currentDir.y *= -1;
		}
		else if( other.tag == "Player" )
		{
			// player paddle, reverse X direction
			currentDir.x *= -1;
		}
		else if( other.tag == "Goal" )
		{
			// reset the ball
			StartCoroutine( resetBall() );
			// inform goal of the score
			other.SendMessage( "GetPoint", SendMessageOptions.DontRequireReceiver );
		}
		
		// increase speed
		currentSpeed += SpeedIncrease;
		
		// clamp speed to maximum
		currentSpeed = Mathf.Clamp( currentSpeed, StartSpeed, MaxSpeed );
	}
	
	IEnumerator resetBall()
	{
		// reset position, speed, and direction
		resetting = true;
		transform.position = Vector3.zero;
		
		currentDir = Vector3.zero;
		currentSpeed = 0f;
		
		// wait for 3 seconds before starting the round
		yield return new WaitForSeconds( 3f );
		
		Start();
		
		resetting = false;
	}
}

*/

