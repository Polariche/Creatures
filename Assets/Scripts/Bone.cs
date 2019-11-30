using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(CapsuleCollider))]
public class Bone : AbstractBone {

	public AbstractBone root;

	public int orientation = 0;		// whether children will go 'up' or 'down' in v-direction
	//public Bone[] children;

	public float radius;
	public float length;

	//public int u_len=0;
	//public int[] u_coords;
	public List<Point> hull = new List<Point>();
	
	public Bone pair;
	public Vector2 movementRad = new Vector2(1.0f, 1.0f);	//should be same for pair

	public Quaternion canonRotation;
	public float t = 0.0f;

	public bool init = false;
	public bool left = false;

	// Use this for initialization
	

	void Start () {
		canonRotation = transform.rotation;

		GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
		CapsuleCollider coll = GetComponent<CapsuleCollider>();

		coll.radius = radius;
		coll.height = length;
		coll.center = Vector3.forward*length*0.5f;
		coll.direction = 2;
		
		if (transform.parent != null) {
			if (transform.parent.GetComponent<AbstractBone>() == null) {
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
	        Gizmos.DrawLine(p1.position + p1.bone.transform.position, p2.position + p2.bone.transform.position); 
	    }
		
		if (init) {
			Gizmos.color = new Color(1, 0, 0, 0.5F);
			List<Vector3> circle = new List<Vector3>();
			for(int i=0;i<10;i++) {
				float theta = 2.0f*Mathf.PI / 10 * i;
	        	//circle.Add(length * movementRad.x * Mathf.Cos(theta) * canonRotation[0] + length * movementRad.y *Mathf.Sin(theta) * canonRotation[1] + length * canonRotation[2] + transform.position);
		    	circle.Add(root.transform.rotation * canonRotation * (length * new Vector3(movementRad.x * Mathf.Sin(theta), movementRad.y * Mathf.Cos(theta), 1).normalized) + transform.position);
		    }

		    for(int i=0;i<10;i++) {
		    	Gizmos.DrawLine(circle[i], circle[(i+1)%10]); 
		    }
		}
		
	}

	void setMovementRad(Vector2 v) {
		movementRad = v;
		if (pair != null)
			pair.movementRad = v;
	}

	void pointAt(float theta) {
		if (!pair || (pair && left)) {
			t = theta;
			Vector3 v = canonRotation * (length * new Vector3(-movementRad.x * Mathf.Sin(theta), movementRad.y * Mathf.Cos(theta), 1).normalized) + transform.position;
			transform.LookAt(v, canonRotation * Vector3.up);
			transform.rotation = root.transform.rotation * transform.rotation;
			
			foreach(Bone child in children) {
				//TODO: tree propagation
				child.transform.LookAt(child.canonRotation * (length * new Vector3(-child.movementRad.x * Mathf.Sin(child.t), child.movementRad.y * Mathf.Cos(child.t), 1).normalized) + child.transform.position, child.canonRotation * Vector3.up);
				child.transform.rotation = root.transform.rotation * child.transform.rotation;
			}

			if (pair) {
				pair.movementRad = movementRad;
				pair.t = -theta+Mathf.PI;
				//do the same for the pair
				Vector3 v2 = pair.canonRotation * (length * new Vector3(-pair.movementRad.x * Mathf.Sin(-theta+Mathf.PI), pair.movementRad.y * Mathf.Cos(-theta+Mathf.PI), 1).normalized) + pair.transform.position;
				pair.transform.LookAt(v2, pair.canonRotation * Vector3.up);
				pair.transform.rotation = root.transform.rotation * pair.transform.rotation;
				
				foreach(Bone child in pair.children) {
					//TODO: tree propagation
					child.transform.LookAt(child.canonRotation * (length * new Vector3(-child.movementRad.x * Mathf.Sin(child.t), child.movementRad.y * Mathf.Cos(child.t), 1).normalized) + child.transform.position, child.canonRotation * Vector3.up);
					child.transform.rotation = root.transform.rotation * child.transform.rotation;
				}
			}
		}
		
	}

	public void Generate() {
		// bottom - up parts
		
		orientation = Vector3.Angle(Vector3.forward, transform.forward) > 90 ? 1 : -1;

		setchildrenSetting();
		
		// convex hull of children circles
		childrenConvexHull();

		for(int i=0;i<u_len;i++)
			u_coords[i] = i;

		setChildrenUCoords();


		// top - down parts

		// from core circle, start generating mesh

		

		// create convex hull vertices

		// generate children mesh, connect it with convex hull

		// add to total mesh
		Mesh mesh = createChildrenMesh();
		GetComponent<MeshFilter>().mesh = mesh;

		init = true;
		
	}	

	Mesh createChildrenMesh(int v=0) {
		int h = (int)(length * 4)/4+1;
		Mesh mesh = generateMesh(v);

		if (children.Length > 0) {
			Dictionary<Bone, int> vertexOffset = new Dictionary<Bone, int>();

			for(int i=0;i<children.Length;i++) {
				vertexOffset.Add(children[i], mesh.vertices.Length);
				//child.GetComponent<MeshFilter>().mesh =;

				mesh = concatMesh(mesh, children[i].createChildrenMesh(v+h), children[i].transform);
			}

			int[] newfaces = new int[u_len * 6];

			for (int i=0;i<u_len;i++) {
				// connector
				
				Point p1 = hull[i];
				Point p2 = hull[(i+1)%u_len];

				int a = (h-1)*u_len + i;
				int b = (h-1)*u_len + (i+1)%u_len;
				int c = vertexOffset[p1.bone] + p1.cylinderIndex;
				int d = vertexOffset[p2.bone] + p2.cylinderIndex;

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

	Mesh generateMesh(int v=0) {
		Mesh mesh = new Mesh();

		List<Point> points = new List<Point>();

		Vector3[] vertices;
		Vector2[] uvs;
		int[] faces;

		int w = u_len;
		int h = (int)(length * 4)/4+1;

		vertices = new Vector3[w*h];
		uvs = new Vector2[w*h];
		faces = new int[6*w*(h-1)];


		for(int i=0;i<h;i++) {
			points.AddRange(coreCircle(i, true, true));
		}

		for(int i=0;i<w*h;i++) {
			vertices[i] = points[i].position;
			uvs[i] = new Vector2(1.0f*orientation*points[i].u_index /10 + (orientation > 0 ? 0.0f : 1.0f), 0.5f + 1.0f*orientation*(points[i].verticalIndex+v) / 10);
			Debug.Log(((float)orientation*points[i].u_index) /10);
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

		return mesh;

	}

	Mesh concatMesh(Mesh m1, Mesh m2, Transform t) {
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

		return mesh;
	}

	List<Bone> childrenLeftSorted() {
		List<Bone> bones = new List<Bone>(children);
		bones.Sort(delegate(Bone b1, Bone b2) {return Vector3.Dot(b1.transform.position - transform.position, transform.right).CompareTo(Vector3.Dot(b2.transform.position - transform.position, transform.right)); });
		return bones;
			
	}

	void setchildrenSetting() {
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
				child.radius = radius / children.Length;
				child.orientation = orientation;
				child.root = root;

				child.setchildrenSetting();

				child.init = true;
				
			}


		}
	}
/*	
	void childrenConvexHull() {
		//set u_len via convex hull of children
		if (children.Length > 0) {
			if(u_len != 0)
				return;

			List<Bone> bones = new List<Bone>();
			List<Point> points = new List<Point>();
			List<Vector3> centers = new List<Vector3>();

			foreach(Bone child in children) {
				child.childrenConvexHull();
				bones.Add(child);

			}

			bones.Sort(delegate(Bone b1, Bone b2) {return Vector3.Dot(b1.transform.position - transform.right, transform.right).CompareTo(Vector3.Dot(b2.transform.position - transform.up, transform.right)); });

			

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

	
*/	

	void childrenConvexHull() {
		//set u_len via convex hull of children
		if (children.Length > 0) {
			if(u_len != 0)
				return;

			List<Point> points = new List<Point>();
			Point start, v0, v1;

			foreach(Bone child in children) {
				child.childrenConvexHull();
				points.AddRange(child.coreCircle());
			}

			start = points[0];
			foreach(Point p in points) {
				//project
				p.position = new Vector3(Vector3.Dot(p.position - transform.up, transform.up), Vector3.Dot(p.position - transform.right, transform.right), 0);

				if(start.position.x < p.position.x)		// refine this rule
					start = p;
				else if(Mathf.Abs(start.position.x - p.position.x) < 0.0001 && Mathf.Abs(start.position.y) < Mathf.Abs(p.position.y)) {
					start = p;
				}
			}

			hull.Add(start);

			v0 = start;

			do {
				v1 = points[1];
				foreach(Point p in points) {
					float c = Vector3.SignedAngle((v1.position - v0.position).normalized, (p.position - v0.position).normalized, Vector3.forward);
					if(c < 0) {
						v1 = p;
					}
				}

				points.Remove(v1);
				hull.Add(v1);
				v0 = v1;
			} while (v1 != start && points.Count > 1);

			int i=0;
			foreach(Point p in hull) {
				p.calcPos(true);
				p.u_index = i++;
			}

			u_len = hull.Count;	
			u_coords = new int[u_len];

		} else {
			u_len = 5;
			u_coords = new int[u_len];
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