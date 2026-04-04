using WorldBattleNaval.Entities;
using WorldBattleNaval.Enums;

namespace WorldBattleNaval;

public class GameState
{
    public Player Player { get; } = new();
    public Cpu Cpu { get; } = new();

    public GamePhase Phase { get; private set; } = GamePhase.Placement;
    public Turn CurrentTurn { get; private set; } = Turn.Player;

    public bool IsGameOver => Phase == GamePhase.GameOver;
    public bool PlayerWon => Cpu.IsDefeated;
    public bool CpuWon => Player.IsDefeated;

    public void StartBattle()
    {
        Phase = GamePhase.Battle;
        CurrentTurn = Turn.Player;
    }

    public void SwitchTurn()
    {
        CurrentTurn = CurrentTurn == Turn.Player ? Turn.Cpu : Turn.Player;
    }

    public void CheckGameOver()
    {
        if (Player.IsDefeated || Cpu.IsDefeated)
            Phase = GamePhase.GameOver;
    }
}