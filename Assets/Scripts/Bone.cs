using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(CapsuleCollider))]
public class Bone : RootBone {

	[HideInInspector]
	public RootBone root;

	private int orientation = 0;		// whether children will go 'up' or 'down' in v-direction

	public float radius;

	
	[HideInInspector]
	public Bone pair;

	public Vector2 movementRad = new Vector2(1.0f, 1.0f);	//should be same for pair

	private Quaternion canonRotation;
	private float t = 0.0f;

	private bool init = false;
	private bool left = false;

	private int boneindex;

	// Use this for initialization
	

	void Start () {
		canonRotation = transform.rotation;
		
		if (transform.parent != null) {
			if (transform.parent.GetComponent<RootBone>() == null) {
				Generate();
			}
		} else {
			Generate();
		}
	}

	void Update() {
		if(init)
			pointAt(t+=1.0f/7);
	}
	

	void OnDrawGizmos() {
		Gizmos.color = new Color(0, 0, 1, 0.5F);
        Gizmos.DrawRay(transform.position, transform.forward*length);

        
        if(children.Length > 0) {
        	Gizmos.color = new Color(0, 1, 1, 0.5F);
        	
        } else {
        	Gizmos.color = new Color(0, 1, 0, 0.5F);
        }
        for(int i=0;i<hull.Count;i++) {
        	Point p1 = hull[i];
        	Point p2 = hull[(i+1)%hull.Count];
	        //Gizmos.DrawLine(p1.position + p1.bone.transform.position, p2.position + p2.bone.transform.position); 
	    }
		
		if (init) {
			Gizmos.color = new Color(1, 0, 0, 0.5F);
			List<Vector3> circle = new List<Vector3>();
			for(int i=0;i<10;i++) {
				float theta = 2.0f*Mathf.PI / 10 * i;
	        	//circle.Add(length * movementRad.x * Mathf.Cos(theta) * canonRotation[0] + length * movementRad.y *Mathf.Sin(theta) * canonRotation[1] + length * canonRotation[2] + transform.position);
		    	// root.transform.rotation * 

		    	circle.Add(canonRotation * (length * new Vector3(movementRad.x * Mathf.Sin(theta), movementRad.y * Mathf.Cos(theta), 1).normalized) + transform.position);
		    }

		    for(int i=0;i<10;i++) {
		    	Gizmos.DrawLine(circle[i], circle[(i+1)%10]); 
		    }
		}
		
	}

	void pointAt(float theta) {
		if (!pair || (pair && left)) {
			// root.transform.rotation * 
			t = theta;
			Vector3 v = canonRotation * (length * new Vector3(-movementRad.x * Mathf.Sin(theta), movementRad.y * Mathf.Cos(theta), 1).normalized) + transform.position;
			transform.LookAt(v, canonRotation * Vector3.up);
			if (!root.useTrick)
				transform.rotation = root.transform.rotation * transform.rotation;
			
			foreach(Bone child in children) {
				//TODO: tree propagation
				child.transform.LookAt(child.canonRotation * (length * new Vector3(-child.movementRad.x * Mathf.Sin(child.t), child.movementRad.y * Mathf.Cos(child.t), 1).normalized) + child.transform.position, child.canonRotation * Vector3.up);
				if (!root.useTrick)
					child.transform.rotation = root.transform.rotation * child.transform.rotation;
			}

			if (pair) {
				pair.movementRad = movementRad;
				pair.t = -theta+Mathf.PI;
				//do the same for the pair
				Vector3 v2 = pair.canonRotation * (length * new Vector3(-pair.movementRad.x * Mathf.Sin(-theta+Mathf.PI), pair.movementRad.y * Mathf.Cos(-theta+Mathf.PI), 1).normalized) + pair.transform.position;
				pair.transform.LookAt(v2, pair.canonRotation * Vector3.up);
				if (!root.useTrick)
					pair.transform.rotation = root.transform.rotation * pair.transform.rotation;
				
				foreach(Bone child in pair.children) {
					//TODO: tree propagation
					child.transform.LookAt(child.canonRotation * (length * new Vector3(-child.movementRad.x * Mathf.Sin(child.t), child.movementRad.y * Mathf.Cos(child.t), 1).normalized) + child.transform.position, child.canonRotation * Vector3.up);
					if (!root.useTrick)
						child.transform.rotation = root.transform.rotation * child.transform.rotation;
				}
			}
		}
		
	}

