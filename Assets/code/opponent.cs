using UnityEngine;
using System.Collections;

public class opponent : MonoBehaviour 
{

	// how fast the paddle can move
	public float MoveSpeed;
	
	public int opponentNum;
	
	public int growSize = 2;
	public int score = 0;

	
	private GameProcess gp;
	
	float vert; //input; 
	float horiz; //input2;
	public Vector3 pos;
	
	public bool resetting = false;
	public float move = 1.0f;

    int opponentScore = 0;
	
	//public TextMesh scoreDisplay;
    
    void OnGUI()
    {
        if (opponentNum == 1)
        {
            GUI.Box(new Rect(Screen.width - 200, opponentNum * 100, 100, 100), "Player " + opponentNum + " High score: " + opponentScore);
        }
    }
    
	void Start()
	{
		
		gp = GameObject.Find("GameProcess").GetComponent<GameProcess>(); 
		
		//scoreDisplay.transform.position = this.transform.position;

		
		MoveSpeed = 10f;
		//opponentNum = 0;
		
	}
	
	void Update()
	{
		//scoreDisplay.text = score.ToString();

		pos.z += vert * MoveSpeed * Time.deltaTime;
		pos.x += horiz *MoveSpeed * Time.deltaTime;	
		
		this.transform.position = pos;
		//scoreDisplay.transform.position = pos;
	}
	
	void OnTriggerEnter( Collider c )
	{
	  ///*	
		if(c.tag == "Wall")
		{

			switch(this.opponentNum) //(tempClient.clientNumber)
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
			
		}
		//*/
		if(c.tag == "Pellet")
		{
			//print("size of pellet array(BEFORE): " +gp.pellets.Count + " c NAME: " + c.gameObject.name);

			Pellets tempPell = (Pellets)c.GetComponent("Pellets");
			//gp.pellets.Remove(tempPell);
			//Destroy(c.gameObject);

			//gp.pellets.RemoveAt(0); //(Convert.ToInt32(c.gameObject.name));


		 /*
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
		 */
		}

	}
	
}



