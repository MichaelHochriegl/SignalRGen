﻿//HintName: ExtendedNotificationHubClient.g.cs
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using SignalRGen.Abstractions;
using SignalRGen.Abstractions.Attributes;
using SignalRGen.Generator.Tests.TestData;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections.Client;

#nullable enable

namespace SignalRGen.Clients;

/// <summary>
/// Represents a HubClient for the <see cref = "IExtendedNotificationHubClient"/> interface.
/// </summary>
public class ExtendedNotificationHubClient : HubClientBase
{
    public static string HubUri { get; } = "extended-notifications";
    public ExtendedNotificationHubClient(Action<IHubConnectionBuilder>? hubConnectionBuilderConfiguration, Uri baseHubUri, Action<HttpConnectionOptions>? httpConnectionOptionsConfiguration) : base(hubConnectionBuilderConfiguration, baseHubUri, httpConnectionOptionsConfiguration)
    {
    }
    
    /// <summary>
    /// Is invoked whenever the client method ReceiveExtendedNotification of the <see cref = "IExtendedNotificationHubClient"/> gets invoked.
    /// </summary>
    public Func<string, int, Task>? OnReceiveExtendedNotification = default;
    private Task ReceiveExtendedNotificationHandler(string message, int priority)
    {
        return OnReceiveExtendedNotification?.Invoke(message, priority) ?? Task.CompletedTask;
    }
    /// <summary>
    /// Is invoked whenever the client method ReceiveCustomTypeNotification of the <see cref = "IExtendedNotificationHubClient"/> gets invoked.
    /// </summary>
    public Func<SignalRGen.Generator.Tests.TestData.CustomTypeDto, Task>? OnReceiveCustomTypeNotification = default;
    private Task ReceiveCustomTypeNotificationHandler(SignalRGen.Generator.Tests.TestData.CustomTypeDto dto)
    {
        return OnReceiveCustomTypeNotification?.Invoke(dto) ?? Task.CompletedTask;
    }
    /// <summary>
    /// Is invoked whenever the client method ReceiveBaseNotification of the <see cref = "IExtendedNotificationHubClient"/> gets invoked.
    /// </summary>
    public Func<string, Task>? OnReceiveBaseNotification = default;
    private Task ReceiveBaseNotificationHandler(string message)
    {
        return OnReceiveBaseNotification?.Invoke(message) ?? Task.CompletedTask;
    }



    
    protected override void RegisterHubMethods()
    {
        _hubConnection?.On<string, int>("ReceiveExtendedNotification", ReceiveExtendedNotificationHandler);
	    _hubConnection?.On<SignalRGen.Generator.Tests.TestData.CustomTypeDto>("ReceiveCustomTypeNotification", ReceiveCustomTypeNotificationHandler);
	    _hubConnection?.On<string>("ReceiveBaseNotification", ReceiveBaseNotificationHandler);
    }
    
    private void ValidateHubConnection()
    {
        if (_hubConnection is null)
        {
            throw new InvalidOperationException("The HubConnection is not started! Call `StartAsync` before initiating any actions.");
        }
    }
}