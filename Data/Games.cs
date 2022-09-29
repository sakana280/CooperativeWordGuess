using CooperativeWordGuess.Entities;

namespace CooperativeWordGuess.Data
{
    /// <summary>
    /// Threadsafe access to the repository of all games.
    /// </summary>
    public class Games
    {
        // Persist all games in memory for now.
        // This is not resiliant to any server reboots - all games will be lost.
        private readonly Dictionary<string, Game> _games = new();

        public Game NewGame(CreateGameDTO props)
        {
            var game = new Game(props);
            lock (_games)
            {
                _games.Add(game.PublicToken, game);
            }
            return game;
        }

        public Game? GetGame(string publicToken)
        {
            lock (_games)
            {
                return _games.TryGetValue(publicToken, out var game) ? game : null;
            }

        }
    }
}
