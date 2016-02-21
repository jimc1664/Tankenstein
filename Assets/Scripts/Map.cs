using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Map : MonoBehaviour {

    public int Seed = 0;
    public bool ReGen = false, IncSeed = false;

    public float Width = 20, Height = 20;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if(IncSeed) {
            Seed++;
            IncSeed = false;
            ReGen = true;
        }
        if(ReGen) {
            gen();
            ReGen = false;
        }
	}
    public Material M1, M2;

    delegate void Dlg(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4);
    void subBox(Vector2 _a, Vector2 _b, Vector2 _c, Vector2 _d, float h, Material m) {
        Vector3 a = _a, b = _b, c = _c, d = _d;
        var go = new GameObject();
        var t = go.transform;
        t.parent = transform;
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = m;
        var mesh = new Mesh();

        mf.sharedMesh = mesh;

        int qc = 4, vc = qc * 4;
        var vertices = new Vector3[vc];
        var normals = new Vector3[vc];
        var uv = new Vector2[vc];
        var tri = new int[qc * 6];

        int qi = 0;
        Dlg quad = (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) => {

            int vi = qi * 4, ti = qi * 6;
            vertices[vi + 0] = p1;
            vertices[vi + 1] = p2;
            vertices[vi + 2] = p3;
            vertices[vi + 3] = p4;


            var nrm = -(Vector3.Cross(p2 - p1, p3 - p1) + Vector3.Cross(p3 - p4, p2 - p4)).normalized;
            normals[vi + 0] = nrm;
            normals[vi + 1] = nrm;
            normals[vi + 2] = nrm;
            normals[vi + 3] = nrm;

            uv[vi + 0] = new Vector2(0, 0);
            uv[vi + 1] = new Vector2(1, 0);
            uv[vi + 2] = new Vector2(0, 1);
            uv[vi + 3] = new Vector2(1, 1);

            tri[ti + 0] = vi + 0;
            tri[ti + 1] = vi + 2;
            tri[ti + 2] = vi + 1;

            tri[ti + 3] = vi + 2;
            tri[ti + 4] = vi + 3;
            tri[ti + 5] = vi + 1;
            qi++;
        };
        // float hScl = 1.01f;
        Vector3 hOff = Vector3.back * h;
        quad(a + hOff, b + hOff, c + hOff, d + hOff);
        quad(a, b, a + hOff, b + hOff);
        quad(c, a, c + hOff, a + hOff);
        quad(b, d, b + hOff, d + hOff);

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = tri;

        var pc = go.AddComponent<PolygonCollider2D>();

        Vector2[] v = { a, b, d, c };
        pc.SetPath(0, v);
    }
    void box(Vector2 _a, Vector2 _b, Vector2 _c, Vector2 _d, float h, Material m) {
        Vector3 a = _a, b = _b, c = _c, d = _d;
        var go = new GameObject();
        var t = go.transform;
        t.parent = transform;
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = m;
        var mesh = new Mesh();

        mf.sharedMesh = mesh;

        int qc = 5, vc = qc * 4;
        var vertices = new Vector3[vc];
        var normals = new Vector3[vc];
        var uv = new Vector2[vc];
        var tri = new int[qc * 6];

        int qi = 0;
        Dlg quad = (Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) => {

            int vi = qi * 4, ti = qi * 6;
            vertices[vi + 0] = p1;
            vertices[vi + 1] = p2;
            vertices[vi + 2] = p3;
            vertices[vi + 3] = p4;


            var nrm = -(Vector3.Cross(p2 - p1, p3 - p1) + Vector3.Cross(p3 - p4, p2 - p4)).normalized;
            normals[vi + 0] = nrm;
            normals[vi + 1] = nrm;
            normals[vi + 2] = nrm;
            normals[vi + 3] = nrm;

            uv[vi + 0] = new Vector2(0, 0);
            uv[vi + 1] = new Vector2(1, 0);
            uv[vi + 2] = new Vector2(0, 1);
            uv[vi + 3] = new Vector2(1, 1);

            tri[ti + 0] = vi + 0;
            tri[ti + 1] = vi + 2;
            tri[ti + 2] = vi + 1;

            tri[ti + 3] = vi + 2;
            tri[ti + 4] = vi + 3;
            tri[ti + 5] = vi + 1;
            qi++;
        };
        // float hScl = 1.01f;
        Vector3 hOff = Vector3.back * h;
        quad(a + hOff, b + hOff, c + hOff, d + hOff);
        quad(a, b, a + hOff, b + hOff);
        quad(c, a, c + hOff, a + hOff);
        quad(b, d, b + hOff, d + hOff);
        quad(d, c, d + hOff, c + hOff);

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.triangles = tri;

        var pc = go.AddComponent<PolygonCollider2D>();

        Vector2 [] v = { a, b, d, c };
        pc.SetPath( 0, v );

    }

    void gen() {

        foreach(var t in transform.GetComponentsInChildren<Transform>()) {
            if(t.gameObject != gameObject) 
                DestroyImmediate(t.gameObject);
        }

        //box(Vector2.zero, new Vector2(Width, 0), new Vector2(0, Height), new Vector2(Width, Height), 1,M1 );

        Random.seed = Seed;

        var spurs = new Vector2[Spurs, 5];
        float sS = Random.Range(-Mathf.PI, Mathf.PI);
        for(int i = Spurs; i-- > 0; ) {
            var a = i * (Mathf.PI * 2 / Spurs) + sS;
            Vector2 p = new Vector2(Mathf.Sin(a), Mathf.Cos(a));

            var d = Mathf.Abs(p.x) + Mathf.Abs(p.y);

            p += (p * d - p) * Clover;

            spurs[i, 0] = p;

            p.Scale(new Vector2(Width, Height) * 0.5f);
          //  Gizmos.DrawLine(transform.position, transform.TransformPoint(p));
        }



        //Gizmos.color = Color.black;
        float sw = 0.3f;
        float s1 = 0.1f, s2 = 0.3f;
        for(int i = Spurs; i-- > 0; ) {
            var p = spurs[i, 0];
            var n0 = spurs[(i + Spurs - 1) % Spurs, 0];
            var n1 = spurs[(i + 1) % Spurs, 0];

            var d = ((p - n0).magnitude + (p - n1).magnitude) * 0.5f * sw * Random.Range(0.9f, 1.1f);

            var op = p;
            Vector2 tan = Vector3.Cross(p + Random.insideUnitCircle * s2, Vector3.forward).normalized;
            p += Random.insideUnitCircle * s1;


            Vector2 p1 = p + tan * d; p1.Scale(new Vector2(Width, Height) * 0.5f);
            Vector2 p2 = p - tan * d; p2.Scale(new Vector2(Width, Height) * 0.5f);
            float m1 = p1.magnitude, m2 = p2.magnitude;
            Vector2 p3 = (p1 + p + Random.insideUnitCircle * s2).normalized * (m1 + m2) * 0.9f * Random.Range(0.9f, 1.1f);
            Vector2 p4 = (p2 + p + Random.insideUnitCircle * s2).normalized * (m1 + m2) * 0.9f * Random.Range(0.9f, 1.1f);

            spurs[i, 1] = p1;
            spurs[i, 2] = p2;
            spurs[i, 3] = p3;
            spurs[i, 4] = p4;

            subBox(p2, p1, p4, p3, 1, M1);
          //  Gizmos.DrawLine(transform.TransformPoint(p1), transform.TransformPoint(p2));

          //  Gizmos.DrawLine(transform.TransformPoint(p1), transform.TransformPoint(p3));
          //  Gizmos.DrawLine(transform.TransformPoint(p2), transform.TransformPoint(p4));
        }

       // Gizmos.color = Color.red;
        for(int i = Spurs, j = 0; i-- > 0; j = i) {

            Vector2 p1 = spurs[i, 1], p2 = spurs[j, 2], p3 = spurs[i, 3], p4 = spurs[j, 4];
            float inst = 0.72f;
            p1 = (p3*0.3f + p1 * inst) ;
            p2 = (p4 * 0.3f + p2 * inst) ;
            subBox(p1, p2, p3, p4, 0.5f, M2);
         //   Gizmos.DrawLine(transform.TransformPoint(spurs[i, 3]), transform.TransformPoint(spurs[j, 4]));
        }

        float bS = Random.Range(-Mathf.PI, Mathf.PI);
        int Boxes = 3;
        for(int i = Boxes; i-- > 0; ) {


            var a = i * (Mathf.PI * 2 / Boxes) + bS;
            Vector2 p = new Vector2(Mathf.Sin(a), Mathf.Cos(a));

            var d = Mathf.Abs(p.x) + Mathf.Abs(p.y);

            p += (p * d - p) * Clover;
            p.Scale(new Vector2(Width, Height) *Random.Range(0.175f, 0.225f ) );

            var c1 = Random.insideUnitCircle.normalized *Random.Range(1.25f,1.75f);
            var c2 = new Vector2(c1.y, -c1.x) + Random.insideUnitCircle * 0.35f;

            box(p + c1, p - c2, p + c2 + Random.insideUnitCircle * 0.15f, p - c1 + Random.insideUnitCircle * 0.15f, 1.5f, M2);
        }
    }

    public int Spurs = 8;
    public float Clover = 0.2f;
    void OnDrawGizmos() {

       
    }
}
