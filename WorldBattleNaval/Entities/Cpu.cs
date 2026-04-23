using System;
using System.Collections.Generic;
using System.Linq;
using WorldBattleNaval.Enums;

namespace WorldBattleNaval.Entities;

public class Cpu
{
    public Board OwnBoard { get; private set; }
    public List<Ship> Ships { get; } = [];
    public bool IsDefeated => Ships.All(s => s.IsSunk);

    public ECpuState State { get; private set; } = ECpuState.SEARCH;

    private readonly bool[,] tried = new bool[Board.Size, Board.Size];
    private readonly List<(int r, int c)> currentHits = [];
    private readonly Queue<(int r, int c)> candidates = new();
    private readonly Random rand;

    public Cpu() : this(Random.Shared) { }

    public Cpu(Random rand)
    {
        this.rand = rand;
    }

    public void Setup(List<Ship> ships)
    {
        Ships.AddRange(ships);
        OwnBoard = Board.CreateRandom(ships);
    }

    public (bool hit, bool sunk) ReceiveAttack(int row, int col)
    {
        var ship = Ships.FirstOrDefault(s => s.Covers(row, col));
        if (ship != null)
        {
            ship.TakeDamage();
            OwnBoard.MarkHit(row, col);
            return (true, ship.IsSunk);
        }

        OwnBoard.MarkMiss(row, col);
        return (false, false);
    }

    public (int row, int col) ChooseShot()
    {
        while (candidates.Count > 0)
        {
            var next = candidates.Dequeue();
            if (!tried[next.r, next.c]) return next;
        }

        State = ECpuState.SEARCH;
        return PickRandomUntried();
    }

    public void ReportResult(int row, int col, bool hit, bool sunk)
    {
        tried[row, col] = true;

        if (sunk)
        {
            currentHits.Clear();
            candidates.Clear();
            State = ECpuState.SEARCH;
            return;
        }

        if (!hit) return;

        currentHits.Add((row, col));

        switch (State)
        {
            case ECpuState.SEARCH:
                State = ECpuState.TARGET;
                EnqueueNeighbors(row, col);
                break;
            case ECpuState.TARGET:
                State = ECpuState.DESTROY;
                RebuildAxisCandidates();
                break;
            case ECpuState.DESTROY:
                RebuildAxisCandidates();
                break;
        }
    }

    private void EnqueueNeighbors(int row, int col)
    {
        int[] dr = [-1, 1, 0, 0];
        int[] dc = [0, 0, -1, 1];
        for (int i = 0; i < 4; i++)
        {
            int nr = row + dr[i];
            int nc = col + dc[i];
            if (nr < 0 || nr >= Board.Size || nc < 0 || nc >= Board.Size) continue;
            if (tried[nr, nc]) continue;
            candidates.Enqueue((nr, nc));
        }
    }

    private void RebuildAxisCandidates()
    {
        candidates.Clear();
        if (currentHits.Count < 2)
        {
            foreach (var (r, c) in currentHits)
                EnqueueNeighbors(r, c);
            return;
        }

        bool horizontal = currentHits[0].r == currentHits[1].r;

        if (horizontal)
        {
            int row = currentHits[0].r;
            int minC = currentHits.Min(h => h.c);
            int maxC = currentHits.Max(h => h.c);
            if (minC - 1 >= 0 && !tried[row, minC - 1]) candidates.Enqueue((row, minC - 1));
            if (maxC + 1 < Board.Size && !tried[row, maxC + 1]) candidates.Enqueue((row, maxC + 1));
        }
        else
        {
            int col = currentHits[0].c;
            int minR = currentHits.Min(h => h.r);
            int maxR = currentHits.Max(h => h.r);
            if (minR - 1 >= 0 && !tried[minR - 1, col]) candidates.Enqueue((minR - 1, col));
            if (maxR + 1 < Board.Size && !tried[maxR + 1, col]) candidates.Enqueue((maxR + 1, col));
        }
    }

    private (int row, int col) PickRandomUntried()
    {
        var parity = new List<(int, int)>();
        var other = new List<(int, int)>();

        for (int r = 0; r < Board.Size; r++)
        for (int c = 0; c < Board.Size; c++)
        {
            if (tried[r, c]) continue;
            if ((r + c) % 2 == 0) parity.Add((r, c));
            else other.Add((r, c));
        }

        var list = parity.Count > 0 ? parity : other;
        return list[rand.Next(list.Count)];
    }
}
