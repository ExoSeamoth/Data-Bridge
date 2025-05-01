using DataBridgeRework.Utils.Services.SftpClientService;
using DataBridgeRework.ViewModels;
using FluentAssertions;

namespace DataBridgeRework.Tests;

public sealed class ExplorerViewModelTests
{
    [Fact]
    public void CanInitialize_WithFakeSftpService()
    {
        var service = new FakeSftpClientService();
        var vm = new ExplorerViewModel(service);

        vm.Should().NotBeNull();
        // vm.CurrentDirectory.Should()...
    }
}