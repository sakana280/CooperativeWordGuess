namespace CooperativeWordGuess.Entities
{
    public record CreateGameDTO(string? Word, int? Length, int MaxGuesses, int GuessDurationSeconds)
    {
        // Allow Word to be mutated eg when a random word needs to be set.
        public string? Word { get; set; } = Word;
    }
}
