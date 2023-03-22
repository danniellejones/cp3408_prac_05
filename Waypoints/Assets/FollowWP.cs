using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWP : MonoBehaviour
{
    // Create a list of game objects indexed
    public GameObject[] waypoints;

    // Tracker is used to ensure the tanks follow the correct path
    GameObject tracker;

    // Current index of waypoint
    int currentWP = 0;
    // Speed of movement
    public float speed = 10.0f;
    public float rotSpeed = 10.0f;
    public float lookAhead = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Create visible for purposes of demo and destroy the collider
        tracker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        DestroyImmediate(tracker.GetComponent<Collider>());
        tracker.GetComponent<MeshRenderer>().enabled = false;
        // Set position and rotation of tracker
        tracker.transform.position = this.transform.position;
        tracker.transform.rotation = this.transform.rotation;   
    }

    void ProgressTracker()
    {
        // Make the tracker stop if it gets too far ahead
        if (Vector3.Distance(tracker.transform.position, this.transform.position) > lookAhead) return;
        
        // Get within distance of waypoint then move to next waypoint: change the distance to waypoint to change accuracy
        if (Vector3.Distance(tracker.transform.position, waypoints[currentWP].transform.position) < 3)
            currentWP++;
        // When reached last waypoint, go back to start
        if (currentWP >= waypoints.Length)
            currentWP = 0;

        tracker.transform.LookAt(waypoints[currentWP].transform);
        tracker.transform.Translate(0, 0, (speed + 2) * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        ProgressTracker();
        
        // Snap to tank turning
        //this.transform.LookAt(waypoints[currentWP].transform);
        
        // Quaternion turning thank smoothly
        Quaternion lookatWP = Quaternion.LookRotation(tracker.transform.position - this.transform.position);
        this.transform.rotation = Quaternion.Slerp(transform.rotation, lookatWP, rotSpeed * Time.deltaTime);

        // Pushing tank forward
        this.transform.Translate(0, 0, speed * Time.deltaTime);

    }
}
