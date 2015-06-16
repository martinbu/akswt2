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
open FsCheck.Commands
open System.Collections.Generic

//type AlarmSystemState = OpenAndUnlocked | ClosedAndUnlocked | OpenAndLocked
//                        | ClosedAndLocked | Armed | SilentAndOpen | AlarmFlashAndSound | AlarmFlash

let switchToArmedTime = 20
let switchToFlashTime = 40
let switchToSilentAndOpenTime = 80
let allowedWrongPinCodeCount = 3;

let DELTA_COMPARE = 5
let DELTA_WAIT = 15

[<Struct>]
type StateChangedEvent =
    val State: StateChangedEventArgs
    val Time: DateTime
    new(state, time) = {State = state; Time = time}

type WaitResult =
    { isOK : bool; 
      Waited : bool }

// ----------------------------------------------------------------------------------

let implEventList = new Stack<StateChangedEvent>()
let modelEventList = new Stack<StateChangedEvent>()

let implementationEventHandler (args : StateChangedEventArgs) = 
    lock implEventList
        (fun () -> implEventList.Push(new StateChangedEvent(args, DateTime.Now)))

let modelEventHandler (args : StateChangedEventArgs) = 
    lock modelEventList
        (fun () -> modelEventList.Push(new StateChangedEvent(args, DateTime.Now)))

let popModelEvent() =
    lock modelEventList (fun () -> modelEventList.Pop())        

let popImplEvent() =
    lock implEventList (fun () -> implEventList.Pop())

let clearAll() =
    lock modelEventList (fun () -> modelEventList.Clear())        
    lock implEventList (fun () -> implEventList.Clear())

// ----------------------------------------------------------------------------------

let compareTime (time1 : TimeSpan) (time2 : TimeSpan)=
    let diff = (time1 - time2)

    if -DELTA_COMPARE <= diff.Milliseconds && diff.Milliseconds <= DELTA_COMPARE then 
        true
    else 
        printfn "The time needed for the state switch was too big: %d" diff.Milliseconds
        false

// ----------------------------------------------------------------------------------

let waitAndCheckTimedEvent (model : AlarmSystem) (impl : AlarmSystem) timeToWait = 
    let fromState = model.CurrentState

    System.Threading.Thread.Sleep(timeToWait + DELTA_WAIT)

    let currentModelEvent = popModelEvent()
    let beforeModelEvent = popModelEvent()

    let currentImplEvent = popImplEvent()
    let beforeImplEvent = popImplEvent()

    let isTimeDiffOk = compareTime (currentModelEvent.Time - beforeModelEvent.Time) (currentImplEvent.Time - beforeImplEvent.Time)

    if beforeModelEvent.State.NewStateType = beforeImplEvent.State.NewStateType
        && currentModelEvent.State.NewStateType = currentImplEvent.State.NewStateType
        && isTimeDiffOk then
        {WaitResult.isOK = true; WaitResult.Waited = true}
    else
        {WaitResult.isOK = false; WaitResult.Waited = true}

let rnd = System.Random(DateTime.Now.Millisecond)

let checkForTimedStateChange (model : AlarmSystem) (impl : AlarmSystem) wait = 

    if not wait then
        {WaitResult.isOK = true; WaitResult.Waited = false}

    else
        let timeToWait =
            match model.CurrentState with
            | AlarmSystemState.ClosedAndLocked -> Some switchToArmedTime
            | AlarmSystemState.AlarmFlashAndSound -> Some switchToFlashTime
            | AlarmSystemState.AlarmFlash -> Some switchToSilentAndOpenTime
            | _ -> None

        if timeToWait.IsSome then
            waitAndCheckTimedEvent model impl timeToWait.Value
        else
            {WaitResult.isOK = true; WaitResult.Waited = false}

// ----------------------------------------------------------------------------------

let pinGenerator = Gen.frequency [ (1, (Gen.sized <| fun s -> Gen.choose (0,9999)) ); (1, gen { return 1234 })]
let getRandomPin () = (Gen.sample 1 1 (pinGenerator)).Head

let waitGenerator = Gen.oneof [ gen { return true }; gen { return false }]
let getRandomShouldWait () = (waitGenerator |> Gen.sample 1 1).Head

let getRandomShouldWait1 () = (waitGenerator |> Gen.sample 1 1).Head

let toString text wait = 
    if wait then
        text + "-w"
    else
        text
        
// ----------------------------------------------------------------------------------
let spec =

    let specOpen wait =
        let waited = ref false;
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Open(); c
            member x.RunModel m = m.Open(); m
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c wait
                waited := timedCheck.Waited
                (timedCheck.isOK && m.CurrentState = c.CurrentState) |> Prop.ofTestable

            override x.ToString() = toString "open" !waited}

    let specClose wait = 
        let waited = ref false;
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Close(); c
            member x.RunModel m = m.Close(); m
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c wait
                waited := timedCheck.Waited
                (timedCheck.isOK && m.CurrentState = c.CurrentState) |> Prop.ofTestable
            
            override x.ToString() = toString "close" !waited}

    let specLock wait = 
        let waited = ref false;
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Lock(); c
            member x.RunModel m = m.Lock(); m

            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c wait
                waited := timedCheck.Waited
                (timedCheck.isOK && m.CurrentState = c.CurrentState) |> Prop.ofTestable
            
            override x.ToString() = toString "lock" !waited}

    let specUnlock wait p = 
        let waited = ref false;
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Unlock(p.ToString()); c
            member x.RunModel m = m.Unlock(p.ToString()); m
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c wait
                waited := timedCheck.Waited
                (timedCheck.isOK && m.CurrentState = c.CurrentState) |> Prop.ofTestable

            override x.ToString() = toString "unlock" !waited}

    { new ISpecification<AlarmSystem,AlarmSystem> with
      member x.Initial() = 

        let alarmSystemImpl = new AlarmSystemImpl(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime, allowedWrongPinCodeCount) :> AlarmSystem
        alarmSystemImpl.StateChanged.Add(implementationEventHandler)

        let alarmSystemModel = new AlarmSystemModel(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime, allowedWrongPinCodeCount) :> AlarmSystem
        alarmSystemModel.StateChanged.Add(modelEventHandler)

        (alarmSystemImpl, alarmSystemModel)
        
      member x.GenCommand _ = Gen.oneof [ gen {return getRandomShouldWait() |> specOpen};
                                            gen {return getRandomShouldWait() |> specClose};
                                            gen {return getRandomShouldWait() |> specLock};
                                            gen {return getRandomPin() |> specUnlock (getRandomShouldWait())}] }

//let config = {
//    Config.Quick with 
//        MaxTest = 105
//    }

AlarmSystemImpl.ShutDownAll()
//Check.Verbose(asProperty spec)
Check.Quick (asProperty spec)
//Check.One (config, (asProperty spec))
AlarmSystemImpl.ShutDownAll()
