using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class CylinderTest : MonoBehaviour {

	public float t = 0.1f;
	private Mesh mesh;
	// Use this for initialization
	void Start () {
		mesh = GetComponent<MeshFilter>().mesh;

		Vector3[] vertices = new Vector3[mesh.vertices.Length];
		Vector2[] uv = new Vector2[mesh.uv.Length];
		int[] faces = new int[mesh.triangles.Length];

		Array.Copy(mesh.vertices, 0, vertices, 0, mesh.vertices.Length);
		Array.Copy(mesh.uv, 0, uv, 0, mesh.uv.Length);
		Array.Copy(mesh.triangles, 0, faces, 0, mesh.triangles.Length);

		GetComponent<MeshFilter>().mesh = mesh = new Mesh();

		for (int i=0;i<uv.Length;i++) {

			uv[i] = 0.5f*uv[i];

			Debug.Log(uv[i]);
		}
		
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = faces;

		mesh.RecalculateNormals();
	}

}
