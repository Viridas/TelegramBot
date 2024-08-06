### SkillBot Documentation

#### Overview

SkillBot is a Telegram bot developed to showcase skills, projects, and provide contact information. The bot also includes a quiz feature to engage users.

#### Features
- Display skills
- Show project information
- Provide contact details
- Play a quiz
- Play a game

#### Technologies Used
- C#
- .NET
- Telegram.Bot library

#### Bot Commands

- **/start**: Starts the bot and displays the main menu with options.
- **/skills**: Displays the skills of the bot.
- **/project**: Shows information about a project, including a video.
- **/contact**: Provides contact information.
- **/quiz**: Starts a quiz.

#### Inline Keyboard Options
- **Skills**: Shows the skills of the bot.
- **Project**: Shows project information and a video.
- **Contact**: Provides contact details.
- **Quiz**: Starts a quiz.
- **Play Game**: Opens a web game.

#### Code Structure

- **questions**: A dictionary to track the quiz state for each user.
- **answers**: A list of possible answers for the quiz.
- **bot**: The Telegram bot client.
- **cts**: A `CancellationTokenSource` to handle bot termination.

#### Event Handlers

- **OnError**: Handles errors in polling or other parts of the bot.
- **OnMessage**: Handles messages received by the bot.
- **OnUpdate**: Handles other types of updates received by the bot.

#### Functions

- **SendQuestionAsync**: Sends a quiz question to the user.
- **ProcessAnswerAsync**: Processes the user's answer and updates the quiz state.
- **GetQuestionText**: Retrieves the text for a given quiz question index.
- **GetTotalQuestions**: Returns the total number of quiz questions.
- **GetKeyboardMarkup**: Creates a keyboard markup for quiz answers.
- **CheckAnswer**: Checks if the given answer is correct for the current question.

#### Example Usage

1. **Start the bot**: 
    ```sh
    dotnet run
    ```
2. **Send /start command**:
    - The bot will display the main menu with inline keyboard options.
3. **Select "Skills"**:
    - The bot will send a message listing its skills.
4. **Select "Project"**:
    - The bot will send a message with project information and a video.
5. **Select "Contact"**:
    - The bot will send a message with contact details.
6. **Select "Quiz"**:
    - The bot will start the quiz and ask the first question.
7. **Play the Game**:
    - The bot will open the web game.
