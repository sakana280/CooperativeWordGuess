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
            return list.Take(list.Count - 1);
        }

        private static Letter[] AnalyseGuess(string guess, string word)
        {
            // The answer word has letters replaced with space as they are matched,
            // to handle letter colouring in the presence of duplicate letters,
            // eg guessing FILL when the answer is SALT, the first L will show green
            // but the second L should show grey indicating no double L in the answer.
            var wordUpper = word.ToUpperInvariant();
            var guessUpper = guess.ToUpperInvariant();

            // Match correct/green characters first,
            // so that guess=ELLE word=FILL doesn't colour both Ls yellow.
            // All non-matches are marked Absent and will be analysed for Present status after.
            var analysis = wordUpper
                .Zip(guess, (w, g) => new Letter(g, w == g ? LetterState.Correct : LetterState.Absent))
                .ToArray();

            // wordUpper with any Correct matches replaced with a space character.
            var remainingUpper = analysis.Zip(wordUpper, (a, c) => a.State == LetterState.Correct ? ' ' : c).ToList();

            // Analyse the remaining characters as Present or leave as Absent.
            for (var i = 0; i < wordUpper.Length; i++)
            {
                var a = analysis[i];
                if (a.State != LetterState.Correct)
                {
                    var j = remainingUpper.IndexOf(a.Character);
                    if (j >= 0)
                    {
                        analysis[i] = new Letter(a.Character, LetterState.Present);
                        remainingUpper[j] = ' '; // replace matched letters with a space character
                    }
                }
            }

            return analysis;
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
