using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public static readonly string blockComb = "Prefabs/BlockComb";

	public void spawnNext() {
		if (!GameManager.gameEnd) {
			Instantiate (Resources.Load (blockComb, typeof(GameObject)) as GameObject,
				transform.position,
				Quaternion.identity);
		}
	}

	// Use this for initialization
	void Start () {
		spawnNext();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
