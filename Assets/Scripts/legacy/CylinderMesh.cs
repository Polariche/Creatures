using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CylinderMesh : MonoBehaviour {
	//use SkeletonMesh instead

	private Mesh mesh;
	private Vector3[] vertices;
	public int width, height;	//num of points in each line

	// Use this for initialization
	void Start () {
		drawCylinder();
	}

	void drawCylinder() {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();

		bool[] shouldDraw;
		mesh.vertices = vertices = makeVertices(out shouldDraw);
		mesh.triangles = drawFaces(shouldDraw);
		mesh.RecalculateNormals();
	}

	Vector3[] makeVertices(out bool[] shouldDraw) {
		Vector3[] vertice = new Vector3[width*height];
		shouldDraw = new bool[width*height];

		for (int u=0;u<width;u++) {
			for(int v=0;v<height;v++) {
				Vector3 dir = vertexDir(u,v);
				Vector3 pos = vertexPos(u,v);
				float dist = cylinderSdf(pos);

				while(Mathf.Abs(dist) > 0.001) { 
					pos = -cylinderSdf(pos)*dir+pos;
					dist = cylinderSdf(pos);
				}

				if (childrenSdf(pos) > 0)
					shouldDraw[u*height+v] = true;
				vertice[u*height+v] = pos;
			}
		}
		return vertice;
	}

	int[] drawFaces(bool[] shouldDraw) {
		int[] trigs = new int[width*(height-1)*2*3];

		for (int i=0;i<width*(height-1);i++) {
			int x = i/(height-1)*height+i%(height-1);

			if(shouldDraw[x]&&shouldDraw[x+1]&&shouldDraw[(x+height)%(width*height)]&&shouldDraw[(x+height+1)%(width*height)]) {
				trigs[i*6] = x;
				trigs[i*6+1] = x+1;
				trigs[i*6+2] = (x+height)%(width*height);
				
				trigs[i*6+3] = (x+height)%(width*height);
				trigs[i*6+4] = x+1;
				trigs[i*6+5] = (x+height+1)%(width*height);	
			}

		}
		return trigs;
	}

	Vector3 vertexDir(int u, int v) {
		float theta = 2*Mathf.PI/width*u;
		return new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta));
	}

	Vector3 vertexPos(int u, int v) {
		float theta = 2*Mathf.PI/width*u;
		return new Vector3(0, -0.9f+1.8f*(float)v/(height-1), 0);
	}

	float childrenSdf(Vector3 pos) {
		float selfSdf = 99999f;
		
		CylinderMesh[] children = GetComponentsInChildren<CylinderMesh>();
		foreach (CylinderMesh child in children) {
			if (child == this)
				continue;

			Vector3 t = child.transform.InverseTransformPoint(transform.TransformPoint(pos));

			selfSdf = Mathf.Min(selfSdf, child.cylinderSdf(t));
		}

		return selfSdf;
	}
	
	float cylinderSdf(Vector3 pos) {
		if (pos.y >= 0.5) {
			return -0.5f + new Vector3(pos.x, pos.y-0.5f, pos.z).magnitude;
		} else if (pos.y <= -0.5) {
			return -0.5f + new Vector3(pos.x, pos.y+0.5f, pos.z).magnitude;
		}
		return -0.5f + new Vector3(pos.x, 0, pos.z).magnitude;
	}

	// Update is called once per frame
	void Update () {
	}
}
