require('dotenv').config();
const { REST, Routes, SlashCommandBuilder } = require('discord.js');

const commands = [
    new SlashCommandBuilder()
        .setName('roll')
        .setDescription('Roll dice using standard dice notation')
        .addStringOption(option => 
            option.setName('expression')
                  .setDescription('e.g. 1d20+3')
                  .setRequired(true))
].map(cmd => cmd.toJSON());

const rest = new REST({ version: '10' }).setToken(process.env.DISCORD_TOKEN);

(async () => {
    try {
        console.log('Started refreshing application (/) commands...');
        await rest.put(
            Routes.applicationGuildCommands(process.env.CLIENT_ID, process.env.GUILD_ID),
            { body: commands }
        );
        console.log('✅ Successfully registered commands.');
    } catch (err) {
        console.error(err);
    }
})();