	public void Generate() {
		// bottom - up parts
		orientation = Vector3.Angle(Vector3.forward, transform.forward) > 90 ? 1 : -1;

		if(root.randomizeLength) 
			randomLength();
		if(root.randomizeMovement) 
			randomMovement();

		setchildrenSetting(root.transform.localToWorldMatrix);
		
		// convex hull of children circles
		childrenConvexHull();

		for(int i=0;i<u_len;i++)
			u_coords[i] = i;

		setChildrenUCoords();


		init = true;
	}	
	

	protected override Mesh generateMesh(int v=0) {
		Mesh mesh = new Mesh();

		List<Point> points = new List<Point>();

		Vector3[] vertices;
		Vector2[] uvs;
		int[] faces;
		BoneWeight[] bones;

		int w = u_len;
		int h = (int)(length * 4)/4+1;

		vertices = new Vector3[w*h];
		uvs = new Vector2[w*h];
		faces = new int[6*w*(h-1)];
		bones = new BoneWeight[w*h];

		for(int i=0;i<h;i++) {
			points.AddRange(coreCircle(i, true, true));
		}

		for(int i=0;i<w*h;i++) {
			vertices[i] = points[i].position;
			uvs[i] = new Vector2(1.0f*orientation*points[i].u_index /root.u_len + (orientation > 0 ? 0.0f : 1.0f), 0.5f + 1.0f*orientation*(points[i].verticalIndex+v) / 10);
			bones[i].boneIndex0 = boneindex;
			bones[i].weight0 = 1;
		}

		
		for(int j=0;j<h-1;j++) {
			for(int i=0;i<w;i++) {
				int a = j*w+i;
				int b = j*w+(i+1)%w; 
				int c = (j+1)*w+i;
				int d =	(j+1)*w+(i+1)%w;

				faces[6*a] = a;
				faces[6*a+1] = c;
				faces[6*a+2] = b;

				faces[6*a+3] = b;
				faces[6*a+4] = c;
				faces[6*a+5] = d;

			}
		}
		
		mesh.vertices = vertices;
		mesh.triangles = faces;
		mesh.uv = uvs;
		mesh.boneWeights = bones;

		return mesh;
	}

	protected override int[] childrenConnect(Dictionary<Bone, int> vertexOffset, int i) {
		int h = (int)(length * 4)/4+1;

		Point p1 = hull[i];
		Point p2 = hull[(i+1)%u_len];

		int a = (h-1)*u_len + i;
		int b = (h-1)*u_len + (i+1)%u_len;
		int c = vertexOffset[p1.bone] + p1.cylinderIndex;
		int d = vertexOffset[p2.bone] + p2.cylinderIndex;

		return new int[4]{a,b,c,d};
	}


	List<Bone> childrenLeftSorted() {
		List<Bone> bones = new List<Bone>(children);
		bones.Sort(delegate(Bone b1, Bone b2) {return Vector3.Dot(b1.transform.position - transform.position, transform.right).CompareTo(Vector3.Dot(b2.transform.position - transform.position, transform.right)); });
		return bones;
			
	}

	void randomMovement() {
		float x = Random.Range(-1.0f, 1.0f);
		float y = Random.Range(-1.0f, 1.0f);
		Vector2 v = new Vector2(x,y);

		if(pair & left) {
			movementRad = v;
			pair.movementRad = v;
		}
		else if (!pair)
			movementRad = v;
	}

	void randomLength() {
		float oldlength = length;
		float newlength = Random.Range(root.minLength, root.maxLength);

		if(pair & left) {
			float oldlength_pair = pair.length;

			length = newlength;

			pair.length = newlength;

			if(children.Length > 0) {
				foreach(Bone child in children)
					child.transform.localPosition += Vector3.forward*(newlength - oldlength);
			}

			if(pair.children.Length > 0) {
				foreach(Bone child in pair.children)
					child.transform.localPosition += Vector3.forward*(newlength - oldlength_pair);
			}
		}
		else if (!pair) {
			length = newlength;

			if(children.Length > 0) {
				foreach(Bone child in children)
					child.transform.localPosition += Vector3.forward*(newlength - oldlength);
			}
		}
	}

