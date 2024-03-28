using Microsoft.Xna.Framework.Graphics;

namespace TestMonoGame.Utils;

public static class MeshUtils
{
    // Invert normals function
    public static void InvertNormals(ModelMesh modelMesh)
    {
        foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
        {
            // Get the vertex buffer
            VertexBuffer vertexBuffer = meshPart.VertexBuffer;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[vertexBuffer.VertexCount];
            vertexBuffer.GetData(vertices);

            // Invert normals for each vertex
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal *= -1;
            }

            // Set the modified vertices back to the vertex buffer
            vertexBuffer.SetData(vertices);
        }
    }
}