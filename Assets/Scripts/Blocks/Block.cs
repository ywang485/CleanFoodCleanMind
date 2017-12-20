using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Block : MonoBehaviour {

	public bool falling;

	public abstract string getBlockType ();

	// Use this for initialization
	void Start () {
		falling = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}