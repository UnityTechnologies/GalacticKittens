using System;
using System.Threading.Tasks;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : Singleton<RelayManager>
{
    public bool IsRelayEnabled => Transport != null &&
        Transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport;

    private UnityTransport Transport =>
        NetworkManager.Singleton.gameObject.GetComponent<UnityTransport>();

    [SerializeField]
    private string m_Environment = "production";

    [SerializeField]
    private int m_MaxConnections = 4;

    public async Task<RelayHostData> SetupRelayAsync()
    {
        Debug.Log("Relay setup starting!");
        InitializationOptions options = await GetInitializationOptionsAsync();

        Allocation allocation = await Relay.Instance.CreateAllocationAsync(m_MaxConnections);

        RelayHostData relayHostData = new RelayHostData
        {
            Key = allocation.Key,
            Port = (ushort)allocation.RelayServer.Port,
            AllocationID = allocation.AllocationId,
            AllocationIDBytes = allocation.AllocationIdBytes,
            IPv4Address = allocation.RelayServer.IpV4,
            ConnectionData = allocation.ConnectionData
        };

        relayHostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(relayHostData.AllocationID);

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

            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

            RelayJoinData relayJoinData = new()
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
        catch(RelayServiceException relayServiceException)
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
