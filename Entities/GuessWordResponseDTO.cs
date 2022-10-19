namespace CooperativeWordGuess.Entities
{
    public record GuessWordResponseDTO(GuessState Status);

    public enum GuessState
    {
        OK,
        UnknownWord,
    }
}