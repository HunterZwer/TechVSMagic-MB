using UnityEngine;

public class PauseScript : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    

    public void PauseButton()
    {
        Debug.Log("Pause");
        pausePanel.SetActive(true);
        //RTSCameraController.instance.stopWork = true;
        Time.timeScale = 0f;
    }
    public void ContinueButton()
    {
        Time.timeScale = 1f;
        //RTSCameraController.instance.stopWork = false;
        pausePanel.SetActive(false);
    }
    public void MenuBackButton()
    {
        
    }
}
