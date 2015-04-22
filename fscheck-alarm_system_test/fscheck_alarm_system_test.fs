module fscheck_alarm_system_test

// https://github.com/fsharp/FsCheck/blob/master/Docs/Documentation.md
// https://github.com/fsharp/FsUnit
// https://code.google.com/p/unquote/

open FsCheck
open alarm_system

//type AlarmSystemState = OpenAndUnlocked | ClosedAndUnlocked | OpenAndLocked
//                        | ClosedAndLocked | Armed | SilentAndOpen | AlarmFlashAndSound | AlarmFlash

type AlarmSystemModel() =
    let mutable currentState = AlarmSystemStateType.OpenAndUnlocked
    member x.Open() = 
        match currentState with
        | AlarmSystemStateType.ClosedAndUnlocked -> currentState <- AlarmSystemStateType.OpenAndUnlocked
        | AlarmSystemStateType.ClosedAndLocked -> currentState <- AlarmSystemStateType.OpenAndLocked
        | AlarmSystemStateType.Armed -> currentState <- AlarmSystemStateType.AlarmFlashAndSound
        | _ -> printfn "No action for OPEN"

    member x.Close() = 
        match currentState with
        | AlarmSystemStateType.OpenAndUnlocked -> currentState <- AlarmSystemStateType.ClosedAndUnlocked
        | AlarmSystemStateType.OpenAndLocked -> currentState <- AlarmSystemStateType.ClosedAndLocked
        | AlarmSystemStateType.SilentAndOpen -> currentState <- AlarmSystemStateType.Armed
        | _ -> printfn "No action for CLOSE"

    member x.Lock() =
        match currentState with
        | AlarmSystemStateType.OpenAndUnlocked -> currentState <- AlarmSystemStateType.OpenAndLocked
        | AlarmSystemStateType.ClosedAndUnlocked -> currentState <- AlarmSystemStateType.ClosedAndLocked
        | _ -> printfn "No action for LOCK"
    
    member x.Unlock() =
        match currentState with
        | AlarmSystemStateType.OpenAndLocked -> currentState <- AlarmSystemStateType.OpenAndUnlocked
        | AlarmSystemStateType.ClosedAndLocked -> currentState <- AlarmSystemStateType.ClosedAndUnlocked
        | AlarmSystemStateType.Armed -> currentState <- AlarmSystemStateType.ClosedAndUnlocked
        | AlarmSystemStateType.AlarmFlashAndSound -> currentState <- AlarmSystemStateType.OpenAndUnlocked
        | AlarmSystemStateType.AlarmFlash -> currentState <- AlarmSystemStateType.OpenAndUnlocked
        | AlarmSystemStateType.SilentAndOpen -> currentState <- AlarmSystemStateType.OpenAndUnlocked
        | _ -> printfn "No action for UNLOCK"

    member x.GetCurrentState() = currentState

    override x.ToString() = currentState.ToString()

open FsCheck.Commands
open System.Collections.Generic

let spec =
    let specOpen =
        { new ICommand<AlarmSystem, AlarmSystemModel>() with
            member x.RunActual c = c.Open(); c
            member x.RunModel m = m.Open(); m
            member x.Post (c,m) = m.GetCurrentState() = c.CurrentStateType |> Prop.ofTestable
            override x.ToString() = "open"}

    let specClose = 
        { new ICommand<AlarmSystem, AlarmSystemModel>() with
            member x.RunActual c = c.Close(); c
            member x.RunModel m = m.Close(); m
            member x.Post (c,m) = m.GetCurrentState() = c.CurrentStateType |> Prop.ofTestable
            override x.ToString() = "close"}

    let specLock = 
        { new ICommand<AlarmSystem, AlarmSystemModel>() with
            member x.RunActual c = c.Lock(); c
            member x.RunModel m = m.Lock(); m
            member x.Post (c,m) = m.GetCurrentState() = c.CurrentStateType |> Prop.ofTestable
            override x.ToString() = "lock"}

    let specUnlock = 
        { new ICommand<AlarmSystem, AlarmSystemModel>() with
            member x.RunActual c = c.Unlock(); c
            member x.RunModel m = m.Unlock(); m
            member x.Post (c,m) = m.GetCurrentState() = c.CurrentStateType |> Prop.ofTestable
            override x.ToString() = "unlock"}

    { new ISpecification<AlarmSystem,AlarmSystemModel> with
      member x.Initial() = (new AlarmSystemImpl() :> AlarmSystem, new AlarmSystemModel())
      member x.GenCommand _ = Gen.elements [specOpen;specClose;specLock;specUnlock] }

//Check.Verbose(asProperty spec)
Check.Quick(asProperty spec)
AlarmSystemImpl.ShutDownAll();