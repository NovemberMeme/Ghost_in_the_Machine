using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject food;

    public void SpawnFood()
    {
        Instantiate(food, transform.position, Quaternion.identity);
    }
}
