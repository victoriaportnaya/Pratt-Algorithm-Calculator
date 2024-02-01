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

        Queue<string> tokenized = Tokenizer.Tokenize(expression);
        Queue<Token> tokens = Convert(tokenized);

        var parser = new Parser(tokens);
        var ast = parser.ParseExpression();
        
        var result = ast.Evaluate();
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
                int precedence = str == "+" || str == "-" ? Precedence.Sum : Precedence.Product;
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

}

// set token 
public abstract class Token
{
   public virtual int Precedence => 0;
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
   public char Operator { get; }
   public override int Precedence { get; }

   public OperatorToken(char op, int precedence)
    {
        Operator = op;
        Precedence = precedence;
    }
}

public class ParenthesisToken : Token
{
    public char Parenthesis { get; }
    public ParenthesisToken(char parenthesis)
    {
        Parenthesis = parenthesis;
    }
}


public class Parser
{
    private Queue<Token> tokens;
    private Token currentToken;

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
        Token token = currentToken;
        Advance();
        Node left = ParsePrimary(token);

        while (currentToken != null && precedence < currentToken.Precedence)
        {
            token = currentToken;
            Advance();
            left = ParseBinary(left, token);
        }

    }

    Node ParsePrimary(Token token)
    {
        if (token is NumberToken numbertoken)
        {
            return new NumberNode(NumberToken.Value);
        }

        else if (token is ParenthesisToken parenthesisToken && parenthesisToken.Parenthesis == '(')
        {
            Node node = ParseExpression();
            if (currentToken is ParenthesisToken && currentToken.Parenthesis == ")")
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
            Node right = ParseExpression(opToken.Precedence);
            return new BinaryOperationNode(left, right, opToken.Operator);
        }
        throw new Exception("Invalid operator!");
    }

}




public abstract class Node
{
    public abstract int Evaluate();
}

public class NumberNode : Node
{
    private int value;

    public NumberNode(int value)
    {
        this.value = value;
    }

    public override int Evaluate() => value;
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
            '/' => left.Evaluate() / right.Evaluate(),
            _ => throw new InvalidOperationException("Unsupported operator!")
        }; 
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




