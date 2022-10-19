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

            // Validate id before proceeding further.
            var gameId = CurrentGameId();
            var state = _gameService.GetSummary(gameId); // throws if invalid id

            await base.OnConnectedAsync();

            // Send current state upon new connection.
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).GameState(state);
        }

        private string CurrentGameId()
        {
            var gameId = Context?.GetHttpContext()?.GetRouteValue("id") as string;

            if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException();

            return gameId;
        }

        public Task<GuessWordResponseDTO> GuessWord(string word)
        {
            try
            {
                _gameService.GuessWord(CurrentGameId(), Context.ConnectionId, word);
                return Task.FromResult(new GuessWordResponseDTO(GuessState.OK));
            }
            catch (UnknownWordException)
            {
                return Task.FromResult(new GuessWordResponseDTO(GuessState.UnknownWord));
            }
        }
    }

    public interface IGameHubOutboundMessages
    {
        public Task GameState(GameSummaryDTO state);
    }
}