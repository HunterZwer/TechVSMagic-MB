using UnityEngine;

public class PauseScript : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    private bool isPaused = false;

    void Update()
    {
        // Check for Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle pause state
            if (isPaused)
                ContinueButton();
            else
                PauseButton();
        }
    }

    public void PauseButton()
    {
        Debug.Log("Pause");
        pausePanel.SetActive(true);
        //RTSCameraController.instance.stopWork = true;
        Time.timeScale = 0f;
        isPaused = true;
    }
    
    public void ContinueButton()
    {
        Time.timeScale = 1f;
        //RTSCameraController.instance.stopWork = false;
        pausePanel.SetActive(false);
        isPaused = false;
    }
    
    public void MenuBackButton()
    {
        
    }
}