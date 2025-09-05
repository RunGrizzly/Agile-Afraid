public interface ILetterDestination
{
    public bool IsValid();
    public void SendLetter(Letter letter);
}
