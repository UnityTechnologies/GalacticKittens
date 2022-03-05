using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/*
    Script that controls the boss health ui
    set by boss controller when spawn
*/

public class BossUI : NetworkBehaviour
{
    [SerializeField]
    Slider m_healthSlider;

    [SerializeField]
    Image m_healthImage;

    [SerializeField]
    HealthColor m_healthColor;

    int maxHealth;

    [ClientRpc]
    private void SetHealthClientRpc(int health)
    {
        if (IsServer)
            return;

        maxHealth = health;
        m_healthImage.color = m_healthColor.normalColor;
        gameObject.SetActive(true);
    }

    [ClientRpc]
    private void UpdateUIClientRpc(float currentHealth)
    {
        if (IsServer)
            return;

        m_healthSlider.value = currentHealth;
        m_healthImage.color = m_healthColor.GetHealthColor(currentHealth);
    }

    public void SetHealth(int health)
    {
        if (!IsServer)
            return;
        
        maxHealth = health;
        m_healthImage.color = m_healthColor.normalColor;
        gameObject.SetActive(true);
        SetHealthClientRpc(health);
    }

    public void UpdateUI(int currentHealth)
    {
        if (!IsServer)
            return;
        
        float convertedHealth = (float)currentHealth / maxHealth;
        m_healthSlider.value = convertedHealth;
        m_healthImage.color = m_healthColor.GetHealthColor(convertedHealth);

        UpdateUIClientRpc(convertedHealth);
    }

    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(false);

        base.OnNetworkSpawn();
    }
}
