using CooperativeWordGuess.Data;
using CooperativeWordGuess.Entities;
using Microsoft.AspNetCore.SignalR;

namespace CooperativeWordGuess.Hubs
{
    public class GameHub : Hub<IGameHubOutboundMessages>
    {
        private readonly GameService _gameService;

        public GameHub(GameService gameService)
        {
            _gameService = gameService;
        }

        public override async Task OnConnectedAsync()
        {
            var gameId = CurrentGameId();
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);

            await base.OnConnectedAsync();

            // Send current state upon new connection.
            var state = _gameService.GetSummary(gameId);
            await Clients.Group(gameId).GameState(state);
        }

        private string CurrentGameId()
        {
            var gameId = Context?.GetHttpContext()?.GetRouteValue("id") as string;

            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException();

            return gameId;
        }

        public Task GuessWord(string word)
        {
            _gameService.GuessWord(CurrentGameId(), Context.ConnectionId, word);
            return Task.CompletedTask;
        }
    }

    public interface IGameHubOutboundMessages
    {
        public Task GameState(GameSummaryDTO state);
    }
}