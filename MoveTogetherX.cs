using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTogetherX : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject Follwer;
    [SerializeField] private GameObject Leader;
    void Start()
    {
        Follwer.transform.position = new Vector3(Leader.transform.position.x, Follwer.transform.position.y, Follwer.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        Follwer.transform.position = new Vector3(Leader.transform.position.x, Follwer.transform.position.y, Follwer.transform.position.z);
    }
}
