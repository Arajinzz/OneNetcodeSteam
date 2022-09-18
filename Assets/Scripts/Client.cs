using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    // to run at fixed tick rate
    private float clientTimer;
    public uint clientTick;
    private float minTimeBetweenTicks;
    private const float CLIENT_TICK_RATE = 60f;

    void Start()
    {
        clientTimer = 0.0f;
        clientTick = 0;
        minTimeBetweenTicks = 1 / CLIENT_TICK_RATE;
    }


    void Update()
    {
        clientTimer += Time.deltaTime;

        while (clientTimer >= minTimeBetweenTicks)
        {
            clientTimer -= minTimeBetweenTicks;

            // Handle tick here

            clientTick++;
        }
    }
}
