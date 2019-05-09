using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {
    
	private Mesh mesh;
	private Vector3[] vertices;
    private int[] triangles;

    public int xSize = 20;
    public int zSize = 20;
    public float noiseFactor = 2f;
    public float perlinNoiseZoomingFactor = 0.2f;
    public GameObject player;

    private bool ya = false;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();
    }

    private void Update()
    {
        bool shiftForward = player.transform.position.z - this.transform.position.z > 0;
        float shiftZ = player.transform.position.z - this.transform.position.z;
        Vector3[] newVertices = new Vector3[(xSize + 1) * (zSize + 1)];
        int[] newTriangles = new int[xSize * zSize * 6];

        if (shiftForward)
        {
            for (int index = 0, z = 0; z <= zSize; z++)
            {
                for (int x = 0; x <= xSize; x++)
                {
                    vertices[index].z -= shiftZ;
                    index++;
                }
            }

            this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, player.transform.position.z);
            
            if (vertices[0].z < -5) {
                ya = true;
                System.Array.Copy(vertices, xSize + 1, newVertices, 0, (xSize + 1) * zSize); // Skip the first row of vertices;
                System.Array.Copy(triangles, 0, newTriangles, 0, (xSize * 6) * (zSize - 1));

                // Construct new vertices
                int i = (xSize + 1) * zSize;
                float newZPos = newVertices[i - 1].z + 1;
                float perlinNoiseZPos = this.transform.position.z + newZPos; // World space z coordinate for vertex
                for (int x = 0; x <= xSize; x++)
                {
                    float y = Mathf.PerlinNoise(x * perlinNoiseZoomingFactor, perlinNoiseZPos * perlinNoiseZoomingFactor) * noiseFactor;
                    newVertices[i] = new Vector3(x, y, newZPos);
                    i++;
                }

                // Construct new triangles
                int vert = (xSize + 1) * (zSize - 1); // Reconstruct triangle from the second last row of vertices
                int tris = (xSize * 6) * (zSize - 1);

                for (int x = 0; x < xSize; x++)
                {
                    newTriangles[0 + tris] = vert + 0;
                    newTriangles[1 + tris] = vert + xSize + 1;
                    newTriangles[2 + tris] = vert + 1;
                    newTriangles[3 + tris] = vert + 1;
                    newTriangles[4 + tris] = vert + xSize + 1;
                    newTriangles[5 + tris] = vert + xSize + 2;

                    vert++;
                    tris += 6;
                }

                vertices = newVertices;
                triangles = newTriangles;
            }
        }

        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * perlinNoiseZoomingFactor, z * perlinNoiseZoomingFactor) * noiseFactor;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }


        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for(int z = 0; z < zSize; z++)
        { 
            for (int x = 0; x < xSize; x++)
            {
                triangles[0 + tris] = vert + 0;
                triangles[1 + tris] = vert + xSize + 1;
                triangles[2 + tris] = vert + 1;
                triangles[3 + tris] = vert + 1;
                triangles[4 + tris] = vert + xSize + 1;
                triangles[5 + tris] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}