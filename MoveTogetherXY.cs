using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTogetherXY : MonoBehaviour
{
    [SerializeField] private GameObject FollwerXY;
    [SerializeField] private GameObject LeaderXY;
    void Start()
    {
        FollwerXY.transform.position = new Vector3(LeaderXY.transform.position.x, LeaderXY.transform.position.y, FollwerXY.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        FollwerXY.transform.position = new Vector3(LeaderXY.transform.position.x, LeaderXY.transform.position.y, FollwerXY.transform.position.z);
    }
}

