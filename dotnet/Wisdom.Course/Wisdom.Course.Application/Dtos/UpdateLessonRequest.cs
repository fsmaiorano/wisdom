namespace Wisdom.Course.Application.Dtos;

public record UpdateLessonRequest(string Title, string Description, int Position, bool IsPreview);
