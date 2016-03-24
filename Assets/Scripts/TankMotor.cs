using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TankMotor : MonoBehaviour {


    public float MaxForce = 15f;
    public float Acceleration = 10;
    public float DeAcceleration = 15;
    public float TurretSpd = 90f;
    public float RoF = 0.75f, BulletSpeed = 40.0F;



    public GameObject BulletFab;
    public Transform Turret, Barrel;


    public float In_LeftMv = 0, In_RightMv = 0, In_TurretRot = 0, In_Fire = 0;
    public Vector2 Out_Vel, Out_TurretDir;
    public float Out_RoFTimer;

    [System.Serializable]
    public class Ray {
        public float Out_Dis;
        public float RangeMod;
        public Vector2 Dir;
    };
    public List<Ray> Scanner = new List<Ray>();
    public int RayCount = 32;
    public float BaseRange = 10;
    public float ScannerSkewDir = 0.6f;
    public float ScannerSkewRange = 0.8f;
    ScannerHlpr ScanH;

    //estimate - cos too lazy to calculate...
    public float EffMaxSpeed = 15;
    public float CurrentSpeed = 0;

    [HideInInspector]
    public Transform Trnsfrm;
    [HideInInspector]
    public Rigidbody2D Body;

    [HideInInspector]
    public Vector2 Forward;

    float TurretAngle;
    float RightMtr, LeftMtr;
    float RoFTimer;

    int LayerMask;

    Test Tst;
    void OnEnable() {
        Trnsfrm = transform;
        Body = GetComponent<Rigidbody2D>();
        ScanH = GetComponent<ScannerHlpr>();
        setLayer(gameObject.layer);

        reset(Trnsfrm);

        Tst = FindObjectOfType<Test>();
    }

    public void reset( Transform t ) {
        Trnsfrm.position = t.position;
        Trnsfrm.rotation = t.rotation;

        Body.velocity = Vector2.zero;
        Body.angularVelocity = 0;

        RightMtr = LeftMtr = TurretAngle = RoFTimer = 0;
        In_LeftMv = In_RightMv = In_TurretRot = In_Fire = 0;
    }

    public void setLayer( int l ) {

        gameObject.layer = l;
        l += (l & 1) == 0 ? 1 : -1;
        LayerMask = (1 << l) | (1 << 31); 
    }

    public bool UseJimCast = true;

    void FixedUpdate() {
        Vector2 fwd = Trnsfrm.up, right = Trnsfrm.right, pos = Body.position, vel = Body.velocity, turretFwd = Turret.forward;
        
        Forward = fwd;

        CurrentSpeed = vel.magnitude;
        vel /= EffMaxSpeed;
        Out_Vel = new Vector2(Vector2.Dot(fwd, vel), Vector2.Dot(right, vel));
        Out_TurretDir = (Vector2)Trnsfrm.InverseTransformDirection(turretFwd);
        Out_RoFTimer = Mathf.Max(RoFTimer / RoF, -1);

        if(Tst != null) UseJimCast = Tst.UseJimCast;
        if(UseJimCast) {
            ScanH.proc();
        } else {
            foreach(var r in Scanner) {
                var hit = Physics2D.Raycast(pos, Trnsfrm.TransformDirection(r.Dir), r.RangeMod * BaseRange, LayerMask);
                if(hit.collider != null) {

                    r.Out_Dis = hit.fraction;
                    //if is other tank...
                    //...do stuff
                } else
                    r.Out_Dis = 1;
            }
        }

        RightMtr = Mathf.Lerp(RightMtr, In_RightMv, (Mathf.Abs(RightMtr) > In_RightMv * Mathf.Sign(RightMtr) ? DeAcceleration : Acceleration) * Time.deltaTime);
        LeftMtr = Mathf.Lerp(LeftMtr, In_LeftMv, (Mathf.Abs(LeftMtr) > In_LeftMv * Mathf.Sign(LeftMtr) ? DeAcceleration : Acceleration) * Time.deltaTime);

        Body.AddForceAtPosition(fwd * MaxForce * RightMtr, pos + right * 0.5f, ForceMode2D.Force);
        Body.AddForceAtPosition(fwd * MaxForce * LeftMtr, pos - right * 0.5f, ForceMode2D.Force);

        
        TurretAngle = Mathf.MoveTowardsAngle(TurretAngle, TurretAngle + In_TurretRot * 120.0f, TurretSpd * Time.deltaTime);

        Turret.localEulerAngles = new Vector3(0, TurretAngle, 0);

        if((RoFTimer -= Time.deltaTime) < 0 && In_Fire > 0) {

            var go=Instantiate(BulletFab, Barrel.position, Turret.rotation) as GameObject;
            go.layer = gameObject.layer;
            go.GetComponent<Rigidbody2D>().velocity = turretFwd * BulletSpeed;
            RoFTimer = RoF;
        }


    }


    void OnDrawGizmos() {

     
        if(Scanner.Count != RayCount) {
            Scanner.Clear();

            float a = 0;
            for(int i = 0; i < RayCount; i++) {
                var r = new Ray();
                r.Out_Dis = 1;
                r.Dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                r.Dir += Vector2.up * ScannerSkewDir;
                float mag = r.Dir.magnitude;
                mag += (1 - mag) * (1-ScannerSkewRange);
                r.Dir.Normalize();
                r.RangeMod = mag;


                a += 360.0f * Mathf.Deg2Rad / (float)RayCount;
                Scanner.Add( r );
            }
        }
        Trnsfrm = transform;
        foreach( var r in Scanner ) {
            Gizmos.color = Color.blue;            
            Gizmos.DrawLine( Trnsfrm.position, Trnsfrm.TransformPoint( r.Dir*r.RangeMod*BaseRange) );
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Trnsfrm.position, Trnsfrm.TransformPoint(r.Dir * r.RangeMod * BaseRange * r.Out_Dis));
        }
    }
}
