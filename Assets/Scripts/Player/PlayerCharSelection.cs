using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class PlayerCharSelection : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<int> m_charSelected = new NetworkVariable<int>(-1);
    [SerializeField]
    private NetworkVariable<int> m_playerId = new NetworkVariable<int>(-1);
    [SerializeField]
    private AudioClip _changedCharacterClip;

    public int CharSelected => m_charSelected.Value;

    private void Start()
    {
        if (IsServer)
        {
            m_playerId.Value = CharacterSelectionManager.Instance.GetPlayerId(OwnerClientId);
            // _charSelected.Value = _playerId.Value;
        }
        else if (!IsOwner && m_playerId.Value != -1)
        {
            // print($"player {_playerId.Value} char {_charSelected.Value}");
            CharacterSelectionManager.Instance.SetPlayebleChar(m_playerId.Value, m_charSelected.Value, IsOwner);
        }

        // Asign the name of the object base on the player id on every instance
        gameObject.name = $"Player{m_playerId.Value + 1}";
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
        // print($"OnPlayerChanged {OwnerClientId} -> old {oldValue} : new {newValue}");
        // print($"Set player Id -> OwnerId {OwnerClientId} : {oldValue} : {newValue}");
        CharacterSelectionManager.Instance.SetPlayebleChar(newValue, newValue, IsOwner);
        if (IsServer)
            m_charSelected.Value = newValue;
    }

    // Event call when server changes de network variable    
    private void OnCharacterChanged(int oldValue, int newValue)
    {
        // print($"OnCharacterChanged {OwnerClientId} -> old {oldValue} : new {newValue}");
        // print($"OncharChange -> Owner:: {OwnerClientId} : {oldValue} : {newValue} : PlayerId:: {_playerId.Value}");        

        // If i am not the owner update the character selection UI
        if (!IsOwner && m_playerId.Value != -1)
            CharacterSelectionManager.Instance.SetCharacterUI(m_playerId.Value, newValue);
    }

    // TODO: For production is better to use the new Unity Input system
    private void Update()
    {
        if (IsOwner && CharacterSelectionManager.Instance.GetConnectionState(m_playerId.Value) != ConnectionState.ready)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeCharacterSelection(-1);
                AudioManager.Instance.PlaySound(_changedCharacterClip);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                ChangeCharacterSelection(1);
                AudioManager.Instance.PlaySound(_changedCharacterClip);
            }
        }
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {

                // Check the characer is not selected
                if (!CharacterSelectionManager.Instance.IsReady(m_charSelected.Value))
                {
                    CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(true, m_charSelected.Value);
                    ReadyServerRpc();
                }
                else
                {
                    // if selected check if is selected by me
                    if (CharacterSelectionManager.Instance.IsSelectedByPlayer(m_playerId.Value, m_charSelected.Value))
                    {
                        // If is selected by my, unselect
                        CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(false, m_charSelected.Value);
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
                    // NetworkManager.Singleton.Shutdown();
                    // LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
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
        if (IsServer) return;

        Shutdown();
    }

    private void ChangeCharacterSelection(int value)
    {
        // Assing a temp value to prevent the call of onchange event in the charSelected
        int charTemp = m_charSelected.Value;
        charTemp += value;
        if (charTemp >= CharacterSelectionManager.Instance.charactersData.Length)
            charTemp = 0;
        else if (charTemp < 0)
            charTemp = CharacterSelectionManager.Instance.charactersData.Length - 1;

        if (IsOwner)
        {
            // CharacterSelectionManager.Instance.SetCharacterColor(_playerId.Value, charTemp);
            // Notify server of the change
            ChangeCharacterSelectionServerRpc(charTemp);
            // Owner dont wait fot the onvaluechange
            CharacterSelectionManager.Instance.SetPlayebleChar(m_playerId.Value, charTemp, IsOwner);
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
        CharacterSelectionManager.Instance.PlayerReady(OwnerClientId, m_playerId.Value, m_charSelected.Value);
    }

    [ServerRpc]
    private void NotReadyServerRpc()
    {
        CharacterSelectionManager.Instance.PlayerNotReady(OwnerClientId, m_charSelected.Value);
    }

    // The arrows on the player are not meant to works as buttons
    private void OnUIButtonPress(ButtonActions buttonAction)
    {
        // print($"ClientId {OwnerClientId} IsServer {IsServer} IsOwner {IsOwner} buttonAction {buttonAction}");
        if (IsOwner)
        {
            switch (buttonAction)
            {
                case ButtonActions.lobby_ready:
                    CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(true, m_charSelected.Value);
                    ReadyServerRpc();
                    break;
                case ButtonActions.lobby_not_ready:
                    CharacterSelectionManager.Instance.SetPlayerReadyUIButtons(false, m_charSelected.Value);
                    NotReadyServerRpc();
                    break;
            }
        }
    }

    public void Despawn()
    {
        NetworkObject.Despawn();
    }
}
