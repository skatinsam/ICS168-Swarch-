
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
