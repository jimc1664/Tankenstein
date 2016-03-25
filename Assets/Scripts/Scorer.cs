using UnityEngine;
using System.Collections;

public class Scorer : MonoBehaviour {


    [HideInInspector]
    public TankMotor Motor;
    [HideInInspector]
    public AiTankController Ctrl;

    protected void OnEnable() {
        Ctrl = GetComponent<AiTankController>();
        Motor = Ctrl.Motor;
    }
    public float Score = 0;


    public virtual void reset(Transform t, Scorer s ) {

    }
}
