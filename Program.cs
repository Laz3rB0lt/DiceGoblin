using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using System.Text.RegularExpressions;

class Program
{
    private DiscordSocketClient _client;
    private InteractionService _interactions;

    // ⚠️ Replace this with your bot token
    private readonly string TOKEN = Environment.GetEnvironmentVariable("DISCORD_TOKEN");


    static async Task Main(string[] args)
        => await new Program().MainAsync();

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged,
            LogLevel = LogSeverity.Info
        });

        _interactions = new InteractionService(_client.Rest);
        _client.Log += LogAsync;

        _client.Ready += ReadyAsync;
        _client.InteractionCreated += HandleInteraction;

        await _client.LoginAsync(TokenType.Bot, TOKEN);
        await _client.StartAsync();

        Console.WriteLine("✅ Bot is running...");
        await Task.Delay(-1);
    }

    private async Task ReadyAsync()
    {
        await _interactions.AddModuleAsync<DiceModule>(null);
        await _interactions.RegisterCommandsGloballyAsync(true);
        Console.WriteLine("🎲 Slash commands registered!");
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _interactions.ExecuteCommandAsync(context, null);
    }

    private Task LogAsync(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }
}

public class DiceModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("roll", "Rolls dice using standard dice notation (e.g. 1d20+3, 4d6, etc.)")]
    public async Task RollDice([Summary("expression", "The dice to roll, e.g. 1d20+5")] string expression)
    {
        string result = Roll(expression);
        await RespondAsync(result);
    }

    private string Roll(string input)
    {
        // Strict pattern: optional count, 'd', sides, optional +/-(modifier)
        var match = Regex.Match(input, "^(\\d*)d(\\d+)([+-]\\d+)?$");

        if (!match.Success)
            return "❌ Invalid dice format! Use e.g. `/roll 1d20+3`.";

        int count = string.IsNullOrEmpty(match.Groups[1].Value) ? 1 : int.Parse(match.Groups[1].Value);
        int sides = int.Parse(match.Groups[2].Value);
        int modifier = string.IsNullOrEmpty(match.Groups[3].Value) ? 0 : int.Parse(match.Groups[3].Value);

        if (count <= 0 || sides <= 0)
            return "❌ Dice count and sides must be positive numbers.";

        // Allowed dice types
        var allowed = new List<int> { 4, 6, 8, 10, 12, 20, 100 };
        if (!allowed.Contains(sides))
        {
            var allowedText = string.Join(", ", allowed.ConvertAll(s => "d" + s));
            return $"❌ Invalid die type: d{sides}. Allowed dice are: {allowedText}.";
        }

        var rand = new Random();
        var rolls = new List<int>();

        for (int i = 0; i < count; i++)
            rolls.Add(rand.Next(1, sides + 1));

        int total = rolls.Sum() + modifier;
        string rollsText = string.Join(", ", rolls);
        string modifierText = modifier != 0 ? $" ({(modifier > 0 ? "+" : "")}{modifier})" : "";

        return $"🎲 **Roll:** {input}\nResults: [{rollsText}]{modifierText}\n**Total:** {total}";
    }
}
