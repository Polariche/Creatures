using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RootBone : AbstractBone {


	void Start () {
		foreach(Bone child in children) {
			child.root = this;
			child.Generate();
		}
	}
}