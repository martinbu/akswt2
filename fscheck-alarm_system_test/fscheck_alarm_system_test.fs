module fscheck_alarm_system_test

// https://github.com/fsharp/FsCheck/blob/master/Docs/Documentation.md
// https://github.com/fsharp/FsUnit
// https://code.google.com/p/unquote/

open FsCheck

open alarm_system_common
open alarm_system
open alarm_system_model

open System
open System.Threading
//type AlarmSystemState = OpenAndUnlocked | ClosedAndUnlocked | OpenAndLocked
//                        | ClosedAndLocked | Armed | SilentAndOpen | AlarmFlashAndSound | AlarmFlash

let switchToArmedTime = 50
let switchToFlashTime = 80
let switchToSilentAndOpenTime = 150

let DIFF = 10

let mutable wait = false

open FsCheck.Commands
open System.Collections.Generic

let list = new List<StateChangedEventArgs>()

let eventHandler (args : StateChangedEventArgs) = 
    if wait then
        list.Add(args)

let doWait (model : AlarmSystem) (impl : AlarmSystem) timeToWait = 
    wait <- true
    System.Threading.Thread.Sleep(timeToWait + DIFF)
    if list.Count > 1 then 
        printfn "To much states %s | %s"  (model.CurrentStateType.ToString()) (impl.CurrentStateType.ToString())
        for element in list do
            printfn "    o: %s | n: %s" (element.OldStateType.ToString()) (element.NewStateType.ToString())
    else
        printfn "Current State in wait: %s | %s" (model.CurrentStateType.ToString()) (impl.CurrentStateType.ToString())
    wait <- false
    list.Clear()

let waitFor (model : AlarmSystem) (impl : AlarmSystem) = 
    match model.CurrentStateType with
    | AlarmSystemStateType.ClosedAndLocked -> doWait model impl switchToArmedTime
    | AlarmSystemStateType.AlarmFlashAndSound -> doWait model impl switchToFlashTime
    | AlarmSystemStateType.AlarmFlash -> doWait model impl switchToSilentAndOpenTime
    | _ -> ()


let spec =
    let specOpen =
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Open(); c
            member x.RunModel m = m.Open(); m
            
            member x.Post (c,m) = 
                let r1 = m.CurrentStateType = c.CurrentStateType
                waitFor m c
                (m.CurrentStateType = c.CurrentStateType && r1) |> Prop.ofTestable

            override x.ToString() = "open"}

    let specClose = 
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Close(); c
            member x.RunModel m = m.Close(); m
            
            member x.Post (c,m) = 
                let r1 = m.CurrentStateType = c.CurrentStateType
                waitFor m c
                (m.CurrentStateType = c.CurrentStateType && r1) |> Prop.ofTestable
            
            override x.ToString() = "close"}

    let specLock = 
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Lock(); c
            member x.RunModel m = m.Lock(); m

            member x.Post (c,m) = 
                let r1 = m.CurrentStateType = c.CurrentStateType
                waitFor m c
                (m.CurrentStateType = c.CurrentStateType && r1) |> Prop.ofTestable
            
            override x.ToString() = "lock"}

    let specUnlock = 
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.Pre c = true
            member x.RunActual c = c.Unlock(); c
            member x.RunModel m = m.Unlock(); m
            
            member x.Post (c,m) = 
                let r1 = m.CurrentStateType = c.CurrentStateType
                waitFor m c
                (m.CurrentStateType = c.CurrentStateType && r1) |> Prop.ofTestable

            override x.ToString() = "unlock"}

    { new ISpecification<AlarmSystem,AlarmSystem> with
      member x.Initial() = 
        let alarmSystem = new AlarmSystemImpl(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime) :> AlarmSystem
        alarmSystem.StateChanged.Add(eventHandler)
        (alarmSystem, new AlarmSystemModel(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime) :> AlarmSystem)

      member x.GenCommand _ = Gen.elements [specOpen;specClose;specLock;specUnlock] }

//Check.Verbose(asProperty spec)
Check.Quick(asProperty spec)