using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerCharSelection : NetworkBehaviour
{
    public int CharSelected => m_charSelected.Value;

    private const int k_noCharacterSelectedValue = -1;

    [SerializeField]
    private NetworkVariable<int> m_charSelected =
        new NetworkVariable<int>(k_noCharacterSelectedValue);

    [SerializeField]
    private NetworkVariable<int> m_playerId =
        new NetworkVariable<int>(k_noCharacterSelectedValue);

    [SerializeField]
    private AudioClip _changedCharacterClip;

    private void Start()
    {
        if (IsServer)
        {
            m_playerId.Value = CharacterSelectionManager.Instance.GetPlayerId(OwnerClientId);
        }
        else if (!IsOwner && HasAcharacterSelected())
        {
            CharacterSelectionManager.Instance.SetPlayebleChar(
                m_playerId.Value,
                m_charSelected.Value,
                IsOwner);
        }

        // Assign the name of the object base on the player id on every instance
        gameObject.name = $"Player{m_playerId.Value + 1}";
    }

    private bool HasAcharacterSelected()
    {
        return m_playerId.Value != k_noCharacterSelectedValue;
    }

    private void OnEnable()
    {
        m_playerId.OnValueChanged += OnPlayerIdSet;
        m_charSelected.OnValueChanged += OnCharacterChanged;
        OnButtonPress.a_OnButtonPress += OnUIButtonPress;
    }

    private void OnDisable()
    {
        m_playerId.OnValueChanged -= OnPlayerIdSet;
        m_charSelected.OnValueChanged -= OnCharacterChanged;
        OnButtonPress.a_OnButtonPress -= OnUIButtonPress;
    }

    private void OnPlayerIdSet(int oldValue, int newValue)
    {
        CharacterSelectionManager.Instance.SetPlayebleChar(newValue, newValue, IsOwner);

        if (IsServer)
            m_charSelected.Value = newValue;
    }

    // Event call when server changes the network variable
    private void OnCharacterChanged(int oldValue, int newValue)
    {
        // If I am not the owner, update the character selection UI
        if (!IsOwner && HasAcharacterSelected())
            CharacterSelectionManager.Instance.SetCharacterUI(m_playerId.Value, newValue);
    }

    private void Update()
    {
        if (IsOwner && CharacterSelectionManager.Instance.GetConnectionState(m_playerId.Value) != ConnectionState.ready)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeCharacterSelection(-1);
                AudioManager.Instance.PlaySoundEffect(_changedCharacterClip);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeCharacterSelection(1);
                AudioManager.Instance.PlaySoundEffect(_changedCharacterClip);
            }
        }

        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {

                // Check that the character is not selected
                if (!CharacterSelectionManager.Instance.IsReady(m_charSelected.Value))
                {
                    CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(
                        true,
                        m_charSelected.Value);

                    ReadyServerRpc();
                }
                else
                {
                    // if selected check if is selected by me
                    if (CharacterSelectionManager.Instance.IsSelectedByPlayer(
                            m_playerId.Value, m_charSelected.Value))
                    {
                        // If it's selected by me, de-select
                        CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(
                            false,
                            m_charSelected.Value);

                        NotReadyServerRpc();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // exit the network state and return to the menu
                if (m_playerId.Value == 0) // Host
                {
                    // All player should shutdown and exit
                    StartCoroutine(HostShutdown());
                }
                else
                {
                    Shutdown();
                }
            }
        }
    }

    IEnumerator HostShutdown()
    {
        // Tell the clients to shutdown
        ShutdownClientRpc();

        // Wait some time for the message to get to clients
        yield return new WaitForSeconds(0.5f);

        // Shutdown server/host
        Shutdown();
    }

    void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }

    [ClientRpc]
    void ShutdownClientRpc()
    {
        if (IsServer)
            return;

        Shutdown();
    }

    private void ChangeCharacterSelection(int value)
    {
        // Assign a temp value to prevent the call of onchange event in the charSelected
        int charTemp = m_charSelected.Value;
        charTemp += value;
        
        if (charTemp >= CharacterSelectionManager.Instance.charactersData.Length)
            charTemp = 0;
        else if (charTemp < 0)
            charTemp = CharacterSelectionManager.Instance.charactersData.Length - 1;

        if (IsOwner)
        {
            // Notify server of the change
            ChangeCharacterSelectionServerRpc(charTemp);

            // Owner doesn't wait for the onvaluechange
            CharacterSelectionManager.Instance.SetPlayebleChar(
                m_playerId.Value,
                charTemp,
                IsOwner);
        }
    }

    [ServerRpc]
    private void ChangeCharacterSelectionServerRpc(int newValue)
    {
        m_charSelected.Value = newValue;
    }

    [ServerRpc]
    private void ReadyServerRpc()
    {
        CharacterSelectionManager.Instance.PlayerReady(
            OwnerClientId,
            m_playerId.Value,
            m_charSelected.Value);
    }

    [ServerRpc]
    private void NotReadyServerRpc()
    {
        CharacterSelectionManager.Instance.PlayerNotReady(OwnerClientId, m_charSelected.Value);
    }

    // The arrows on the player are not meant to works as buttons
    private void OnUIButtonPress(ButtonActions buttonAction)
    {
        if (!IsOwner)
            return;

        switch (buttonAction)
        {
            case ButtonActions.lobby_ready:
                CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(
                    true,
                    m_charSelected.Value);

                ReadyServerRpc();
                break;

            case ButtonActions.lobby_not_ready:
                CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(
                    false,
                    m_charSelected.Value);

                NotReadyServerRpc();
                break;
        }
    }

    public void Despawn()
    {
        NetworkObjectDespawner.DespawnNetworkObject(NetworkObject);
    }
}