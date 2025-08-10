using System;

public enum LetterType { Vowel, Consonant }

[Serializable]
public class Letter
{
    public char character = '#';
    public int score;

    public LetterType type;

    //New letter from scratch
    public Letter(char _character, int _score)
    {
        character = _character;
        score = _score;
    }

    //Clone a letter
    public Letter(Letter _letter)
    {
        character = _letter.character;
        score = _letter.score;
    }

}