using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad01 : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject cat;
    public Animator transition;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        bool shouldLoadFromMuseum = 碰撞校史馆.instance != null && 碰撞校史馆.instance.isTouchMuseum;
        bool shouldLoadFromDoor = Door.instance != null && Door.instance.isfubeng1;

        if (transition != null && (shouldLoadFromMuseum || shouldLoadFromDoor))
        {
            LoadNextScene();

            // 安全地重置状态
            if (碰撞校史馆.instance != null)
            {
                碰撞校史馆.instance.isTouchMuseum = false;
                cat.SetActive(false);

            }
            if (Door.instance != null)
            {
                Door.instance.isfubeng1 = false;
            }
        }

    }
    private void LoadNextScene()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));

    }
    private IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("SceneLoadFinish");
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(levelIndex);
    }
}
