using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DuckGame;

[Serializable]
internal class ModCircularDependencyException : Exception
{
    public ModCircularDependencyException()
    {
    }

    public ModCircularDependencyException(string message)
        : base(message)
    {
    }

    public ModCircularDependencyException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected ModCircularDependencyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    private static string CompileStack(Stack<string> stack)
    {
        string msg = "A circular dependency was detected in the list. Mod load order:\r\n";
        foreach (string item in stack)
        {
            msg = msg + " " + item + "\r\n";
        }
        return msg;
    }

    public ModCircularDependencyException(Stack<string> stack)
        : base(CompileStack(stack))
    {
    }
}