	void setchildrenSetting(Matrix4x4 thing) {
		boneindex = root.bone_transforms.Count;
		root.bone_transforms.Add(transform);
		root.bindposes.Add(transform.worldToLocalMatrix * thing);


		CapsuleCollider coll = GetComponent<CapsuleCollider>();

		coll.radius = radius;
		coll.height = length;
		coll.center = Vector3.forward*length*0.5f;
		coll.direction = 2;

		// initialize children
		if (children.Length > 0) {
			

			if(pair & left==true) {
				List<Bone> bones1 = childrenLeftSorted();
				List<Bone> bones2 = pair.childrenLeftSorted();

				int n = Mathf.Min(bones1.Count, bones2.Count);

				for(int i=0;i<n;i++) {
					bones1[i].pair = bones2[n-1-i];
					bones2[n-1-i].pair = bones1[i];
					bones1[i].left = true;
					bones2[i].left = false;
				}

			} else if (!pair) {
				List<Bone> bones = childrenLeftSorted();
				for(int i=0;i<bones.Count/2;i++) {
					bones[i].pair = bones[bones.Count-1-i];
					bones[bones.Count-1-i].pair = bones[i];
					bones[i].left = true;
				}

			}

			
			foreach(Bone child in children) {
				if (child.radius == 0.0f)
					child.radius = radius / children.Length;

				child.orientation = orientation;
				child.root = root;
				//child.transform.localPosition = Vector3.forward*length;

				if(root.randomizeLength) 
					child.randomLength();
				if(root.randomizeMovement) 
					child.randomMovement();
			}
			
			foreach(Bone child in children) {
				child.setchildrenSetting(thing);

				child.init = true;
			}

		}
	}

	void childrenConvexHull() {
		//set u_len via convex hull of children
		if (children.Length > 0) {
			if(u_len != 0)
				return;

			List<Bone> bones = childrenLeftSorted();
			List<Point> hull_ = new List<Point>();

			foreach(Bone child in bones) {
				child.childrenConvexHull();
				List<Point> points = child.coreCircle();
				int s = child.u_len / 4;
				int x = child.u_len/2;
				int y = child.u_len - x;
				for(int i=0;i<x;i++) {
					hull_.Add(points[(s+i)%child.u_len]);
				}
				for(int i=1;i<=y;i++) {
					hull_.Insert(0,points[s-i<0?s-i+child.u_len:s-i]);
				}
			}

			int a = hull_.Count/2;
			for(int i=a;i<a+hull_.Count;i++)
				hull.Add(hull_[i%hull_.Count]);


			u_len = hull.Count;	
			u_coords = new int[u_len];

		} else {
			u_len = 5;
			u_coords = new int[u_len];
			for(int i=0;i<u_len;i++)
				u_coords[i] = i;

			hull = coreCircle();

			foreach(Point p in hull) {
				p.calcPos(true);
			}
		}
	}

	
	void setChildrenUCoords() {
		int i = 0;
		foreach(Point p in hull) {
			Debug.Log(p);
			p.bone.u_coords[p.cylinderIndex] = u_coords[i++];
		}

		foreach(Bone child in children) {
			child.setChildrenUCoords();
		}
	}


	List<Point> coreCircle(int vInd=0, bool relative=false, bool applyUindex=false) {
		List<Point> points = new List<Point>();

		for(int i=0;i<u_len;i++) {
			points.Add(new Point());
			points[i].cylinderIndex = i;
			points[i].verticalIndex = vInd;
			points[i].bone = this;
			points[i].calcPos(relative);

			if(applyUindex)
				points[i].u_index = u_coords[i];
		}
		return points;
	}



}

public class Point {
		public Vector3 position;
		public int cylinderIndex;
		public int verticalIndex;

		public int u_index;
		public Vector2 uv;

		public Bone bone;

		public void calcPos(bool relative=false) {
			float theta = ((float)cylinderIndex)/bone.u_len*2*Mathf.PI;

			float radius = bone.radius;

			if(!relative)
				position = radius * Mathf.Cos(theta) * bone.transform.up + radius * Mathf.Sin(theta) * bone.transform.right + bone.transform.position + bone.transform.forward * (float)verticalIndex / 4;
			else
				position = radius * Mathf.Cos(theta) * Vector3.up + radius * Mathf.Sin(theta) * Vector3.right + Vector3.forward * (float)verticalIndex;
		}
}