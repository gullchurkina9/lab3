using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<QuizQuestion> _questions = new();

    public MainWindow()
    {
        InitializeComponent();
        QuestionsListBox.ItemsSource = _questions;

        try
        {
            DatabaseInitializer.Initialize();
            LoadQuestions();
            StatusLabel.Text = "Подключение к БД через Entity Framework Core выполнено.";
            StatusLabel.Foreground = Brushes.ForestGreen;
        }
        catch (Exception ex)
        {
            ShowDatabaseError(ex);
        }

        UpdateOptionsInfo();
    }

    // READ: LINQ-запрос преобразуется EF Core в SQL.
    private void LoadQuestions()
    {
        using var db = new AppDbContext();
        var items = db.QuizQuestions
            .AsNoTracking()
            .OrderBy(q => q.QuestionId)
            .ToList();

        _questions.Clear();
        foreach (var item in items)
            _questions.Add(item);

        QuestionCountLabel.Text = $"Вопросов: {db.QuizQuestions.Count()}";
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadQuestions();
            StatusLabel.Text = "Список обновлён через EF Core и LINQ.";
            StatusLabel.Foreground = Brushes.ForestGreen;
        }
        catch (Exception ex) { ShowDatabaseError(ex); }
    }

    // CREATE
    private void AddQuestion_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInput(out string text, out string difficulty)) return;

        try
        {
            using var db = new AppDbContext();
            db.QuizQuestions.Add(new QuizQuestion
            {
                Text = text,
                Difficulty = difficulty
            });
            db.SaveChanges();

            LoadQuestions();
            QuestionTextBox.Clear();
            StatusLabel.Text = "Вопрос добавлен в базу данных.";
            StatusLabel.Foreground = Brushes.ForestGreen;
        }
        catch (DbUpdateException ex) { ShowDatabaseError(ex); }
        catch (Exception ex) { ShowDatabaseError(ex); }
    }

    // UPDATE
    private void UpdateQuestion_Click(object sender, RoutedEventArgs e)
    {
        if (QuestionsListBox.SelectedItem is not QuizQuestion selected)
        {
            ShowInputMessage("Выберите вопрос, который нужно изменить.");
            return;
        }
        if (!ValidateInput(out string text, out string difficulty)) return;

        try
        {
            using var db = new AppDbContext();
            var question = db.QuizQuestions
                .FirstOrDefault(q => q.QuestionId == selected.QuestionId);

            if (question == null)
            {
                ShowInputMessage("Запись уже отсутствует в базе. Обновите список.");
                return;
            }

            question.Text = text;
            question.Difficulty = difficulty;
            db.SaveChanges();

            LoadQuestions();
            StatusLabel.Text = "Изменения сохранены через EF Core.";
            StatusLabel.Foreground = Brushes.ForestGreen;
        }
        catch (DbUpdateException ex) { ShowDatabaseError(ex); }
        catch (Exception ex) { ShowDatabaseError(ex); }
    }

    // DELETE
    private void DeleteQuestion_Click(object sender, RoutedEventArgs e)
    {
        if (QuestionsListBox.SelectedItem is not QuizQuestion selected)
        {
            ShowInputMessage("Сначала выберите вопрос для удаления.");
            return;
        }
        if (MessageBox.Show("Удалить выбранный вопрос?", "Подтверждение",
            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

        try
        {
            using var db = new AppDbContext();
            var question = db.QuizQuestions
                .FirstOrDefault(q => q.QuestionId == selected.QuestionId);

            if (question != null)
            {
                db.QuizQuestions.Remove(question);
                db.SaveChanges();
            }

            LoadQuestions();
            QuestionTextBox.Clear();
            StatusLabel.Text = "Вопрос удалён из базы данных.";
            StatusLabel.Foreground = Brushes.ForestGreen;
        }
        catch (DbUpdateException ex) { ShowDatabaseError(ex); }
        catch (Exception ex) { ShowDatabaseError(ex); }
    }

    private bool ValidateInput(out string text, out string difficulty)
    {
        text = QuestionTextBox.Text.Trim();
        difficulty = (EditDifficultyComboBox.SelectedItem as ComboBoxItem)?
            .Content?.ToString() ?? "Легко";

        if (string.IsNullOrWhiteSpace(QuizTitleTextBox.Text))
        {
            ShowInputMessage("Введите название квиза.");
            QuizTitleTextBox.Focus();
            return false;
        }
        if (string.IsNullOrWhiteSpace(text))
        {
            ShowInputMessage("Введите текст вопроса.");
            QuestionTextBox.Focus();
            return false;
        }
        if (text.Length > 1000)
        {
            ShowInputMessage("Текст вопроса не должен превышать 1000 символов.");
            QuestionTextBox.Focus();
            return false;
        }
        return true;
    }

    private void QuestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (QuestionsListBox.SelectedItem is QuizQuestion question)
        {
            QuestionTextBox.Text = question.Text;
            EditDifficultyComboBox.SelectedIndex = question.Difficulty switch
            {
                "Средне" => 1, "Сложно" => 2, _ => 0
            };
            StatusLabel.Text = $"Выбран вопрос №{question.QuestionId}.";
            StatusLabel.Foreground = Brushes.SteelBlue;
        }
    }

    private void ClearFields_Click(object sender, RoutedEventArgs e)
    {
        QuestionsListBox.SelectedItem = null;
        QuestionTextBox.Clear();
        EditDifficultyComboBox.SelectedIndex = 0;
        StatusLabel.Text = "Поля ввода очищены.";
        StatusLabel.Foreground = Brushes.SlateGray;
    }

    private void Option_Changed(object sender, RoutedEventArgs e) => UpdateOptionsInfo();

    private void UpdateOptionsInfo()
    {
        string mode = ReturnRadioButton?.IsChecked == true ? "с возвратом" : "однопроходное";
        string timer = TimerCheckBox?.IsChecked == true ? "таймер включён" : "без таймера";
        string hints = HintsCheckBox?.IsChecked == true ? "подсказки разрешены" : "подсказки выключены";
        if (OptionsInfoLabel != null)
            OptionsInfoLabel.Text = $"Режим: {mode}; {timer}; {hints}.";
    }

    private void ShowInputMessage(string message)
    {
        StatusLabel.Text = message;
        StatusLabel.Foreground = Brushes.IndianRed;
        MessageBox.Show(message, "Проверка ввода",
            MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void ShowDatabaseError(Exception ex)
    {
        StatusLabel.Text = "Ошибка работы с базой данных.";
        StatusLabel.Foreground = Brushes.IndianRed;
        MessageBox.Show(
            "Не удалось выполнить операцию с базой данных. Проверьте установку SQL Server LocalDB и строку подключения.\n\n" +
            ex.Message,
            "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
