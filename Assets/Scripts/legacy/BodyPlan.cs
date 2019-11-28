using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(SkinnedMeshRenderer))]

public class BodyPlan : MonoBehaviour {
	/*
	protected Bone_old root;
	Vector3[] vertices;
	int[] faces;
	BoneWeight[] boneweights;
	Matrix4x4[] bindposes;

	private Mesh mesh;

	protected void draw() {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();

		vertices = new Vector3[0];
		faces = new int[0];
		boneweights = new BoneWeight[0];
		bindposes = new Matrix4x4[1];

		createRigid();
		generateMesh(root, 36, 5);

		mesh.vertices = vertices;
		mesh.triangles = faces;
		mesh.boneWeights = boneweights;
		//mesh.bindposes = bindposes;

		mesh.RecalculateNormals();
		
		GetComponent<SkinnedMeshRenderer>().sharedMesh = mesh;
		GetComponent<SkinnedMeshRenderer>().bones = new Transform[1]{transform.GetChild(0)};
		GetComponent<SkinnedMeshRenderer>().material = new Material(Shader.Find("Diffuse"));

		Debug.Log(GetComponent<SkinnedMeshRenderer>().bones[0]);

		//bindposes[0] = Matrix4x4.identity;
	}

	void Update() {

	}

	protected void createRigid() {
		root.makeRigid(true).transform.SetParent(this.transform, false);
	}


	protected void generateMesh(Bone_old bone, int w, int h, int orientation=0) {
		faces = faces.Concat(bone.makeFaces(w,h, vertices.Length)).ToArray();
		vertices = vertices.Concat(bone.makeVertices(w,h)).ToArray();
		BoneWeight[] bw = new BoneWeight[w*h]; 

		for (int i=0;i<w*h;i++) {
			bw[i].boneIndex0 = 0;
        	bw[i].weight0 = 1;
		}

		boneweights = boneweights.Concat(bw).ToArray();

		if (bone.upper.Count > 0) {
			int cnt = 0;
			int n = bone.upper.Count;
			int[] child_w = new int[n];

			for(int i=0;i<n;i++) {
				Bone_old child = bone.upper[i];
				child_w[i] = w/n;
				generateMesh(child, child_w[i], h, 1);

				cnt++;
			}
			//connect the parent and child
			/*

			cnt = 0;
			for(int i=0;i<n-1;i++) {
				int a = child_w[i] - Math.Max(1, child_w[i]/4);
				int b = Math.Max(1, child_w[i+1]/4);

				merge(bone.upper[i].vertexOffset+a, bone.upper[i+1].vertexOffset+b);
			}

			int k=0;
			int upper_v = Math.Max(1, child_w[0]/4);
			if (n>1)
				upper_v += Math.Max(1, child_w[1]/4)*(n/2)*2;
			int lower_v = upper_v;


			//merge(bone.vertexOffset+upper_v, bone.upper[0].vertexOffset+child_w[0]*(h-1)+Math.Max(1, child_w[0]/4));

			while(k<n) {
				//TODO: Set UV here

				int a = Math.Max(1, child_w[k]/4);
				int b = child_w[k] - Math.Max(1, child_w[k]/4);

				int off = bone.upper[k].vertexOffset;

				int j = a;
				while(j != b) {
					merge(bone.vertexOffset+upper_v, off+j);
					upper_v+=1;
					j++;
				}

				j=a;
				while(j != b) {
					merge(bone.vertexOffset+lower_v, off+j);
					lower_v=lower_v-1<0?lower_v-1+w:lower_v-1;
					j=j-1<0?j-1+child_w[k]:j-1;
				}

				merge(new int[]{bone.vertexOffset+lower_v, bone.vertexOffset+upper_v, off+b});

				k++;
			}
			*/
			/*
		} 

		if (bone.lower.Count > 0) {
			int cnt = 0;
			int n = bone.lower.Count;
			int[] child_w = new int[n];

			for(int i=0;i<n;i++) {
				Bone_old child = bone.lower[i];
				child_w[i] = w/n;
				generateMesh(child, child_w[i], h, -1);

				cnt++;
			}
			//connect the parent and child
			/*
			cnt = 0;
			for(int i=0;i<n-1;i++) {
				int a = child_w[i] - Math.Max(1, child_w[i]/4);
				int b = Math.Max(1, child_w[i+1]/4);

				merge(bone.lower[i].vertexOffset+a, bone.lower[i+1].vertexOffset+b);
			}

			int k=0;
			int upper_v = Math.Max(1, child_w[0]/4);
			if (n>1)
				upper_v += Math.Max(1, child_w[1]/4)*(n/2)*2;
			Debug.Log(upper_v);
			int lower_v = upper_v;


			merge(bone.vertexOffset+w*(h-1)+upper_v, bone.lower[0].vertexOffset+Math.Max(1, child_w[0]/4));

			while(k<n) {
				int a = Math.Max(1, child_w[k]/4);
				int b = child_w[k] - Math.Max(1, child_w[k]/4);

				int off = bone.lower[k].vertexOffset;

				int j = a;
				while(j != b) {
					merge(bone.vertexOffset+w*(h-1)+upper_v, off+j);
					upper_v+=1;
					j++;
				}

				j=a;
				while(j != b) {
					merge(bone.vertexOffset+w*(h-1)+lower_v, off+j);
					lower_v=lower_v-1<0?lower_v-1+w:lower_v-1;
					j=j-1<0?j-1+child_w[k]:j-1;
				}

				merge(new int[]{bone.vertexOffset+w*(h-1)+lower_v, bone.vertexOffset+w*(h-1)+upper_v, off+b});

				k++;
			}

			*/
			/*
		}

		if (bone.lower.Count == 0 && bone.upper.Count == 0) {
			int[] iss = new int[w];
			for(int i=0;i<w;i++) {
				iss[i] = bone.vertexOffset + w*(h-1) + i;
			}
			merge(iss);
		}
	}


	void merge(int i1, int i2) {
		//replace i2 with i1

		Vector3 mid = (vertices[i1]+vertices[i2])*0.5f;
		vertices[i1] = vertices[i2] = mid;

		for (int i=0;i<faces.Length;i++) {
			if (faces[i] == i2) {
				faces[i] = i1;
			}
		}
	}

	void merge(int[] iss) {
		//replace i2 with i1
		Vector3 mid = Vector3.zero;
		int n = iss.Length;
		int i1 = 192041920;

		for (int i=0;i<n;i++) {
			i1 = Math.Min(i1, iss[i]);
			mid += (float)1/n*vertices[iss[i]];
		}

		for (int i=0;i<n;i++) {
			vertices[iss[i]] = mid;
		}

		for (int i=0;i<faces.Length;i++) {
			if (Array.Exists(iss, x=>x==faces[i])) {
				faces[i] = i1;
			}
		}
	}
	*/
}
