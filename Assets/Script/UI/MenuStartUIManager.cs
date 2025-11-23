using UnityEngine;

public class MenuStartUIManager : MonoBehaviour
{

    public void OnStartButtonPressed()
    {
        CameraController.Instance.SetNormalView();
        gameObject.SetActive(false);    
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
