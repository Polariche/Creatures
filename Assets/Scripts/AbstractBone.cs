using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AbstractBone : MonoBehaviour {

	public Bone[] children;

	public int u_len=0;
	public int[] u_coords;
	

	void Awake() {
		children = new Bone[transform.childCount];

		int i = 0;
		foreach(Transform child in transform) {
			children[i++] = child.GetComponent<Bone>();
		}
	}

}