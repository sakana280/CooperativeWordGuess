using CooperativeWordGuess.Data;
using CooperativeWordGuess.Entities;
using Microsoft.AspNetCore.SignalR;

namespace CooperativeWordGuess.Hubs
{
    /// <summary>
    /// Threadsafe operations on games.
    /// </summary>
    public class GameService
    {
        private readonly ILogger _logger;
        private readonly Games _games;
        private readonly IHubContext<GameHub, IGameHubOutboundMessages> _hub;

        public GameService(ILogger<GameService> logger, Games games, IHubContext<GameHub, IGameHubOutboundMessages> hub)
        {
            _logger = logger;
            _games = games;
            _hub = hub;
        }

        public Game CreateGame(CreateGameDTO props)
        {
            _logger.LogInformation("Creating new game for '{word}' of {count} guesses at {interval}s/guess", props.Word, props.MaxGuesses, props.GuessDurationSeconds);
            var game = _games.NewGame(props);
            return game;
        }

        public GameSummaryDTO GetSummary(string publicToken)
        {
            var game = _games.GetGame(publicToken);
            if (game == null) throw new InvalidOperationException();
            lock (game) { return game.Summary(); }
        }

        public void GuessWord(string publicToken, string connectionId, string word)
        {
            var game = _games.GetGame(publicToken);
            if (game == null || !game.IsGameStarted() || game.IsGameEnded())
                throw new InvalidOperationException();

            lock (game) {
                var raw = game.Guesses[^1].Raw;
                if (word?.Length != game.Word.Length)
                {
                    raw.Remove(connectionId);
                }
                else
                {
                    raw[connectionId] = word;
                }
            }
        }

        public async Task RunGame(string adminToken, string publicToken)
        {
            _logger.LogInformation("Running game {publicToken}", publicToken);

            var game = GetUnstartedGame(publicToken);
            if (game.AdminToken != adminToken)
                throw new InvalidOperationException();

            Timer? timer = null;

            try
            {
                RecordGameStartTime(game);

                timer = new Timer(PublishGameState, game, 0, 1000);
                int guessNumber = 0;
                do
                {
                    guessNumber++;
                    var nextGuessEndUTC = StartNewGuess(game);
                    PublishGameState(game);
                    _logger.LogInformation("guess started");
                    await WaitUntilEndOfGuess(nextGuessEndUTC);
                    _logger.LogInformation("guess ending");
                    EndCurrentGuess(game);
                }
                while (guessNumber < game.MaxGuesses && !game.IsWordGuessed());//todo lock

                _logger.LogInformation("Finished game {publicToken}", publicToken);
                PublishGameState(game);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Game did not complete {public Token}", game.PublicToken);
            }
            finally
            {
                timer?.Dispose();
            }
        }

        private void PublishGameState(object? oGame)
        {
            Game game = (Game)oGame!;
            GameSummaryDTO summary;
            lock (game) { summary = game.Summary(); }
            _hub.Clients.Group(game.PublicToken).GameState(summary);
        }

        private Game GetUnstartedGame(string publicToken)
        {
            var game = _games.GetGame(publicToken);

            if (game == null)
                throw new InvalidOperationException();

            return game;
        }

        private static void RecordGameStartTime(Game game)
        {
            lock (game)
            {
                if (game.StartUTC != null || game.Guesses.Count > 0)
                    throw new InvalidOperationException();

                game.StartUTC = DateTimeOffset.UtcNow;
            }
        }

        private DateTimeOffset StartNewGuess(Game game)
        {
            DateTimeOffset nextGuessEndUTC;
            lock (game)
            {
                _logger.LogInformation("Starting guess #{guessNumber} for game {publicToken}", game.Guesses.Count + 1, game.PublicToken);

                var lastGuess = game.Guesses.LastOrDefault();
                var lastGuessEndUTC = lastGuess?.EndTimeUTC ?? game.StartUTC!.Value;
                nextGuessEndUTC = lastGuessEndUTC.AddSeconds(game.GuessDurationSeconds);
                game.Guesses.Add(new Guess(new RawGuesses(), null, lastGuessEndUTC, nextGuessEndUTC));
            }

            return nextGuessEndUTC;
        }

        private async Task WaitUntilEndOfGuess(DateTimeOffset nextGuessEndUTC)
        {
            var delayMS = nextGuessEndUTC - DateTimeOffset.UtcNow;
            if (delayMS > TimeSpan.Zero)
                await Task.Delay(delayMS);
        }

        private void EndCurrentGuess(Game game)
        {
            lock (game)
            {
                Guess currentGuess = game.Guesses[^1];
                string blankWord = new string(' ', game.Word.Length);

                var bestGuessWord = currentGuess.Raw
                    .GroupBy(g => g.Value)
                    .Select(g => new GuessStat(g.Key, g.Count()))
                    .OrderByDescending(g => g.Count)
                    .Select(g => g.Word)
                    .FirstOrDefault()
                    ?? blankWord;

                currentGuess.Chosen = bestGuessWord;

                _logger.LogInformation("Finalising guess #{guessNumber} word '{word}' for game {publicToken}", game.Guesses.Count, bestGuessWord, game.PublicToken);
            }
        }
    }
}