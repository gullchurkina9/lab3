-- Справочная схема, соответствующая конфигурации AppDbContext.
-- В рабочем приложении таблица создаётся автоматически методом EnsureCreated().
CREATE TABLE dbo.QuizQuestions
(
    QuestionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    QuestionText NVARCHAR(1000) NOT NULL,
    Difficulty NVARCHAR(20) NOT NULL,
    CONSTRAINT CK_QuizQuestions_Difficulty
        CHECK (Difficulty IN (N'Легко', N'Средне', N'Сложно'))
);
