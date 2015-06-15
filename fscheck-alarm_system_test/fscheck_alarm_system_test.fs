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

let DELTA_COMPARE = 5
let DELTA_WAIT = 15

let waitingQueue = new Queue<bool>()

let addWaiting(v) = waitingQueue.Enqueue(v)
let getNextWaiting() = waitingQueue.Dequeue()

[<Struct>]
type StateChangedEvent =
    val State: StateChangedEventArgs
    val Time: DateTime
    new(state, time) = {State = state; Time = time}

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
        printfn "diff is too big: %d" diff.Milliseconds
        false

// ----------------------------------------------------------------------------------

let waitAndCheckTimedEvent (model : AlarmSystem) (impl : AlarmSystem) timeToWait = 
    addWaiting true
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
        true
    else
        false

let rnd = System.Random(DateTime.Now.Millisecond)

let checkForTimedStateChange (model : AlarmSystem) (impl : AlarmSystem) = 
  
    let randomBool = (rnd.Next(0, 2) = 0)

    let timeToWait =
        match model.CurrentState with
        | AlarmSystemState.ClosedAndLocked -> Some switchToArmedTime
        | AlarmSystemState.AlarmFlashAndSound -> Some switchToFlashTime
        | AlarmSystemState.AlarmFlash -> Some switchToSilentAndOpenTime
        | _ -> None

    if randomBool && timeToWait.IsSome then
        waitAndCheckTimedEvent model impl timeToWait.Value
    else
        addWaiting false
        true

// ----------------------------------------------------------------------------------

let getNumber() = Gen.sized <| fun s -> Gen.choose (0,9999)

let x = Gen.frequency [ (2, gen { return 5 }); (1, ((Gen.sized <| fun s -> Gen.choose (0,s)) gen)]

let y = Gen.oneof [ gen { return 5 }; gen { let! r = (Gen.sized <| fun () -> Gen.choose (0,99) r) } ]

// ----------------------------------------------------------------------------------
let spec =
    let specOpen =
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Open(); c
            member x.RunModel m = m.Open(); m
            
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c
                (timedCheck && m.CurrentState = c.CurrentState) |> Prop.ofTestable

            override x.ToString() = if getNextWaiting() then "open w" else "open"}

    let specClose = 
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Close(); c
            member x.RunModel m = m.Close(); m
            
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c
                (timedCheck && m.CurrentState = c.CurrentState) |> Prop.ofTestable
            
            override x.ToString() = if getNextWaiting() then "close w" else "close"}

    let specLock = 
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Lock(); c
            member x.RunModel m = m.Lock(); m

            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c
                (timedCheck && m.CurrentState = c.CurrentState) |> Prop.ofTestable
            
            override x.ToString() = if getNextWaiting() then "lock w" else "lock"}

    let specUnlock = 
        { new ICommand<AlarmSystem, AlarmSystem>() with
            member x.Pre c = true
            member x.RunActual c = c.Unlock("1234"); c
            member x.RunModel m = m.Unlock("1234"); m
            
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c
                (timedCheck && m.CurrentState = c.CurrentState) |> Prop.ofTestable

            override x.ToString() = if getNextWaiting() then "unlock w" else "unlock"}

    { new ISpecification<AlarmSystem,AlarmSystem> with
      member x.Initial() = 

        let alarmSystemImpl = new AlarmSystemImpl(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime) :> AlarmSystem
        alarmSystemImpl.StateChanged.Add(implementationEventHandler)

        let alarmSystemModel = new AlarmSystemModel(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime) :> AlarmSystem
        alarmSystemModel.StateChanged.Add(modelEventHandler)

        (alarmSystemImpl, alarmSystemModel)

      member x.GenCommand _ = Gen.elements [specOpen;specClose;specLock;specUnlock] }

AlarmSystemImpl.ShutDownAll()
//Check.Verbose(asProperty spec)
Check.Quick(asProperty spec)
AlarmSystemImpl.ShutDownAll()