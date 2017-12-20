using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

	// States : 0 - thinking, 1 - workout, 2 - eating junk food, 3 - eating healthy food, 4 - entertained
	private int state = 0;
	public GameObject patrolStartPoint;
	public GameObject patrolEndingPoint;
	public GameObject workoutLoc;
	public GameObject junkFoodLoc;
	public GameObject healthyFoodLoc;
	public GameObject entertainmentLoc;

	private float activityStartTime = 0f;
	private float activityLength = 3f;

	private GameObject destination;
	private float speed = 1f; 
	private Animator animator;

	public GameObject workoutBgMask;

	public GameObject playGameAnimation;
	public GameObject makingHealthyFoodAnimation;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		enterThinkingState ();
	}
	
	// Update is called once per frame
	void Update () {
		if (destination != null) {
			Vector2 currPos = new Vector2 (transform.position.x, transform.position.y);
			transform.position = Vector2.MoveTowards(currPos, destination.transform.position, speed * Time.deltaTime);
			if (destination.transform.position.x > transform.position.x) {
				transform.localScale = new Vector2 (Mathf.Abs (transform.localScale.x), transform.localScale.y);
			} else {
				transform.localScale = new Vector2 (-Mathf.Abs (transform.localScale.x), transform.localScale.y);
			}
		}
		if (state == 0) {
		} else if (state == 1 || state == 2 || state == 3 || state == 4) {
			if (activityStartTime != 0f && Time.time > activityStartTime + activityLength) {
				activityStartTime = 0f;
				workoutBgMask.SetActive (false);
				playGameAnimation.SetActive (false);
				makingHealthyFoodAnimation.SetActive (false);
				GetComponent<SpriteRenderer> ().enabled = true;
				enterThinkingState ();
			}
		}
	}

	public void enterThinkingState() {
		state = 0;
		speed = 1.0f;
		animator.SetInteger ("State", 0);
		destination = patrolStartPoint;
	}

	public void enterWorkoutState() {
		state = 1;
		speed = 2.0f;
		animator.SetInteger ("State", 0);
		destination = workoutLoc;
	}

	public void enterJunkFoodEatingState() {
		state = 2;
		speed = 2.0f;
		animator.SetInteger ("State", 0);
		destination = junkFoodLoc;
	}

	public void enterHealthyFoodEatingState() {
		state = 3;
		speed = 2.0f;
		animator.SetInteger ("State", 0);
		destination = healthyFoodLoc;
	}

	public void enterEntertainmentState() {
		state = 4;
		speed = 2.0f;
		animator.SetInteger ("State", 0);
		destination = entertainmentLoc;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (state == 0) {
			if (other.tag == "PatrolStartWaypoint") {
				destination = patrolEndingPoint;
			} else if (other.tag == "PatrolEndWaypoint") {
				destination = patrolStartPoint;
			}
		} else if (state == 1) {
			if (other.tag == "WorkoutLoc") {
				workoutBgMask.SetActive (true);
				animator.SetInteger ("State", 1);
				activityStartTime = Time.time;
			}
		} else if (state == 2) {
			if (other.tag == "JunkFoodLoc") {
				animator.SetInteger ("State", 2);
				activityStartTime = Time.time;
			}
		} else if (state == 3) {
			if (other.tag == "HealthyFoodLoc") {
				makingHealthyFoodAnimation.SetActive (true);
				GetComponent<SpriteRenderer> ().enabled = false;
				activityStartTime = Time.time;
			}
		} else if (state == 4) {
			if (other.tag == "EntertainmentLoc") {
				playGameAnimation.SetActive (true);
				GetComponent<SpriteRenderer> ().enabled = false;
				activityStartTime = Time.time;
			}
		}
	}
}

