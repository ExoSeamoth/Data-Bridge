using DataBridgeRework.Utils.Services.SftpSyncManager;
using DataBridgeRework.ViewModels;

namespace DataBridgeRework.Utils.Factories;

public interface IExplorerViewModelFactory
{
    public ExplorerViewModel Create();
    public ExplorerViewModel Create(string path);
}

public sealed class ExplorerViewModelFactory(ISftpSyncManager sftpSyncManager) : IExplorerViewModelFactory
{
    public ExplorerViewModel Create()
    {
        return new ExplorerViewModel(sftpSyncManager);
    }

    public ExplorerViewModel Create(string path)
    {
        return new ExplorerViewModel(sftpSyncManager)
        {
            CurrentFullPath = path
        };
    }
}