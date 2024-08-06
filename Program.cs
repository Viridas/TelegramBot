using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using var cts = new CancellationTokenSource();

var questions = new ConcurrentDictionary<long, int>();
var answers = new List<string> { "4", "20", "50", "30" };

var bot = new TelegramBotClient("7499411039:AAGvIJHyqBmp-9TsAVj-dMO09OgReoGVzZk", cancellationToken: cts.Token);
var me = await bot.GetMeAsync();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel(); // stop the bot

// method to handle errors in polling or in your OnMessage/OnUpdate code
async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception); // just dump the exception to the console
}

// method that handle messages received by the bot:
async Task OnMessage(Message msg, UpdateType type)
{
    if (questions.ContainsKey(msg.Chat.Id))
    {
        // During the quiz, only process answers
        if (answers.Contains(msg.Text))
        {
            await ProcessAnswerAsync(bot, msg.Chat.Id, msg.Text, cts.Token);
        }
        else
        {
            await bot.SendTextMessageAsync(
                chatId: msg.Chat,
                text: "Please select an answer from the keyboard.",
                replyMarkup: GetKeyboardMarkup(answers)
            );
        }
        return;
    }

    if (msg.Text == "/start")
    {

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[] 
            {
                InlineKeyboardButton.WithCallbackData("Skills", "skills"),
                InlineKeyboardButton.WithCallbackData("Project", "project"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Contact", "contact"),
                InlineKeyboardButton.WithCallbackData("Quiz", "quiz"),
            },
            new[]
            {
                InlineKeyboardButton.WithWebApp("Play Game", "https://viridas.github.io/TgGame/www/index.html"),
            }
        });

        await bot.SendTextMessageAsync(
            chatId: msg.Chat,
            text: $@"🎉 *Welcome! This is SkillBot. Here is a list of commands that this bot can execute
            Use /skills to see my skills.
            Use /project to see what sites can I create
            Use /contact to contact whith me
            Use /quiz to test your knowledge
            Press ""Play Game"" to play game",
            replyMarkup: inlineKeyboard
        );
    }
    else if (msg.Text == "/skills")
    {
        await bot.SendTextMessageAsync(
            chatId: msg.Chat,
            text: "📚 *My skills are:\n- Development of websites with responsive design using ASP.NET Core + Angular\n- Telegram Bot Development using C#\n- Creating an API\n- Working with the database (MSSQL, MySQL)"
        );

    }
    else if (msg.Text == "/project")
    {
        await bot.SendTextMessageAsync(
            chatId: msg.Chat,
            text: "👕 *Here is the Landing Page of the clothing store I created"
        );

        await bot.SendVideoAsync(msg.Chat, "https://viridas.github.io/Video/LandingPage.mp4",
            thumbnail: "https://telegrambots.github.io/book/2/docs/thumb-clock.jpg", supportsStreaming: true);

    }
    else if (msg.Text == "/contact")
    {
        await bot.SendTextMessageAsync(
            chatId: msg.Chat,
            text: "📧 *You can contact me at:\nEmail: dlyawarfeisa@gmail.com\nTelegram: @BoyFromSelo\nGitHub: https://github.com/Viridas"
        );
    }
    else if (msg.Text == "/quiz")
    {
        questions[msg.Chat.Id] = 0; // Start with the first question
        await SendQuestionAsync(bot, msg.Chat.Id);
    }
    else
    {
        await bot.SendTextMessageAsync(
            chatId: msg.Chat,
            text: "❗ *Incorrect command* ❗"
        );
    }
}


async Task SendQuestionAsync(ITelegramBotClient botClient, long chatId)
{
    var questionIndex = questions[chatId];
    var questionText = GetQuestionText(questionIndex);
    var replyMarkup = GetKeyboardMarkup(answers);

    await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: questionText,
        replyMarkup: replyMarkup
    );
}

async Task ProcessAnswerAsync(ITelegramBotClient botClient, long chatId, string answer, CancellationToken cancellationToken)
{
    // Перевірте відповідь і оновіть рахунок
    bool isCorrect = await CheckAnswer(chatId, answer);
    questions[chatId]++;

    if (isCorrect)
    {
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "✅ Correct! Well done.",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );
    }
    else
    {
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "❌ Incorrect. Please try again.",
            cancellationToken: cancellationToken
        );
    }

    // Перевірте, чи закінчилися питання
    if (questions[chatId] >= GetTotalQuestions())
    {
        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "🏁 Quiz finished! 🏁",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: cancellationToken
        );
        questions.TryRemove(chatId, out _);
    }
    else
    {
        await SendQuestionAsync(botClient, chatId);
    }
}

string GetQuestionText(int index)
{
    var questionsList = new List<string>
    {
        "Question 1: How much is 2 + 2?🤔",
        "Question 2: What percentage of small businesses close within the first year of operation?🤔",
        "Question 3: What percentage of small businesses close within the first five years?🤔",
        "Question 4: What is the average percentage of annual revenue growth for tech startups in the first three years?🤔",
        "Question 5: How many years does it take to fully depreciate computer equipment in a business according to American standards?🤔"
    };

    return questionsList[index];
}

int GetTotalQuestions()
{
    return 5; // Total number of questions
}

IReplyMarkup GetKeyboardMarkup(List<string> answers)
{
    var keyboardButtons = new List<KeyboardButton[]>
    {
        new KeyboardButton[] { answers[0], answers[1] },
        new KeyboardButton[] { answers[2], answers[3] }
    };

    return new ReplyKeyboardMarkup(keyboardButtons)
    {
        ResizeKeyboard = true,
        OneTimeKeyboard = true
    };
}


// method that handle other types of updates received by the bot:
async Task OnUpdate(Update update)
{
    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
    {
        var chatId = query.Message.Chat.Id;

        if (query.Data == "skills")
        {
            await bot.SendTextMessageAsync(
                chatId: chatId,
                text: "📚 *My skills are:\n- Development of websites with responsive design using ASP.NET Core + Angular\n- Telegram Bot Development using C#\n- Creating an API\n- Working with the database (MSSQL, MySQL)"
            );

        }
        else if (query.Data == "project")
        {
            await bot.SendTextMessageAsync(
                chatId: chatId,
                text: "👕 *Here is the Landing Page of the clothing store I created"
            );

            await bot.SendVideoAsync(chatId, "https://viridas.github.io/Video/LandingPage.mp4",
                thumbnail: "https://telegrambots.github.io/book/2/docs/thumb-clock.jpg", supportsStreaming: true);

        }
        else if (query.Data == "contact")
        {
            await bot.SendTextMessageAsync(
                chatId: chatId,
                text: "📧 *You can contact me at:\nEmail: dlyawarfeisa@gmail.com\nTelegram: @BoyFromSelo\nGitHub: https://github.com/Viridas"
            );
        }
        else if (query.Data == "quiz")
        {
            questions[chatId] = 0; // Start with the first question
            await SendQuestionAsync(bot, chatId);
        }
    }
}

async Task<bool> CheckAnswer(long chatId, string answer)
{
    switch (questions[chatId])
    {
        case 0:
            if (answer == "4")
            {
                return true;
            }
            return false;
        case 1:
            if (answer == "20")
            {
                return true;
            }
            return false;
        case 2:
            if (answer == "50")
            {
                return true;
            }
            return false;
        case 3:
            if (answer == "30")
            {
                return true;
            }
            return false;
        case 4:
            if (answer == "4")
            {
                return true;
            }
            return false;
        default:
            return false;
    }
}
