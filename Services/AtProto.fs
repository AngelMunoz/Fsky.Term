namespace Fsky.Term.Services

open System
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open FSharp.Data.Adaptive
open FsToolkit.ErrorHandling

open FishyFlip
open Navs.Terminal.Gui

type AuthError =
  | UnableToAuthenticate
  | AuthException of exn


type AuthService(proto: ATProtocol, logger: ILogger<AuthService>) =

  let session: Models.Session option cval = cval None

  let saveToDisk () =
    try
      let bytes =
        System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
          proto.PasswordSession
        )

      System.IO.File.WriteAllBytes("session.json", bytes)
    with ex ->
      logger.LogError(ex, "An exception occurred while saving session.")
      ()

  let loadFromDisk () =
    try
      let bytes = System.IO.File.ReadAllBytes("session.json")

      System.Text.Json.JsonSerializer.Deserialize<Models.AuthSession>(bytes)
      |> Option.ofObj
    with ex ->
      logger.LogError(ex, "An exception occurred while loading session.")
      None


  member val BaseUrl =
    proto.BaseAddress |> Option.ofObj |> Option.map(fun u -> u.ToString())

  member val Handle =
    session |> AVal.map(fun s -> s |> Option.map(fun s -> s.Handle))

  member val Did = session |> AVal.map(fun s -> s |> Option.map(fun s -> s.Did))

  member _.TryRestoreSession() = task {
    let s = loadFromDisk()

    match s with
    | Some s ->
      let! res = proto.AuthenticateWithPasswordSessionAsync(s)
      let sess = res |> Option.ofObj
      AVal.setValue session sess

      return saveToDisk()
    | None -> return ()
  }

  member _.Login(username: string, password: string) = task {
    try
      let! response = proto.AuthenticateWithPasswordAsync(username, password)

      match response with
      | null ->
        logger.LogInformation("Unable to obtain session.")
        AVal.setValue session None
        return Error UnableToAuthenticate
      | response ->
        try
          saveToDisk()
        with _ ->
          ()

        logger.LogInformation("Session obtained.")
        AVal.setValue session (Some response)
        return Ok()
    with ex ->
      logger.LogError(ex, "An exception occurred while logging in.")
      return Error(AuthException ex)
  }


type FeedService(proto: ATProtocol, logger: ILogger<FeedService>) =

  let feeds = cval List.empty
  let selectedFeed: (Lexicon.App.Bsky.Feed.GeneratorView option) cval = cval None

  let posts = cval (None, List.empty)

  member _.Feeds = feeds :> aval<_>

  member _.SelectedFeed = selectedFeed :> aval<_>

  member _.Posts = posts :> aval<_>

  member _.LoadFeeds(?limit: int, ?cursor, ?token) = task {
    let! loadedFeeds = taskOption {
      let! session = proto.Session |> Option.ofNull
      let! did = session.Did |> Option.ofNull

      try
        let! res =
          proto.Feed.GetActorFeedsAsync(
            did,
            ?limit = limit,
            ?cursor = cursor,
            ?cancellationToken = token
          )

        let! feeds =
          res.Match(
            (fun (res: Lexicon.App.Bsky.Feed.GetActorFeedsOutput | null) -> option {
              let! (res: Lexicon.App.Bsky.Feed.GetActorFeedsOutput) = res |> Option.ofNull
              return res.Feeds
            }),
            fun _ -> None
          )
        return feeds |> List.ofSeq
      with ex ->
        logger.LogError(ex, "An exception occurred while loading feeds.")
        return List.empty
    }

    match loadedFeeds with
    | Some loadedFeeds ->
      AVal.setValue feeds loadedFeeds

      return ()
    | None ->
      AVal.setValue feeds List.empty
      return ()
  }

  member _.LoadFeed(uri : Models.ATUri | null, ?limit, ?cursor) = task {
    let! feed = taskOption {
      let! uri = uri |> Option.ofObj
      let! res = proto.Feed.GetFeedAsync(uri, ?limit = limit, ?cursor = cursor)

      let! feed =
        res.Match(
          (fun (res: Lexicon.App.Bsky.Feed.GetFeedOutput) -> option { return res }),
          fun _ -> None
        )

      return feed
    }

    match feed with
    | Some feed ->
      let cursor = feed.Cursor |> Option.ofObj
      let feed = feed.Feed |> List.ofSeq
      AVal.setValue posts (cursor, feed)
      return ()
    | None ->
      AVal.setValue selectedFeed None
      return ()
  }

  member this.AdvanceCurentFeed() = task {
    match posts.Value with
    | (None, _) -> return ()
    | (Some cursor, _) ->

      match selectedFeed.Value with
      | None -> return ()
      | Some feed ->
        match feed.Uri with
        | null -> return ()
        | feed ->
          return! this.LoadFeed(feed, cursor = cursor)
  }

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
      | None -> atProto.WithInstanceUrl(Uri("https://bsky.social"))
      | Some instanceUrl -> atProto.WithInstanceUrl(instanceUrl)

    atProto.Build()
