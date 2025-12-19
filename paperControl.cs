using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paperControl : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject paper;
    [SerializeField] private GameObject paper0;

    void Start()
    {
        paper.SetActive(false);
        paper0.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetPapertrue()
    {
        paper.SetActive(true);
    }
    public void SetPaperfalse()
    {
        paper.SetActive(false);
    }
    public void SetPaper0true()
    {
        paper0.SetActive(true);
    }
}
