using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
//using Unity.Services.Matchmaker;
//using Unity.Services.Matchmaker.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

public class NetworkManagerUI : NetworkBehaviour
{
    private string lobbyId;

    private async void Start()
    {

        SceneManager.activeSceneChanged += ChangedActiveScene;
        lobbyId = null;
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }

    public async void FindMatchButtonPressed()
    {

        /*List<Player> players = new()
        //{
        //    new("Player1", new Dictionary<string, object>())
       // };

       // CreateTicketOptions options = new("queue", new Dictionary<string, object>());

       // CreateTicketResponse ticketResponse = 
       //     await MatchmakerService.Instance.CreateTicketAsync(players, options);

      //  bool isAssigned = false;
      //  MultiplayAssignment assignment = null;

      //  while (!isAssigned)
       // {
       //     await Task.Delay(TimeSpan.FromSeconds(1f));
       //
       //     if (ticketResponse == null)
       //     {
       //         continue;
       //    }
       //
      //      if (ticketResponse.GetType() == typeof(MultiplayAssignment))
       //     {
       //         assignment = ticketResponse.Value as MultiplayAssignment;
       //     }

         //   switch (assignment?.Status)
        //    {
        //        case MultiplayAssignment.StatusOptions.Found:
        //            isAssigned = true;
        //            break;
        //    }
        //}*/

        SceneManager.LoadSceneAsync("GameScreen");
        QueryLobbiesOptions queryLobbiesOptions = new()
        {
            Filters = new List<QueryFilter>
            {
                new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
        };
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

        if (queryResponse.Results.Count > 0)
        {
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
            lobbyId = lobby.Id;

            JoinAllocation joinAllocation
                = await RelayService.Instance.JoinAllocationAsync(lobby.Data["relayCode"].Value);
            RelayServerData relayServerData = new(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();

            DontDestroyOnLoad(NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject());

            SceneManager.LoadSceneAsync("GameScreen");
        }
        else
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            RelayServerData relayServerData = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            CreateLobbyOptions createOptions = new()
            {
                IsPrivate = false,
                Player = new Player(id: AuthenticationService.Instance.PlayerId, data: null),
                Data = new Dictionary<string, DataObject>
                {
                    {"relayCode", new DataObject(DataObject.VisibilityOptions.Public
                    , await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId))}
                }    
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", 2, createOptions);
            lobbyId = lobby.Id;

            DontDestroyOnLoad(NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject());   
            SceneManager.LoadSceneAsync("GameScreen");
        }
    }

    public async void ChangedActiveScene(Scene current, Scene next)
    {
        
        if (current.name == "GameScreen")
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, 
                AuthenticationService.Instance.PlayerId);
            lobbyId = null;
        }
    }
}
