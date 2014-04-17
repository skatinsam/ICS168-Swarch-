using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour
{
	// how fast the paddle can move
	public float MoveSpeed = 10f;
	
	//public int playerNum;
	

	
	// whether this paddle can accept player input
	public bool AcceptsInput = true;
	
	public double thresholdMove;
	
	private GameProcess gp; 
	public float timeLerp;
	public float moveDiff;
	//public GameObject p;
	public bool hasPadNum;
	Vector3 lastPos;
	float input; 
	float lastInput;
	public Vector3 pos;
	
	// new additions

	public bool sendPacket;
	public double change;
	
	void Start()
	{
		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>();
		hasPadNum = false;
		//thresholdMove = .2; //.5;
	
		
		//***** co-rutine
		//StartCoroutine(SendDelay());
	}
	
	void Update()   /// ****** try fixed update to give better change vaule
	{
	  /*	
		// does not accept input, abort
		if( !AcceptsInput )
			return;
		

			
			//if(Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow) )
			//{
			input = Input.GetAxis( "Vertical" );
			//}
			//if(Input.GetKeyUp(KeyCode.UpArrow)||Input.GetKeyUp(KeyCode.DownArrow))
			//{
			//	lastInput = input;
			//}
			
			
			lastPos = p.transform.position;
			

			// move paddle
			//Vector3 pos = p.transform.position;
			pos.z += input * MoveSpeed * Time.deltaTime;
			//input
			// clamp paddle position
		p.transform.position = pos;
    */
		
	}
 }