using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalState : MonoBehaviour
{
    private GameObject fastTravelUi;
    private GameObject player;
    [SerializeField]List<GameObject> portals = new List<GameObject>();
    private int waypointSelected;
    private Dropdown waypoints;
   /// private string lastPortal;
    
    void Awake()
    {
        player = GameObject.Find("player");
        fastTravelUi = GameObject.Find("FastTravel_UI");
        waypoints = fastTravelUi.GetComponentInChildren<Dropdown>();
        fastTravelUi.SetActive(false);
    }

    void Update ()
    {
        waypointSelected = waypoints.value;
    }

    public void addToPortals (GameObject newPortal)
    {
        if(portals.Contains(newPortal) == false)
        {
            portals.Add(newPortal);
            waypoints.options.Add(new Dropdown.OptionData(newPortal.name));
        }
    }
    public void DisplayUI ()
    {
        fastTravelUi.SetActive(true);
    }

    public void HideUI ()
    {
        fastTravelUi.SetActive(false);
    }
    
    public void Teleport ()
    {
        player.transform.position = portals[waypointSelected].transform.position;
    }
}
