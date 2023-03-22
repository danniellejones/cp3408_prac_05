using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWP : MonoBehaviour
{
    // Create a list of game objects indexed
    public GameObject[] waypoints;
    // Current index of waypoint
    int currentWP = 0;
    // Speed of movement
    public float speed = 10.0f;
    public float rotSpeed = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get within distance of waypoint then move to next waypoint: change the distance to waypoint to change accuracy
        if (Vector3.Distance(this.transform.position, waypoints[currentWP].transform.position) < 3)
            currentWP++;
        // When reached last waypoint, go back to start
        if (currentWP >= waypoints.Length)
            currentWP = 0;
        
        // Snap to tank turning
        //this.transform.LookAt(waypoints[currentWP].transform);
        
        // Quaternion turning thank smoothly
        Quaternion lookatWP = Quaternion.LookRotation(waypoints[currentWP].transform.position - this.transform.position);
        this.transform.rotation = Quaternion.Slerp(transform.rotation, lookatWP, rotSpeed * Time.deltaTime);

        // Pushing tank forward
        this.transform.Translate(0, 0, speed * Time.deltaTime);

    }
}
