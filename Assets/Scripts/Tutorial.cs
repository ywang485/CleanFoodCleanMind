using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour {

	public GameObject[] pages;
	private int currPage = 0;

	// Use this for initialization
	void Start () {
		pages [0].SetActive (true);
		currPage += 1;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void goToNext() {
		if (currPage < pages.Length) {
			pages [currPage - 1].SetActive (false);
			pages [currPage].SetActive (true);
			currPage += 1;
		} else {
			SceneManager.LoadScene ("GamePlay");
		}
	}
}
