using Unity.Netcode;
using UnityEngine;

public class SimpleMenuController : MonoBehaviour
{    
    [SerializeField]
    private Animator _menu;

    [SerializeField]
    private GameObject m_MenuContainer;

    [SerializeField]
    private GameObject m_Logo;

    public SimpleEnemySpawner m_SimpleEnemySpawner;
    
    private bool _pressAnyKeyActive = true;

    public void OnClickHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("No Network manager present, cannot start as host");
            return;
        }

        NetworkManager.Singleton.StartHost();
        HideMenu();
        
        EnableSimpleEnemySpawner();
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

        EnableSimpleEnemySpawner();
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
    
    private void Update()
    {
        if (_pressAnyKeyActive)
        {
            if (Input.anyKey)
            {
                ToMenu();
                _pressAnyKeyActive = false;
            }
        }
    }

    private void HideMenu()
    {
        _menu.enabled = false;
        m_MenuContainer.SetActive(false);
        m_Logo.SetActive(false);
    }

    private void ToMenu()
    {        
        _menu.SetTrigger("enter_menu");
    }

    private void EnableSimpleEnemySpawner()
    {
        if (m_SimpleEnemySpawner != null)
        {
            m_SimpleEnemySpawner.gameObject.SetActive(true);
        }
    }
}
