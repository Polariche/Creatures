using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AbstractBone : MonoBehaviour {

	[HideInInspector]
	public Bone[] children;

	public float length;

	[HideInInspector]
	public int u_len=0;

	[HideInInspector]
	public int[] u_coords;
	
	protected List<Point> hull = new List<Point>();

	void Awake() {
		children = new Bone[transform.childCount];

		int i = 0;
		foreach(Transform child in transform) {
			children[i++] = child.GetComponent<Bone>();
		}
	}

	protected Mesh concatMesh(Mesh m1, Mesh m2, Transform t) {
		Mesh mesh = new Mesh();

		int off = m1.vertices.Length;

		int[] trigs = new int[m2.triangles.Length];
		Vector3[] pos = new Vector3[m2.vertices.Length];
		Vector2[] uv = new Vector2[m2.vertices.Length];


		for(int i=0;i<m2.vertices.Length;i++){
			pos[i] =  (Vector3)(transform.worldToLocalMatrix * t.localToWorldMatrix * m2.vertices[i]) + t.localPosition;
			uv[i] = m2.uv[i];
		}


		for(int i=0;i<m2.triangles.Length;i++) {
			trigs[i] = m2.triangles[i] +  off;
		}
		

		mesh.vertices = m1.vertices.Concat(pos).ToArray();
		mesh.uv = m1.uv.Concat(uv).ToArray();
		mesh.triangles = m1.triangles.Concat(trigs).ToArray();
		mesh.boneWeights = m1.boneWeights.Concat(m2.boneWeights).ToArray();

		return mesh;
	}

	protected virtual Mesh generateMesh(int v=0) {
		return new Mesh();
	}

	protected virtual int[] childrenConnect(Dictionary<Bone, int> vertexOffset, int i) {
		return null;
	}

	protected Mesh createChildrenMesh(int v=0) {
		int h = (int)(length * 4)/4+1;
		Mesh mesh = generateMesh(v);

		if (children.Length > 0) {
			Dictionary<Bone, int> vertexOffset = new Dictionary<Bone, int>();

			for(int i=0;i<children.Length;i++) {
				vertexOffset.Add(children[i], mesh.vertices.Length);

				mesh = concatMesh(mesh, children[i].createChildrenMesh(v+h), children[i].transform);
			}

			int[] newfaces = new int[u_len * 6];

			for (int i=0;i<u_len;i++) {
				// connector
				
				int[] x = childrenConnect(vertexOffset, i);

				int a = x[0];
				int b = x[1];
				int c = x[2];
				int d = x[3];

				newfaces[i*6] = a;
				newfaces[i*6+1] = c;
				newfaces[i*6+2] = b;

				newfaces[i*6+3] = b;
				newfaces[i*6+4] = c;
				newfaces[i*6+5] = d;
			}

			mesh.triangles = mesh.triangles.Concat(newfaces).ToArray();
			mesh.RecalculateNormals();
		}
		
		return mesh;
	}
}