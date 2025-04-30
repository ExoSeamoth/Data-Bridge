using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DataBridgeRework.Utils.Models;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace DataBridgeRework.Utils.Services.SftpClientService;

public interface ISftpClientService : IAsyncDisposable
{
    /// <summary>
    ///     Подключается к SFTP серверу с возможностью отмены
    /// </summary>
    /// <param name="connectionData">Данные подключения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="SftpConnectionException">Ошибка подключения</exception>
    Task ConnectAsync(ConnectionInfo connectionData, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Отключается от SFTP сервера с возможностью отмены
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Получает список файлов и каталогов по указанному пути с возможностью отмены
    /// </summary>
    /// <param name="path">Удалённый путь</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Коллекция элементов удалённой файловой системы</returns>
    /// <exception cref="SftpPathNotFoundException">Указанный путь не существует</exception>
    /// <exception cref="SftpPermissionDeniedException">Нет прав доступа</exception>
    IAsyncEnumerable<RemoteFileModel> ListDirectoryAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Загружает файл с SFTP сервера с возможностью отмены и прогрессом
    /// </summary>
    /// <param name="remoteFilePath">Путь к файлу на сервере</param>
    /// <param name="localPath">Локальный путь для сохранения</param>
    /// <param name="overwrite">Перезаписывать существующий файл</param>
    /// <param name="progress">Отслеживание прогресса</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="SftpFileNotFoundException">Файл не найден</exception>
    Task DownloadFileAsync(
        string remoteFilePath,
        string localPath,
        bool overwrite = false,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Загружает файл на SFTP сервер с возможностью отмены и прогрессом
    /// </summary>
    /// <param name="localFilePath">Локальный файл</param>
    /// <param name="remotePath">Удалённый путь для сохранения</param>
    /// <param name="overwrite">Перезаписывать существующий файл</param>
    /// <param name="progress">Отслеживание прогресса</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="FileNotFoundException">Локальный файл не найден</exception>
    Task UploadFileAsync(
        string localFilePath,
        string remotePath,
        bool overwrite = false,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Удаляет файл или пустую директорию по указанному пути
    /// </summary>
    /// <param name="remotePath">Удалённый путь</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <exception cref="SftpPathNotFoundException">Путь не существует</exception>
    /// <exception cref="SftpDirectoryNotEmptyException">Директория не пуста</exception>
    Task DeleteAsync(string remotePath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Рекурсивно удаляет директорию со всем содержимым
    /// </summary>
    /// <param name="remoteDirectoryPath">Удалённый путь директории</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteDirectoryRecursiveAsync(string remoteDirectoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Проверяет существование пути на SFTP сервере
    /// </summary>
    /// <param name="remotePath">Удалённый путь</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<bool> ExistsAsync(string remotePath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Создаёт директорию по указанному пути
    /// </summary>
    /// <param name="remoteDirectoryPath">Удалённый путь</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task CreateDirectoryAsync(string remoteDirectoryPath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Переименовывает файл или директорию
    /// </summary>
    /// <param name="oldPath">Текущий путь</param>
    /// <param name="newPath">Новый путь</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task RenameAsync(string oldPath, string newPath, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Возвращает информацию о файле/директории
    /// </summary>
    /// <param name="remotePath">Удалённый путь</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task<RemoteFileModel> GetFileInfoAsync(string remotePath, CancellationToken cancellationToken = default);
}