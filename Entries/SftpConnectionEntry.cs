using CommunityToolkit.Mvvm.ComponentModel;
using Data_Bridge.Converters;

namespace Data_Bridge.Entries;

public partial class SftpConnectionEntry(
    ushort port = 22,
    string hostName = "",
    string userName = "",
    SecurityType securityType = SecurityType.Password,
    string password = "",
    string sshKeyPath = "",
    string passPhrase = "")
    : ObservableObject
{
    [ObservableProperty] private ushort _port = port;

    [ObservableProperty] private string _hostName = hostName;
    [ObservableProperty] private string _userName = userName;
    
    [ObservableProperty] private SecurityType _securityType = securityType;
    
    [ObservableProperty] private string _password = password;
    
    [ObservableProperty] private string _sshKeyPath = sshKeyPath;
    [ObservableProperty] private string _passPhrase = passPhrase;
}

public enum SecurityType
{
    Password,
    SshKey,
}