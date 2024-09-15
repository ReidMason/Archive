using UnityEngine;
using System.Collections;

namespace NoxCore.Utilities.Geometry
{
	public static class SimpleConvex
	{	
		public static Mesh BuildSimplifiedConvexMesh(Mesh mesh, int maxTriangles = 85)
		{
			//Debug.Log(mesh.triangles.Length/3 + " tris");
			
			MeshBuilder builder = new MeshBuilder();
			
			for (int i = 0; i < maxTriangles; i++)
			{
				int index = Random.Range(0, mesh.triangles.Length/3) * 3;
				
				Vector3[] triangle = new Vector3[]{mesh.vertices[mesh.triangles[index]], mesh.vertices[mesh.triangles[index + 1]], mesh.vertices[mesh.triangles[index + 2]]};
				Vector2[] uvs = new Vector2[]{mesh.uv[mesh.triangles[index]], mesh.uv[mesh.triangles[index + 1]], mesh.uv[mesh.triangles[index + 2]]};
				Vector3[] normals = new Vector3[]{Vector3.up, Vector3.up, Vector3.up};
				
				builder.AddTriangleToMesh(triangle, normals, uvs);
			}
			
			Mesh polygonSoup = builder.Build();
			
			//Debug.Log ("Number of tris: " + polygonSoup.triangles.Length/3);
			
			if (polygonSoup.triangles.Length > 255)
			{
				D.warn("Utility: {0}", "Tried to make simple convex mesh but number of vertices is too high: " + polygonSoup.triangles.Length*3);
			}
			
			D.log("Utility", "Collision mesh built from " + polygonSoup.triangles.Length/3 + " tris");
			
			return polygonSoup;
		}
	}
}