using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone_old {
	//a one-way Linked List
	/*
	public List<Bone> upper;
	public List<Bone> lower;

	public float length=1.0f;
	public float radius=0.5f;
	public Vector3 posOffset = Vector3.zero;
	public Vector3 dir = -Vector3.forward;

	public Vector3[] vertexRef;
	public int vertexOffset = 0;


	public Bone() {
		upper = new List<Bone>();
		lower = new List<Bone>();
	}

	public Bone(Vector3 d) {
		upper = new List<Bone>();
		lower = new List<Bone>();
		dir = d.normalized;
	}

	public void addToUpper(Bone bone) {
		upper.Add(bone);
		//bone.lower.Add(this);
	}

	public void addToLower(Bone bone) {
		lower.Add(bone);
		//bone.upper.Add(this);
	}

	public void calcPosUpper() {
		float radi = radius/upper.Count;
		int n = upper.Count;

		for(int i=0;i<n;i++) {
			Bone bone = upper[i];

			Quaternion q = Quaternion.LookRotation(dir);
			Vector3 u = q*Vector3.right;

			bone.radius = radi;
			bone.posOffset = posOffset+(-radius + (1+2*i)*radi)*u;
			
		}
	}

	public void calcPosLower() {
		float radi = radius/lower.Count;
		int n = lower.Count;

		for(int i=0;i<n;i++) {
			Bone bone = lower[i];

			Quaternion q = Quaternion.LookRotation(dir);
			Vector3 u = q*Vector3.right;

			bone.radius = radi;
			bone.posOffset = posOffset+(-radius + (1+2*i)*radi)*u +length*dir;
			
		}
	}

	public Vector3[] makeVertices(int width, int height) {
		Vector3[] vertices = new Vector3[width*height];

		Quaternion q = Quaternion.LookRotation(dir);
		Vector3 u = q*(-Vector3.right);
		Vector3 v = q*Vector3.up;

		for(int i=0;i<height;i++) {
			for(int j=0;j<width;j++) {
				float theta = 2*Mathf.PI*j/width-0.5f*Mathf.PI;
				vertices[i*width+j] = dir*length*((float)i/(height-1)) + u*radius*Mathf.Cos(theta) + v*radius*Mathf.Sin(theta) + posOffset;
			}
		}
		return vertices;
	}

	public GameObject makeRigid(bool root=false) {

		Rigidbody rb;

		GameObject cylinder = new GameObject("Cylinder");
		cylinder.AddComponent<MeshFilter>();
		cylinder.AddComponent<CapsuleCollider>();
		cylinder.AddComponent<MeshRenderer>();
		cylinder.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));

		CapsuleCollider coll = cylinder.GetComponent<CapsuleCollider>();
		coll.center = new Vector3(0.0f, 0.0f, -0.5f);
		coll.direction = 2;
		coll.radius = radius;
		coll.height = length;
		
		
		Quaternion q = Quaternion.LookRotation(dir);
		Quaternion q_ = Quaternion.LookRotation(-dir);
		
		Vector3 u = q*(-Vector3.right);
		Vector3 v = q*Vector3.up;
		
		Mesh mesh = cylinder.GetComponent<MeshFilter>().mesh;
		Vector3 dir_ = dir;
		dir = -Vector3.forward;

		mesh.vertices = makeVertices(10,10);
		mesh.triangles = makeFaces(10,10);
		mesh.RecalculateNormals();

		dir = dir_;
		
		if (root == false) {
			GameObject sphere = new GameObject("Sphere");
			sphere.AddComponent<Rigidbody>();
			sphere.AddComponent<CharacterJoint>();
			sphere.AddComponent<SphereCollider>();
			sphere.GetComponent<Rigidbody>().useGravity = false;
			sphere.GetComponent<SphereCollider>().radius = radius;

			rb = sphere.GetComponent<Rigidbody>();
			cylinder.transform.SetParent(sphere.transform);
		} else {
			cylinder.AddComponent<Rigidbody>();
			rb = cylinder.GetComponent<Rigidbody>();
		}
		

		foreach(Bone child in upper) {
			Vector3 childOffset = child.posOffset;
			child.posOffset = Vector3.zero;
			Transform childTrans = child.makeRigid().transform;
			childTrans.LookAt(q_*(-child.dir));
			childTrans.Translate(childTrans.InverseTransformPoint(childOffset));
			childTrans.SetParent(cylinder.transform);
			childTrans.GetComponent<CharacterJoint>().connectedBody = rb;
			
			child.posOffset = childOffset;
		}

		foreach(Bone child in lower) {

			Debug.Log(child.posOffset);
			
			Vector3 childOffset = child.posOffset;
			child.posOffset = Vector3.zero;
			Transform childTrans = child.makeRigid().transform;
			childTrans.LookAt(q_*(-child.dir));
			childTrans.Translate(childTrans.InverseTransformPoint(childOffset));
			childTrans.SetParent(cylinder.transform);
			childTrans.GetComponent<CharacterJoint>().connectedBody = rb;
			
			child.posOffset = childOffset;
		}

		return rb.gameObject;
	}

	public int[] makeFaces(int width, int height, int offset=0) {
		int[] face = new int[width*(height-1)*6];
		vertexOffset = offset;

		for(int i=0;i<width*(height-1);i++) {
			int x = i/width*width+ i%width + offset;
			int y = x + width;
			int z = i/width*width+ (i+1)%width + offset;
			int w = z + width;

			if(i%width >= width/2) {
				face[6*i] = x;
				face[6*i+1] = y;
				face[6*i+2] = z;

				face[6*i+3] = z;
				face[6*i+4] = y;
				face[6*i+5] = w;
			} else {
				face[6*i] = z;
				face[6*i+1] = x;
				face[6*i+2] = w;

				face[6*i+3] = w;
				face[6*i+4] = x;
				face[6*i+5] = y;
			}
		}
		return face;
	}

	*/
}