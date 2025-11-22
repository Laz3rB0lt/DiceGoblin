require('dotenv').config();
const { Client, GatewayIntentBits } = require('discord.js');

const client = new Client({ intents: [GatewayIntentBits.Guilds] });

const allowedDice = [4, 6, 8, 10, 12, 20, 100];

client.once('ready', () => {
    console.log('✅ Bot is online!');
});

client.on('interactionCreate', async interaction => {
    if (!interaction.isChatInputCommand()) return;

    if (interaction.commandName === 'roll') {
        const expression = interaction.options.getString('expression');
        const result = rollDice(expression);
        await interaction.reply(result);
    }
});

function rollDice(input) {
    const regex = /^(\d*)d(\d+)([+-]\d+)?$/;
    const match = input.match(regex);

    if (!match) return '❌ Invalid dice format! Use e.g. `1d20+3`.';

    const count = match[1] ? parseInt(match[1]) : 1;
    const sides = parseInt(match[2]);
    const modifier = match[3] ? parseInt(match[3]) : 0;

    if (count <= 0 || sides <= 0) return '❌ Dice count and sides must be positive numbers.';
    if (!allowedDice.includes(sides)) return `❌ Invalid die type: d${sides}. Allowed: ${allowedDice.map(d => 'd'+d).join(', ')}`;

    const rolls = Array.from({ length: count }, () => Math.floor(Math.random() * sides) + 1);
    const total = rolls.reduce((a, b) => a + b, 0) + modifier;
    const modifierText = modifier !== 0 ? ` (${modifier > 0 ? '+' : ''}${modifier})` : '';
    return `🎲 **Roll:** ${input}\nResults: [${rolls.join(', ')}]${modifierText}\n**Total:** ${total}`;
}

client.login(process.env.DISCORD_TOKEN);
