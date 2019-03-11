using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeCycle : MonoBehaviour
{
    public TextMeshProUGUI foodText;
    public int foodCollected;
    public GameObject food;

	public float time;
	private int intTime;
	public int day;
	public TextMeshProUGUI displayDay;

    public List<GameObject> enemySpawners = new List<GameObject>();

    public List<GameObject> foodSpawners = new List<GameObject>();
    public List<int> randomFoodSpawnerIndex = new List<int>();

    void Start()
    {
        SpawnAllEnemies();

        SpawnAllFood();
    }

	void Update(){
		time += Time.deltaTime;
		intTime = Mathf.RoundToInt(time);

        foodText.text = "Food Collected: " + foodCollected.ToString();
		
		if(intTime == 180)
        {
            NextDay();
		}	
	}

    void NextDay()
    {
        day++;
        displayDay.text = "Day: " + (day).ToString("f0");
        time = 0;

        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");

        foreach (GameObject food in foods)
        {
            Destroy(food);
        }

        SpawnAllEnemies();

        SpawnAllFood();
    }

    void SpawnAllEnemies()
    {
        for (int i = 0; i < enemySpawners.Count; i++)
        {
            enemySpawners[i].GetComponent<EnemySpawner>().SpawnMonsters();
        }
    }

    void SpawnAllFood()
    {
        //for (int i = 0; i < 3; i++)
        //{
        //    randomFoodSpawnerIndex[i] = Random.Range(0, foodSpawners.Count);
        //}



        for(int i = 0; i < foodSpawners.Count; i++)
        {
            foodSpawners[i].GetComponent<FoodSpawner>().SpawnFood();
        }

        
    }
} 
