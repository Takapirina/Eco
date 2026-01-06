using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MenuState { Root, Moves, Targets }
public enum RootMenu { Attack, Defend, Items, Run }

public class MenuController : MonoBehaviour
{
    public MenuState state { get; private set; } = MenuState.Root;
    public int index { get; private set; } = 0;

    private BattleUnit currentPk;
    private List<Move_base> moves = new();

    [SerializeField] private MenuView view;

    public bool HasDecision { get; private set; } = false;
    public BattleAction Decision { get; private set; }
    private bool blockConfirm = false;

    private List<BattleUnit> enemies = new();
    private List<BattleUnit> allies = new();

    private Move_base pendingMove;
    List<BattleUnit> candidates = new();
    int targetIndex = 0;
    private int moveIndex = 0;

    public void Open(BattleUnit pk, List<BattleUnit> enemies, List<BattleUnit> allies)
    {
        blockConfirm = false;
        currentPk = pk;

        this.enemies = enemies ?? new List<BattleUnit>();
        this.allies  = allies  ?? new List<BattleUnit>();

        moves = (pk.Instance.Moves != null) ? pk.Instance.Moves.ToList() : new List<Move_base>();

        state = MenuState.Root;
        index = 0;

        HasDecision = false;
        Decision = null;

        pendingMove = null;
        candidates.Clear();
        targetIndex = 0;

        Refresh();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;
        if (HasDecision) return;

        if (blockConfirm)
        {
            if (Input.GetKey(KeyCode.Space)) return;
            blockConfirm = false;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) Move(-1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Move(+1);

        if (Input.GetKeyDown(KeyCode.LeftArrow)) Enter();
        if (Input.GetKeyDown(KeyCode.RightArrow)) Back();

        if (Input.GetKeyDown(KeyCode.Space)) Confirm();
        if (Input.GetKeyDown(KeyCode.Escape)) Back();
    }

    void Move(int delta)
    {
        if (state == MenuState.Root)
        {
            int count = 4;
            index = (index + delta + count) % count;
            Refresh();
            return;
        }

        if (state == MenuState.Moves)
        {
            int count = moves.Count;
            if (count <= 0) return;

            moveIndex = (moveIndex + delta + count) % count;
            Refresh();
            return;
        }

        if (state == MenuState.Targets)
        {
            int count = candidates.Count;
            if (count <= 0) return;

            targetIndex = (targetIndex + delta + count) % count;
            UpdateTargetIndicator();
            return;
        }
    }

    void Enter()
    {
        if (state == MenuState.Root && (RootMenu)index == RootMenu.Attack)
            EnterMoves();
    }

    void EnterMoves()
    {
        state = MenuState.Moves;
        moveIndex = 0;
        Refresh();
    }

    void Confirm()
    {
        if (state == MenuState.Root)
        {
            var choice = (RootMenu)index;

            if (choice == RootMenu.Attack)
            {
                EnterMoves();
                return;
            }

            if (choice == RootMenu.Defend)
            {
                Decide(new DefendAction(currentPk));
                return;
            }

            if (choice == RootMenu.Items)
            {
                Decide(new ItemsAction());
                return;
            }

            if (choice == RootMenu.Run)
            {
                Decide(new RunAction());
                return;
            }
        }
        else if (state == MenuState.Moves)
        {
            if (moves.Count == 0)
            {
                Back();
                return;
            }

            pendingMove = moves[moveIndex];

            if (pendingMove.targetsType == MoveTargets.SelfT)
            {
                Decide(new AttackAction(currentPk, pendingMove, new List<BattleUnit> { currentPk }));
                return;
            }

            if (pendingMove.targetsType == MoveTargets.AOE)
            {
                var tgs = AliveOnly(enemies);
                Decide(new AttackAction(currentPk, pendingMove, tgs));
                return;
            }

            if (pendingMove.targetsType == MoveTargets.AOEPartyT)
            {
                var tgs = AliveOnly(allies);
                Decide(new AttackAction(currentPk, pendingMove, tgs));
                return;
            }

            if (pendingMove.targetsType == MoveTargets.SingleT)
            {
                EnterTargets(AliveOnly(enemies));
                return;
            }

            if (pendingMove.targetsType == MoveTargets.SinglePartyT)
            {
                EnterTargets(AliveOnly(allies));
                return;
            }

            Decide(new AttackAction(currentPk, pendingMove, new List<BattleUnit>()));
        }

        else if (state == MenuState.Targets)
        {
            if (candidates.Count == 0)
            {
                state = MenuState.Moves;
                index = 0;
                Refresh();
                return;
            }

            var chosen = candidates[targetIndex];
            UpdateTargetIndicator();

            Decide(new AttackAction(currentPk, pendingMove, new List<BattleUnit> { chosen }));

            for (int i = 0; i < candidates.Count; i++)
                candidates[i].View.SetTargeted(false);
        }
    }

    void Back()
    {
        if (state == MenuState.Targets)
        {
            for (int i = 0; i < candidates.Count; i++)
                candidates[i].View.SetTargeted(false);

            state = MenuState.Moves;
            moveIndex = 0;
            Refresh();
            return;
        }

        if (state == MenuState.Moves)
        {
            state = MenuState.Root;
            index = 0;
            Refresh();
        }
    }

    void Decide(BattleAction action)
    {
        Decision = action;
        HasDecision = true;

        Close();
    }

    void Refresh()
    {
        if (view == null) return;

        if (state == MenuState.Root)
        {
            view.ShowRoot(index);
            return;
        }

        var names = moves.Select(m => m.move_name).ToList();
        view.ShowMoves(names, moveIndex);
    }


    void EnterTargets(List<BattleUnit> list)
    {
        state = MenuState.Targets;

        candidates = list ?? new List<BattleUnit>();

        targetIndex = 0;
        UpdateTargetIndicator();

        Refresh();
    }

    void UpdateTargetIndicator()
    {
        for (int i = 0; i < candidates.Count; i++)
            candidates[i].View.SetTargeted(false);

        if (candidates.Count > 0)
            candidates[targetIndex].View.SetTargeted(true);
    }

    static List<BattleUnit> AliveOnly(List<BattleUnit> units)
    {
        var result = new List<BattleUnit>();
        if (units == null) return result;

        foreach (var u in units)
        {
            if (u != null && u.Instance != null && u.Instance.CurrentHP > 0)
                result.Add(u);
        }
        return result;
    }
}