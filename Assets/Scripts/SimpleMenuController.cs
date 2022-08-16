using Unity.Netcode;
using UnityEngine;

public class SimpleMenuController : MonoBehaviour
{    
    [SerializeField]
    private Animator m_menuAnimator;

    [SerializeField]
    private GameObject m_MenuContainer;

    [SerializeField]
    private GameObject m_Logo;
    
    private bool m_pressAnyKeyActive = true;

    public void OnClickHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("No Network manager present, cannot start as host");
            return;
        }

        NetworkManager.Singleton.StartHost();
        HideMenu();
    }

    public void OnClickJoin()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("No Network manager present, cannot start as client");
            return;
        }

        NetworkManager.Singleton.StartClient();
        HideMenu();
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
    
    private void Update()
    {
        if (m_pressAnyKeyActive)
        {
            if (Input.anyKey)
            {
                ToMenu();
                m_pressAnyKeyActive = false;
            }
        }
    }

    private void HideMenu()
    {
        m_menuAnimator.enabled = false;
        m_MenuContainer.SetActive(false);
        m_Logo.SetActive(false);
    }

    private void ToMenu()
    {        
        m_menuAnimator.SetTrigger("enter_menu");
    }
}
