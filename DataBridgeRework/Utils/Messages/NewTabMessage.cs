using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DataBridgeRework.Utils.Messages;

public sealed class NewTabMessage(string value) : ValueChangedMessage<string>(value);