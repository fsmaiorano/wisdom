namespace Wisdom.Course.Application.Dtos;

public record CreateLessonRequest(string Title, string Description, int Position, bool IsPreview);
