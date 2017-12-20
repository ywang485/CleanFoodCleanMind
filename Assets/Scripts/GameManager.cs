using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public readonly static int w = 10;
	public readonly static int h = 9;
	public readonly static int clearHeight = 6;
	public readonly static int Row2Clear = 2;
	public readonly static int caloriesUpperBound = 3500;

	public static Block[,] grid = new Block[w, h];
	public static int taskProgress = 0;
	public static int hunger = 50;
	public static int mood = 50;
	public static int caloriesIntake = 0;

	public readonly static int numConnected2Remove = 4;

	public static ArrayList removableAreas;
	public static HashSet<string> removableBlocks;

	public static readonly string path2BlockAreaEliminationBtn = "Prefabs/EliminateBlockAreaBtn";

	private Text taskProgressIndicator;
	private Text hungerIndicator;
	private Text moodIndicator;
	private Text caloriesIntakeIndicator;

	// Audio Clips
	public static readonly string rotateBlockSFX = "SFX/RotateBlock";
	public static readonly string junkFoodBlockBreakSFX = "SFX/JunkFoodBlockExplode";
	public static readonly string removeBlockSFX = "SFX/RemoveBlock";

	public GameObject gameResultPanel;

	public static bool gameEnd = false;

	public void restartGame() {
		hunger = 50;
		mood = 50;
		taskProgress = 0;
		caloriesIntake = 0;
		gameEnd = false;
		SceneManager.LoadScene ("Openning");
	}

	public static void playSFX(string SFXPath) {
		GameObject.Find("GameManager").GetComponent<AudioSource>().PlayOneShot(Resources.Load(SFXPath, typeof(AudioClip)) as AudioClip, 1f);
	}

	public static Vector2 roundVec2(Vector2 v) {
		return new Vector2(Mathf.Round(v.x),
			Mathf.Round(v.y));
	}

	public static bool insideBorder(Vector2 pos) {
		return ((int)pos.x >= 0 &&
			(int)pos.x < w &&
			(int)pos.y >= -8) &&
			(int)pos.y <= 0;
	}

	public static bool insideBorderIdx(Vector2 pos) {
		return ((int)pos.x >= 0 &&
		(int)pos.x < w &&
		(int)pos.y >= 0 &&
		(int)pos.y < h);
	}

	public static int turnXPos2ColIdx(int x) {
		return x;
	}

	public static int turnYPos2RowIdx(int y) {
		return -y;
	}

	public static void printRemovableAreas() {
		Debug.Log ("Removable Areas:");
		foreach (HashSet<string> area in removableAreas) {
			Debug.Log ("Area:");
			foreach (string pos in area) {
				Debug.Log (pos);
			}
		}
	}

	public static bool checkBlockType(Vector2 pos, string blockType) {
		if (insideBorderIdx(new Vector2 (pos.x, pos.y)) && grid [(int)pos.x, (int)pos.y] != null && !grid [(int)pos.x, (int)pos.y].falling &&grid [(int)pos.x, (int)pos.y].getBlockType () == blockType) {
			return true;
		} else {
			return false;
		}
	}

	public static void eliminateBlockArea(HashSet<string> area) {
		clearOneRemovableArea (area);
		dropAllBlocks ();
		detectRemovableAreas ();
		while (junkFoodAreaExist()) {
			clearAllRemovableJunkFood ();
			// DropBlocksInTheAir
			dropAllBlocks ();
			detectRemovableAreas ();
		}
		placeBlockEliminatingBtn ();
	}

	public static void clearAllRemovableJunkFood() {
		foreach (HashSet<string> area in removableAreas) {
			HashSet<string>.Enumerator e = area.GetEnumerator ();
			e.MoveNext();
			string[] pos = e.Current.Split(',');
			Block curr = grid [int.Parse (pos [0]), int.Parse (pos [1])];
			if (curr != null && curr.getBlockType () == "JunkFood") {
				clearOneRemovableArea(area);
			}
		}
	}

	public static string getRemovableAreaType(HashSet<string> area) {
		HashSet<string>.Enumerator e = area.GetEnumerator ();
		e.MoveNext();
		string[] pos = e.Current.Split(',');
		Block curr = grid [int.Parse (pos [0]), int.Parse (pos [1])];
		if (curr != null) {
			return curr.getBlockType ();
		} else {
			return null;
		}
	}	

	public static bool junkFoodAreaExist() {
		foreach (HashSet<string> area in removableAreas) {
			if (getRemovableAreaType (area) == "JunkFood") {
				return true;
			}
		}
		return false;
	}

	public static void clearOneRemovableArea(HashSet<string> area) {
		string blockType = "";
		int s = area.Count;
		foreach (string posStr in area){
			string[] pos = posStr.Split(',');
			if (grid [int.Parse (pos [0]), int.Parse (pos [1])] != null) {
				blockType = grid [int.Parse (pos [0]), int.Parse (pos [1])].getBlockType ();
				Destroy (grid [int.Parse (pos [0]), int.Parse (pos [1])].gameObject);
				grid [int.Parse (pos [0]), int.Parse (pos [1])] = null;
			}
		}
		imposeBlockEliminationEffect (blockType, s);

	}

	public static void imposeBlockEliminationEffect(string blockType, int size) {
		if (blockType == "Task") {
			GameObject.Find ("Character").GetComponent<Character> ().enterWorkoutState ();
			taskProgress += size * 2 * (Mathf.CeilToInt((float)(size - 3)/2.0f));
			hunger += 2 * size;
			mood -= 2 * size;
			playSFX (removeBlockSFX);
		} else if (blockType == "HealthyFood") {
			GameObject.Find ("Character").GetComponent<Character> ().enterHealthyFoodEatingState ();
			hunger -= size * 2;
			mood += size;
			caloriesIntake += 30 * size;
			playSFX (removeBlockSFX);
		} else if (blockType == "JunkFood") {
			GameObject.Find ("Character").GetComponent<Character> ().enterJunkFoodEatingState();
			hunger -= size * 2;
			mood += size * 2;
			caloriesIntake += 100 * size;
			playSFX (junkFoodBlockBreakSFX);
		} else if (blockType == "Entertainment") {
			GameObject.Find ("Character").GetComponent<Character> ().enterEntertainmentState();
			hunger += size;
			mood += Mathf.RoundToInt((float)size * 1.5f);
			playSFX (removeBlockSFX);
		}

		if (hunger > 100) {
			hunger = 100;
		}
		if (hunger < 0) {
			hunger = 0;
		}
		if (mood > 100) {
			mood = 100;
		}
		if (mood < 0) {
			mood = 0;
		}
		if (caloriesIntake > caloriesUpperBound) {
			gameOver ();
		}
		if (taskProgress >= 100) {
			win ();
		}
	}

	public static void gameOver() {
		gameEnd = true;
		Debug.Log ("GAME OVER");
		GameObject.Find ("GameManager").GetComponent<GameManager> ().gameResultPanel.SetActive (true);
		GameObject.Find("GameManager").GetComponent<GameManager>().gameResultPanel.transform.GetComponentInChildren<Text>().text = "My Workout Plan Failed Again.\n Workout Progress: " + taskProgress + "\n(Click to Restart Game)";
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("EliminationBtns")) {
			Destroy (obj);
		}
	}

	public static void win() {
		gameEnd = true;
		Debug.Log ("YOU WIN!");
		GameObject.Find ("GameManager").GetComponent<GameManager> ().gameResultPanel.SetActive (true);
		GameObject.Find("GameManager").GetComponent<GameManager>().gameResultPanel.transform.GetComponentInChildren<Text>().text = "You win!\n Calorie Intake: " + caloriesIntake + "\n(Click to Restart Game)";
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("EliminationBtns")) {
			Destroy (obj);
		}
	}

	public static void placeBlockEliminatingBtn() {
		// Clear Old Buttons Placed
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("EliminationBtns")) {
			Destroy (obj);
		}
		foreach (HashSet<string> area in removableAreas) {
			foreach (string posStr in area) {
				string[] pos = posStr.Split (',');
				GameObject btn = Instantiate(Resources.Load (path2BlockAreaEliminationBtn, typeof(GameObject)) as GameObject,
					GameObject.Find("Canvas").transform,
					true);
				btn.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (turnXPos2ColIdx(int.Parse (pos [0])), turnYPos2RowIdx(int.Parse (pos [1])));
				btn.GetComponent<Button> ().onClick.AddListener (() => eliminateBlockArea(area));
			}
		}
	}

	public static void detectRemovableAreas () {
		removableAreas = new ArrayList ();
		removableBlocks = new HashSet<string> ();
		for (int y = 0; y < h; y++) {
			for (int x = 0; x < w; x++) {
				if (grid [x, y] != null && !grid[x,y].falling && !removableBlocks.Contains (x.ToString () + "," + y.ToString ())) {
					string blockType = grid [x, y].getBlockType ();
					Queue<string> frontier = new Queue<string> ();
					frontier.Enqueue (x.ToString() + "," + y.ToString());
					HashSet<string> area = new HashSet<string> ();
					while (frontier.Count > 0) {
						string curr = frontier.Dequeue ();
						area.Add (curr);
						string[] pos = curr.Split (',');
						if (checkBlockType(new Vector2(int.Parse(pos[0])-1, int.Parse(pos[1])), blockType) && !area.Contains((int.Parse (pos [0])-1).ToString() + "," + pos [1])) {
							frontier.Enqueue ((int.Parse (pos [0])-1).ToString() + "," + pos [1]);
						}
						if (checkBlockType(new Vector2(int.Parse(pos[0])+1, int.Parse(pos[1])), blockType) && !area.Contains((int.Parse (pos [0])+1).ToString() + "," + pos [1])) {
							frontier.Enqueue ((int.Parse (pos [0])+1).ToString() + "," + pos [1]);
						}
						if (checkBlockType(new Vector2(int.Parse(pos[0]), int.Parse(pos[1]) + 1), blockType) && !area.Contains(pos [0].ToString() + "," + (int.Parse(pos [1]) + 1).ToString())) {
							frontier.Enqueue (pos [0].ToString() + "," + (int.Parse(pos [1]) + 1).ToString());
						}
						if (checkBlockType(new Vector2(int.Parse(pos[0]), int.Parse(pos[1]) - 1), blockType) && !area.Contains(pos [0].ToString() + "," + (int.Parse(pos [1]) - 1).ToString())) {
							frontier.Enqueue (pos [0].ToString() + "," + (int.Parse(pos [1]) - 1).ToString());
						}
					}

					if (area.Count >= numConnected2Remove) {
						removableAreas.Add (area);
						foreach(string pos in area) {
							removableBlocks.Add (pos);
						}
					}
				}
			}
		}
	}

	public static void dropAllBlocks() {
		for (int y = h-1; y >= 0; y--) {
			dropOneRowOfBlocks (y);
		}
	}

	public static void dropOneRowOfBlocks(int y) {
		for (int x = 0; x < w; x++) {
			if (grid [x, y] == null) {
				continue;
			}
			if (grid [x, y].falling) {
				continue;
			}
			int y0 = y;
			while (insideBorderIdx(new Vector2(x, y0 + 1)) && grid [x, y0 + 1] == null) {
				y0 += 1;
			}
			if (y0 == y) {
				continue;
			}
			grid [x, y0] = grid [x, y];
			grid [x, y] = null;
			grid [x, y0].transform.position += new Vector3 (0, y - y0, 0);
		}
	}

	public static void deleteRow(int y) {
		for (int x = 0; x < w; ++x) {
			if (grid [x, y] != null) {
				Destroy (grid [x, y].gameObject);
				grid [x, y] = null;
			}
		}
	}

	public static void decreaseRow(int y) {
		for (int x = 0; x < w; ++x) {
			if (grid[x, y] != null && !grid[x, y].falling) {
				// Move one towards bottom
				grid[x, y+1] = grid[x, y];
				grid[x, y] = null;

				// Update Block position
				grid[x, y+1].transform.position += new Vector3(0, -1, 0);
			}
		}
	}

	public static void decreaseRowsAbove(int y) {
		for (int i = h - 1; i >= 0; --i)
			decreaseRow(i);
	}

	public static bool isRowFull(int y) {
		for (int x = 0; x < w; ++x)
			if (grid[x, y] == null)
				return false;
		return true;
	}

	public static void deleteFullRows() {
		for (int y = 0; y < h; ++y) {
			if (isRowFull(y)) {
				deleteRow(y);
				decreaseRowsAbove(y+1);
				--y;
			}
		}
	}

	// Use this for initialization
	void Start () {
		taskProgressIndicator = GameObject.Find("TaskProgressIndicator").GetComponent<Text>();
		hungerIndicator = GameObject.Find("HungerIndicator").GetComponent<Text>();
		moodIndicator = GameObject.Find("MoodIndicator").GetComponent<Text>();
		caloriesIntakeIndicator = GameObject.Find("CaloriesIntakeIndicator").GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		taskProgressIndicator.text = "Workout Progress: " + taskProgress + "%";
		hungerIndicator.text = "Hunger: " + hunger + "%";
		moodIndicator.text = "Mood: " + mood + "%";
		caloriesIntakeIndicator.text = "Calories Intake: " + caloriesIntake;
		GameObject.Find ("HungerBarContent").transform.localScale = new Vector3(((float)hunger/100.0f) * 0.51f, 1f, 1f);
		GameObject.Find ("MoodBarContent").transform.localScale = new Vector3(((float)mood/100.0f) * 0.51f, 1f, 1f);
		GameObject.Find ("TaskProgressBarContent").transform.localScale = new Vector3(((float)taskProgress/100.0f) * 1.0f, 1f, 1f);
		GameObject.Find ("CaloriesIntakeBarContent").transform.localScale = new Vector3(((float)caloriesIntake/(float)caloriesUpperBound) * 0.51f, 1f, 1f);
	}
}
