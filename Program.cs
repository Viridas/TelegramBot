using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.InputFiles;

class Program
{

    static ConcurrentDictionary<long, int> questions = new ConcurrentDictionary<long, int>();
    static List<string> answers = new List<string> { "4", "20", "50", "30" };
    static async Task Main()
    {
        using var cts = new CancellationTokenSource();

        var bot = new TelegramBotClient("7499411039:AAGvIJHyqBmp-9TsAVj-dMO09OgReoGVzZk");

        var me = await bot.GetMeAsync();
        Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");

        // Configure receiver options
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        Console.ReadLine();
        cts.Cancel(); // stop the bot
    }

    static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message!.Text != null)
        {
            await OnMessage(botClient, update.Message, cancellationToken);
        }
        else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            await OnCallbackQuery(botClient, update.CallbackQuery, cancellationToken);
        }
    }

    static async Task OnMessage(ITelegramBotClient botClient, Message msg, CancellationToken cancellationToken)
    {
        var chatId = msg.Chat.Id;

        if (questions.ContainsKey(chatId))
        {
            // During the quiz, only process answers
            if (answers.Contains(msg.Text))
            {
                await ProcessAnswerAsync(botClient, chatId, msg.Text, cancellationToken);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Please select an answer from the keyboard.",
                    replyMarkup: GetKeyboardMarkup(answers),
                    cancellationToken: cancellationToken
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
                    InlineKeyboardButton.WithWebApp("Play Game", new WebAppInfo { Url = "https://viridas.github.io/TgGame/www/index.html" })
                }
            });

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: @"🎉 Welcome! This is SkillBot. Here is a list of commands that this bot can execute
- Use /skills to see my skills
- Use /project to see what sites can I create
- Use /contact to contact with me
- Use /quiz to test your knowledge
- Press 'Play Game' to play the game",
                replyMarkup: inlineKeyboard,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if (msg.Text == "/skills")
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "📚 My skills are:\n- Development of websites with responsive design using ASP.NET Core + Angular\n- Telegram Bot Development using C#\n- Creating an API\n- Working with the database (MSSQL, MySQL)",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );

        }
        else if (msg.Text == "/project")
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "👕 Here is the Landing Page of the clothing store I created",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );

            await botClient.SendVideoAsync(
                chatId: chatId,
                video: new InputOnlineFile("https://viridas.github.io/Video/LandingPage.mp4"),
                supportsStreaming: true,
                cancellationToken: cancellationToken
            );

        }
        else if (msg.Text == "/contact")
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "📧 You can contact me at:\nEmail: dlyawarfeisa@gmail.com\nTelegram: @BoyFromSelo\nGitHub: https://github.com/Viridas",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if (msg.Text == "/quiz")
        {
            questions[chatId] = 0; // Start with the first question
            await SendQuestionAsync(botClient, chatId, cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❗ Incorrect command ❗",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
    }

    static async Task SendQuestionAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
    {
        var questionIndex = questions[chatId];
        var questionText = GetQuestionText(questionIndex);
        var replyMarkup = GetKeyboardMarkup(answers);

        await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: questionText,
            replyMarkup: replyMarkup,
            cancellationToken: cancellationToken
        );
    }

    static async Task ProcessAnswerAsync(ITelegramBotClient botClient, long chatId, string answer, CancellationToken cancellationToken)
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
            await SendQuestionAsync(botClient, chatId, cancellationToken);
        }
    }

    static string GetQuestionText(int index)
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

    static int GetTotalQuestions()
    {
        return 5; // Total number of questions
    }

    static IReplyMarkup GetKeyboardMarkup(List<string> answers)
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

    static async Task OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery query, CancellationToken cancellationToken)
    {
        var chatId = query.Message.Chat.Id;

        if (query.Data == "skills")
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "📚 My skills are:\n- Development of websites with responsive design using ASP.NET Core + Angular\n- Telegram Bot Development using C#\n- Creating an API\n- Working with the database (MSSQL, MySQL)",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if (query.Data == "project")
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "👕 Here is the Landing Page of the clothing store I created",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );

            await botClient.SendVideoAsync(
                chatId: chatId,
                video: new InputOnlineFile("https://viridas.github.io/Video/LandingPage.mp4"),
                supportsStreaming: true,
                cancellationToken: cancellationToken
            );

        }
        else if (query.Data == "contact")
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "📧 You can contact me at:\nEmail: dlyawarfeisa@gmail.com\nTelegram: @BoyFromSelo\nGitHub: https://github.com/Viridas",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if (query.Data == "quiz")
        {
            questions[chatId] = 0; // Start with the first question
            await SendQuestionAsync(botClient, chatId, cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "❗ Incorrect command ❗",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
    }

    static async Task<bool> CheckAnswer(long chatId, string answer)
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

        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
