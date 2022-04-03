using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Butterfly : MonoBehaviour
{
    [SerializeField]
    float maxFlapAngle = 25f;

    [SerializeField]
    float flapsPerSecond = 3;

    [SerializeField]
    Transform leftWing;

    [SerializeField]
    Transform rightWing;

    [SerializeField]
    float minLifetime = 4f;

    [SerializeField]
    float maxLifetime = 10f;

    [SerializeField]
    float minSpeed = 4f;

    [SerializeField]
    float maxSpeed = 10f;

    float speed = 0;
    float lifetime = 0;
    float birth = 0f;
    float angleSpeed = 2f;
    float angle = 0;
    private void Start()
    {
        lifetime = Random.Range(minLifetime, maxLifetime);
        speed = Random.Range(minSpeed, maxSpeed);
        birth = Time.timeSinceLevelLoad;
        angle = Random.Range(0, 360);
        angleSpeed = AngleSpeed;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    private float AngleSpeed
    {
        get
        {
            return (Random.value > 0.5f ? -1 : 1) * Random.Range(10, 140);
        }
    }
    
    void Update()
    {
        if (Time.timeSinceLevelLoad - birth > lifetime)
        {
            Destroy(gameObject);
        }
        float rotY = maxFlapAngle * Mathf.Abs(Mathf.Sin(Time.timeSinceLevelLoad * Mathf.PI * flapsPerSecond));        
        leftWing.localRotation = Quaternion.Euler(0, rotY, 0);
        rightWing.localRotation = Quaternion.Euler(0, -rotY, 0);

        angle += angleSpeed * Time.deltaTime;
        angle %= 360;

        if (Random.value < 0.001f)
        {
            angleSpeed = AngleSpeed;
        }
        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.position += transform.up * speed * Time.deltaTime;
    }
}
