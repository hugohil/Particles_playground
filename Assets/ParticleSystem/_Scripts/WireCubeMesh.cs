using UnityEngine;

public class WireCubeMesh  {
    public static Mesh CreateWireCube(float size = 1f)  {
        Mesh mesh = new Mesh();
        mesh.name = "WireCube";

        size *= 0.5f;

        // Define the 8 vertices of the cube
        Vector3[] vertices = new Vector3[8]  {
            new Vector3(-size, -size, -size), // 0: left bottom back
            new Vector3( size, -size, -size), // 1: right bottom back
            new Vector3(-size,  size, -size), // 2: left top back
            new Vector3( size,  size, -size), // 3: right top back
            new Vector3(-size, -size,  size), // 4: left bottom front
            new Vector3( size, -size,  size), // 5: right bottom front
            new Vector3(-size,  size,  size), // 6: left top front
            new Vector3( size,  size,  size)  // 7: right top front
        };

        // Define the 12 edges (24 vertices for lines)
        int[] indices = new int[24]  {
            // Back face
            0, 1,  // bottom
            1, 3,  // right
            3, 2,  // top
            2, 0,  // left

            // Front face
            4, 5,  // bottom
            5, 7,  // right
            7, 6,  // top
            6, 4,  // left

            // Connecting edges
            0, 4,  // bottom left
            1, 5,  // bottom right
            2, 6,  // top left
            3, 7   // top right
        };

        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Lines, 0);
        mesh.RecalculateBounds();

        return mesh;
    }
}