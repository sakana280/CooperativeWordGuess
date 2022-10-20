using CooperativeWordGuess.Util;

namespace CooperativeWordGuess.Entities
{
    public class Game
    {
        public Game(CreateGameDTO props)
        {
            if (props.Word == null)
                throw new ArgumentNullException(nameof(props));

            AdminToken = Id.Generate();
            PublicToken = Id.Generate();
            Word = props.Word.ToUpper();
            MaxGuesses = props.MaxGuesses;
            GuessDurationSeconds = props.GuessDurationSeconds;
        }

        public string AdminToken { get; init; }
        public string PublicToken { get; init; }
        public string Word { get; init; }
        public int MaxGuesses { get; init; }
        public int GuessDurationSeconds { get; init; }

        public DateTimeOffset? StartUTC { get; set; }
        //public List<string> UserTokens { get; set; } = new();
        public List<Guess> Guesses { get; set; } = new();

        public bool IsWordGuessed() => Word == Guesses.LastOrDefault()?.Chosen?.ToUpper();
        public bool IsGameStarted() => StartUTC != null;
        public bool IsGameEnded()
        {
            var maxGuessesReached = Guesses.Count == MaxGuesses;
            var lastGuessComplete = Guesses.LastOrDefault()?.Chosen != null;
            return IsWordGuessed() || (maxGuessesReached && lastGuessComplete);
        }
    }

    public record Guess(RawGuesses Raw, string? Chosen, DateTimeOffset StartTimeUTC, DateTimeOffset EndTimeUTC)
    {
        public string? Chosen { get; set; } = Chosen; // Make "Chosen" mutable
    }

    /// <summary>
    /// Map of connectionId-to-guessedWord
    /// </summary>
    public class RawGuesses : Dictionary<string, string> { }
}
