open System
open Serilog
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open FishyFlip

open Terminal.Gui
open Navs

open Fsky.Term
open Fsky.Term.Services

[<EntryPoint; STAThread>]
let main argv =

  Log.Logger <-
    LoggerConfiguration()
#if DEBUG
      .MinimumLevel.Debug()
#else
      .MinimumLevel.Information()
#endif
      .WriteTo.Console()
      .WriteTo.File("fsky.term.log", rollingInterval = RollingInterval.Day)
      .CreateLogger()

  let host = Host.CreateApplicationBuilder(argv)
  Routes.validateAndSetDeepLink(host.Configuration, argv)

  host.Services
    .AddSerilog(Log.Logger)
    .AddSingleton<IRouter<Window>>(Routes.build)
    .AddSingleton<ATProtocol>(AtProto.getAtProto)
    .AddSingleton<FeedService>()
    .AddSingleton<AuthService>()
    .AddHostedService<FskyTermHost>()
  |> ignore

  let app = host.Build()

  app.Start()

  0 // return an integer exit code
