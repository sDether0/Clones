using System;
using System.Collections.Generic;
using System.Linq;

namespace Clones;

public class CloneVersionSystem : ICloneVersionSystem
{
    private readonly Dictionary<string, Clonne> _clonnes = new() { { "1", new Clonne() } };
    public string Execute(string query)
    {
        string[] commands = query.Split(' ');
        switch (commands[0])
        {
            case "learn":
                _clonnes[commands[1]].AddItem(commands[2]);
                break;
            case "rollback":
                _clonnes[commands[1]].Undo();
                break;
            case "relearn":
                _clonnes[commands[1]].Redo();
                break;
            case "clone":
                _clonnes.Add((_clonnes.Count + 1).ToString(), (Clonne)_clonnes[commands[1]].Clone());
                break;
            case "check":
                return _clonnes[commands[1]].Check();
        }
        return null;
    }
}

public class Clonne : ICloneable
{
    public HashSet<string> Programms { get; private set; }
    private RsStack<ActionForUndo> _actionsHistory;
    private RsStack<ActionForUndo> _undoHistory;
    private Clonne _clonnedFrom;
    private List<Clonne> _clonnes = new();
    public Clonne()
    {
        Programms = new();
        _undoHistory = new();
        _actionsHistory = new();
    }

    private Clonne(Clonne clon)
    {
        _clonnedFrom = clon;
    }
    public enum ActionType
    {
        Add,
        Remove
    }

    public class ActionForUndo
    {
        public ActionType ActionType;
        public string? ChangedItem;
    }

    public string Check()
    {
        GetReady();
        return Programms.LastOrDefault() ?? "basic";
    }

    public void AddItem(string item)
    {
        ClonesReady();
        GetReady();
        var ch = Programms.Add(item);
        if (ch)
        {
            _actionsHistory.Push(new ActionForUndo() { ActionType = ActionType.Add, ChangedItem = item });
            _undoHistory = new();
        }
    }

    public void RemoveItem()
    {
        ClonesReady();
        GetReady();
        _actionsHistory.Push(new ActionForUndo() { ActionType = ActionType.Remove, ChangedItem = Programms.Last() });
        Programms.Remove(Programms.Last());
    }


    public void Undo()
    {
        ClonesReady();
        GetReady();


        var last = _actionsHistory.Pop();
        switch (last.ActionType)
        {
            case ActionType.Add:
                Programms.Remove(Programms.Last()); break;
            case ActionType.Remove:
                Programms.Add(last.ChangedItem);
                break;
        }
        _undoHistory.Push(last);

    }

    public void Redo()
    {
        ClonesReady();
        GetReady();

        var last = _undoHistory.Pop();
        switch (last.ActionType)
        {
            case ActionType.Remove:
                Programms.Remove(Programms.Last());
                break;
            case ActionType.Add:
                Programms.Add(last.ChangedItem);
                break;
        }
        _actionsHistory.Push(last);

    }

    public void ClonesReady()
    {
        _clonnes.ForEach(x => x.GetReady());
    }

    public void GetReady()
    {
        if (_actionsHistory is null || _undoHistory is null || Programms is null )
        {
            _clonnedFrom.GetReady();
            _undoHistory = new(_clonnedFrom._undoHistory.Peek());
            _actionsHistory = new(_clonnedFrom._actionsHistory.Peek());
            Programms = _clonnedFrom.Programms.ToHashSet();
            //_clonnedFrom = null;
        }
    }

    public object Clone()
    {
        var clone = new Clonne(this);
        _clonnes.Add(clone);
        return clone;
    }
}

public class LinkedListNode<T>
{
    public T data;
    public LinkedListNode<T> next;
    public LinkedListNode(T data)
    {
        this.data = data;
        this.next = null;
    }
}

public class RsStack<T>
{
    LinkedListNode<T> _top;

    public RsStack()
    {
        _top = null;
    }

    public RsStack(LinkedListNode<T> top)
    {
        _top = top;
    }
    public LinkedListNode<T> GetNode(T data)
    {
        LinkedListNode<T> node = new LinkedListNode<T>(data);
        return node;
    }

    public void Push(T data)
    {
        LinkedListNode<T> newNode = GetNode(data);
        if (_top == null)
        {
            _top = newNode;
            return;
        }
        newNode.next = _top;
        _top = newNode;

    }
    public LinkedListNode<T> Peek()
    {
        return _top;
    }

    public T Pop()
    {
        T peek = default;
        if (_top != null)
        {
            peek = _top.data;
            _top = _top.next;
        }
        return peek;
    }
}