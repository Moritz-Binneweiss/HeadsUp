using System;

[Serializable]
public class Player
{
    public string name;
    public int score;

    public Player()
    {
        name = "Spieler";
        score = 0;
    }

    public Player(string playerName)
    {
        name = playerName;
        score = 0;
    }
}
