namespace CooperativeWordGuess.Entities
{
    public record GameSummaryDTO(GuessSummary? CurrentGuess, Letter[][] PastGuesses, GameState State, int Length, int MaxGuesses, string? Answer);

    public record GuessSummary(GuessStat[] GuessCandidates, DateTimeOffset StartTimeUTC, DateTimeOffset EndTimeUTC);

    public record GuessStat(string Word, int Count);

    public record Letter(char Character, LetterState State);

    public enum LetterState
    {
        Initial,
        Correct,
        Present,
        Absent,
    }

    public enum GameState
    {
        Ready,
        Playing,
        Win,
        Loss,
    }

    public static class GameSummaryExtension
    {
        public static GameSummaryDTO Summary(this Game game)
        {
            // There is no 'current guess' once the game is over.
            var (pastGuesses, currentGuess) = game.IsGameEnded()
                ? (game.Guesses, null)
                : (game.Guesses.AllExceptLast(), game.Guesses.LastOrDefault());

            var guesses = pastGuesses.Select(g => AnalyseGuess(g.Chosen!, game.Word)).ToArray();

            var currentSummary = currentGuess != null
                ? new GuessSummary(GuessHistogram(currentGuess.Raw, 10), currentGuess.StartTimeUTC, currentGuess.EndTimeUTC)
                : null;

            var state = game.IsWordGuessed() ? GameState.Win
                : game.IsGameEnded() ? GameState.Loss
                : game.IsGameStarted() ? GameState.Playing
                : GameState.Ready;

            var answer = game.IsGameEnded() ? game.Word : null;

            return new(currentSummary, guesses, state, game.Word.Length, game.MaxGuesses, answer);
        }

        public static IEnumerable<T> AllExceptLast<T>(this ICollection<T> list)
        {
            var count = Math.Max(list.Count - 1, 0);
            return list.Take(list.Count - 1);
        }

        private static Letter[] AnalyseGuess(string guess, string word)
        {
            var wordUpper = word.ToUpperInvariant();
            var guessUpper = guess.ToUpperInvariant();
            return guessUpper.Select((c, i) => AnalyseLetter(i, c, wordUpper)).ToArray();
        }

        private static Letter AnalyseLetter(int index, char charUpper, string wordUpper)
        {
            //todo make this smarter to account for duplicate letters,
            //eg guessing FILL when the answer is SALT, the first L will show green but the second L should show grey indicating no double L in the answer.
            if (wordUpper[index] == charUpper)
                return new(charUpper, LetterState.Correct);
            else if (wordUpper.Contains(charUpper))
                return new(charUpper, LetterState.Present);
            else
                return new(charUpper, LetterState.Absent);
        }

        private static GuessStat[] GuessHistogram(RawGuesses raw, int top)
        {
            return raw.GroupBy(g => g.Value)
                .Select(g => new GuessStat(g.Key, g.Count()))
                .OrderByDescending(g => g.Count)
                .Take(top)
                .ToArray();
        }
    }
}
