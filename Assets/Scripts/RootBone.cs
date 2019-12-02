using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class RootBone : AbstractBone {
	[HideInInspector]
	public List<Transform> bone_transforms = new List<Transform>();
	[HideInInspector]
	public List<Matrix4x4> bindposes = new List<Matrix4x4>();

	public bool useTrick=true;
	public bool randomizeLength=false;
	public bool randomizeMovement=false;
	public float minLength = 0.3f;
	public float maxLength = 3.0f;

	void Start () {
		foreach(Bone child in children) {
			child.root = this;
			child.Generate();
		}

		length = 0.0f;
		
		u_len = children[0].u_len;
		Mesh mesh = createChildrenMesh();

		SkinnedMeshRenderer sk = GetComponent<SkinnedMeshRenderer>();

		sk.sharedMesh = mesh;
		sk.bones = bone_transforms.ToArray();
		mesh.bindposes = bindposes.ToArray();
	}

	protected override int[] childrenConnect(Dictionary<Bone, int> vertexOffset, int i) {
		return new int[4]{vertexOffset[children[0]]+i,vertexOffset[children[0]]+(i+1)%u_len,vertexOffset[children[1]]+i,vertexOffset[children[1]]+(i+1)%u_len};
	}
}