using UnityEngine;
using System.Collections;

public class Scorer : MonoBehaviour {


    [HideInInspector]
    public TankMotor Motor;
    [HideInInspector]
    public AiTankController Ctrl;

    public float Score = 0;

    protected void OnEnable() {
        Ctrl = GetComponent<AiTankController>();
        Motor = GetComponent<TankMotor>();
        Sys.get().add(this);
    }


    void OnDisable() {
        if(Sys.get())
            Sys.get().Scorers.Remove(this);
    }

    public virtual void aFixedUpdate() { }
    public virtual void reset(Transform t, Scorer s, Test.ArenaData ad) {

    }
}
