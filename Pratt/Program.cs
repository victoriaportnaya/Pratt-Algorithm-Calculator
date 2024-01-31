using System;
using System.Collections.Generic;

// perform programme
public class TryCalculate
{
    public static void Main()
    {
        var tokens = new Queue<Token>();
        var evaluator = new Evaluator(tokens);
        var result = evaluator.Calculator();

        Console.WriteLine(result);
    }
}


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
        {'-', 1},
        {'*', 2},
        {'/', 2},
        {')', 0},
        {'(', 0},
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

// observe syntax tree and rpnize
public class Pratt
{
    private Queue<Token> tokens;
    
    public Rpnizer(Queue<Token> tokens)
    {
        this.tokens = tokens;
    }

    public Node Parse()
    {
        return ParseExpression(0);
    }

    private Node ParseExpression(int precedence)
    {
        Token token = tokens.Dequeue();

        Node left = null;
        if (token.Type == Token.TokenType.Number)
        {
            left = new Node(token);
        }
        
        else if (token.Type == Token.TokenType.Parenthesis && token.Operator == '(')
        {
            left = ParseExpression(0);
            Expect(')');
        }

        while (tokens.Count > 0 && precedence < tokens.Peek().Precedence)
        {
            token = tokens.Dequeue();

            if (token.Type == Token.TokenType.Operator)
            {
                var right = ParseExpression(token.Precedence);
                var parent = new Node(token) { Left = left, Right = right };
                left = parent;
            }
        }

        return left;
    
    
    private void Expect(char expected)
        {
            if (tokens.Count > 0) || (tokens.Peek().Type != Token.TokenType.Parenthesis || tokens.Peek().Operator != expected))
                    {
                throw new InvalidOperationException(&"Expected '{expected}'");
            }
            tokens.Dequeue();

        }

    }
}

// evaluator 
public class Evaluator
{
    private Queue<Token> tokens;

    public Evaluator(Queue<Token> tokens)
    {
        this.tokens = tokens;
    }

    public int Calculator()
    {
        var stack = new Stack<Token>();

        while (tokens.Count > 0)
        {
            var token = tokens.Dequeue();

            if (token.IsNumber)
            {
                stack.Push(token);
            }
            else
            {
                int right = stack.Pop().Value;
                int left = stack.Pop().Value;
                int result = token.Operator switch
                {
                    '+' => left + right,
                    '-' => left - right,
                    '*' => left * right,
                    '/' => left / right,
                    _ => 0
                };
                stack.Push(new Token(result));
              
            }
        }

        return stack.Pop().Value;
    }
}
