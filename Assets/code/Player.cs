using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
	// how fast the paddle can move
	public float MoveSpeed;
	
	//public int playerNum;
	
	public int growSize = 2;
	public int score = 0;
	
	// whether this paddle can accept player input
	public bool AcceptsInput = true;

	private GameProcess gp; //= GameObject.Find("GameProcess").GetComponent<GameProcess>(); 

	float input; 
	float input2;
	public Vector3 pos;

	public bool resetting = false;

	public TextMesh scoreDisplay;


	void Start()
	{

		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>(); 

		scoreDisplay.transform.position = this.transform.position;
		//p = GameObject.Find("Player");

		MoveSpeed = 10f;

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
			StartCoroutine( resetBall());

			print ("\n\nHIT WALL");

			GameObject p = GameObject.Find("Player");
			pos =  Vector3.zero;

			this.transform.localScale = new Vector3(2,2,2);
			score = 0;

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

			score += growSize;

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


