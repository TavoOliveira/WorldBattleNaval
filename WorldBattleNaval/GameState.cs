using WorldBattleNaval.Entities;
using WorldBattleNaval.Enums;

namespace WorldBattleNaval;

public class GameState
{
    public Player Player { get; } = new();
    public Cpu Cpu { get; } = new();

    public EGamePhase Phase { get; private set; } = EGamePhase.PLACEMENT;
    public ETurn CurrentTurn { get; private set; } = ETurn.PLAYER;

    public bool IsGameOver => Phase == EGamePhase.GAME_OVER;
    public bool PlayerWon => Cpu.IsDefeated;
    public bool CpuWon => Player.IsDefeated;

    public void StartBattle()
    {
        Phase = EGamePhase.BATTLE;
        CurrentTurn = ETurn.PLAYER;
    }

    public void SwitchTurn()
    {
        CurrentTurn = CurrentTurn == ETurn.PLAYER ? ETurn.CPU : ETurn.PLAYER;
    }

    public void CheckGameOver()
    {
        if (Player.IsDefeated || Cpu.IsDefeated)
            Phase = EGamePhase.GAME_OVER;
    }
}