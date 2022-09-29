namespace CooperativeWordGuess.Entities
{
    public record CreateGameDTO(string Word, int MaxGuesses, int GuessDurationSeconds);
}
