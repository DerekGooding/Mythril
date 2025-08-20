using Microsoft.Xna.Framework;
using Mythril.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mythril.GameLogic.Combat;

public enum CombatState
{
    PlayerTurn,
    EnemyTurn,
    CombatOver
}

public class CombatManager
{
    public CombatState State { get; private set; }
    private readonly PartyManager _partyManager;
    public IReadOnlyList<Character> PlayerParty => _playerParty;
    public IReadOnlyList<Character> EnemyParty => _enemyParty;
    private readonly List<Character> _playerParty = new List<Character>();
    private readonly List<Character> _enemyParty = new List<Character>();
    private readonly List<Character> _turnOrder = new List<Character>();
    private int _turnIndex = 0;
    public Character CurrentCombatant => _turnOrder[_turnIndex];

    public CombatManager(PartyManager partyManager)
    {
        _partyManager = partyManager;
    }

    public void StartCombat(List<Character> enemies)
    {
        _playerParty.Clear();
        _playerParty.AddRange(_partyManager.PartyMembers);

        _enemyParty.Clear();
        _enemyParty.AddRange(enemies);

        _turnOrder.Clear();
        _turnOrder.AddRange(_playerParty);
        _turnOrder.AddRange(_enemyParty);
        _turnIndex = 0;
        State = CombatState.PlayerTurn;
    }

    public void Update(GameTime gameTime)
    {
        if (State == CombatState.CombatOver) return;

        if (IsCombatOver())
        {
            State = CombatState.CombatOver;
            return;
        }

        if (State == CombatState.EnemyTurn)
        {
            var enemy = CurrentCombatant;
            if (_playerParty.Count > 0)
            {
                var target = _playerParty[new Random().Next(_playerParty.Count)];
                PerformAttack(enemy, target);
            }
            TakeTurn();
            State = CombatState.PlayerTurn;
        }
    }

    private void TakeTurn()
    {
        if (IsCombatOver()) return;

        var currentCombatant = _turnOrder[_turnIndex];
        _turnIndex = (_turnIndex + 1) % _turnOrder.Count;
    }

    private bool IsCombatOver()
    {
        if (_playerParty.Count == 0)
        {
            return true;
        }

        if (_enemyParty.Count == 0)
        {
            return true;
        }

        return false;
    }

    private void PerformAttack(Character attacker, Character target)
    {
        // target.TakeDamage(attacker.AttackPower);
    }

    public void PlayerTurn_Attack(Character target)
    {
        PerformAttack(CurrentCombatant, target);
        TakeTurn();
        State = CombatState.EnemyTurn;
    }

    public void PlayerTurn_Defend()
    {
        TakeTurn();
        State = CombatState.EnemyTurn;
    }
}
