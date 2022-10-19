namespace CooperativeWordGuess.Entities
{
    public record CreatedGameDTO(string? AdminToken, string? PublicToken, GuessState Status);
}
