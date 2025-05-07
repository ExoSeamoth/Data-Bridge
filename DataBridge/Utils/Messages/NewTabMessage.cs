using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DataBridge.Utils.Messages;

public sealed class NewTabMessage(string value) : ValueChangedMessage<string>(value);