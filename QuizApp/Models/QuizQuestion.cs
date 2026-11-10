using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuizApp.Models;

/// <summary>
/// Сущность вопроса квиза. EF Core сопоставляет класс с таблицей QuizQuestions.
/// </summary>
[Table("QuizQuestions")]
public class QuizQuestion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int QuestionId { get; set; }

    [Required(ErrorMessage = "Введите текст вопроса.")]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Difficulty { get; set; } = "Легко";

    public override string ToString() => $"{QuestionId}. {Text} — {Difficulty}";
}
