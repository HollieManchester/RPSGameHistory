using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Interface defining the rules for the game
interface IRules
{
    bool IsWinner(string playerChoice, string computerChoice);
    bool IsValidChoice(string choice);
    string[] GetChoices();
}

// Base class for game rules
abstract class BaseRules : IRules
{
    protected string[] choices;

    public abstract bool IsWinner(string playerChoice, string computerChoice);

    public bool IsValidChoice(string choice)
    {
        return choices.Contains(choice);
    }

    public string[] GetChoices()
    {
        return choices;
    }
}

// Default implementation of game rules
class DefaultRules : BaseRules
{
    private readonly Dictionary<string, Dictionary<string, string>> outcomes;

    public DefaultRules()
    {
        choices = new string[] { "rock", "paper", "scissors", "lizard", "spock" };

        outcomes = new Dictionary<string, Dictionary<string, string>>
        {
            { "rock", new Dictionary<string, string> { { "rock", "Draw" }, { "paper", "Computer" }, { "scissors", "Player" }, { "lizard", "Player" }, { "spock", "Computer" } } },
            { "paper", new Dictionary<string, string> { { "rock", "Player" }, { "paper", "Draw" }, { "scissors", "Computer" }, { "lizard", "Computer" }, { "spock", "Player" } } },
            { "scissors", new Dictionary<string, string> { { "rock", "Computer" }, { "paper", "Player" }, { "scissors", "Draw" }, { "lizard", "Player" }, { "spock", "Computer" } } },
            { "lizard", new Dictionary<string, string> { { "rock", "Computer" }, { "paper", "Player" }, { "scissors", "Computer" }, { "lizard", "Draw" }, { "spock", "Player" } } },
            { "spock", new Dictionary<string, string> { { "rock", "Player" }, { "paper", "Computer" }, { "scissors", "Player" }, { "lizard", "Computer" }, { "spock", "Draw" } } }
        };
    }

    public override bool IsWinner(string playerChoice, string computerChoice)
    {
        string result = outcomes[playerChoice][computerChoice];
        Console.WriteLine(result == "Draw" ? "It's a draw!" : $"{result} wins this round!");

        return result == "Player";
    }
}

// Custom implementation of game rules
class CustomRules : BaseRules
{
    private readonly Dictionary<string, Dictionary<string, string>> outcomes;

    public CustomRules()
    {
        choices = new string[] { "a", "b", "c", "d", "e" };

        outcomes = new Dictionary<string, Dictionary<string, string>>
        {
            { "a", new Dictionary<string, string> { /* Add custom rules for 'a' */ } },
            { "b", new Dictionary<string, string> { /* Add custom rules for 'b' */ } },
            { "c", new Dictionary<string, string> { /* Add custom rules for 'c' */ } },
            { "d", new Dictionary<string, string> { /* Add custom rules for 'd' */ } },
            { "e", new Dictionary<string, string> { /* Add custom rules for 'e' */ } }
        };
    }

    public override bool IsWinner(string playerChoice, string computerChoice)
    {
        string result = outcomes[playerChoice][computerChoice];
        Console.WriteLine(result == "Draw" ? "It's a draw!" : $"{result} wins this round!");

        return result == "Player";
    }
}

// Class representing the game history
class GameHistory
{
    private List<string> history;

    public GameHistory()
    {
        history = new List<string>();
    }

    // Add a result to the history
    public void AddResult(string result)
    {
        history.Add(result);
    }

    // Save the game history to a file
    public void SaveToFile(string filePath)
    {
        File.WriteAllLines(filePath, history);
    }

    // Display the game history
    public void DisplayHistory()
    {
        if (history.Count > 0)
        {
            Console.WriteLine("\nGame History:");
            Console.WriteLine(string.Join("\n", history));
            Console.WriteLine();
        }
    }
}

// Class representing the game
class Game
{
    private readonly IRules rules;
    private readonly Random random;
    private int playerScore;
    private int computerScore;
    private readonly int roundsToWin;
    private readonly GameHistory gameHistory;
    private Dictionary<string, int> playerChoiceFrequency = new Dictionary<string, int>(); // Track player's choice frequency

    public Game(IRules rules, int roundsToWin = 3)
    {
        this.rules = rules;
        this.roundsToWin = roundsToWin;
        random = new Random();
        gameHistory = new GameHistory();
    }

