using System;
using System.Collections.Generic;
using System.Text;

// perform programme
public class TryCalculate
{
    public static void Main()
    {
        Console.WriteLine("Type your expression >>");
        string expression = Console.ReadLine() ?? string.Empty;
        var tokenized = Tokenizer.Tokenize(expression);
        var tokens = Convert(tokenized);

        var pratt = new Pratt(tokens);
        var rpnized = pratt.ToRPN();
        // print RPN
        foreach (var token in rpnized)
        {
            if (token.Type == Token.TokenType.Number)
            {
                Console.Write(token.Value);
            }
            else if (token.Type == Token.TokenType.Operator || token.Type == Token.TokenType.Parenthesis)
            {
                Console.Write(token.MathOperator);
            }
        }

        var evaluator = new Evaluator(rpnized);
        var result = evaluator.Calculator();
        Console.WriteLine();
        Console.WriteLine(result);
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
                tokens.Enqueue(Token.Operator(str[0]));
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

}


public static class Precedence
{
    public const int Lowest = 0;
    public const int Sum = 10;
    public const int Product = 20;
    public const int Prefix = 30;
}

public class NumberToken : Tojen
{
    public override Node Parse(Parser parse, Token token)
    {
        return new NumberNode(this.Value);
    }

    public override int Precendece => 0;
}


// set token types and precedences 
public abstract class Token
{
    public abstract Node Parse(Parser parse, Token token);
    public abstract int Precedence { get; }
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

public class Parser
{
    private Queue<Token> tokens;
    private Token currentToken;

    public Parser(Queue<Token> tokens)
    {
        this.tokens = tokens;
        NextToken();
    }

    public void NextToken()
    {
        currentToken = tokens.Count > 0 ? tokens.Dequeue() : null;
    }

    public Node ParseExpression(int rightBindingPiwer = 0)
    {
        Token token = currentToken;
        NextToken();
        if (token == null)
            throw new Exception("End of input cannot be handled");

        Node left = token.Parse(this, left);

        while (rightBindingPiwer < currentToken.Precedence)
        {
            token = currentToken;
            NextToken();
            left = token.Parse(this, left);
        }

        return left;
    }

   
}

public abstract class Node
{
    public abstract int Evaluate();
}

public class NumberNode : Node
{
    private int value;

    publuc NumberNode(int value)
    {
        this.value = value;
    }
    public override int Evaluate() => value;
}

public class BinaryOperationNode : Node
{
    private Node left;
    private Node right;
    private Func<int, int, int> operation;

    public BinaryOperationNode(Node left, Node right, Func<int, int, int> operation)
    {
        this.left = left;
        this.right = right;
        this.operation = operation;
    }

    public override int Evaluate() => operation(left.Evaluate(), right.Evaluate());
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

            if (token.Type == Token.TokenType.Number)
            {
                stack.Push(token);
            }
            else
            {
                int right = stack.Pop().Value;
                int left = stack.Pop().Value;
                int result = token.MathOperator switch
                {
                    '+' => left + right,
                    '-' => left - right,
                    '*' => left * right,
                    '/' => left / right,
                    _ => 0
                };
                stack.Push(Token.Number(result));

            }
        }

        return stack.Pop().Value;
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




