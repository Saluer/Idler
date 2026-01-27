using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class MenuScript : MonoBehaviour
    {
        public void LoadGame()
        {
            SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        }
    }
}