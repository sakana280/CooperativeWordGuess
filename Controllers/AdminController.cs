using CooperativeWordGuess.Data;
using CooperativeWordGuess.Entities;
using CooperativeWordGuess.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace CooperativeWordGuess.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly GameService _gameService;

        public AdminController(ILogger<AdminController> logger, GameService gameService)
        {
            _logger = logger;
            _gameService = gameService;
        }

        [HttpPost]
        [Route(nameof(CreateGame))]
        public CreatedGameDTO CreateGame(CreateGameDTO props)
        {
            _logger.LogInformation("Creating new game for '{word}' of {count} guesses at {interval}s/guess", props.Word, props.MaxGuesses, props.GuessDurationSeconds);
            var game = _gameService.CreateGame(props);
            return new(game.AdminToken, game.PublicToken);
        }

        [HttpPost]
        [Route(nameof(StartGame))]
        public void StartGame([FromQuery] string adminToken, [FromQuery] string publicToken)
        {
            _logger.LogInformation("Starting game {publicToken}", publicToken);
            Task.Run(() => _gameService.RunGame(adminToken, publicToken).Wait());
        }
    }
}