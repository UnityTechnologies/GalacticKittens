using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance => s_instance;
    private static RelayManager s_instance = null;

    [SerializeField]
    private string m_Environment = "production";

    [SerializeField]
    private int m_MaxConnections = 4;

    public bool IsRelayEnabled => Transport != null &&
        Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    public UnityTransport Transport => NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

    public Task<string> JoinCodeAsyncTask => 
        m_hostAllocationID != System.Guid.Empty ?
            RelayService.Instance.GetJoinCodeAsync(m_hostAllocationID) : null;

    private System.Guid m_hostAllocationID;

    private void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<RelayHostData> SetupRelay()
    {
        Debug.Log("Relay setup starting!");
        InitializationOptions options = await GetInitializationOptionsAsync();

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(m_MaxConnections);

        var relayHostData = new RelayHostData()
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            IPv4Address = allocation.RelayServer.IpV4,
            ConnectionData = allocation.ConnectionData
        };

        relayHostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(relayHostData.AllocationID);
        m_hostAllocationID = relayHostData.AllocationID;

        Transport.SetRelayServerData(
            relayHostData.IPv4Address,
            relayHostData.Port,
            relayHostData.AllocationIDBytes,
            relayHostData.Key,
            relayHostData.ConnectionData,
            null,
            true);

        Debug.Log($"Relay setup started with code: {relayHostData.JoinCode} ");

        return relayHostData;
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode)
    {
        try
        {
            InitializationOptions options = await GetInitializationOptionsAsync();

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayJoinData relayJoinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)allocation.RelayServer.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = allocation.RelayServer.IpV4,
                JoinCode = joinCode
            };

            Transport.SetRelayServerData(
                relayJoinData.IPv4Address,
                relayJoinData.Port,
                relayJoinData.AllocationIDBytes,
                relayJoinData.Key,
                relayJoinData.ConnectionData,
                relayJoinData.HostConnectionData,
                true);

            return relayJoinData;
        }
        catch (RelayServiceException relayServiceException)
        {
            Debug.Log(relayServiceException.Message);
            return new RelayJoinData();
        }
    }

    private async Task<InitializationOptions> GetInitializationOptionsAsync()
    {
        //string profileName = "default" + (ParrelSync.ClonesManager.IsClone() ? "_clone" : "");

        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(m_Environment);
            //.SetProfile(profileName);

        //Debug.Log($"Profile name: {profileName}");

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        return options;
    }
}
