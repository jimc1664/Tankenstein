using UnityEngine;
using System.Collections;

public class PathScorer : Scorer  {

    public Transform Target;

    public float RotMod = 0.2f;
    Vector2 TPos, TFwd;

    public float OScore, RScore;
    public float MaxScore;


    public float AV = 0;
    public float AV_Mod = 0.0025f;



    public override void aFixedUpdate() {

        AV += Motor.AngVel;
        float s = OScore - (Motor.Pos - TPos).magnitude;// * (1 + RotMod * (0.5f - Vector2.Dot(Motor.Forward, TFwd) * 0.5f));
        MaxScore = Mathf.Max(MaxScore, s);
        RScore += 1+Vector2.Dot(Motor.Forward, TFwd);
        Score = (s +MaxScore)*0.5f - Mathf.Abs(AV)*AV_Mod  + RScore *RotMod;

    }

    public override void reset(Transform t, Scorer _s, Test.ArenaData ad, int layer) {
        var s = _s as PathScorer;

        RotMod = s.RotMod;
        AV_Mod = s.AV_Mod;

        Target = ad.S2;

        Motor.reset(t, layer);


        AV = RScore = MaxScore = Score = 0;
        TPos = Target.position;
        TFwd = Target.up;

        OScore = (Motor.Body.position - TPos).magnitude;// * (1.0f+ RotMod);
    }

}
