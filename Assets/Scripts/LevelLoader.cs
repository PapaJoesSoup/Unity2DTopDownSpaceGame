using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
  public class LevelLoader : MonoBehaviour
  {
    public Animator Transition;

    // Update is called once per frame
    void Update()
    {
      if (Input.GetKeyDown(KeyCode.PageDown))
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
      int index = SceneManager.GetActiveScene().buildIndex;
      if (index >= SceneManager.sceneCount) index = 0;
      StartCoroutine(LoadLevel(index));
    }

    IEnumerator LoadLevel(int index)
    {
      Transition.SetTrigger("Start");
      yield return new WaitForSeconds(1f);
      SceneManager.LoadScene(index);

    }
  }
}
