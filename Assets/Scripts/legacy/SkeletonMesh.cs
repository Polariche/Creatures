using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class SkeletonMesh : MonoBehaviour {

	private Mesh mesh;
	public int width, height;	//num of points in each line

	// Use this for initialization
	void Start () {
		draw();
	}

	void draw() {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();

		mesh.vertices = makeVertices();
		mesh.triangles = makeFaces();
		mesh.RecalculateNormals();
	}

	Vector3[] makeVertices() {
		Vector3[] vertices = new Vector3[width*height];

		for(int i=0;i<height;i++) {
			for(int j=0;j<width;j++) {
				float theta = 2*Mathf.PI*j/width;
				vertices[i*width+j] = new Vector3(Mathf.Cos(theta), -1.0f+2.0f*i/height, Mathf.Sin(theta));
			}
		}
		return vertices;
	}

	int[] makeFaces() {
		int[] face = new int[width*(height-1)*6];

		for(int i=0;i<width*(height-1);i++) {
			int x = i;
			int y = i+width;
			int z = i/width*width+(i+1)%width;
			int w = (i/width+1)*width+(i+1)%width;

			face[6*i] = x;
			face[6*i+1] = y;
			face[6*i+2] = z;

			face[6*i+3] = z;
			face[6*i+4] = y;
			face[6*i+5] = w;
		}
		return face;
	}

	float sdf(Vector3 p) {
		return 0.0f;
	}

	// Update is called once per frame
	void Update () {
	}
}
