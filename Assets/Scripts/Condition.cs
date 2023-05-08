using System;

public class Condition
{
    private string name;
    private Func<bool> func;

    public Condition(string name, Func<bool> func)
    {
        this.name = name;
        this.func = func;
    }

    public string Name { get => name; }
    public Func<bool> Func { get => func; }
}