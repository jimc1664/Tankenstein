using UnityEngine;
using System.Collections;

public class MovementScorer : Scorer {




    public float Vel = 0;
    public float AV = 0;
    public float Spacing = 0;

    public float Vel_Mod = 1;
    public float AV_Mod = 0.0025f;
    public float Spacing_Mod = 0.5f;
    public float Dir_Mod = 0.5f;

    Vector2[] PosList = new Vector2[20];
    int PLi = -1;
    int Skip = 0;


    public override void aFixedUpdate() {

        AV += Motor.AngVel;
        if(Skip-- > 0) return;
        Skip = 5;
        var np = Motor.Pos;

        float dirFactor = Dir_Mod;
        Vel += (np - PosList[PLi]).magnitude
            * (1.0f - dirFactor + Mathf.Sign(Vector2.Dot(Motor.Pos, Motor.Forward)) * dirFactor);
        PosList[PLi] = np;

        foreach(var r in Motor.Scanner) {
            float m = Mathf.Min(0.2f, r.Out_Dis * r.RangeMod);
            Spacing += m / r.RangeMod;
        }
        PLi = (PLi + 1) % PosList.Length;

        Score = Spacing* Spacing_Mod + Vel* Vel_Mod - Mathf.Abs(AV) * AV_Mod;
        //Score = Spacing;
    }

    public override void reset(Transform t, Scorer _s, Test.ArenaData ad) {
        var s = _s as MovementScorer;
        Motor.reset(t);
 
        for(int i = PosList.Length; i-- > 0;) {
            PosList[i] = Motor.Pos;
        }
        PLi = 0;
        //LastPos = Motor.Pos;
        Spacing = Vel = AV = Score = 0;
        //Score = 1000;
        
        Vel_Mod = s.Vel_Mod;
        AV_Mod = s.AV_Mod;
        Spacing_Mod = s.Spacing_Mod;
        Dir_Mod = s.Dir_Mod;
    }


};

