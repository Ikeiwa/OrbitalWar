using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PathFinder : MonoBehaviour
{
    public class Connection
    {
        public Node target;
        public float length = 1;
    }

    public class Node
    {
        public Vector3 position;
        public Vector3 up;
        public List<Connection> connections;
        public int type = 0;

        public Node()
        {
            connections = new List<Connection>();
        }
    }

    public List<Node> nodes;

    public void GenerateGraph(Mesh mesh)
    {
        nodes = new List<Node>();

        SubMeshDescriptor subMesh = mesh.GetSubMesh(0);
        int[] indices = mesh.GetIndices(0);

        Debug.Log(indices.Length / 4);

        List<int> nodeQuadIndex = new List<int>();

        for (int i=0;i< indices.Length; i += 4)
        {
            Vector3 center = mesh.vertices[indices[i]];
            center += mesh.vertices[indices[i+1]];
            center += mesh.vertices[indices[i+2]];
            center += mesh.vertices[indices[i+3]];
            center /= 4.0f;

            Vector3 normal = mesh.normals[indices[i]];
            normal += mesh.normals[indices[i]];
            normal += mesh.normals[indices[i]];
            normal += mesh.normals[indices[i]];
            normal.Normalize();

            nodes.Add(new Node { 
                position = center + normal * 0.3f, 
                up = normal 
            });

            nodeQuadIndex.Add(i);
        }

        for(int i = 0; i < nodes.Count; i++)
        {
            for (int t = 0; t < nodes.Count; t++)
            {
                if (t == i) continue;
                Vector3 dir = nodes[t].position - nodes[i].position;
                if (dir.sqrMagnitude > 9) continue;
                if (Physics.Raycast(nodes[i].position, dir, dir.magnitude, 1 << 6)) continue;

                nodes[i].connections.Add(new Connection { length = dir.sqrMagnitude, target = nodes[t] });
            }
        }

        Debug.Log(nodes.Count);
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        if(nodes != null)
        {
            foreach (Node node in nodes)
            {
                Gizmos.DrawWireSphere(node.position, 0.1f);

                foreach(Connection connection in node.connections)
                {
                    Gizmos.DrawLine(node.position, connection.target.position);
                }
            }
        }
        
    }
}
