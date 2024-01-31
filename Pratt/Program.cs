using System;
using System.Collections.Generic;

// set token types and precedences 
public class Token
{
    public enum TokenType
    {
        Number, 
        Operator, 
        Parenthesis
    }

    public TokenType Type { get; }
    public int Value { get; }
    public char Operator { get; }
    public static Dictionary<char, int> Precedence = new Dictionary<char, int>
    {
        {'+', 1},
        {"-", 1},
        {"*", 2},
        {"/", 2},
        {")", 0},
        {"(", 0},
    };

    private Token(TokenType type, int value, char op)
    {
        Type = type; Value = value; Operator = op;
    }

    public static Token Number(int value) => new Token(TokenType.Number, value);
    public static Token Operator(char op) => new Token(TokenType.Operator, 0, op);
    public static Token Parenthesis(char op) => new Token(TokenType.Parenthesis, 0, op);

    public int Precedence => Type == TokenType.Operator ? Precedence.TryGetValue(Operator, out int precedence) ? precedence : 0 : 0;


}

// nodes for graph 
public class Node
{
    public Token Token;
    public Node Left;
    public Node Right;

    public Node(Token token)
    {
        Token = token;
    }
}

// observe syntax tree and parse 

// evaluator 