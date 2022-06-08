using System.Linq;
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

    public UnityTransport Transport =>
        NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

    public Task<string> JoinCodeAsyncTask => 
        m_hostAllocationID != System.Guid.Empty ?
            RelayService.Instance.GetJoinCodeAsync(m_hostAllocationID) : null;

    private System.Guid m_hostAllocationID;

    private readonly string k_dtlsConnectionType = "dtls";

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
        try
        {
            Debug.Log("Relay setup starting!");
            InitializationOptions options = await GetInitializationOptionsAsync();

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(
                m_MaxConnections);

            var dtlsEndpoint = allocation.ServerEndpoints.First(
                relayServerEndPoint => relayServerEndPoint.ConnectionType == k_dtlsConnectionType);

            var relayHostData = new RelayHostData()
            {
                Key = allocation.Key,
                Port = (ushort)dtlsEndpoint.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                IPv4Address = dtlsEndpoint.Host,
                ConnectionData = allocation.ConnectionData
            };

            m_hostAllocationID = relayHostData.AllocationID;

            relayHostData.JoinCode = await RelayService.Instance.GetJoinCodeAsync(m_hostAllocationID);

            Transport.SetHostRelayData(
                relayHostData.IPv4Address,
                relayHostData.Port,
                relayHostData.AllocationIDBytes,
                relayHostData.Key,
                relayHostData.ConnectionData,
                true);

            Debug.Log($"Relay setup started with code: {relayHostData.JoinCode} ");

            return relayHostData;
        }
        catch (RelayServiceException relayServiceException)
        {
            Debug.Log(relayServiceException.Message);

            return new RelayHostData();
        }
    }

    public async Task<RelayJoinData> JoinRelay(string joinCode)
    {
        try
        {
            InitializationOptions options = await GetInitializationOptionsAsync();

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var dtlsEndpoint = allocation.ServerEndpoints.First(
                relayServerEndPoint => relayServerEndPoint.ConnectionType == k_dtlsConnectionType);
            
            var relayJoinData = new RelayJoinData
            {
                Key = allocation.Key,
                Port = (ushort)dtlsEndpoint.Port,
                AllocationID = allocation.AllocationId,
                AllocationIDBytes = allocation.AllocationIdBytes,
                ConnectionData = allocation.ConnectionData,
                HostConnectionData = allocation.HostConnectionData,
                IPv4Address = dtlsEndpoint.Host,
                JoinCode = joinCode
            };

            Transport.SetClientRelayData(
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
        InitializationOptions options = new InitializationOptions()
            .SetEnvironmentName(m_Environment);

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        return options;
    }
}
