using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockComb : MonoBehaviour {

	public static readonly string taskBlock = "Prefabs/TaskBlock";
	public static readonly string junkFoodBlock = "Prefabs/JunkFoodBlock";
	public static readonly string healthyFoodBlock = "Prefabs/HealthyFoodBlock";
	public static readonly string entertainmentBlock = "Prefabs/EntertainmentBlock";

	// Time since last gravity tick
	float lastFall = 0;
	float fallDuration = 1.0f;

	bool isValidGridPos() {        
		foreach (Transform child in transform) {
			Vector2 v = GameManager.roundVec2(child.position);

			// Not inside Border?
			if (!GameManager.insideBorder(v))
				return false;

			// Block in grid cell (and not part of same group)?
			if (GameManager.grid[GameManager.turnXPos2ColIdx((int)v.x), GameManager.turnYPos2RowIdx((int)v.y)] != null &&
				GameManager.grid[GameManager.turnXPos2ColIdx((int)v.x), GameManager.turnYPos2RowIdx((int)v.y)].gameObject.transform.parent != transform)
				return false;
		}
		return true;
	}

	void updateGrid() {
		// Remove old children from grid
		for (int y = 0; y < GameManager.h; ++y)
			for (int x = 0; x < GameManager.w; ++x)
				if (GameManager.grid[x, y] != null)
				if (GameManager.grid[x, y].transform.parent == transform)
					GameManager.grid[x, y] = null;

		// Add new children to grid
		foreach (Transform child in transform) {
			Vector2 v = GameManager.roundVec2(child.position);
			GameManager.grid[GameManager.turnXPos2ColIdx((int)v.x), GameManager.turnYPos2RowIdx((int)v.y)] = child.GetComponent<Block>();
		}

		//GameManager.printRemovableAreas ();

	}

	Block generateOneBlock() {
		string toBeGenerated = "";
		// First, Decide whether or not to generate food blocks
		int r1 = Random.Range(1, 100);
		if (r1 < GameManager.hunger) {
			// Generate food block
			int r3 = Random.Range(1, 100);
			if (r3 < 50) {
				// Generate Healthy Food Block
				toBeGenerated = healthyFoodBlock;
			} else {
				// Generate Junk Food Block
				toBeGenerated = junkFoodBlock;
			}
		} else {
			int r2 = Random.Range (1, 100);
			if (r2 < 100 - GameManager.mood) {
				// Generate junk food block
				toBeGenerated = junkFoodBlock;
			} else {
				int r4 = Random.Range (1, 100);
				if (r4 < 50) {
					// Generate Task Blocks
					toBeGenerated = taskBlock;
				} else {
					// Generate Entertainment Block
					toBeGenerated = entertainmentBlock;
				}
			}
		}
		return Instantiate (Resources.Load(toBeGenerated, typeof(GameObject)) as GameObject,
			this.transform,false).GetComponent<Block>();
	}

	// Use this for initialization
	void Start () {
		// Generate Block According to Current State
		for (int i = 0; i < 2; i++) {
			Block blk = generateOneBlock ();
			blk.transform.localPosition = new Vector2 (0, -i);
			blk.GetComponent<SpriteRenderer> ().sortingOrder = 3;
		}
		// Default position not valid? Then it's game over
		//if (!isValidGridPos()) {
		//	Debug.Log("GAME OVER");
		//	Destroy(gameObject);
		//}
	}
	
	// Update is called once per frame
	void Update() {
		// Move Left
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			GameManager.playSFX (GameManager.rotateBlockSFX);
			// Modify position
			transform.position += new Vector3(-1, 0, 0);

			// See if valid
			if (isValidGridPos())
				// It's valid. Update grid.
				updateGrid();
			else
				// It's not valid. revert.
				transform.position += new Vector3(1, 0, 0);
		}

		// Move Right
		else if (Input.GetKeyDown(KeyCode.RightArrow)) {
			GameManager.playSFX (GameManager.rotateBlockSFX);
			// Modify position
			transform.position += new Vector3(1, 0, 0);

			// See if valid
			if (isValidGridPos())
				// It's valid. Update grid.
				updateGrid();
			else
				// It's not valid. revert.
				transform.position += new Vector3(-1, 0, 0);
		}

		// Rotate
		else if (Input.GetKeyDown(KeyCode.UpArrow)) {
			GameManager.playSFX (GameManager.rotateBlockSFX);
			transform.Rotate(0, 0, -90);

			// See if valid
			if (isValidGridPos())
				// It's valid. Update grid.
				updateGrid();
			else
				// It's not valid. revert.
				transform.Rotate(0, 0, 90);
		}

		// Move Downwards and Fall
		else if (Input.GetKeyDown(KeyCode.DownArrow) ||
			Time.time - lastFall >= fallDuration) {
			GameManager.playSFX (GameManager.rotateBlockSFX);
			// Modify position
			transform.position += new Vector3(0, -1, 0);

			// See if valid
			if (isValidGridPos()) {
				// It's valid. Update grid.
				updateGrid();
			} else {
				// It's not valid. revert.
				transform.position += new Vector3(0, 1, 0);

				foreach (Transform child in transform) {
					child.GetComponent<Block> ().falling = false;
				}

				if (GameManager.h - GameManager.turnYPos2RowIdx (Mathf.RoundToInt(transform.position.y)) > GameManager.clearHeight) {
					for (int i = 0; i < GameManager.Row2Clear; i++) {
						GameManager.deleteRow (GameManager.h - 1);
						GameManager.decreaseRowsAbove (GameManager.h - 1);
					}
				}
				GameManager.dropAllBlocks ();
				GameManager.detectRemovableAreas ();
				while (GameManager.junkFoodAreaExist()) {
					GameManager.clearAllRemovableJunkFood ();
					// DropBlocksInTheAir
					GameManager.dropAllBlocks ();
					GameManager.detectRemovableAreas ();
				}
				GameManager.placeBlockEliminatingBtn ();

				// Spawn next Group
				FindObjectOfType<Spawner>().spawnNext();

				// Disable script
				enabled = false;

			}

			lastFall = Time.time;
		}
	}
}
