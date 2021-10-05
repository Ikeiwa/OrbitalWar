using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PathFinder : MonoBehaviour
{
    [System.Serializable]
    public class Node
    {
        public Vector3 position;
        public Vector3 up;
        public List<int> connections;
        public int type = 0;

        public Node()
        {
            connections = new List<int>();
        }
    }

    public struct Face
    {
        public int[] vertices;

        public Face(int a, int b, int c, int d)
        {
            vertices = new int[] { a, b, c, d };
        }
    }

    class AStarNode
    {
        public Node node;
        public AStarNode parent;
        public float cost;

        public AStarNode(Node node, AStarNode parent,float cost)
        {
            this.node = node;
            this.parent = parent;
            this.cost = cost;
        }
    }

    public enum SearchAlgo
    {
        Dijkstra,
        AStar
    }

    public List<Node> graph;
    public Mesh previewNodeMesh;
    public Mesh previewLinkMesh;
    public SearchAlgo searchMode;

    private List<Node> testPath;

    private Mesh graphPreview;
    public static PathFinder instance;


    public static List<Node> GenerateGraph(Mesh mesh)
    {
        List<Node> graph = new List<Node>();

        int[] indices = mesh.GetIndices(0);

        List<Color32> colors = new List<Color32>();

        mesh.GetColors(colors);

        List<Face> nodeFace = new List<Face>();

        for (int i = 0; i < indices.Length; i += 4)
        {
            Vector3 center = mesh.vertices[indices[i]];
            center += mesh.vertices[indices[i + 1]];
            center += mesh.vertices[indices[i + 2]];
            center += mesh.vertices[indices[i + 3]];
            center /= 4.0f;

            Vector3 normal = mesh.normals[indices[i]];
            normal += mesh.normals[indices[i]];
            normal += mesh.normals[indices[i]];
            normal += mesh.normals[indices[i]];
            normal.Normalize();

            int type = 0;
            if (mesh.colors32.Length > 0)
            {
                Color32 col = mesh.colors32[indices[i]];
                col = Color32.Lerp(col, mesh.colors32[indices[i + 1]], 0.5f);
                col = Color32.Lerp(col, mesh.colors32[indices[i + 2]], 0.5f);
                col = Color32.Lerp(col, mesh.colors32[indices[i + 3]], 0.5f);

                if (col.r > 225)
                    type = 1;
                else if (col.g > 225)
                    type = 2;
            }


            graph.Add(new Node
            {
                position = center + normal * 0.5f,
                up = normal,
                type = type
            });

            nodeFace.Add(new Face(indices[i], indices[i + 1], indices[i + 2], indices[i + 3]));
        }

        for (int i = 0; i < graph.Count; i++)
        {
            for (int t = 0; t < graph.Count; t++)
            {
                if (t == i) continue;
                Vector3 dir = graph[t].position - graph[i].position;
                if (dir.sqrMagnitude > 12) continue;
                if (Physics.SphereCast(new Ray(graph[i].position, dir), 0.1f, dir.magnitude, (1 << 6) | (1 << 8))) continue;

                for (int va = 0; va < 4; va++)
                {
                    for (int vb = 0; vb < 4; vb++)
                    {
                        if (nodeFace[i].vertices[va] == nodeFace[t].vertices[vb])
                        {
                            //nodes[i].connections.Add(new Connection { length = dir.sqrMagnitude, target = nodes[t] });
                            graph[i].connections.Add(t);
                            continue;
                        }
                        else if (mesh.vertices[nodeFace[i].vertices[va]] == mesh.vertices[nodeFace[t].vertices[vb]])
                        {
                            //nodes[i].connections.Add(new Connection { length = dir.sqrMagnitude, target = nodes[t] });
                            graph[i].connections.Add(t);
                            continue;
                        }
                    }
                }

            }
        }

        return graph;
    }

    public void SetupGraph(Mesh mesh)
    {
        graph = GenerateGraph(mesh);

        //GeneratePreview();
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void LoadGraph(List<Node> newGraph)
    {
        graph = newGraph;
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshRenderer>().enabled = false;
    }

    public Mesh CopyMesh(Mesh mesh,Color col)
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = mesh.vertices;
        newMesh.triangles = mesh.triangles;
        newMesh.normals = mesh.normals;
        newMesh.SetColors(Enumerable.Repeat(col, newMesh.vertexCount).ToList());
        return newMesh;
    }

    public void GeneratePreview()
    {
        graphPreview = new Mesh();
        graphPreview.indexFormat = IndexFormat.UInt32;

        List<CombineInstance> meshes = new List<CombineInstance>();

        Mesh blackNode = CopyMesh(previewNodeMesh, Color.black);
        Mesh redNode = CopyMesh(previewNodeMesh, Color.red);
        Mesh greenNode = CopyMesh(previewNodeMesh, Color.green);

        Mesh blackLink = CopyMesh(previewLinkMesh, Color.black);
        Mesh redLink = CopyMesh(previewLinkMesh, Color.red);
        Mesh greenLink = CopyMesh(previewLinkMesh, Color.green);

        for (int i=0;i<graph.Count;i++)
        {
            Mesh nodeMesh = blackNode;
            Mesh linkMesh = blackLink;
            switch (graph[i].type)
            {
                case 1: nodeMesh = redNode; linkMesh = redLink; break;
                case 2: nodeMesh = greenNode; linkMesh = greenLink; break;
            }

            meshes.Add(new CombineInstance { mesh = nodeMesh, transform = Matrix4x4.TRS(graph[i].position, Quaternion.identity, new Vector3(0.1f, 0.1f, 0.1f)) });

            for(int l = 0; l < graph[i].connections.Count; l++)
            {
                Vector3 dir = graph[graph[i].connections[l]].position - graph[i].position;
                meshes.Add(new CombineInstance { mesh = linkMesh, transform = Matrix4x4.TRS(graph[i].position, Quaternion.LookRotation(dir, graph[i].up), new Vector3(0.5f,0.5f, dir.magnitude/2)) });
            }
        }

        graphPreview.CombineMeshes(meshes.ToArray());

        GetComponent<MeshFilter>().mesh = graphPreview;
        GetComponent<MeshRenderer>().enabled = false;

    }

    public Node GetClosestNode(Vector3 pos)
    {
        Node closest = null;
        float minDist = float.MaxValue;

        foreach(Node node in graph)
        {
            float dist = (node.position - pos).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    public IEnumerator GetShortestPath(Node start, Node end, System.Action<List<Node>> callback, int type = 0)
    {
        switch (searchMode)
        {
            case SearchAlgo.Dijkstra: 
                yield return GetShortestPathDijkstra(start, end,(path) => { callback(path); } ,type); 
                break;
            case SearchAlgo.AStar: 
                yield return GetShortestPathAStart(start, end, (path) => { callback(path); }, type); 
                break;
        }
        yield return null;
    }

    public IEnumerator GetShortestPathDijkstra(Node start, Node end, System.Action<List<Node>> callback, int type = 0)
    {
        List<Node> path = new List<Node>();

        if(start == end)
        {
            path.Add(start);
            callback(path);
        }

        List<Node> unvisited = graph.ToList();
        Dictionary<Node, Node> visited = new Dictionary<Node, Node>();

        List<float> values = Enumerable.Repeat(float.MaxValue, unvisited.Count).ToList();
        Dictionary<Node, float> distances = graph.Zip(values, (k, v) => new { k, v })
                                            .ToDictionary(x => x.k, x => x.v); ;

        distances[start] = 0;
        while (unvisited.Count != 0)
        {
            unvisited = unvisited.OrderBy(node => distances[node]).ToList();
            Node current = unvisited[0];
            unvisited.Remove(current);

            if (current == end)
            {
                while (visited.ContainsKey(current))
                {
                    path.Insert(0, current);
                    current = visited[current];
                }

                path.Insert(0, current);
                break;
            }

            for (int i = 0; i < current.connections.Count; i++)
            {
                Node neighbor = graph[current.connections[i]];
                float length = Vector3.Distance(current.position, neighbor.position);

                if(type != 0 && neighbor.type != 0 && neighbor.type != type)
                    length *= 4;

                float other = distances[current] + length;

                if (other < distances[neighbor])
                {
                    distances[neighbor] = other;
                    visited[neighbor] = current;
                }
            }
            yield return null;
        }

        yield return null;
        callback(path);
    }

    public IEnumerator GetShortestPathAStart(Node start, Node end, System.Action<List<Node>> callback, int type = 0)
    {
        List<Node> path = new List<Node>();
        if (start == end)
        {
            path.Add(start);
            callback(path);
        }

        List<AStarNode> openNodes = new List<AStarNode>();
        List<AStarNode> closedNodes = new List<AStarNode>();

        openNodes.Add(new AStarNode(start,null,0));

        AStarNode current = openNodes[0];

        while(openNodes.Count > 0)
        {
            current = openNodes.MinOf(n => n.cost);
            if (current.node == end)
                break;

            closedNodes.Add(current);
            openNodes.Remove(current);

            foreach (int neighborIndex in current.node.connections)
            {
                Node neighbor = graph[neighborIndex];

                if (!openNodes.Any(n => n.node == neighbor) && !closedNodes.Any(n => n.node == neighbor))
                {
                    float nodeDistance = (neighbor.position - current.node.position).sqrMagnitude;
                    if (type != 0 && neighbor.type != 0 && neighbor.type != type)
                        nodeDistance *= 4;

                    float cost = nodeDistance + (end.position - neighbor.position).sqrMagnitude;
                    openNodes.Add(new AStarNode(neighbor, current, cost));
                }
            }
            yield return null;
        }
        

        if (openNodes.Count > 0)
        {
            while (current.parent != null)
            {
                path.Add(current.node);
                current = current.parent;
            }

            path.Reverse();
        }

        path.Insert(0, start);

        callback(path);
        yield return null;
    }

    private void Awake()
    {
        if (instance != null)
            Destroy(this);

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKey(KeyCode.Return))
        {
            testPath = GetShortestPath(graph[Random.Range(0, graph.Count - 1)], graph[Random.Range(0, graph.Count - 1)]);

            List<Vector3> positions = new List<Vector3>();
            foreach (Node node in testPath)
                positions.Add(node.position);

            GetComponent<LineRenderer>().positionCount = positions.Count;
            GetComponent<LineRenderer>().SetPositions(positions.ToArray());
        }*/

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (GetComponent<MeshFilter>().sharedMesh == null)
                GeneratePreview();
            GetComponent<MeshRenderer>().enabled = !GetComponent<MeshRenderer>().enabled;
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (searchMode == SearchAlgo.Dijkstra)
                searchMode = SearchAlgo.AStar;
            else
                searchMode = SearchAlgo.Dijkstra;
        }
    }

#if UNITY_EDITOR

    public static void DrawThickLine(Vector3 start, Vector3 end, float thickness)
    {
        Camera c = Camera.current;
        if (c == null) return;

        // Only draw on normal cameras
        if (c.clearFlags == CameraClearFlags.Depth || c.clearFlags == CameraClearFlags.Nothing)
        {
            return;
        }

        // Only draw the line when it is the closest thing to the camera
        // (Remove the Z-test code and other objects will not occlude the line.)
        var prevZTest = Handles.zTest;
        Handles.zTest = CompareFunction.LessEqual;

        Handles.color = Gizmos.color;
        Handles.DrawAAPolyLine(thickness * 10, new Vector3[] { start, end });

        Handles.zTest = prevZTest;
    }

    /*private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        Handles.zTest = CompareFunction.LessEqual;

        if(graph != null)
        {
            foreach (Node node in graph)
            {
                switch (node.type)
                {
                    case 0:Gizmos.color = Color.black;break;
                    case 1:Gizmos.color = Color.red;break;
                    case 2:Gizmos.color = Color.green;break;
                }
                Handles.color = Gizmos.color;
                Gizmos.DrawWireSphere(node.position, 0.1f);

                foreach(Node connection in node.connections)
                {
                    //Gizmos.DrawLine(node.position, connection.position);
                    Handles.DrawLine(node.position, connection.position);
                }
            }
        }

        if(testPath != null && testPath.Count>1)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(testPath[0].position, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(testPath.Last().position, 0.2f);

            Gizmos.color = Color.blue;
            for (int i=0;i<testPath.Count-1;i++)
            {
                DrawThickLine(testPath[i].position, testPath[i+1].position, 1);
            }
        }
        
    }*/
#endif
}
