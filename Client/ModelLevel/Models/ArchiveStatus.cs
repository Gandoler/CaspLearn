namespace AwesomeFiles.Client.Models;

public enum ArchiveStatus
{
    Pending,    // только создан, в очереди
    Processing, // выполняется (сжатие, упаковка и т.п.)
    Ready,  // архив готов
    Failed      // ошибка при создании
}