    // Start the game
    public void Play()
    {
        string[] choices = rules.GetChoices(); // Store choices in a variable

        string playerName;

        Console.WriteLine("================================");
        Console.WriteLine("Welcome to Rock, Paper, Scissors, Lizard, Spock!");
        Console.WriteLine("================================");

        Console.WriteLine("Enter your name:");
        playerName = Console.ReadLine();

        while (playerScore < roundsToWin && computerScore < roundsToWin)
        {
            playerChoiceFrequency.Clear(); // Reset player's choice frequency for each game

            gameHistory.DisplayHistory(); // Display history at the start of each game

            string computerChoice = GetComputerChoice(choices);

            Console.WriteLine($"\n{playerName}, choose your weapon: ({string.Join(", ", choices)})");
            string playerChoice = Console.ReadLine().ToLower();

            if (!rules.IsValidChoice(playerChoice))
            {
                Console.WriteLine("Invalid choice. Please choose from the available options.");
                continue;
            }

            Console.WriteLine($"Computer chose: {computerChoice}");

            bool playerWinsRound = rules.IsWinner(playerChoice, computerChoice);
            Console.WriteLine(playerWinsRound ? $"{playerName} wins this round!" : "Computer wins this round!");

            gameHistory.AddResult(playerWinsRound ? $"{playerName} wins this round!" : "Computer wins this round.");

            UpdatePlayerChoiceFrequency(playerChoice); // Update player's choice frequency

            if (playerWinsRound) playerScore++;
            else computerScore++;

            Console.WriteLine($"{playerName}: {playerScore} - Computer: {computerScore}");

            if (playerScore >= roundsToWin || computerScore >= roundsToWin)
            {
                break; // Exit the loop if either player or computer wins enough rounds
            }

            Console.WriteLine("\nDo you want to stick (s) or twist (t)?");
            string stickOrTwist = Console.ReadLine().ToLower();

            if (stickOrTwist == "s" && playerScore >= roundsToWin)
            {
                break; // Exit the loop if the player chooses to stick and has won enough rounds
            }
        }

        gameHistory.SaveToFile("game_history.txt"); // Save history to a file

        AnalysePlayerStrategy(); // Analyse player's strategy at the end of the game

        gameHistory.DisplayHistory(); // Display history at the end of the game

        Console.WriteLine(playerScore > computerScore
            ? $"\nCongratulations, {playerName}! You win the game!"
            : "\nComputer wins the game. Better luck next time!");

        Console.WriteLine("\nThanks for playing!");
    }

    // Get computer choice based on player's strategy analysis
    private string GetComputerChoice(string[] choices)
    {
        string mostFrequentChoice;

        if (playerChoiceFrequency.Any())
        {
            mostFrequentChoice = playerChoiceFrequency.OrderByDescending(pair => pair.Value).First().Key;
        }
        else
        {
            // If playerChoiceFrequency is empty, choose randomly
            mostFrequentChoice = choices[random.Next(choices.Length)];
        }

        // Common calculations outside the loop
        int randomIndex = random.Next(choices.Length);

        // Use the common calculations within the loop
        string computerChoice = choices[randomIndex];

        // Implement your own strategy here based on playerChoiceFrequency
        // This is a simple example, and you can enhance it based on more complex strategies
        return computerChoice;
    }

    // Update player's choice frequency
    private void UpdatePlayerChoiceFrequency(string playerChoice)
    {
        if (playerChoiceFrequency.ContainsKey(playerChoice))
        {
            playerChoiceFrequency[playerChoice]++;
        }
        else
        {
            playerChoiceFrequency[playerChoice] = 1;
        }
    }

    // Analyse player's strategy and display the results
    private void AnalysePlayerStrategy()
    {
        Console.WriteLine("\nPlayer's Strategy Analysis:");

        foreach (var pair in playerChoiceFrequency.OrderByDescending(pair => pair.Value))
        {
            Console.WriteLine($"Choice: {pair.Key}, Frequency: {pair.Value}");
        }
    }
}

// Factory class to create instances of the game with different rules
class GameFactory
{
    public static Game CreateGame(IRules rules, int roundsToWin = 3)
    {
        return new Game(rules, roundsToWin);
    }
}

class Program
{
    // Entry point of the program
    static void Main()
    {
        // Instantiate the game with default rules
        IRules defaultRules = new DefaultRules();
        Game gameWithDefaultRules = GameFactory.CreateGame(defaultRules);
        gameWithDefaultRules.Play();

        // If you want to use custom rules:
        IRules customRules = new CustomRules();
        Game gameWithCustomRules = GameFactory.CreateGame(customRules);
        gameWithCustomRules.Play();
    }
}
