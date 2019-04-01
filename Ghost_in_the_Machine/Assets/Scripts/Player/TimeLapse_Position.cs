using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLapse_Position : MonoBehaviour
{
    private GameObject player;

    private float timeLapseDelay = 3.0f;
    private float changeTimeLapsePositionCooldown = 0.2f;
    private float timer = 0;

    [SerializeField] private int currentIndex = 0;
    [SerializeField] private int currentTimeLapseIndex = 0;

    [SerializeField] private int[] currentPlayerHealth = new int[15];
    [SerializeField] public int timeLapsePlayerHealth;

    [SerializeField] private Vector2[] currentPlayerPosition = new Vector2[15];
    [SerializeField] public Vector2 timeLapsePosition;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.gameObject;
        timeLapsePosition = player.transform.position;
        timeLapsePlayerHealth = player.GetComponent<Player>().Health;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= changeTimeLapsePositionCooldown)
        {
            timer = 0;
            StartCoroutine(ChangeTimeLapsePosition());
        }
    }

    private IEnumerator ChangeTimeLapsePosition()
    {
        currentPlayerPosition[currentIndex] = player.transform.position;
        currentPlayerHealth[currentIndex] = player.GetComponent<Player>().Health;

        yield return new WaitForSeconds(timeLapseDelay);

        timeLapsePosition = currentPlayerPosition[currentTimeLapseIndex];
        timeLapsePlayerHealth = currentPlayerHealth[currentTimeLapseIndex];

        if(currentIndex == currentPlayerPosition.Length - 1)
        {
            currentIndex = 0;
        }
        else
        {
            currentIndex++;
        }
        yield return new WaitForSeconds(timeLapseDelay);
        if(currentTimeLapseIndex == currentPlayerPosition.Length - 1)
        {
            currentTimeLapseIndex = 0;
        }
        else
        {
            currentTimeLapseIndex++;
        }
    }
}
