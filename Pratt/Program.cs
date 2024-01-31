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
    public char MathOperator { get; }
    public static Dictionary<char, int> MathPrecedence = new Dictionary<char, int>
    {
        {'+', 1},
        {'-', 1},
        {'*', 2},
        {'/', 2},
        {')', 0},
        {'(', 0},
    };

    private Token(TokenType type, int value = 0, char op = '\0')
    {
        Type = type;
        Value = value;
        MathOperator = op;
    }

    public static Token Number(int value) => new Token(TokenType.Number, value, '\0');
    public static Token Operator(char op) => new Token(TokenType.Operator, 0, op);
    public static Token Parenthesis(char op) => new Token(TokenType.Parenthesis, 0, op);

    public int Precedence => Type == TokenType.Operator ? MathPrecedence.TryGetValue(MathOperator, out int precedence) ? precedence : 0 : 0;


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

    public Pratt(Queue<Token> tokens)
    {
        this.tokens = tokens;
    }

    public Queue<Token> ToRPN()
    {
        var outputQueue = new Queue<Token>();
        var operatorStack = new Stack<Token>();

        while (tokens.Count > 0)
        {
            var token = tokens.Dequeue();

            switch (token.Type)
            {
                case Token.TokenType.Number:
                    outputQueue.Enqueue(token);
                    break;
                case Token.TokenType.Operator:
                    while (operatorStack.Count > 0 && operatorStack.Peek().Type != Token.TokenType.Parenthesis && operatorStack.Peek().Precedence >= token.Precedence)
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Push(token);
                    break;
                case Token.TokenType.Parenthesis:
                    if (token.MathOperator == '(')
                    {
                        operatorStack.Push(token);
                    }
                    else
                    {
                        while (operatorStack.Peek().MathOperator != '(')
                        {
                            outputQueue.Enqueue(operatorStack.Pop());
                        }
                        operatorStack.Pop();
                    }
                    break;
            }

        }

        while (operatorStack.Count > 0)
        {
            outputQueue.Enqueue(operatorStack.Pop());
        }

        return outputQueue;
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




