namespace Fsky.Term.Services

open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open FishyFlip

module AtProto =

  let getAtProto (services: IServiceProvider) =
    let config = services.GetRequiredService<IConfiguration>()
    let lf = services.GetRequiredService<ILoggerFactory>()

    let instanceUrl =
      config.GetValue<string>("PdsInstance")
      |> Option.ofObj
      |> Option.bind(fun url ->
        try
          Some(Uri(url))
        with _ ->
          None
      )

    let atProto =
      ATProtocolBuilder()
        .WithLogger(lf.CreateLogger<ATProtocol>())
        .WithUserAgent("Fsky.Term/0.1")
        .EnableAutoRenewSession(true)

    let atProto =
      match instanceUrl with
      | None -> atProto
      | Some instanceUrl -> atProto.WithInstanceUrl(instanceUrl)

    atProto.Build()
