module fscheck_alarm_system_test

// https://github.com/fsharp/FsCheck/blob/master/Docs/Documentation.md
// https://github.com/fsharp/FsUnit
// https://code.google.com/p/unquote/

open FsCheck
open alarm_system

type AlarmSystemState = OpenAndUnlocked | ClosedAndUnlocked | OpenAndLocked
                        | ClosedAndLocked | Armed | SilentAndOpen | AlarmFlashAndSound | AlarmFlash

type AlarmSystemModel() =
    let mutable currentState = AlarmSystemState.OpenAndUnlocked
    member x.Open() = 
        match currentState with
        | AlarmSystemState.ClosedAndUnlocked -> currentState <- AlarmSystemState.OpenAndUnlocked
        | AlarmSystemState.ClosedAndLocked -> currentState <- AlarmSystemState.OpenAndLocked
        | AlarmSystemState.Armed -> currentState <- AlarmSystemState.AlarmFlashAndSound
        | _ -> printfn "No action for OPEN"

    member x.Close() = 
        match currentState with
        | AlarmSystemState.OpenAndUnlocked -> currentState <- AlarmSystemState.ClosedAndUnlocked
        | AlarmSystemState.OpenAndLocked -> currentState <- AlarmSystemState.ClosedAndLocked
        | AlarmSystemState.SilentAndOpen -> currentState <- AlarmSystemState.Armed
        | _ -> printfn "No action for CLOSE"

    member x.Lock() =
        match currentState with
        | AlarmSystemState.OpenAndUnlocked -> currentState <- AlarmSystemState.OpenAndLocked
        | AlarmSystemState.ClosedAndUnlocked -> currentState <- AlarmSystemState.ClosedAndLocked
        | _ -> printf "No action for LOCK"
    
    member x.Unlock() =
        match currentState with
        | AlarmSystemState.OpenAndLocked -> currentState <- AlarmSystemState.OpenAndUnlocked
        | AlarmSystemState.ClosedAndLocked -> currentState <- AlarmSystemState.ClosedAndUnlocked
        | AlarmSystemState.Armed -> currentState <- AlarmSystemState.ClosedAndUnlocked
        | AlarmSystemState.AlarmFlashAndSound -> currentState <- AlarmSystemState.OpenAndUnlocked
        | AlarmSystemState.AlarmFlash -> currentState <- AlarmSystemState.OpenAndUnlocked
        | AlarmSystemState.SilentAndOpen -> currentState <- AlarmSystemState.OpenAndUnlocked
        | _ -> printfn "No action for UNLOCK"

    member x.GetCurrentState() = currentState.ToString()

    override x.ToString() = currentState.ToString()

open FsCheck.Commands

let spec =
    let specOpen =
        { new ICommand<AlarmSystemImpl, AlarmSystemModel>() with
            member x.RunActual c = c.Open(); c
            member x.RunModel m = m.Open(); m
            member x.Post (c,m) = m.GetCurrentState() = c.CurrentStateType.ToString() |> Prop.ofTestable
            override x.ToString() = "open"}

    let specClose = 
        { new ICommand<AlarmSystemImpl, AlarmSystemModel>() with
            member x.RunActual c = c.Close(); c
            member x.RunModel m = m.Close(); m
            member x.Post (c,m) = m.GetCurrentState() = c.CurrentStateType.ToString() |> Prop.ofTestable
            override x.ToString() = "close"}

    let specLock = 
        { new ICommand<AlarmSystemImpl, AlarmSystemModel>() with
            member x.RunActual c = c.Lock(); c
            member x.RunModel m = m.Lock(); m
            member x.Post (c,m) = m.GetCurrentState() = c.CurrentStateType.ToString() |> Prop.ofTestable
            override x.ToString() = "lock"}

    let specUnlock = 
        { new ICommand<AlarmSystemImpl, AlarmSystemModel>() with
            member x.RunActual c = c.Unlock(); c
            member x.RunModel m = m.Unlock(); m
            member x.Post (c,m) = m.GetCurrentState() = c.CurrentStateType.ToString() |> Prop.ofTestable
            override x.ToString() = "unlock"}

    { new ISpecification<AlarmSystemImpl,AlarmSystemModel> with
      member x.Initial() = (new AlarmSystemImpl(), new AlarmSystemModel())
      member x.GenCommand _ = Gen.elements [specOpen;specClose;specLock;specUnlock] }

Check.Verbose(asProperty spec)
//Check.Quick(asProperty spec)

//let spec =
//  let inc = 
//      { new ICommand<Counter,int>() with
//          member x.RunActual c = c.Inc(); c
//          member x.RunModel m = m + 1
//          member x.Post (c,m) = m = c.Get |> Prop.ofTestable
//          override x.ToString() = "inc"}
//  let dec = 
//      { new ICommand<Counter,int>() with
//          member x.RunActual c = c.Dec(); c
//          member x.RunModel m = m - 1
//          member x.Post (c,m) = m = c.Get |> Prop.ofTestable
//          override x.ToString() = "dec"}
//  { new ISpecification<Counter,int> with
//      member x.Initial() = (new Counter(),0)
//      member x.GenCommand _ = Gen.elements [inc;dec] }
//
//
//Check.Quick(asProperty spec)

//type Counter() =
//  let mutable n = 0
//  member x.Inc() = n <- n + 1
//  member x.Dec() = if n > 2 then n <- n - 2 else n <- n - 1
//  member x.Get = n
//  member x.Reset() = n <- 0
//  override x.ToString() = n.ToString()

//open FsCheck.Commands


