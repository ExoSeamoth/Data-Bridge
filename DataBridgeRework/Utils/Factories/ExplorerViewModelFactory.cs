using DataBridgeRework.Utils.Services.SftpClientService;
using DataBridgeRework.ViewModels;

namespace DataBridgeRework.Utils.Factories;

public interface IExplorerViewModelFactory
{
    public ExplorerViewModel Create();
    public ExplorerViewModel Create(string path);
}

public sealed class ExplorerViewModelFactory(ISftpClientService sftpClientService) : IExplorerViewModelFactory
{
    public ExplorerViewModel Create()
    {
        return new ExplorerViewModel(sftpClientService);
    }

    public ExplorerViewModel Create(string path)
    {
        return new ExplorerViewModel(sftpClientService)
        {
            CurrentFullPath = path
        };
    }
}