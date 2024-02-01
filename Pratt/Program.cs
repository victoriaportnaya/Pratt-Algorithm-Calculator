using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

// perform programme
public class TryCalculate
{
    public static void Main()
    {
        Console.WriteLine("Type your expression >>");
        string expression = Console.ReadLine() ?? string.Empty;

        Queue<string> tokenized = Tokenizer.Tokenize(expression);
        Queue<Token> tokens = Convert(tokenized);

        var parser = new Parser(tokens);
        var ast = parser.ParseExpression();

        var result = ast.Evaluate();
        Console.WriteLine();
        Console.WriteLine($"The result: {result}");
        Console.WriteLine("Abstract Syntax Tree");
        ast.PrintTree("", true);
    }

    static Queue<Token> Convert(Queue<string> tokenized)
    {
        var tokens = new Queue<Token>();
        while (tokenized.Count > 0)
        {
            var str = tokenized.Dequeue();
            if (int.TryParse(str, out int number))
            {
                tokens.Enqueue(Token.Number(number));
            }
            else if (IsOperator(str))
            {
                int precedence = GetOperatorPrecedence(str);
                tokens.Enqueue(Token.Operator(str[0], precedence));
            }
            else if (str == "(" || str == ")")
            {
                tokens.Enqueue(Token.Parenthesis(str[0]));
            }
        }
        return tokens;
    }

    static bool IsOperator(string str)
    {
        return str == "+" || str == "-" || str == "*" || str == "/";
    }

    static int GetOperatorPrecedence(string op)
    {
        return op switch
        {
            "+" => Precedence.Sum,
            "-" => Precedence.Sum,
            "*" => Precedence.Product,
            "/" => Precedence.Product,
            _ => 0,
        };
    }

}

// set token 
public abstract class Token
{
    public virtual int Precedence { get; private set; }

    protected Token(int precedence = 0)
    {
        Precedence = precedence;
    }

    public static Token Number(int value) => new NumberToken(value);
    public static Token Operator(char op, int precedence) => new OperatorToken(op, precedence);
    public static Token Parenthesis(char parenthesis) => new ParenthesisToken(parenthesis);

}

public class NumberToken : Token
{
    public int Value { get; private set; }

    public NumberToken(int value)
    {
        Value = value;
    }

}


public class OperatorToken : Token
{
    public char Operator { get; private set; }


    public OperatorToken(char op, int precedence) : base(precedence)
    {
        Operator = op;
    }
}

public class ParenthesisToken : Token
{
    public char Parenthesis { get; private set; }
    public ParenthesisToken(char parenthesis)
    {
        Parenthesis = parenthesis;
    }
}


public class Parser
{
    private Queue<Token> tokens;
    private Token? currentToken;

    public Parser(Queue<Token> tokens)
    {
        this.tokens = tokens;
        Advance();
    }

    public void Advance()
    {
        currentToken = tokens.Count > 0 ? tokens.Dequeue() : null;
    }

    public Node ParseExpression(int precedence = 0)
    {
        if (currentToken == null)
            throw new EvaluateException("Unexpected end of the expression!");

        Node left = ParsePrimary(currentToken);
        Advance();

        while (currentToken != null && precedence < currentToken.Precedence)
        {
            Token token = currentToken;
            Advance();
            left = ParseBinary(left, token);
        }

        return left;
    }

    Node ParsePrimary(Token token)
    {

        if (token is NumberToken numberToken)
        {
            return new NumberNode(numberToken.Value);
        }

        else if (token is ParenthesisToken parenthesisToken && parenthesisToken.Parenthesis == '(')
        {
            Advance();
            Node node = ParseExpression();
            if (currentToken is ParenthesisToken pt && pt.Parenthesis == ')')
            {
                Advance();
                return node;
            }
            else
            {
                throw new Exception("Not closed paretheses!");
            }
        }
        throw new Exception("Unexpected token!");
    }

    Node ParseBinary(Node left, Token token)
    {
        if (token is OperatorToken opToken)
        {
            int precedent = opToken.Precedence;
            Node right = ParseExpression(opToken.Precedence);
            return new BinaryOperationNode(left, right, opToken.Operator);
        }
        throw new Exception("Invalid operator!");
    }

}




public abstract class Node
{
    public abstract int Evaluate();
    public abstract void PrintTree(string indent, bool last);
}

public class NumberNode : Node
{
    private int value;

    public NumberNode(int value)
    {
        this.value = value;
    }

    public override int Evaluate() => value;

    public override void PrintTree(string indent, bool last)
    {
        Console.WriteLine(indent + (last ? "└─" : "├─") + value);
    }
}

public class BinaryOperationNode : Node
{
    private Node left;
    private Node right;
    private char op;

    public BinaryOperationNode(Node left, Node right, char op)
    {
        this.left = left;
        this.right = right;
        this.op = op;
    }

    public override int Evaluate()
    {
        return op switch
        {
            '+' => left.Evaluate() + right.Evaluate(),
            '-' => left.Evaluate() - right.Evaluate(),
            '*' => left.Evaluate() * right.Evaluate(),
            '/' => Division(left.Evaluate(), right.Evaluate()),
            _ => throw new InvalidOperationException("Unsupported operator!")
        };
    }

    private static int Division(int left, int right)
    {
        if (right == 0)
        {
            throw new Exception("Cannot divide by zero!");
        }

        return left / right;
    }

    public override void PrintTree(string indent, bool last)
    {
        Console.WriteLine(indent + (last ? "└─" : "├─") + op);
        left.PrintTree(indent + (last ? "  " : "|"), false);
        right.PrintTree(indent + (last ? "  " : "|"), true);
    }
}





// tokenizer
public class Tokenizer
{
    public static HashSet<char> Operators = new HashSet<char> { '+', '-', '*', '/', ')', '(' };

    public static Queue<string> Tokenize(string expression)
    {
        Queue<string> tokens = new Queue<string>();
        StringBuilder currentToken = new StringBuilder();

        foreach (char c in expression)
        {
            if (Operators.Contains(c))
            {
                if (currentToken.Length > 0)
                {
                    tokens.Enqueue(currentToken.ToString());
                    currentToken.Clear();
                }
                tokens.Enqueue(c.ToString());
            }

            else if (!char.IsWhiteSpace(c))
            {
                currentToken.Append(c);
            }
        }
        if (currentToken.Length > 0)
        {
            tokens.Enqueue(currentToken.ToString());
        }

        return tokens;

    }
}

public static class Precedence
{
    public const int Sum = 1;
    public const int Product = 2;
}




