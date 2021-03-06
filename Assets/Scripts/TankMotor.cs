﻿using UnityEngine;
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

    public TankMotor Opponent;
    public float Rad;

    public float In_LeftMv = 0, In_RightMv = 0, In_TurretRot = 0, In_Fire = 0;
    public Vector2 Out_Vel, Out_TurretDir;
    public float Out_RoFTimer;

    [System.Serializable]
    public class Ray {
        public float Out_Dis;
        public float Out_Opponent;
        public float RangeMod;
        public Vector2 Dir;
        public Vector2 ODir;
    };
    public List<Ray> Scanner = new List<Ray>();
    public int RayCount = 32;
    public float BaseRange = 10;
    public float ScannerSkewDir = 0.6f;
    public float ScannerSkewRange = 0.8f;

    [HideInInspector]
    public ScannerHlpr ScanH;

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

        Rad = GetComponent<CircleCollider2D>().radius;
        reset(Trnsfrm, gameObject.layer);

        Tst = FindObjectOfType<Test>();

        Sys.get().add(this);
    }
    void OnDisable() {
        if(Sys.get())
            Sys.get().Tanks.Remove(this);
    }

    public void reset( Transform t, int layer ) {
        Trnsfrm.position = t.position;
        Trnsfrm.rotation = t.rotation;

        Body.velocity = Vector2.zero;
        Body.angularVelocity = 0;

        RightMtr = LeftMtr = TurretAngle = RoFTimer = 0;
        In_LeftMv = In_RightMv = In_TurretRot = In_Fire = 0;

        setLayer(layer);
    }

    public void setLayer( int l ) {

        gameObject.layer = l;
        l += (l & 1) == 0 ? 1 : -1;
        LayerMask = (1 << l) | (1 << 31); 
    }

    public Vector2 Pos, Vel;
    public float AngVel;


    Matrix4x4 Mat;
    Vector3 TurretFwd;

    public void aFixedUpdate_UnThreaded() {
        AngVel = Body.angularVelocity;
        Mat = Trnsfrm.localToWorldMatrix;
        TurretFwd = Turret.forward;

        Vector2 fwd = Forward = Mat.GetColumn(1), right = Mat.GetColumn(0);
        Vector2 pos = Pos = Body.position, vel = Vel =Body.velocity;

        Body.AddForceAtPosition(fwd * MaxForce * RightMtr, pos + right * 0.5f, ForceMode2D.Force);
        Body.AddForceAtPosition(fwd * MaxForce * LeftMtr, pos - right * 0.5f, ForceMode2D.Force);

        Turret.localEulerAngles = new Vector3(0, TurretAngle, 0);

        if((RoFTimer -= Time.deltaTime) < 0 && In_Fire > 0) {

            var go=Instantiate(BulletFab, Barrel.position, Turret.rotation) as GameObject;
            go.layer = gameObject.layer;
            go.GetComponent<Rigidbody2D>().velocity = TurretFwd * BulletSpeed;
            RoFTimer = RoF;
        }


    }
    public void aFixedUpdate_Threaded() {
        Vector2 fwd = Forward = Mat.GetColumn(1), right = Mat.GetColumn(0);

        var vel = Vel;
        CurrentSpeed = vel.magnitude;
        vel /= EffMaxSpeed;
        Out_Vel = new Vector2(Vector2.Dot(fwd, vel), Vector2.Dot(right, vel));
        Out_TurretDir = (Vector2)Mat.transpose.MultiplyVector(TurretFwd);
        Out_RoFTimer = Mathf.Max(RoFTimer / RoF, -1);

        //  Debug.Log("scan");
        foreach(var r in Scanner) {
            // Debug.Log(" d  " + r.Out_Dis);
            r.Dir = Mat.MultiplyVector(r.ODir).normalized;
        }

        RightMtr = Mathf.Lerp(RightMtr, In_RightMv, (Mathf.Abs(RightMtr) > In_RightMv * Mathf.Sign(RightMtr) ? DeAcceleration : Acceleration) * Sys.DeltaTime);
        LeftMtr = Mathf.Lerp(LeftMtr, In_LeftMv, (Mathf.Abs(LeftMtr) > In_LeftMv * Mathf.Sign(LeftMtr) ? DeAcceleration : Acceleration) * Sys.DeltaTime);

        TurretAngle = Mathf.MoveTowardsAngle(TurretAngle, TurretAngle + In_TurretRot * 120.0f, TurretSpd * Sys.DeltaTime);

    }

    void OnDrawGizmos() {

     
        if(Scanner.Count != RayCount) {
            Scanner.Clear();

            float a = 0;
            for(int i = 0; i < RayCount; i++) {
                var r = new Ray();
                r.Out_Dis = 1;
                r.ODir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                r.ODir += Vector2.up * ScannerSkewDir;
                float mag = r.ODir.magnitude;
                mag += (1 - mag) * (1-ScannerSkewRange);
                r.ODir.Normalize();
                r.RangeMod = mag;


                a += 360.0f * Mathf.Deg2Rad / (float)RayCount;
                Scanner.Add( r );
            }
        }
        Trnsfrm = transform;
        Pos = Trnsfrm.position;
        Forward = Trnsfrm.up;
        foreach( var r in Scanner ) {
            r.Dir = Trnsfrm.TransformDirection(r.ODir);
            Gizmos.color = Color.blue;            
            Gizmos.DrawLine( Trnsfrm.position, Trnsfrm.TransformPoint( r.ODir*r.RangeMod*BaseRange) );
            if( r.Out_Opponent != 0 )
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.green;
            Gizmos.DrawLine(Trnsfrm.position, Trnsfrm.TransformPoint(r.ODir * r.RangeMod * BaseRange * r.Out_Dis));
        }
    }
}
