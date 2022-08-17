using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSelf : MonoBehaviour
{
    //生命周期
    public float selfTime = 1f;
    public float hitTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        hitTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - hitTime > selfTime)
        {
            Destroy(gameObject);
        }
    }
}
