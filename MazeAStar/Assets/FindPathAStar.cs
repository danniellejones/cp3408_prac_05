using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Use linq library to do sorting
using System.Linq;
using Unity.VisualScripting;

// Track the map positions
public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    // Compare if one path marker is the same as another - override for equals method
    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathMarker)obj).location);
        }
    }
    public override int GetHashCode()
    {
        return 0;
    }

}


public class FindPathAStar : MonoBehaviour
{
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    // Physical markers on map
    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;

    PathMarker lastPos;
    bool done = false;

    // Quickly remove the markers set on the path, start and end prefabs
    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
        {
            Destroy(m);
        }
    }

    void BeginSearch()
    {
        done = false;
        RemoveAllMarkers();

        // Add the maze spaces into a list - less obstructions; shuffle the list and pick off top
        List<MapLocation> locations = new List<MapLocation>();
        // Loop around the map
        for (int z = 1; z < maze.depth - 1; z++)
        {
            for (int x = 1; x < maze.width - 1; x++)
            {
                if (maze.map[x, z] != 1) // Add to list if not equal to 1
                {
                    locations.Add(new MapLocation(x, z));
                }
            }
        }
        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale, 0, locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0, 0, 0, Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0, Instantiate(end, goalLocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;
    }


    void Search(PathMarker thisNode)
    {
        if (thisNode == null) return;
        if (thisNode.Equals(goalNode)) { done = true; return; } // Goal found

        foreach (MapLocation dir in maze.directions)
        {
            MapLocation neighbour = dir + thisNode.location;
            // If it is not an open space - obstacle
            if (maze.map[neighbour.x, neighbour.z] == 1) continue;
            // If outside the map continue
            if (neighbour.x < 1 || neighbour.x >= maze.width || neighbour.z < 1 || neighbour.z >= maze.depth) continue;
            // If neighbour already in closed list
            if (IsClosed(neighbour)) continue;

            // Calculate the G, H, F - using pythagoreous 
            float G = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float H = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            float F = G + H;

            // Object to put on the path for a open spot
            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0, neighbour.z * maze.scale), Quaternion.identity);

            // Add text mesh numbers to the path markers
            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            values[0].text = "G: " + G.ToString("0.00");
            values[1].text = "H: " + H.ToString("0.00");
            values[2].text = "F: " + F.ToString("0.00");

            // Add the marker to open list by checking if it does exist first as only want to update existing one
            if (!UpdateMarker(neighbour, G, H, F, thisNode))
            {
                open.Add(new PathMarker(neighbour, G, H, F, pathBlock, thisNode));
            }
        }

        // Pick the next node - determine which by lowest F value or random - use linq library for sorting, then by H
        open = open.OrderBy(p => p.F).ThenBy(n => n.H).ToList<PathMarker>();
        PathMarker pm = (PathMarker)open.ElementAt(0);

        // Move marker from open to closed and change to close material
        closed.Add(pm);
        open.RemoveAt(0);
        pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos = pm;
    }

    // Check if marker does exist as only want to update existing one
    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker)
    {
        foreach (PathMarker p in closed)
        {
            if (p.location.Equals(marker)) return true;
        }
        return false;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Get the A* path by following parents back
    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker begin = lastPos;

        while (!startNode.Equals(begin) && begin != null)
        {
            // Create a path marker at lastPos
            Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0, begin.location.z * maze.scale), Quaternion.identity);
            // Step up to parent
            begin = begin.parent;
        }
        // Start Node does not have a parent so it needs to add path marker
        Instantiate(pathP, new Vector3(startNode.location.x * maze.scale, 0, startNode.location.z * maze.scale), Quaternion.identity);
    }


    // Update is called once per frame
    void Update()
    {
        // Show markers
        if (Input.GetKeyDown(KeyCode.P)) BeginSearch();
        if (Input.GetKeyDown(KeyCode.C) && !done) Search(lastPos);
        if (Input.GetKeyDown(KeyCode.M)) GetPath();

    }
}
