using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D c) {


        Destroy(gameObject);
    }
}
